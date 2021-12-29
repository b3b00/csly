using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using sly.i18n;
using sly.lexer;

namespace sly.parser
{
    public class UnexpectedTokenSyntaxError<IN> : ParseError, IComparable where IN : struct
    {
        private string I18n;
        
        public UnexpectedTokenSyntaxError(Token<IN> unexpectedToken, string i18n=null, params IN[] expectedTokens )
        {
            I18n = i18n;
            ErrorType = unexpectedToken.IsEOS ? ErrorType.UnexpectedEOS : ErrorType.UnexpectedToken;
            
            UnexpectedToken = unexpectedToken;
            if (expectedTokens != null)
            {
                ExpectedTokens = new List<IN>();
                ExpectedTokens.AddRange(expectedTokens);
            }
            else
            {
                ExpectedTokens = null;
            }

        }

        public UnexpectedTokenSyntaxError(Token<IN> unexpectedToken, string i18n = null, List<IN> expectedTokens = null) 
        {
            I18n = i18n;
            ErrorType = unexpectedToken.IsEOS ? ErrorType.UnexpectedEOS : ErrorType.UnexpectedToken;
            
            UnexpectedToken = unexpectedToken;
            if (expectedTokens != null)
            {
                ExpectedTokens = new List<IN>();
                ExpectedTokens.AddRange(expectedTokens);
            }
            else
            {
                ExpectedTokens = null;
            }
        }


        public Token<IN> UnexpectedToken { get; set; }

        public List<IN> ExpectedTokens { get; set; }

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
                Message message = Message.UnexpectedToken;
                if (UnexpectedToken.IsEOS)
                {
                    message = Message.UnexpectedEos;
                    if (ExpectedTokens != null && ExpectedTokens.Any())
                    {
                        message = Message.UnexpectedEosExpecting;
                    }
                }
                else
                {
                    message = Message.UnexpectedToken;
                    if (ExpectedTokens != null && ExpectedTokens.Any())
                    {
                        message = Message.UnexpectedTokenExpecting;
                    }
                }
                
                
                var expecting = new StringBuilder();
                
                if (ExpectedTokens != null && ExpectedTokens.Any())
                {
                    

                    foreach (var t in ExpectedTokens)
                    {
                        expecting.Append(t);
                        expecting.Append(", ");
                    }
                }

                string value = UnexpectedToken.ToString();
                
                return I18N.Instance.GetText(I18n,message, value, UnexpectedToken.TokenID.ToString(), expecting.ToString());
            }
        }

        public override string ToString()
        {
            return ErrorMessage;
        }
    }
}