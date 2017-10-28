
using sly.parser.syntax;
using sly.lexer;

namespace sly.parser.syntax
{

    public class SyntaxEpsilon<IN> : ISyntaxNode<IN> where IN : struct {


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

        public ISyntaxNode<IN> Clone()
        {
            return new SyntaxEpsilon<IN>();
        }
    }
}