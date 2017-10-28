
using sly.parser.syntax;
using sly.lexer;

namespace sly.parser.syntax
{

    public class SyntaxLeaf<IN> : ISyntaxNode<IN> {

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

        public ISyntaxNode<IN> Clone()
        {
            return new  SyntaxLeaf<IN>(Token);
        }
    }
}