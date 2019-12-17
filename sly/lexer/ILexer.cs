
using System;

namespace sly.lexer
{
    public interface ILexer<T> where T : struct
    {
        void AddDefinition(TokenDefinition<T> tokenDefinition);
        LexerResult<T> Tokenize(string source);
        
        LexerResult<T> Tokenize(ReadOnlyMemory<char> source);
        
        void ResetLexer();
    }
}