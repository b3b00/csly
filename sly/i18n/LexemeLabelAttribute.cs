using System;

namespace sly.i18n
{
    
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class LexemeLabelAttribute : Attribute
    {
        public  string Label { get;  }
        
        public string Language { get; }

        public LexemeLabelAttribute(string language, string label)
        {
            Label = label;
            Language = language;
        }
    }
}