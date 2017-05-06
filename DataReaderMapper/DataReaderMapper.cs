using Mapping;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DataReaderMapper
{
    public class DataReaderMapper<TReader> where TReader : IDataReader
    {
        private readonly Dictionary<Type, Tuple<Delegate, Expression>> _mapperCache = new Dictionary<Type, Tuple<Delegate, Expression>>();

        private readonly Dictionary<Type, Expression> _typeConvertors;
        private readonly Dictionary<string, Expression> _idConvertors;
        private readonly bool _hasIdConvertorsEnabled;

        public DataReaderMapper(Dictionary<Type, Expression> customTypeConvertors = null, Dictionary<string, Expression> idConvertors = null)
        {
            _typeConvertors = customTypeConvertors ?? TypeConvertors.DefaultConvertors;

            if (idConvertors != null)
            {
                _idConvertors = idConvertors;
                _hasIdConvertorsEnabled = true;
            }            
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
            var targetInstanceParameter = Expression.Variable(typeof(TTarget),"TargetInstance");
            var dataReaderParameter = Expression.Parameter(typeof(TReader), "TReaderParameter");          

            // IDataReader instance has to implement "Item" property which is used for index access to IDataReader's table columns
            var indexerProperty = typeof(TReader).GetProperty("Item", new[] { typeof(string) });

            var statements = new List<Expression>();

            statements.Add(Expression.Assign(targetInstanceParameter, Expression.New(typeof(TTarget)))); // new TTarget()

            // build the expressions for primitive (string/int/..) properties
            var primitivePropertySetters = BuildNullablePrimitivePropertiesExpressions(typeof(TTarget), targetInstanceParameter, dataReaderParameter, indexerProperty);
            statements.AddRange(primitivePropertySetters);

            // build the expressions for complex (classes) properties
            statements.AddRange(MapComplexProperties(typeof(TTarget), targetInstanceParameter, dataReaderParameter, indexerProperty));

            statements.Add(targetInstanceParameter); // return TTarget()

            var expressionBody = Expression.Block(targetInstanceParameter.Type, new[] { targetInstanceParameter }, statements.ToArray());

            var lambda = Expression.Lambda<Func<TReader, TTarget>>(expressionBody, "TReader->TTarget lambda",  new[] { dataReaderParameter });
            return Tuple.Create<Delegate, Expression>(lambda.Compile(), lambda);
        }

        private IEnumerable<Expression> MapComplexProperties(Type targetType, Expression targetInstance, Expression dataReaderParameter, PropertyInfo indexerProperty)
        {
            var statements = new List<Expression>();
            foreach (var property in GetMappableSourceProperties(targetType))
            {
                // targetInstance.SomePropertyInstance
                var targetsPropertyInstance = Expression.Property(targetInstance, property);
                // targetInstance.SomePropertyInstance = new SomePropertyInstance()
                //var newPropertyInstanceExpression = Expression.Assign(targetsPropertyInstance, Expression.New(property.PropertyType));

                //statements.Add(newPropertyInstanceExpression);

                //var assignPropertiesToTargetInstance = BuildNullablePrimitivePropertiesExpressions(property.PropertyType, targetsPropertyInstance, dataReaderParameter, indexerProperty);
                //statements.AddRange(assignPropertiesToTargetInstance);
                var newPropertyInstanceExpression = Expression.Assign(targetsPropertyInstance, TryCache(property, dataReaderParameter, indexerProperty));
                statements.Add(newPropertyInstanceExpression);

                var setComplexProperties = MapComplexProperties(property.PropertyType, targetsPropertyInstance, dataReaderParameter, indexerProperty);
                statements.AddRange(setComplexProperties);
            }

            return statements;
        }

        private Expression TryCache(PropertyInfo property, Expression dataReaderParameter, PropertyInfo indexerProperty)
        {
            var cachelist = new List<Expression>();

            var targetInstanceParameter = Expression.Variable(property.PropertyType, "TargetInstance.Property");
            cachelist.Add(Expression.Assign(targetInstanceParameter, Expression.New(property.PropertyType)));

            var assignPropertiesToTargetInstance = BuildNullablePrimitivePropertiesExpressions(property.PropertyType, targetInstanceParameter, dataReaderParameter, indexerProperty);
            cachelist.AddRange(assignPropertiesToTargetInstance);
            cachelist.Add(targetInstanceParameter);
            var variablePropAssign = Expression.Block(targetInstanceParameter.Type, new[] {targetInstanceParameter}, cachelist);
            return variablePropAssign;
        }

        private static IEnumerable<PropertyInfo> GetMappableSourceProperties(Type targetType)
        {
            return targetType.GetRuntimeProperties().Where(x => x.IsDefined(typeof(MappableSourceAttribute), true));
        }

        private void CacheComplexPropertySetterExpression(Expression dataReaderParameter, Type propertyType, Expression setPrimitivePropertiesExpression)
        {
            if (_mapperCache.ContainsKey(propertyType))            
                return;

                // we should cache each Func<IDataReader, T> where T != TTarget and wher T is a nested complex property of TTarget or its other complex properties
                // we can use it later (if we want to map only the nested classes for example)
            var lambda = Expression.Lambda(setPrimitivePropertiesExpression, (ParameterExpression)dataReaderParameter);
            _mapperCache.Add(propertyType, Tuple.Create<Delegate, Expression>(lambda.Compile(), lambda));
        }

        private IEnumerable<Expression> BuildNullablePrimitivePropertiesExpressions(Type targetType, Expression targetInstance, Expression dataReaderParameter, PropertyInfo indexerProperty)
        {
            foreach (var property in GetMappableProperties(targetType))
            {
                var targetsPropertyInstance = Expression.Property(targetInstance, property);

                var mappableAttribute = (MappableAttribute)Attribute.GetCustomAttribute(property, typeof(MappableAttribute));
                var recordColumnAccessor = ReaderColumnAccessor(dataReaderParameter, indexerProperty, mappableAttribute);

                var getOrdinalExpression = GetOrdinalExpression(dataReaderParameter, mappableAttribute);
                var isDbNullMethodExpression = IsDbNullExpression(dataReaderParameter, getOrdinalExpression);

                var assignConvertedValue = Expression.Assign(targetsPropertyInstance, ConvertExpression(property.PropertyType, recordColumnAccessor, mappableAttribute));

                yield return Expression.IfThen(Expression.IsFalse(isDbNullMethodExpression), assignConvertedValue);
            }
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
                ? ConvertExpression(recordColumnAccessor, typeToConvertTo, mappableAttribute.CustomConvertorId)
                : Expression.Convert(recordColumnAccessor, typeToConvertTo);
        }

        private Expression ConvertExpression(Expression sourceToConvertExpression, Type conversionTargetType, string convertorId)
        {
            // try to get a convertor specified by his ID
            if (_hasIdConvertorsEnabled && _idConvertors.TryGetValue(convertorId, out Expression convertorExpression))
                return Expression.Invoke(convertorExpression, sourceToConvertExpression);

            // try to get custom convertor function
            if (_typeConvertors.TryGetValue(conversionTargetType, out convertorExpression))            
                return Expression.Invoke(convertorExpression, sourceToConvertExpression);            

            throw new InvalidOperationException($"The conversion to type {conversionTargetType.FullName} is not supported. Check if the convertor for the specified type should be used.");
        }

        private static MethodCallExpression ObjectToStringExpression(Expression sourceToConvertExpression)
        {
            var toStringMethod = typeof(object).GetMethod("ToString");
            return Expression.Call(sourceToConvertExpression, toStringMethod);
        }
    }
}
