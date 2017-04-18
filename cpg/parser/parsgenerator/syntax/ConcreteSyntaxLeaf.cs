
using lexer;

namespace parser.parsergenerator.syntax
{

    public class ConcreteSyntaxLeaf<T>: ConcreteSyntaxNode<T> {

        public Token<T> Token {get; set;}

        public override bool IsTerminal() {
            return true;
        }

    }
}