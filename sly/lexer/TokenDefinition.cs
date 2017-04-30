using System;
using System.Text.RegularExpressions;

namespace lexer
{
    public class TokenDefinition<T>
    {
        
        public TokenDefinition(T token, string regex)
            : this(token, regex, false,false)
        {
        }

        public TokenDefinition(T token, string regex,bool isIgnored= false,bool isEndOfLine = false)
        {
            TokenID = token;
            Regex = new Regex(regex);
            IsIgnored = isIgnored;
            IsEndOfLine = isEndOfLine;
        }

        public bool IsIgnored { get; private set; }

        public bool IsEndOfLine { get; private set; }

        public Regex Regex { get; private set; }
        public T TokenID { get; private set; }
    }
}
