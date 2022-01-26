
using System;
using sly.lexer.fsm;

namespace sly.lexer
{
    public interface ILexer<T> where T : struct
    {
        void AddDefinition(TokenDefinition<T> tokenDefinition);
        LexerResult<T> Tokenize(string source);
        
        LexerResult<T> Tokenize(ReadOnlyMemory<char> source);
        
        string I18n { get; set; }
        
        LexerPostProcess<T> LexerPostProcess { get; set; }
    }
}