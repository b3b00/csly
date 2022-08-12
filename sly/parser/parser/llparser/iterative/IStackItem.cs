using System.Collections.Generic;
using sly.lexer;

namespace sly.parser.llparser.iterative
{
    public interface IStackItem<IN> where IN : struct
    {
        
        int Position { get; set; }
        
        IList<Token<IN>> Tokens { get; set; }
        
        bool IsRuleTrial { get; }

        bool IsClause { get; }
        
        bool HasBackTracked { get; set; }

        bool IsRoot { get; }
        string Dump();
    }
}