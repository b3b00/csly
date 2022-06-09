using System.Collections.Generic;
using sly.lexer;
using sly.parser.generator;

namespace ParserTests
{
    public class RegexLexAndImplicitTokensParser
    {
        [Production("main : item*")]
        public string Main(List<string> items)
        {
            return string.Join(",", items);
        }

        [Production("item : 'test' INT")]
        public string Item(Token<RegexLexAndImplicitTokensLexer> test, Token<RegexLexAndImplicitTokensLexer> integer)
        {
            return $"{test.Value}:{integer.IntValue}";
        }
    }
}