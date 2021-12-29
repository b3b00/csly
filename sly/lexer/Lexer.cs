using System;
using System.Collections.Generic;

namespace sly.lexer
{
    /// <summary>
    ///     T is the token type
    /// </summary>
    /// <typeparam name="IN">T is the enum Token type</typeparam>
    public class Lexer<IN> : ILexer<IN> where IN : struct
    {
        public string I18n { get; set; }
        
        private readonly IList<TokenDefinition<IN>> tokenDefinitions = new List<TokenDefinition<IN>>();

        public void AddDefinition(TokenDefinition<IN> tokenDefinition)
        {
            tokenDefinitions.Add(tokenDefinition);
        }


        public LexerResult<IN> Tokenize(string source)
        {
            List<Token<IN>> tokens = new List<Token<IN>>();
            
            var currentIndex = 0;
            var currentLine = 1;
            var currentColumn = 0;
            var currentLineStartIndex = 0;
            Token<IN> previousToken = null;

            while (currentIndex < source.Length)
            {
                currentColumn = currentIndex - currentLineStartIndex + 1;
                TokenDefinition<IN> matchedDefinition = null;
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
                    return new LexerResult<IN>(new LexicalError(currentLine, currentColumn, source[currentIndex],I18n));
                }

                var value = source.Substring(currentIndex, matchLength);

                if (matchedDefinition.IsEndOfLine)
                {
                    currentLineStartIndex = currentIndex + matchLength;
                    currentLine++;
                }

                if (!matchedDefinition.IsIgnored)
                {
                    previousToken = new Token<IN>(matchedDefinition.TokenID, value,
                        new LexerPosition(currentIndex, currentLine, currentColumn));
                    tokens.Add(previousToken);
                }

                currentIndex += matchLength;
            }

            var eos = new Token<IN>();
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
            return new LexerResult<IN>(tokens);
        }

        public LexerResult<IN> Tokenize(ReadOnlyMemory<char> source)
        {
            return new LexerResult<IN>(new LexicalError(0, 0, '.',I18n));
        }
    }
}