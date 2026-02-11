using System;

namespace sly.parser.generator;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class FirstDerivationAttribute : Attribute
{
    public FirstDerivationAttribute()
    {
            
    }
}