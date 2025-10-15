
using System;
using System.Collections.Generic;
using sly.lexer.fsm;

namespace sly.lexer
{
    public interface ILexer<T> where T : struct, Enum
    {
        
        Dictionary<T, Dictionary<string, string>> LexemeLabels { get; set; }
        
        void AddDefinition(TokenDefinition<T> tokenDefinition);
        LexerResult<T> Tokenize(string source, bool applyPostProcess = false);
        
        LexerResult<T> Tokenize(ReadOnlyMemory<char> source, bool applyPostProcess = false);
        
        string I18n { get; set; }
        
        LexerPostProcess<T> LexerPostProcess { get; set; }
    }
}