using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace sly.lexer
{
    public class LexerResult<IN> where IN : struct
    {
        public bool IsError { get; set; }

        public bool IsOk => !IsError;
        
        public LexicalError Error { get; }
        
        public TokenChannels<IN> Tokens { get; set; }
        
        public LexerResult(List<Token<IN>> tokens)
        {
            SetTokens(tokens);
        }

        public void SetTokens(List<Token<IN>> tokens)
        {
            Tokens = new TokenChannels<IN>(tokens);
            Tokens.PreCompute();
        }
        
        public LexerResult(LexicalError error, List<Token<IN>> tokens)
        {
            IsError = true;
            Error = error;
            SetTokens(tokens);
        }
        
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            if (IsOk)
            {
                return "lexing OK.";
            }
            else
            {
                return $"lexing failed : {Error}";
            }
        }

        
    }
}