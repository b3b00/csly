
using sly.parser.syntax;
using sly.lexer;

namespace sly.parser.syntax
{

    public class SyntaxLeaf<T> : ISyntaxNode<T> {

        public Token<T> Token {get; set;}

        public SyntaxLeaf(Token<T> token)
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

    }
}