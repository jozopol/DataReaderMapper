using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DataReaderMapper.ExpressionExtensions
{
    public class ExpressionBlockBuilder
    {
        private readonly Type _blockReturnType;
        private readonly ParameterExpression[] _parameters;
        private readonly List<Expression> _expressions = new List<Expression>();

        public ExpressionBlockBuilder(Type blockReturnType, ParameterExpression[] parameters)
        {
            _blockReturnType = blockReturnType;
            _parameters = parameters;
        }

        public ExpressionBlockBuilder Add(Expression expression)
        {
            _expressions.Add(expression);
            return this;
        }

        public ExpressionBlockBuilder AddRange(IEnumerable<Expression> expressions)
        {
            _expressions.AddRange(expressions);
            return this;
        }

        public BlockExpression Build()
        {
            return Expression.Block(_blockReturnType, _parameters, _expressions);
        }
    }
}