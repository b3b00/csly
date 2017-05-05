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
    /// <typeparam name="T"></typeparam>
    public class Lexer<T> : ILexer<T>
    {

        Regex GlobalRegex = null;
        
        IList<TokenDefinition<T>> tokenDefinitions = new List<TokenDefinition<T>>();

        public void AddDefinition(TokenDefinition<T> tokenDefinition)
        {
            tokenDefinitions.Add(tokenDefinition);
            
        }


        public void InitGlobalRegex()
        {
            StringBuilder globReg = new StringBuilder();

            for (int i = 0; i < tokenDefinitions.Count; i++)
            {
                string unitRegex = tokenDefinitions[i].Regex.ToString();
                globReg.Append($"(?<{tokenDefinitions[i].TokenID}>{unitRegex})");
                if (i < tokenDefinitions.Count -1)
                {
                    globReg.Append("|");
                }
            }

            GlobalRegex = new Regex(globReg.ToString());
        }
        

        public IEnumerable<Token<T>> Tokenize(string source)
        {
            if (GlobalRegex == null)
            {
                InitGlobalRegex();
            }

            int currentIndex = 0;
            List<Token<T>> tokens = new List<Token<T>>();
            int currentLine = 1;
            int currentColumn = 0;
            int currentLineStartIndex = 0;

            TokenDefinition<T> defEol = tokenDefinitions.ToList<TokenDefinition<T>>().Find(t => t.IsEndOfLine);
            T eol = defEol.TokenID;

            while (currentIndex < source.Length)
            {
                currentColumn = currentIndex - currentLineStartIndex;
                TokenDefinition<T> matchedDefinition = null;
                int matchLength = 0;

                T globTok =eol ;
                var globMatch = GlobalRegex.Match(source,currentIndex);
                bool globalFound = globMatch.Success;

                if (globalFound)
                {
                    int index = -1;
                    int i = 1;
                    while (i < globMatch.Groups.Count && index < 0)
                    {
                        if (!string.IsNullOrEmpty(globMatch.Groups[i].Value))
                        {
                            index = i;
                        }
                        i++;
                    }
                    string tokenName = GlobalRegex.GroupNameFromNumber(index);
                    globTok = (T)Enum.Parse(typeof(T), tokenName, false);
                }
                else
                {
                    ;
                }
                



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
                    if (globalFound)
                    {
                        // dommage
                        ;
                    }
                    throw new LexerException<T>(new LexicalError(currentLine,currentColumn, source[currentIndex]));
                }
                else
                {
                    var value = source.Substring(currentIndex, matchLength);

                    if (!matchedDefinition.IsIgnored)
                    {
                       
                        if (!matchedDefinition.TokenID.Equals(globTok))
                        {
                            // dommage
                            ;
                        }                       
                        yield return new Token<T>(matchedDefinition.TokenID, value, new TokenPosition(currentIndex - matchLength, currentLine, currentColumn));
                    }
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
