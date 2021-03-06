﻿using System.Linq.Expressions;
using System.Reflection;

namespace DataReaderMapper.ExpressionExtensions
{
    public static class ExpressionExtensions
    {
        // from https://stackoverflow.com/questions/5999668/accessing-expression-debugview-from-code
        public static string GetDebugView(this Expression exp)
        {
            if (exp == null)
                return null;

            var propertyInfo = typeof(Expression).GetProperty("DebugView", BindingFlags.Instance | BindingFlags.NonPublic);
            if (propertyInfo != null)
                return propertyInfo.GetValue(exp) as string;

            return "An unexpected error when reading the DebugView Property of an Expression.";
        }
    }
}
