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
        
        

        public Token<IN> Token { get; set; }
        public bool Discarded { get; }
        public string Name => Token.TokenID.ToString();

        [ExcludeFromCodeCoverage]
        public string Dump(string tab)
        {
            return $"{tab}TOKEN[{Token}]";
        }
    }
}