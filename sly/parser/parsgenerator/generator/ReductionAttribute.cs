using System;
using System.Collections.Generic;
using System.Text;

namespace parser.parsergenerator.generator
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ReductionAttribute : Attribute
    {

        public string RuleString { get; set; }

        public ReductionAttribute(string rule)        {
            RuleString = rule;
        }
    }
}
