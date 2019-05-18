using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using sly.lexer;

namespace sly.parser
{
    public class UnexpectedTokenSyntaxError<T> : ParseError, IComparable
    {
        public UnexpectedTokenSyntaxError(Token<T> unexpectedToken, params T[] expectedTokens)
        {
            UnexpectedToken = unexpectedToken;
            if (expectedTokens != null)
            {
                ExpectedTokens = new List<T>();
                ExpectedTokens.AddRange(expectedTokens);
            }
            else
            {
                ExpectedTokens = null;
            }
        }


        public Token<T> UnexpectedToken { get; set; }

        public List<T> ExpectedTokens { get; set; }

        public override int Line
        {
            get
            {
                var l = UnexpectedToken?.Position?.Line;
                return l.HasValue ? l.Value : 1;
            }
        }

        public override int Column
        {
            get
            {
                var c = UnexpectedToken?.Position?.Column;
                return c.HasValue ? c.Value : 1;
            }
        }

        public override string ErrorMessage
        {
            get
            {
                var message = new StringBuilder();
                if (!UnexpectedToken.IsEOS)
                    message.Append($"unexpected \"{UnexpectedToken.Value}\" ({UnexpectedToken.TokenID}) ");
                else
                    message.Append("unexpected end of stream. ");
                message.Append($"at {UnexpectedToken.Position}.");
                if (ExpectedTokens != null && ExpectedTokens.Any())
                {
                    message.Append("expecting ");

                    foreach (var t in ExpectedTokens)
                    {
                        message.Append(t);
                        message.Append(", ");
                    }
                }

                return message.ToString();
            }
        }

        public override string ToString()
        {
            return ErrorMessage;
        }
    }
}