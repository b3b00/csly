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
                return UnexpectedToken.Position.Line;
            }
        }
        public override int Column
        {
            get {
                return UnexpectedToken.Position.Column;
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

        
    }
}
