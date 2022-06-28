using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using sly.i18n;
using sly.lexer;
using sly.parser.syntax.grammar;

namespace sly.parser
{
    public class UnexpectedTokenSyntaxError<T> : ParseError, IComparable where T : struct
    {
        private string I18n;
        
        public UnexpectedTokenSyntaxError(Token<T> unexpectedToken, string i18n=null, params LeadingToken<T>[] expectedTokens )
        {
            I18n = i18n;
            ErrorType = unexpectedToken.IsEOS ? ErrorType.UnexpectedEOS : ErrorType.UnexpectedToken;
            
            UnexpectedToken = unexpectedToken;
            if (expectedTokens != null)
            {
                ExpectedTokens = new List<LeadingToken<T>>();
                ExpectedTokens.AddRange(expectedTokens);
            }
            else
            {
                ExpectedTokens = null;
            }

        }

        public UnexpectedTokenSyntaxError(Token<T> unexpectedToken, string i18n = null, List<LeadingToken<T>> expectedTokens = null) 
        {
            I18n = i18n;
            ErrorType = unexpectedToken.IsEOS ? ErrorType.UnexpectedEOS : ErrorType.UnexpectedToken;
            
            UnexpectedToken = unexpectedToken;
            if (expectedTokens != null)
            {
                ExpectedTokens = new List<LeadingToken<T>>();
                ExpectedTokens.AddRange(expectedTokens);
            }
            else
            {
                ExpectedTokens = null;
            }
        }


        public Token<T> UnexpectedToken { get; set; }

        public List<LeadingToken<T>> ExpectedTokens { get; set; }

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

        [ExcludeFromCodeCoverage]
        public override string ErrorMessage
        {
            get
            {
                I18NMessage i18NMessage = I18NMessage.UnexpectedToken;
                if (UnexpectedToken.IsEOS)
                {
                    i18NMessage = I18NMessage.UnexpectedEos;
                    if (ExpectedTokens != null && ExpectedTokens.Any())
                    {
                        i18NMessage = I18NMessage.UnexpectedEosExpecting;
                    }
                }
                else
                {
                    i18NMessage = I18NMessage.UnexpectedToken;
                    if (ExpectedTokens != null && ExpectedTokens.Any())
                    {
                        i18NMessage = I18NMessage.UnexpectedTokenExpecting;
                    }
                }
                
                
                var expecting = new StringBuilder();
                
                if (ExpectedTokens != null && ExpectedTokens.Any())
                {
                    foreach (var t in ExpectedTokens)
                    {
                        expecting.Append(t.ToString());
                        expecting.Append(", ");
                    }
                }

                string value = UnexpectedToken.ToString();
                
                return I18N.Instance.GetText(I18n,i18NMessage, value, UnexpectedToken.TokenID.ToString(), expecting.ToString());
            }
        }

        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return ErrorMessage;
        }
    }
}