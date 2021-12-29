
using System;

namespace sly.lexer
{
    public interface ILexer<IN> where IN : struct
    {
        void AddDefinition(TokenDefinition<IN> tokenDefinition);
        LexerResult<IN> Tokenize(string source);
        
        LexerResult<IN> Tokenize(ReadOnlyMemory<char> source);
        
        string I18n { get; set; }
    }
}