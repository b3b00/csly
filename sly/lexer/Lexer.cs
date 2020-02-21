using System;
using System.Collections.Generic;

namespace sly.lexer
{
    /// <summary>
    ///     T is the token type
    /// </summary>
    /// <typeparam name="T">T is the enum Token type</typeparam>
    public class Lexer<T> : ILexer<T> where T : struct
    {
        private readonly IList<TokenDefinition<T>> tokenDefinitions = new List<TokenDefinition<T>>();

        public void AddDefinition(TokenDefinition<T> tokenDefinition)
        {
            tokenDefinitions.Add(tokenDefinition);
        }


        public LexerResult<T> Tokenize(string source)
        {
            List<Token<T>> tokens = new List<Token<T>>();
            
            var currentIndex = 0;
            //List<Token<T>> tokens = new List<Token<T>>();
            var currentLine = 1;
            var currentColumn = 0;
            var currentLineStartIndex = 0;
            Token<T> previousToken = null;

            while (currentIndex < source.Length)
            {
                currentColumn = currentIndex - currentLineStartIndex + 1;
                TokenDefinition<T> matchedDefinition = null;
                var matchLength = 0;

                foreach (var rule in tokenDefinitions)
                {
                    var match = rule.Regex.Match(source.Substring(currentIndex));

                    if (match.Success && match.Index == 0)
                    {
                        matchedDefinition = rule;
                        matchLength = match.Length;
                        break;
                    }
                }

                if (matchedDefinition == null)
                {
                    return new LexerResult<T>(new LexicalError(currentLine, currentColumn, source[currentIndex]));
                }

                var value = source.Substring(currentIndex, matchLength);

                if (matchedDefinition.IsEndOfLine)
                {
                    currentLineStartIndex = currentIndex + matchLength;
                    currentLine++;
                }

                if (!matchedDefinition.IsIgnored)
                {
                    previousToken = new Token<T>(matchedDefinition.TokenID, value,
                        new TokenPosition(currentIndex, currentLine, currentColumn));
                    tokens.Add(previousToken);
                }

                currentIndex += matchLength;
            }

            var eos = new Token<T>();
            eos.Position = new TokenPosition(previousToken.Position.Index + 1, previousToken.Position.Line,
                previousToken.Position.Column + previousToken.Value.Length);


            tokens.Add(eos);
            return new LexerResult<T>(tokens);
        }

        public LexerResult<T> Tokenize(ReadOnlyMemory<char> source)
        {
            throw new NotImplementedException();
        }
        
        public void ResetLexer()
        {
            
        }
    }
}