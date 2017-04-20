
using cpg.parser.parsgenerator.syntax;
using lexer;

namespace parser.parsergenerator.syntax
{

    public class ConcreteSyntaxEpsilon<T> : IConcreteSyntaxNode<T> {


        public override string ToString()
        {
            return $"_";
        }
        public bool IsTerminal() {
            return true;
        }

    }
}