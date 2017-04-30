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
        private Dictionary<Type, Delegate> _mapperCache = new Dictionary<Type, Delegate>();


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
                return ((Func<TReader, TTarget>)_mapperCache[typeof(TTarget)])(source);
            }
            return new TTarget();
        }

        private Delegate BuildMapperExpression<TTarget>() where TTarget : class, new()
        {
            var targetInstanceParameter = Expression.Variable(typeof(TTarget));
            var dataReaderParameter = Expression.Parameter(typeof(TReader));

            var createInstance = Expression.Assign(targetInstanceParameter, Expression.New(typeof(TTarget))); // instance = new TTarget(); 

            var statements = new List<Expression>();
            statements.Add(createInstance);

            // IDataReader instance has to implement "Item" property which is used for index access to IDataReader's table columns
            var indexerProperty = typeof(TReader).GetProperty("Item", new[] { typeof(string) });

            // build the expressions for primitive (string/int/..) properties
            var primitivePropertyAccessors = BuildPrimitivePropertyAccessors(dataReaderParameter, typeof(TTarget), targetInstanceParameter, indexerProperty);
            statements.AddRange(primitivePropertyAccessors);

            // build the expressions for complex (classes) properties
            var complexPropertyAccessors = BuildComplexPropertyAccessors(typeof(TTarget), targetInstanceParameter, dataReaderParameter, indexerProperty);
            statements.AddRange(complexPropertyAccessors);

            statements.Add(targetInstanceParameter); // return the mapped object

            var expressionBody = Expression.Block(targetInstanceParameter.Type, new[] { targetInstanceParameter }, statements.ToArray());

            var lambda = Expression.Lambda<Func<TReader, TTarget>>(expressionBody, dataReaderParameter);
            return lambda.Compile();
        }

        private List<Expression> BuildComplexPropertyAccessors(Type targetType, Expression targetInstanceParameter, Expression dataReaderParameter, PropertyInfo indexerProperty)
        {
            var statements = new List<Expression>();
            foreach (var property in targetType.GetRuntimeProperties().Where(x => x.IsDefined(typeof(MappableSourceAttribute), true)))
            {
                var targetPropertyInstance = Expression.Property(targetInstanceParameter, property); // RootObject.ComplexProperty                
                var createNewInstance = Expression.Assign(targetPropertyInstance, Expression.New(property.PropertyType)); // RootObject.ComplexProperty = new typeof(complexProperty)
                statements.Add(createNewInstance);

                var nestedPropertiesAccessors = BuildPrimitivePropertyAccessors(dataReaderParameter, property.PropertyType, targetPropertyInstance, indexerProperty);
                statements.AddRange(nestedPropertiesAccessors);

                var complexPropertyAccessors = BuildComplexPropertyAccessors(property.PropertyType, targetPropertyInstance, dataReaderParameter, indexerProperty);
                statements.AddRange(complexPropertyAccessors);
            }

            return statements;
        }

        private static IEnumerable<Expression> BuildPrimitivePropertyAccessors(Expression dataReaderParameter, Type mappingTargetType, Expression targetPropertyInstance, PropertyInfo indexerProperty)
        {
            foreach (var property in mappingTargetType.GetRuntimeProperties().Where(x => x.IsDefined(typeof(MappableAttribute), true)))
            {
                var instanceProperty = Expression.Property(targetPropertyInstance, property);

                // DataRecord row[properties mappable atribute column name]
                var mappableAttribute = (MappableAttribute)Attribute.GetCustomAttribute(property, typeof(MappableAttribute));
                var recordColumnAccessor = Expression.MakeIndex(dataReaderParameter, indexerProperty, new[] { Expression.Constant(mappableAttribute.ReaderColumnName) });

                var assignProperty = Expression.Assign(instanceProperty, Expression.Convert(recordColumnAccessor, property.PropertyType)); //TODO add customizable convertors?

                yield return assignProperty;
            }
        }
    }
}
