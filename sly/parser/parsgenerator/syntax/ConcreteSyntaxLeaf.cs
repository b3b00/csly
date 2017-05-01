
using sly.parser.syntax;
using sly.lexer;

namespace sly.parser.syntax
{

    public class ConcreteSyntaxLeaf<T> : IConcreteSyntaxNode<T> {

        public Token<T> Token {get; set;}

        public ConcreteSyntaxLeaf(Token<T> token)
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