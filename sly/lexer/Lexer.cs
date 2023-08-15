using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using sly.i18n;
using sly.lexer.fsm;

namespace sly.lexer
{
    /// <summary>
    ///     T is the token type
    /// </summary>
    /// <typeparam name="T">T is the enum Token type</typeparam>
    public class Lexer<T> : ILexer<T> where T : struct
    {
        public Dictionary<T, Dictionary<string, string>> LexemeLabels { get; set; }
        public string I18n { get; set; }
        public LexerPostProcess<T> LexerPostProcess { get; set; }

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
                    var error = new LexicalError(currentLine, currentColumn, source[currentIndex], I18n);
                    var result = new LexerResult<T>(error,tokens);
                    return result;
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
                        new LexerPosition(currentIndex, currentLine, currentColumn));
                    previousToken.Channel = matchedDefinition.Channel;
                    tokens.Add(previousToken);
                }

                currentIndex += matchLength;
            }

            var eos = new Token<T>();
            eos.Channel = Channels.Main;
            if (previousToken != null)
            {
                eos.Position = new LexerPosition(previousToken.Position.Index + 1, previousToken.Position.Line,
                    previousToken.Position.Column + previousToken.Value.Length);
            }
            else
            {
                eos.Position = new LexerPosition(0,0,0);
            }

            tokens.Add(eos);
            
            return new LexerResult<T>(tokens);
        }

        [ExcludeFromCodeCoverage]
        public LexerResult<T> Tokenize(ReadOnlyMemory<char> source)
        {
            return Tokenize(source.ToString());
        }
    }
}