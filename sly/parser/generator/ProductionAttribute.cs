using System;
using System.Collections.Generic;
using System.Text;

namespace sly.parser.generator
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ProductionAttribute : Attribute
    {

        public string RuleString { get; set; }

        public ProductionAttribute(string rule)        {
            RuleString = rule;
        }
    }
}
