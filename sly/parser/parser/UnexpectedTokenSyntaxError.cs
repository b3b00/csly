using sly.lexer;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace sly.parser
{
    public class UnexpectedTokenSyntaxError<T> : ParseError, IComparable
    {
        

        public Token<T> UnexpectedToken { get; set; }

        public List<T> ExpectedTokens { get; set; }

        public override int Line {
            get {
                int? l = UnexpectedToken?.Position?.Line;
                return l.HasValue ? l.Value : 1;
            }
        }
        public override int Column
        {
            get {
                int? c = UnexpectedToken?.Position?.Column;
                return c.HasValue ? c.Value : 1;
            }            
        }

        public override string ErrorMessage
        {
            get
            {
                StringBuilder message = new StringBuilder();
                message.Append($"unexpected \"{UnexpectedToken.Value}\" ({UnexpectedToken.TokenID}) at {UnexpectedToken.Position}.");
                if (ExpectedTokens != null && ExpectedTokens.Any())
                {
                   
                     message.Append("expecting ");

                    foreach (T t in ExpectedTokens)
                    {
                        message.Append(t.ToString());
                        message.Append(", ");
                        
                    }
                }
                return message.ToString();
            }
        }

        public UnexpectedTokenSyntaxError(Token<T> unexpectedToken, params T[] expectedTokens) 
        {           
            this.UnexpectedToken = unexpectedToken;
            if (expectedTokens != null)
            {
                this.ExpectedTokens = new List<T>();
                this.ExpectedTokens.AddRange(expectedTokens);
            }
            else
            {
                this.ExpectedTokens = null;
            }

        }

        public override string ToString()
        {
            return ErrorMessage;
        }


    }
}
