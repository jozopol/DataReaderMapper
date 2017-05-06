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
        private Dictionary<Type, Tuple<Delegate, Expression>> _mapperCache = new Dictionary<Type, Tuple<Delegate, Expression>>();

        private Dictionary<Type, Expression> _typeConvertors;
        private Dictionary<string, Expression> _idConvertors;
        private bool _hasIdConvertorsEnabled = false;

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

            // build the expressions for primitive (string/int/..) properties
            var primitivePropertySetters = MapPrimitiveProperties(typeof(TTarget), dataReaderParameter, indexerProperty);
            var setPrimitivePropertiesToTargetInstance = Expression.Assign(targetInstanceParameter, primitivePropertySetters);

            statements.Add(setPrimitivePropertiesToTargetInstance);

            // build the expressions for complex (classes) properties
            statements.AddRange(MapComplexProperties(typeof(TTarget), targetInstanceParameter, dataReaderParameter, indexerProperty));

            statements.Add(targetInstanceParameter); // return the mapped object

            var expressionBody = Expression.Block(targetInstanceParameter.Type, new[] { targetInstanceParameter }, statements.ToArray());

            var lambda = Expression.Lambda<Func<TReader, TTarget>>(expressionBody, "TReader->TTarget lambda",  new[] { dataReaderParameter });
            return Tuple.Create<Delegate, Expression>(lambda.Compile(), lambda);
        }

        private IEnumerable<Expression> MapComplexProperties(Type targetType, Expression targetInstance, Expression dataReaderParameter, PropertyInfo indexerProperty)
        {
            var statements = new List<Expression>();
            foreach (var property in targetType.GetRuntimeProperties().Where(x => x.IsDefined(typeof(MappableSourceAttribute), true)))
            {
                // targetInstance.SomePropertyInstance
                var targetsPropertyInstance = Expression.Property(targetInstance, property);

                var setPrimitiveProperties = MapPrimitiveProperties(property.PropertyType, dataReaderParameter, indexerProperty);
                CacheComplexPropertySetterExpression(dataReaderParameter, property.PropertyType, setPrimitiveProperties);

                // we can add the primitive setters directly into the expression (even though we cache them) and assign to the target instance
                var assignPropertiesToTargetInstance = Expression.Assign(targetsPropertyInstance, setPrimitiveProperties);
                statements.Add(assignPropertiesToTargetInstance);

                var setComplexProperties = MapComplexProperties(property.PropertyType, targetsPropertyInstance, dataReaderParameter, indexerProperty);
                statements.AddRange(setComplexProperties);
            }

            return statements;
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

        private Expression MapPrimitiveProperties(Type targetType, Expression dataReaderParameter, PropertyInfo indexerProperty)
        {
            var memberAssignments = new List<MemberAssignment>(); 
            foreach (var property in targetType.GetRuntimeProperties().Where(x => x.IsDefined(typeof(MappableAttribute), true)))
            {
                var mappableAttribute = (MappableAttribute)Attribute.GetCustomAttribute(property, typeof(MappableAttribute));

                // IDataReader["columnName"]
                var recordColumnAccessor = Expression.MakeIndex(dataReaderParameter, indexerProperty, new[] { Expression.Constant(mappableAttribute.ReaderColumnName) });

                // (object)IDataReader["columnName"] => Func<object, property.PropertyType> (dataReader) => convertorFunc(dataReader)
                var convertRecordToTargetPropertyType = BuildConvertorExpression(property.PropertyType, recordColumnAccessor, mappableAttribute);

                memberAssignments.Add(Expression.Bind(property.SetMethod, convertRecordToTargetPropertyType));
            }

            // new DTO { Prop1 = ... , Prop2 = ... , ... }
            return Expression.MemberInit(Expression.New(targetType), memberAssignments);
        }
        

        private Expression BuildConvertorExpression(Type typeToConvertTo, IndexExpression recordColumnAccessor, MappableAttribute mappableAttribute)
        {
            return mappableAttribute.UseCustomConvertor
                ? BuildConvertorExpression(recordColumnAccessor, typeToConvertTo, mappableAttribute.CustomConvertorId)
                : Expression.Convert(recordColumnAccessor, typeToConvertTo);
        }

        private Expression BuildConvertorExpression(Expression sourceToConvertExpression, Type conversionTargetType, string convertorId)
        {
            // try to get a convertor specified by his ID
            if (_hasIdConvertorsEnabled && _idConvertors.TryGetValue(convertorId, out Expression convertorExpression))
                return Expression.Invoke(convertorExpression, sourceToConvertExpression);

            // try to get custom convertor function
            if (_typeConvertors.TryGetValue(conversionTargetType, out convertorExpression))            
                return Expression.Invoke(convertorExpression, sourceToConvertExpression);            

            throw new InvalidOperationException($"The conversion to type {conversionTargetType.FullName} is not supported.");
        }

        private static MethodCallExpression ObjectToStringExpression(Expression sourceToConvertExpression)
        {
            var toStringMethod = typeof(object).GetMethod("ToString");
            return Expression.Call(sourceToConvertExpression, toStringMethod);
        }
    }
}
