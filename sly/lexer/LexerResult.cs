using System.Collections.Generic;
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
            Tokens = new TokenChannels<IN>(tokens);
        }
        
        public LexerResult(TokenChannels<IN> tokens)
        {
            IsError = false;
            Tokens = tokens;
        }

        public LexerResult(LexicalError error)
        {
            IsError = true;
            Error = error;
        }

        
    }
}