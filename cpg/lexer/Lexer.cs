using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace lexer
{
    /// <summary>
    /// T is the token type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Lexer<T> : ILexer<T>
    {
        
        IList<TokenDefinition<T>> tokenDefinitions = new List<TokenDefinition<T>>();

        public void AddDefinition(TokenDefinition<T> tokenDefinition)
        {
            tokenDefinitions.Add(tokenDefinition);
        }

        

        public IEnumerable<Token<T>> Tokenize(string source)
        {
            int currentIndex = 0;
            List<Token<T>> tokens = new List<Token<T>>();
            int currentLine = 1;
            int currentColumn = 0;
            int currentLineStartIndex = 0;

            while (currentIndex < source.Length)
            {
                currentColumn = currentIndex - currentLineStartIndex;
                TokenDefinition<T> matchedDefinition = null;
                int matchLength = 0;

                foreach (var rule in tokenDefinitions)
                {
                    var match = rule.Regex.Match(source, currentIndex);
                    
                    if (match.Success && (match.Index - currentIndex) == 0)
                    {
                        matchedDefinition = rule;
                        matchLength = match.Length;
                        break;
                    }
                }

                if (matchedDefinition == null)
                {
                    throw new Exception(string.Format("Unrecognized symbol '{0}' at index {1} (line {2}, column {3}).", source[currentIndex], currentIndex, currentLine, currentColumn));
                }
                else
                {
                    var value = source.Substring(currentIndex, matchLength);

                    if (!matchedDefinition.IsIgnored)
                        
                        yield return new Token<T>(matchedDefinition.TokenID, value, new TokenPosition(currentIndex-matchLength, currentLine, currentColumn));
                        
                    if (matchedDefinition.IsEndOfLine)
                    {                        
                        currentLineStartIndex = currentIndex;
                        currentLine++;
                    }
                    currentIndex += matchLength;


                    ;
                }
            }

            

            yield return new Token<T>();
        }
    }
}
