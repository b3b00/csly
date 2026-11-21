using System.Collections.Generic;
using System.Linq;

namespace rules.model;

public class Emptyness : Literal
{
    public Emptyness(bool isEmpty, Position position)
    {
        Position = position;
        IsEmpty = isEmpty;
    }

    public bool IsEmpty { get; set; }

    public bool IsNotEmpty => !IsEmpty;

   
    public override string Dump()
    {
        return IsEmpty ? "VIDE" : "NON VIDE";
    }

    public override IEnumerable<Identifier> GetPropertyReferences()
    {
        return Enumerable.Empty<Identifier>();
    }

    public override IEnumerable<Identifier> GetSettedReferences()
    {
        return Enumerable.Empty<Identifier>();
    }
}
