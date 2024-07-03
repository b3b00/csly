using System;

namespace sly.parser.generator;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class NodeNameAttribute(string name) : Attribute
{
    public  string Name { get;  } = name;
}