using System;
using System.Collections.Generic;
using System.Linq;
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
                    { typeof(DateTime), (Expression<Func<object,DateTime>>)((object o) => DateTime.Parse(o.ToString()))},
                    { typeof(Int32), (Expression<Func<object, int>>)((object o) => Int32.Parse(o.ToString()))},
                    { typeof(Boolean), (Expression<Func<object, bool>>)((object o) => o.ToString() == "1")},
                    { typeof(List<string>), (Expression<Func<object, List<string>>>)((object o) => o.ToString().Split(',').ToList())},
                };
            }
        }
    }
}
