using System;

namespace sly.parser.generator
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class OperandAttribute : Attribute
    {
    }
}