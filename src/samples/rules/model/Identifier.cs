using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace rules.model;

public class Identifier : RuleExpression
{
    public const string InputPrefix = "input_";
    public const string OutputPrefix = "output_";

    public Identifier(string name, Position position)
    {
        ArgumentNullException.ThrowIfNull(name, nameof(name));

        Position = position;
        IsIdentifier = true;
        IsInput = false;
        IsOutput = false;
        if (name.StartsWith(InputPrefix))
        {
            Name = name.Substring(InputPrefix.Length);
            IsGlobal = false;
            IsInput = true;
        }
        else if (name.StartsWith(OutputPrefix))
        {
            Name = name.Substring(OutputPrefix.Length);
            IsGlobal = false;
            IsOutput = true;
        }
       else {
            Name = name;
            IsInput = false;
            IsOutput = false;
            IsGlobal = false;
            IsLocal = true;
        }
    }

    public string Name { get; }

    public bool IsInput { get; }

    public bool IsOutput { get; }

    public bool IsLocal { get; }
    
    public bool IsGlobal { get; }

    private string InputName => $"{InputPrefix}{Name}";

    private string OutputName => $"{OutputPrefix}{Name}";

    public string FullName => IsInput ? InputName : IsOutput ? OutputName : Name;


    [ExcludeFromCodeCoverage]
    public override string Dump()
    {
        return FullName;
    }

    public override IEnumerable<Identifier> GetPropertyReferences()
    {
        yield return this;
    }

    public override IEnumerable<Identifier> GetSettedReferences()
    {
        return Enumerable.Empty<Identifier>();
    }

    public override bool Equals(object obj)
    {
        if (obj != null)
        {
            if (obj is Identifier id)
            {
                return id.IsInput == IsInput && id.IsOutput == IsOutput && id.Name == Name;
            }
        }

        return false;
    }
    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}
