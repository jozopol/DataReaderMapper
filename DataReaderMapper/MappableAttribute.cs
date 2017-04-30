using System;

namespace Mapping
{
    [AttributeUsage(AttributeTargets.Property)]
    public class MappableAttribute : Attribute
    {
        public string ReaderColumnName { get; set; }

        public MappableAttribute(string sourceName)
        {
            ReaderColumnName = sourceName;
        }
    }    
}
