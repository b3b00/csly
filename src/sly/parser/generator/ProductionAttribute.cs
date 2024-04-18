using System;

namespace sly.parser.generator
{

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class UseMemoizationAttribute : Attribute
    {
        public UseMemoizationAttribute()
        {
            
        }
    }
    
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ProductionAttribute : Attribute
    {
        public ProductionAttribute(string rule)
        {
            RuleString = rule;
        }

        public string RuleString { get; set; }
    }
}