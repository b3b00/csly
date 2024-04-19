using System;

namespace sly.parser.generator;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class BroadenTokenWindowAttribute : Attribute
{
    public BroadenTokenWindowAttribute()
    {
            
    }
}