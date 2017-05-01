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

        private Dictionary<Type, Expression> _convertors;

        public DataReaderMapper(Dictionary<Type, Expression> customTypeConvertors = null)
        {
            _convertors = customTypeConvertors ?? TypeConvertors.DefaultConvertors;
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
            if (_mapperCache.ContainsKey(typeof(TTarget)))
            {
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
            return new TTarget();
        }        

        public IEnumerable<TTarget>MapAll<TTarget>(TReader source) where TTarget : class, new()
        {
            if (!_mapperCache.ContainsKey(typeof(TTarget)))
                yield break;

            Func<TReader, TTarget> mapperFunction = GetMapperFunction<TTarget>();
            while (source.Read())
            {
                yield return mapperFunction(source);
            }           
        }

        private Func<TReader, TTarget> GetMapperFunction<TTarget>() where TTarget : class, new()
        {
            return ((Func<TReader, TTarget>)_mapperCache[typeof(TTarget)].Item1);
        }

        private Tuple<Delegate, Expression> BuildMapperExpression<TTarget>() where TTarget : class, new()
        {            
            var targetInstanceParameter = Expression.Variable(typeof(TTarget));
            var dataReaderParameter = Expression.Parameter(typeof(TReader));          

            // IDataReader instance has to implement "Item" property which is used for index access to IDataReader's table columns
            var indexerProperty = typeof(TReader).GetProperty("Item", new[] { typeof(string) });

            var statements = new List<Expression>();
            // build the expressions for primitive (string/int/..) properties
            statements.Add(BuildPrimitiveMappings(typeof(TTarget), targetInstanceParameter, dataReaderParameter, indexerProperty));
            // build the expressions for complex (classes) properties
            statements.AddRange(BuildComplexPropertyAccessors(typeof(TTarget), targetInstanceParameter, dataReaderParameter, indexerProperty));

            statements.Add(targetInstanceParameter); // return the mapped object
            var expressionBody = Expression.Block(targetInstanceParameter.Type, new[] { targetInstanceParameter }, statements.ToArray());

            var lambda = Expression.Lambda<Func<TReader, TTarget>>(expressionBody, dataReaderParameter);
            return Tuple.Create<Delegate, Expression>(lambda.Compile(), lambda);
        }

        private List<Expression> BuildComplexPropertyAccessors(Type targetType, Expression rootObjectInstance, Expression dataReaderParameter, PropertyInfo indexerProperty)
        {
            var statements = new List<Expression>();
            foreach (var property in targetType.GetRuntimeProperties().Where(x => x.IsDefined(typeof(MappableSourceAttribute), true)))
            {
                var targetPropertyInstance = Expression.Property(rootObjectInstance, property);

                var mappings = BuildPrimitiveMappings(property.PropertyType, targetPropertyInstance, dataReaderParameter, indexerProperty);
                statements.Add(mappings);
                
                var complexPropertyAccessors = BuildComplexPropertyAccessors(property.PropertyType, targetPropertyInstance, dataReaderParameter, indexerProperty);
                statements.AddRange(complexPropertyAccessors);
            }

            return statements;
        }

        private Expression BuildPrimitiveMappings(Type targetType, Expression targetInstance, Expression dataReaderParameter, PropertyInfo indexerProperty)
        {
            var bindings = new List<MemberAssignment>(); 
            foreach (var property in targetType.GetRuntimeProperties().Where(x => x.IsDefined(typeof(MappableAttribute), true)))
            {
                var mappableAttribute = (MappableAttribute)Attribute.GetCustomAttribute(property, typeof(MappableAttribute));

                var recordColumnAccessor = Expression.MakeIndex(dataReaderParameter, indexerProperty, new[] { Expression.Constant(mappableAttribute.ReaderColumnName) });

                var convertorExpression = BuildConvertorExpression(property.PropertyType, mappableAttribute.UseCustomConvertor, recordColumnAccessor);

                bindings.Add(Expression.Bind(property.GetMethod, convertorExpression));
            }

            var initMember = Expression.MemberInit(Expression.New(targetType), bindings);
            return Expression.Assign(targetInstance, initMember);
        }
        

        private Expression BuildConvertorExpression(Type typeToConvertTo, bool useCustomConvertor, IndexExpression recordColumnAccessor)
        {
            return useCustomConvertor
                ? BuildConvertorExpression(recordColumnAccessor, typeToConvertTo)
                : Expression.Convert(recordColumnAccessor, typeToConvertTo);
        }

        private Expression BuildConvertorExpression(Expression sourceToConvertExpression, Type conversionTargetType)
        {
            if (_convertors.TryGetValue(conversionTargetType, out Expression convertorExpression))
            {
                MethodCallExpression sourceToStringExpression = ObjectToStringExpression(sourceToConvertExpression);
                return Expression.Invoke(convertorExpression, sourceToStringExpression);
            }

            throw new InvalidOperationException($"The conversion to type {conversionTargetType.FullName} is not supported.");
        }

        private static MethodCallExpression ObjectToStringExpression(Expression sourceToConvertExpression)
        {
            var toStringMethod = typeof(object).GetMethod("ToString");
            return Expression.Call(sourceToConvertExpression, toStringMethod);
        }
    }
}
