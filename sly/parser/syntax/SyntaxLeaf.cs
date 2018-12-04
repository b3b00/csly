using sly.lexer;

namespace sly.parser.syntax
{
    public class SyntaxLeaf<IN> : ISyntaxNode<IN> where IN : struct
    {
        public SyntaxLeaf(Token<IN> token)
        {
            Token = token;
        }

        public Token<IN> Token { get; set; }
        public string Name => Token.TokenID.ToString();

        public string Dump(string tab)
        {
            return $"{tab}TOKEN[{Token}]";
        }
    }
}