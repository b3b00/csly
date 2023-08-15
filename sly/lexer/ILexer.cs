
using System;
using System.Collections.Generic;
using sly.i18n;
using sly.lexer.fsm;

namespace sly.lexer
{
    public interface ILexer<T> where T : struct
    {
        
        Dictionary<T, List<LexemeLabelAttribute>> Labels { get; set; }
        
        void AddDefinition(TokenDefinition<T> tokenDefinition);
        LexerResult<T> Tokenize(string source);
        
        LexerResult<T> Tokenize(ReadOnlyMemory<char> source);
        
        string I18n { get; set; }
        
        LexerPostProcess<T> LexerPostProcess { get; set; }
    }
}