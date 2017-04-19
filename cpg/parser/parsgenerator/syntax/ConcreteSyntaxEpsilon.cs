
using cpg.parser.parsgenerator.syntax;
using lexer;

namespace parser.parsergenerator.syntax
{

    public class ConcreteSyntaxEpsilon<T> : IConcreteSyntaxNode<T> {

        

        public bool IsTerminal() {
            return true;
        }

    }
}