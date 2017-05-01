
using sly.parser.syntax;
using sly.lexer;

namespace sly.parser.syntax
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