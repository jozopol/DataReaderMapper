using System;

namespace Mapping
{
    [AttributeUsage(AttributeTargets.Property)]
    public class MappableAttribute: Attribute
    {
        public string ReaderColumnName { get; private set; }
        public bool UseCustomConvertor { get; private set; }
        public string CustomConvertorId { get; private set; }

        public MappableAttribute(string sourceName, bool useCustomConvertor = false)
        {
            ReaderColumnName = sourceName;
            UseCustomConvertor = useCustomConvertor;
        }

        public MappableAttribute(string sourceName, string customConvertorId)
        {
            ReaderColumnName = sourceName;
            CustomConvertorId = customConvertorId;
            UseCustomConvertor = true;
        }
    }    
}
