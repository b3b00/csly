using System;
using sly.lexer;
using sly.parser.generator;

namespace ParserTests.lexer
{
    public class ParserUsingLexerExtensions
    {
        [Production("root : value")]
        public object root(object value)
        {
            return value;
        }

        [Production("value : DATE")]
        public object dateValue(Token<Extensions> token)
        {
            string[] elements = token.Value.Split(new char[] {'.'});
            int day = 0;
            int month = 0;
            int year = 0;
            bool ok = int.TryParse(elements[0], out day) && int.TryParse(elements[1], out month) &&
                      int.TryParse(elements[2], out year);
            if (ok)
            {
                return new DateTime(year, month, day);
            }
            return new DateTime(1789,7,14);
        }

        [Production("value : DOUBLE")]
        public object doubleValue(Token<Extensions> token)
        {
            return token.DoubleValue;
        }
    }
}