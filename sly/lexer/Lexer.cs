using System.Collections.Generic;
using System.Linq;

namespace sly.lexer
{
    /// <summary>
    ///     T is the token type
    /// </summary>
    /// <typeparam name="T">T is the enum Token type</typeparam>
    public class Lexer<T> : ILexer<T>
    {
        private readonly IList<TokenDefinition<T>> tokenDefinitions = new List<TokenDefinition<T>>();

        public void AddDefinition(TokenDefinition<T> tokenDefinition)
        {
            tokenDefinitions.Add(tokenDefinition);
        }


        public IEnumerable<Token<T>> Tokenize(string source)
        {
            var currentIndex = 0;
            //List<Token<T>> tokens = new List<Token<T>>();
            var currentLine = 1;
            var currentColumn = 0;
            var currentLineStartIndex = 0;
            Token<T> previousToken = null;

            var defEol = tokenDefinitions.ToList().Find(t => t.IsEndOfLine);
            var eol = defEol.TokenID;

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
                    throw new LexerException(new LexicalError(currentLine, currentColumn, source[currentIndex]));
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
                    yield return previousToken;
                }

                currentIndex += matchLength;


                ;
            }

            var eos = new Token<T>();
            eos.Position = new TokenPosition(previousToken.Position.Index + 1, previousToken.Position.Line,
                previousToken.Position.Column + previousToken.Value.Length);


            yield return eos;
        }
    }
}