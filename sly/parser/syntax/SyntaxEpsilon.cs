
using sly.parser.syntax;
using sly.lexer;

namespace sly.parser.syntax
{

    public class SyntaxEpsilon<T> : ISyntaxNode<T> {


        public override string ToString()
        {
            return $"_";
        }
        public bool IsTerminal() {
            return true;
        }

        public string Dump(string tab)
        {
            
            return $"{tab}(e)";
        }

    }
}