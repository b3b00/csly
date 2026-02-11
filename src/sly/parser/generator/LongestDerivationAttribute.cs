using System;

namespace sly.parser.generator;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class LongestDerivationAttribute : Attribute
{
    public LongestDerivationAttribute()
    {
            
    }
}