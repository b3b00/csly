using System;
using System.Text.RegularExpressions;

namespace sly.lexer
{
    /// <summary>
    ///     defines a token assiating :
    ///     - a token identifier (which type is T)
    ///     - and a regular expression capturing the token
    ///     a token may be skipped and / or match an end of line
    /// </summary>
    /// <typeparam name="T">T is the enum Token type</typeparam>
    public class TokenDefinition<T>
    {
        

        /// <summary>
        /// </summary>
        /// <param name="token"> the token ID</param>
        /// <param name="regex"> the regular expression for the token</param>
        /// <param name="channel"></param>
        /// <param name="isIgnored">
        ///     true if the token must ignored (i.e the lexer does not return it, used for whitespaces for
        ///     instance)
        /// </param>
        /// <param name="isEndOfLine">true if the token matches an end of line (for line counting)</param>
        public TokenDefinition(T token, string regex, int channel = Channels.Main, bool isIgnored = false, bool isEndOfLine = false)
        {
            TokenID = token;
            Regex = new Regex(regex, RegexOptions.Compiled,TimeSpan.FromMilliseconds(500));
            IsIgnored = isIgnored;
            IsEndOfLine = isEndOfLine;
            Channel = channel;
        }

        public bool IsIgnored { get; }

        public bool IsEndOfLine { get; }

        public Regex Regex { get; }
        public T TokenID { get; }
        
        public int Channel { get; }
    }
}