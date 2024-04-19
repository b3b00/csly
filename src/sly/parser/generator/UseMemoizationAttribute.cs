using System;

namespace sly.parser.generator;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class UseMemoizationAttribute : Attribute
{
    public UseMemoizationAttribute()
    {
            
    }
}