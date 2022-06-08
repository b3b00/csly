using sly.lexer;

namespace sly.parser.syntax.tree
{
    public class SyntaxEpsilon<IN> : ISyntaxNode<IN> where IN : struct
    {
        public bool Discarded { get; } = false;
        public string Name => "Epsilon";

        public bool HasByPassNodes { get; set; } = false;

        public string Dump(string tab)
        {
            return $"Epsilon";
        }

        public string ToJson(int index = 0)
        {
            return $@"""{index}.Epsilon"":""e""";
        }
    }
}