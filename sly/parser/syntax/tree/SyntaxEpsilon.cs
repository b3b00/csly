using sly.lexer;
using System.Diagnostics.CodeAnalysis;

namespace sly.parser.syntax.tree
{
    public class SyntaxEpsilon<IN> : ISyntaxNode<IN> where IN : struct
    {
        public bool Discarded { get; } = false;
        public string Name => "Epsilon";

        public bool HasByPassNodes { get; set; } = false;

        [ExcludeFromCodeCoverage]
        public string Dump(string tab)
        {
            return $"Epsilon";
        }

        [ExcludeFromCodeCoverage]
        public string ToJson(int index = 0)
        {
            return $@"""{index}.Epsilon"":""e""";
        }
    }
}
