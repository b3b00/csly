using System.Collections.Generic;
using System.Linq;
using sly.parser;

namespace sly.lexer
{
    public class LexerResult<IN> where IN : struct
    {
        public LexicalError Error => Errors != null && Errors.Any() ? Errors.First() : null;
        public bool IsError { get; set; }

        public bool IsOk => !IsError;
        
        public List<LexicalError> Errors { get; }
        
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
            Errors = new List<LexicalError>() { error };
        }
        
        public LexerResult(List<LexicalError> errors)
        {
            IsError = true;
            Errors = errors;
        }

        
    }
}