using System;
using System.Linq.Expressions;

namespace Mapping
{
    [AttributeUsage(AttributeTargets.Property)]
    public class MappableAttribute: Attribute
    {
        public string ReaderColumnName { get; set; }
        public bool UseCustomConvertor { get; set; }

        public MappableAttribute(string sourceName, bool useCustomConvertor = false)
        {
            ReaderColumnName = sourceName;
            UseCustomConvertor = useCustomConvertor;
        }
    }    
}
