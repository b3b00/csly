using System.Diagnostics.CodeAnalysis;
using sly.lexer;

namespace sly.parser.syntax
{
    public class SyntaxLeaf<IN> : ISyntaxNode<IN> where IN : struct
    {
        public SyntaxLeaf(Token<IN> token, bool discarded)
        {
            Token = token;
            Discarded = discarded;
        }
        
        

        public Token<IN> Token { get;  }
        public bool Discarded { get; } = false;
        public string Name => Token.TokenID.ToString();

        [ExcludeFromCodeCoverage]
        public string Dump(string tab)
        {
            return $"{tab}TOKEN[{Token}]";
        }
    }
}