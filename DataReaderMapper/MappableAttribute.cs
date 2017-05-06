using System;

namespace DataReaderMapper
{
    [AttributeUsage(AttributeTargets.Property)]
    public class MappableAttribute: Attribute
    {
        public string ReaderColumnName { get; }
        public bool UseCustomConvertor { get; }
        public bool CanBeNull { get; }

        public MappableAttribute(string sourceName, bool useCustomConvertor = false, bool canBeNull = false)
        {
            ReaderColumnName = sourceName;
            UseCustomConvertor = useCustomConvertor;
            CanBeNull = canBeNull;
        }
    }    
}
