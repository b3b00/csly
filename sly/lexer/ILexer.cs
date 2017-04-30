using System;
using System.Collections.Generic;

namespace lexer
{
    public interface ILexer<T>
    {
        void AddDefinition(TokenDefinition<T> tokenDefinition);
        IEnumerable<Token<T>> Tokenize(string source);
    }
}
