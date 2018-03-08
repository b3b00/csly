using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;

namespace sly.lexer
{
    /// <summary>
    /// T is the token type
    /// </summary>
    /// <typeparam name="T">T is the enum Token type</typeparam>
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

            TokenDefinition<T> defEol = tokenDefinitions.ToList<TokenDefinition<T>>().Find(t => t.IsEndOfLine);
            T eol = defEol.TokenID;

            while (currentIndex < source.Length)
            {
                currentColumn = currentIndex - currentLineStartIndex+1;
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
                    throw new LexerException(new LexicalError(currentLine,currentColumn, source[currentIndex]));
                }
                else
                {
                    var value = source.Substring(currentIndex, matchLength);

                    if (matchedDefinition.IsEndOfLine)
                    {
                        currentLineStartIndex = currentIndex+matchLength;
                        currentLine++;
                    }
                    if (!matchedDefinition.IsIgnored)
                    {                      

                        yield return new Token<T>(matchedDefinition.TokenID, value, new TokenPosition(currentIndex, currentLine, currentColumn));
                    }                    
                    currentIndex += matchLength;


                    ;
                }
            }

            

            yield return new Token<T>();
        }
    }
}
