using System;

using sly.lexer;

namespace sly.parser.syntax.tree
{
    public class SyntaxLeaf<IN, OUT> : ISyntaxNode<IN, OUT> where IN : struct, Enum
    {
        public SyntaxLeaf()
        {
            
        }
        
        public SyntaxLeaf(Token<IN> token, bool discarded)
        {
            Token = token;
            Discarded = discarded;
        }
        
        public void Initialize(Token<IN> token, bool discarded)
        {
            Token = token;
            Discarded = discarded;
        }
        
        public bool IsEpsilon => false;

        public Token<IN> Token { get; private set; }
        public bool Discarded { get; private set; }
        public string Name => Token.TokenID.ToString();
        
        public bool HasByPassNodes { get; set; } = false;
        
        public string Dump(string tab)
        {
            return $"{tab}+ {Token.TokenID.ToString()} : {Token.Value} @{Token.PositionInTokenFlow}";
        }

        public string ToJson(int index = 0)
        {
            return $@"""{index}.{Token.TokenID.ToString()}"" : ""{Token.Value}""";
        }

        public void ForceName(string name)
        {
        }
    }
}