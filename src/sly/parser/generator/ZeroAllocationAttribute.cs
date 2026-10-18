using System;

namespace sly.parser.generator;

/// <summary>
/// Opt-in flag enabling zero-allocation code paths in the lexer/parser pipeline.
/// Benchmark projects can detect this attribute via reflection to toggle
/// zero-allocation behavior on/off for dotnetbenchmark comparisons.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ZeroAllocationAttribute : Attribute
{
    public ZeroAllocationAttribute()
    {

    }
}
