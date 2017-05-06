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
                return new Dictionary<Type, Expression>
                {
                    { typeof(DateTime), (Expression<Func<object,DateTime>>)(o => DateTime.Parse(o.ToString()))},
                    { typeof(int), (Expression<Func<object, int>>)(o => int.Parse(o.ToString()))},
                    { typeof(List<string>), (Expression<Func<object, List<string>>>)(o => o.ToString().Split(',').ToList())}
                };
            }
        }        
    }
}
