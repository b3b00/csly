using System;

namespace sly.parser.generator;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class UsePoolAttribute : Attribute
{
    public UsePoolAttribute()
    {
            
    }
}