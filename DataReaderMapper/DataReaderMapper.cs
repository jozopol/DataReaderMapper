using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DataReaderMapper.ExpressionExtensions;

namespace DataReaderMapper
{
    public class DataReaderMapper<TReader> where TReader : IDataReader
    {
        private readonly Dictionary<Type, (Delegate MappingFunction, Expression Expression)> _mapperCache =
            new Dictionary<Type, (Delegate MappingFunction, Expression Expression)>();

        private readonly Dictionary<Type, Expression> _typeConvertors;

        public DataReaderMapper(Dictionary<Type, Expression> customTypeConvertors = null)
        {
            _typeConvertors = customTypeConvertors ?? TypeConvertors.DefaultConvertors;
        }

        public void Configure<TTarget>() where TTarget : class, new()
        {
            if (!_mapperCache.ContainsKey(typeof(TTarget)))
                BuildMapperExpression<TTarget>();
        }

        public TTarget Map<TTarget>(TReader source) where TTarget : class, new()
        {
            if (!_mapperCache.ContainsKey(typeof(TTarget)))
                return new TTarget();

            try
            {
                return GetMapperFunction<TTarget>()(source);
            }
            catch (InvalidCastException ex)
            {
                string invokedExpressionDebugView = _mapperCache[typeof(TTarget)].Expression.GetDebugView();
                throw new InvalidCastException(
                    $"An invalid cast occurred for one of the properties. Make sure that properties which need to be converted via custom convertors have their mappable attribute properly set. Check this expression for details: {Environment.NewLine} {invokedExpressionDebugView}",
                    ex);
            }
        }

        public IEnumerable<TTarget> MapAll<TTarget>(TReader source) where TTarget : class, new()
        {
            if (!_mapperCache.ContainsKey(typeof(TTarget)))
                yield break;

            var mapperFunction = GetMapperFunction<TTarget>();
            while (source.Read())
                yield return mapperFunction(source);
        }

        private Func<TReader, TTarget> GetMapperFunction<TTarget>() where TTarget : class, new()
        {
            return (Func<TReader, TTarget>) _mapperCache[typeof(TTarget)].MappingFunction;
        }

        private void BuildMapperExpression<TTarget>() where TTarget : class, new()
        {
            var targetInstanceParameter = Expression.Variable(typeof(TTarget), "TTargetInstance");
            var dataReaderParameter = Expression.Parameter(typeof(TReader), "TReaderParameter");

            // IDataReader instance has to implement "Item" targetProperty which is used for index access to IDataReader's table columns through IDataRecord interface
            var indexerProperty = typeof(TReader).GetProperty("Item", new[] {typeof(string)});

            var expressionBody = BuildExpressionBlock(targetInstanceParameter.Type, targetInstanceParameter,
                dataReaderParameter, indexerProperty);

            var lambda = Expression.Lambda<Func<TReader, TTarget>>(expressionBody, true, dataReaderParameter);
            _mapperCache[typeof(TTarget)] = (MappingFunction: lambda.Compile(), Expression: lambda);
        }

        private Expression BuildPropertyInitializerBlock(PropertyInfo targetProperty, ParameterExpression dataReaderParameter,
            PropertyInfo indexerProperty)
        {
            var propertyVariable = Expression.Variable(targetProperty.PropertyType, "variable");
            var expressionBlock = BuildExpressionBlock(targetProperty.PropertyType, propertyVariable,
                dataReaderParameter, indexerProperty);

            SaveToCache(targetProperty.PropertyType, expressionBlock, dataReaderParameter);
            return expressionBlock;
        }

        private BlockExpression BuildExpressionBlock(Type targetType, ParameterExpression targetInstanceParameter,
            ParameterExpression dataReaderParameter, PropertyInfo indexerProperty)
        {
            var expressionBlockBuilder = new ExpressionBlockBuilder(targetInstanceParameter.Type, new[] {targetInstanceParameter});

            return expressionBlockBuilder
                .Add(Expression.Assign(targetInstanceParameter, Expression.New(targetType)))
                .AddRange(BuildPrimitivePropertyExpressions(targetType, targetInstanceParameter, dataReaderParameter,
                    indexerProperty))
                .AddRange(BuildComplexPropertyExpressions(targetType, targetInstanceParameter, dataReaderParameter,
                    indexerProperty))
                .Add(targetInstanceParameter)
                .Build();
        }

        private IEnumerable<Expression> BuildComplexPropertyExpressions(Type targetType, Expression targetInstance,
            ParameterExpression dataReaderParameter, PropertyInfo indexerProperty)
        {
            try
            {
                return from property in GetMappableSourceProperties(targetType)
                    // targetInstance.SomeProperty
                    let targetsPropertyInstance = Expression.Property(targetInstance, property)
                    // block = new SomeProperty() { Prop1 = ..., Prop2 = ... }
                    let propertyInitializerBlock = BuildPropertyInitializerBlock(property, dataReaderParameter, indexerProperty)
                    // targetInstance.SomeProperty = block
                    select Expression.Assign(targetsPropertyInstance, propertyInitializerBlock);
            }
            catch (ArgumentException e)
            {
                throw new InvalidOperationException(
                    "A property type does not match the CanBeNull setting. Primitive properties must be nullable in order to correctly apply null conversion.",
                    e);
            }
        }

        private IEnumerable<Expression> BuildPrimitivePropertyExpressions(Type targetType, Expression targetInstance,
            Expression dataReaderParameter, PropertyInfo indexerProperty)
        {
            foreach (var property in GetMappableProperties(targetType))
            {
                var mappableAttribute = (MappableAttribute) Attribute.GetCustomAttribute(property, typeof(MappableAttribute));
                var recordColumnAccessor = ReaderColumnAccessor(dataReaderParameter, indexerProperty, mappableAttribute);

                var targetsPropertyInstance = Expression.Property(targetInstance, property);

                var convertExpression = ConvertExpression(property.PropertyType, recordColumnAccessor,
                    mappableAttribute);

                Expression assignConvertedValue;
                if (mappableAttribute.CanBeNull)
                {
                    var ifNullThenDefaultElseValueExpression = NullableConvertExpression(dataReaderParameter, mappableAttribute, property, convertExpression);

                    assignConvertedValue = Expression.Assign(targetsPropertyInstance, ifNullThenDefaultElseValueExpression);
                }
                else
                {
                    assignConvertedValue = Expression.Assign(targetsPropertyInstance, convertExpression);
                }
                yield return assignConvertedValue;
            }
        }

        private void SaveToCache(Type targetType, Expression expression, ParameterExpression dataReaderParameter)
        {
            if (_mapperCache.ContainsKey(targetType))
                return;

            // we should cache each Func<IDataReader, T> where T != TTarget and where T is a nested complex targetProperty of TTarget or its other complex properties
            // we can use it later (if we want to map only the nested classes for example)
            var lambda = Expression.Lambda(expression, true, dataReaderParameter);
            _mapperCache.Add(targetType, (lambda.Compile(), expression));
        }

        private static IEnumerable<PropertyInfo> GetMappableSourceProperties(Type targetType)
        {
            return targetType.GetRuntimeProperties().Where(x => x.IsDefined(typeof(MappableSourceAttribute), true));
        }

        private static IEnumerable<PropertyInfo> GetMappableProperties(Type targetType)
        {
            return targetType.GetRuntimeProperties().Where(x => x.IsDefined(typeof(MappableAttribute), true));
        }

        private static IndexExpression ReaderColumnAccessor(Expression dataReaderParameter,
            PropertyInfo indexerProperty,
            MappableAttribute mappableAttribute)
        {
            return Expression.MakeIndex(dataReaderParameter, indexerProperty,
                new[] {Expression.Constant(mappableAttribute.ReaderColumnName)});
        }

        private static ConditionalExpression NullableConvertExpression(Expression dataReaderParameter,
            MappableAttribute mappableAttribute, PropertyInfo property, Expression convertExpression)
        {
            var getOrdinalExpression = GetOrdinalExpression(dataReaderParameter, mappableAttribute);
            var isDbNullMethodExpression = IsDbNullExpression(dataReaderParameter, getOrdinalExpression);
            var ifNullThenNullElseValueExpression = Expression.Condition(isDbNullMethodExpression,
                Expression.Constant(null, property.PropertyType), convertExpression);
            return ifNullThenNullElseValueExpression;
        }

        private static MethodCallExpression IsDbNullExpression(Expression dataReaderParameter,
            Expression getOrdinalExpression)
        {
            var isDbNullMethod = typeof(IDataRecord).GetMethod("IsDBNull");
            return Expression.Call(dataReaderParameter, isDbNullMethod, getOrdinalExpression);
        }

        private static MethodCallExpression GetOrdinalExpression(Expression dataReaderParameter,
            MappableAttribute mappableAttribute)
        {
            var getOrdinalMethod = typeof(IDataRecord).GetMethod("GetOrdinal");
            return Expression.Call(dataReaderParameter, getOrdinalMethod,
                Expression.Constant(mappableAttribute.ReaderColumnName));
        }

        private Expression ConvertExpression(Type typeToConvertTo, Expression recordColumnAccessor,
            MappableAttribute mappableAttribute)
        {
            return mappableAttribute.UseCustomConvertor
                ? BuildCustomConvertorExpression(recordColumnAccessor, typeToConvertTo)
                : Expression.Convert(recordColumnAccessor, typeToConvertTo);
        }

        private Expression BuildCustomConvertorExpression(Expression sourceToConvertExpression,
            Type conversionTargetType)
        {
            // try to get custom convertor function
            if (_typeConvertors.TryGetValue(conversionTargetType, out Expression convertorExpression))
                return Expression.Invoke(convertorExpression, sourceToConvertExpression);

            throw new InvalidOperationException(
                $"The conversion to type {conversionTargetType.FullName} is not supported. Provide a custom type convertor via mapper constructor to convert this type.");
        }
    }
}
