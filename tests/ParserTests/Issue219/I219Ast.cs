using System;
using System.Collections.Generic;

namespace ParserTests.Issue219;

public interface I219Ast {
    
}


public class Root219 : I219Ast{
    public List<I219Ast> Sets { get; set; }
}

public class Set219 : I219Ast
{
    public string Id { get; set; }
    public int Value { get; set; }

    public Set219(string id, int value)
    {
        Id = id;
        Value = value;
    }
}

public class Exception219 : Exception
{
    public Exception219(string error) : base(error)
    {
            
    }
}