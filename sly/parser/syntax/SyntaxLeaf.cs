
using sly.parser.syntax;
using sly.lexer;

namespace sly.parser.syntax
{

    public class SyntaxLeaf<IN> : ISyntaxNode<IN> where IN : struct
    {

        public Token<IN> Token {get; set;}

        public SyntaxLeaf(Token<IN> token)
        {
            this.Token = token;
        }

        public override string ToString()
        {            
            return $"<{this.Token.TokenID}>{this.Token.Value}";
        }

        public bool IsTerminal() {
            return true;
        }

        public string Dump(string tab)
        {
            return $"{tab}({this.Token.TokenID} : {this.Token.Value})";
        }
          
    }
}