using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DataReaderMapper
{
    public static class TypeConvertors
    {
        public static Dictionary<Type, Expression> DefaultConvertors
        {
            get
            {
                return new Dictionary<Type, Expression>()
                {
                    { typeof(DateTime), (Expression<Func<string,DateTime>>)((string s) => DateTime.Parse(s))},
                    { typeof(Int32), (Expression<Func<string, int>>)((string s) => Int32.Parse(s))},
                    //{ typeof(bool), (Expression<Func<string, bool>>)((string s) => Boolean.Parse(s))}
                };
            }
        }
    }
}
