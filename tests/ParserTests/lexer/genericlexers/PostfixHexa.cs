using System.Collections.Generic;
using sly.lexer;
using sly.parser.generator;

namespace ParserTests.lexer.genericlexers;


    public enum PostfixHexa
    {
        EOS,

        [Extension] Hexa,

        [Int] Int,

        [AlphaId] Id
    }

    public class Parser
    {
        [Production("main : item+ ")]
        public string Main(List<string> items)
        {
            return string.Join("\n", items);
        }

        [Production("item : [Id | Hexa | Int]" )]
        public string Item(Token<PostfixHexa> token)
        {
            return token.ToString();
        }
	
    }
