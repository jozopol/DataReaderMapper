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
        private readonly Dictionary<Type, Tuple<Delegate, Expression>> _mapperCache = new Dictionary<Type, Tuple<Delegate, Expression>>();

        private readonly Dictionary<Type, Expression> _typeConvertors;

        public DataReaderMapper(Dictionary<Type, Expression> customTypeConvertors = null)
        {
            _typeConvertors = customTypeConvertors ?? TypeConvertors.DefaultConvertors;
        }
        
        public void Configure<TTarget>() where TTarget : class, new()
        {
            if (!_mapperCache.ContainsKey(typeof(TTarget)))
            {
                _mapperCache.Add(typeof(TTarget), BuildMapperExpression<TTarget>());
            }
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
                string invokedExpressionDebugView = _mapperCache[typeof(TTarget)].Item2.GetDebugView();
                throw new InvalidCastException($"An invalid cast occurred for one of the properties. Check this expression for details: {Environment.NewLine} {invokedExpressionDebugView}", ex);
                // log 
            }            
        }        

        public IEnumerable<TTarget>MapAll<TTarget>(TReader source) where TTarget : class, new()
        {
            if (!_mapperCache.ContainsKey(typeof(TTarget)))
                yield break;

            var mapperFunction = GetMapperFunction<TTarget>();
            while (source.Read())
            {
                yield return mapperFunction(source);
            }           
        }

        private Func<TReader, TTarget> GetMapperFunction<TTarget>() where TTarget : class, new()
        {
            return (Func<TReader, TTarget>)_mapperCache[typeof(TTarget)].Item1;
        }

        private Tuple<Delegate, Expression> BuildMapperExpression<TTarget>() where TTarget : class, new()
        {            
            var targetInstanceParameter = Expression.Variable(typeof(TTarget),"TTargetInstance");
            var dataReaderParameter = Expression.Parameter(typeof(TReader), "TReaderParameter");          

            // IDataReader instance has to implement "Item" targetProperty which is used for index access to IDataReader's table columns through IDataRecord interface
            var indexerProperty = typeof(TReader).GetProperty("Item", new[] { typeof(string) });

            var expressionBody = BuildExpressionBlock(targetInstanceParameter.Type, targetInstanceParameter, dataReaderParameter, indexerProperty);

            var lambda = Expression.Lambda<Func<TReader, TTarget>>(expressionBody, "TReader->TTarget lambda",  new[] { dataReaderParameter });
            return Tuple.Create<Delegate, Expression>(lambda.Compile(), lambda);
        }        

        private Expression BuildPropertyInitializerBlock(PropertyInfo targetProperty, Expression dataReaderParameter, PropertyInfo indexerProperty)
        {
            if (_mapperCache.ContainsKey(targetProperty.PropertyType))
                return _mapperCache[targetProperty.PropertyType].Item2;
            
            var propertyVariable = Expression.Variable(targetProperty.PropertyType, "variable");
            var expressionBlock = BuildExpressionBlock(targetProperty.PropertyType, propertyVariable, dataReaderParameter, indexerProperty);
            SaveToCache(targetProperty.PropertyType, expressionBlock, dataReaderParameter);
            return expressionBlock;
        }

        private BlockExpression BuildExpressionBlock(Type targetType, ParameterExpression targetInstanceParameter, Expression dataReaderParameter, PropertyInfo indexerProperty)
        {
            var expressionBlockBuilder = new ExpressionBlockBuilder(targetInstanceParameter.Type, new[] {targetInstanceParameter});
            return expressionBlockBuilder
                .Add(Expression.Assign(targetInstanceParameter, Expression.New(targetType)))
                .AddRange(BuildPrimitivePropertyExpressions(targetType, targetInstanceParameter, dataReaderParameter, indexerProperty))
                .AddRange(BuildComplexPropertyExpressions(targetType, targetInstanceParameter, dataReaderParameter, indexerProperty))
                .Add(targetInstanceParameter)
                .Build();
        }

        private IEnumerable<Expression> BuildComplexPropertyExpressions(Type targetType, Expression targetInstance, Expression dataReaderParameter, PropertyInfo indexerProperty)
        {
            return from property in GetMappableSourceProperties(targetType)
                let targetsPropertyInstance = Expression.Property(targetInstance, property) // targetInstance.SomeProperty
                let propertyInitializerBlock = BuildPropertyInitializerBlock(property, dataReaderParameter, indexerProperty) // block = new SomeProperty() { Prop1 = ..., Prop2 = ... }
                select Expression.Assign(targetsPropertyInstance, propertyInitializerBlock); // targetInstance.SomeProperty = block
        }

        private void SaveToCache(Type targetType, Expression expression, Expression dataReaderParameter)
        {
            if (_mapperCache.ContainsKey(targetType))            
                return;

                // we should cache each Func<IDataReader, T> where T != TTarget and where T is a nested complex targetProperty of TTarget or its other complex properties
                // we can use it later (if we want to map only the nested classes for example)
            var lambda = Expression.Lambda(expression, (ParameterExpression)dataReaderParameter);
            _mapperCache.Add(targetType, Tuple.Create(lambda.Compile(), expression));
        }

        private IEnumerable<Expression> BuildPrimitivePropertyExpressions(Type targetType, Expression targetInstance, Expression dataReaderParameter, PropertyInfo indexerProperty)
        {
            return from property in GetMappableProperties(targetType)
                let mappableAttribute = (MappableAttribute)Attribute.GetCustomAttribute(property, typeof(MappableAttribute))
                let recordColumnAccessor = ReaderColumnAccessor(dataReaderParameter, indexerProperty, mappableAttribute)
                let getOrdinalExpression = GetOrdinalExpression(dataReaderParameter, mappableAttribute)
                let isDbNullMethodExpression = IsDbNullExpression(dataReaderParameter, getOrdinalExpression)
                let targetsPropertyInstance = Expression.Property(targetInstance, property)
                let assignConvertedValue = Expression.Assign(targetsPropertyInstance, ConvertExpression(property.PropertyType, recordColumnAccessor, mappableAttribute))
                select Expression.IfThen(Expression.IsFalse(isDbNullMethodExpression), assignConvertedValue);
        }

        private static IEnumerable<PropertyInfo> GetMappableSourceProperties(Type targetType)
        {
            return targetType.GetRuntimeProperties().Where(x => x.IsDefined(typeof(MappableSourceAttribute), true));
        }

        private static IEnumerable<PropertyInfo> GetMappableProperties(Type targetType)
        {
            return targetType.GetRuntimeProperties().Where(x => x.IsDefined(typeof(MappableAttribute), true));
        }

        private static IndexExpression ReaderColumnAccessor(Expression dataReaderParameter, PropertyInfo indexerProperty,
            MappableAttribute mappableAttribute)
        {
            var recordColumnAccessor = Expression.MakeIndex(dataReaderParameter, indexerProperty, new[] {Expression.Constant(mappableAttribute.ReaderColumnName)});
            return recordColumnAccessor;
        }

        private static MethodCallExpression IsDbNullExpression(Expression dataReaderParameter, Expression getOrdinalExpression)
        {
            var isDbNullMethod = typeof(IDataRecord).GetMethod("IsDBNull");
            var isDbNullMethodExpression = Expression.Call(dataReaderParameter, isDbNullMethod, getOrdinalExpression);
            return isDbNullMethodExpression;
        }

        private static MethodCallExpression GetOrdinalExpression(Expression dataReaderParameter, MappableAttribute mappableAttribute)
        {
            var getOrdinalMethod = typeof(IDataRecord).GetMethod("GetOrdinal");
            var getOrdinalExpression = Expression.Call(dataReaderParameter, getOrdinalMethod, Expression.Constant(mappableAttribute.ReaderColumnName));
            return getOrdinalExpression;
        }


        private Expression ConvertExpression(Type typeToConvertTo, Expression recordColumnAccessor, MappableAttribute mappableAttribute)
        {
            return mappableAttribute.UseCustomConvertor
                ? ConvertExpression(recordColumnAccessor, typeToConvertTo)
                : Expression.Convert(recordColumnAccessor, typeToConvertTo);
        }

        private Expression ConvertExpression(Expression sourceToConvertExpression, Type conversionTargetType)
        {
            // try to get custom convertor function
            if (_typeConvertors.TryGetValue(conversionTargetType, out Expression convertorExpression))            
                return Expression.Invoke(convertorExpression, sourceToConvertExpression);            

            throw new InvalidOperationException($"The conversion to type {conversionTargetType.FullName} is not supported. Provide a custom type convertor via mapper constructor to convert this type.");
        }
    }
}
