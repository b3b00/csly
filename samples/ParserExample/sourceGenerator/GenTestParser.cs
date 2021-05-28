using System.Collections.Generic;
using sly.lexer;
using sly.parser;
using sly.parser.syntax.tree;

namespace ParserExample.sourceGenerator
{
    public class GenTestParser
    {
        private static SyntaxParseResult<TestLexer> Terminal(IList<Token<TestLexer>> tokens, int position, TestLexer expected, bool discarded)
        {
            var currentToken = tokens[position];
            var result = new SyntaxParseResult<TestLexer>();
            result.IsError = !currentToken.TokenID.Equals(expected);
            result.EndingPosition = !result.IsError ? position + 1 : position;
            var token = tokens[position];
            token.Discarded = discarded;
            result.Root = new SyntaxLeaf<TestLexer>(token,discarded);
            return result;
        }

        private static SyntaxParseResult<TestLexer> Error(int position, params TestLexer[] expected)
        {
            var result = new SyntaxParseResult<TestLexer>();
            result.IsError = true;
            result.Errors = errors;
            result.EndingPosition = currentPosition;
        }
        
        public static SyntaxParseResult<TestLexer> PrimaryInt(IList<Token<TestLexer>> tokens, int position)
        {
            var expected = TestLexer.INT;
            bool discarded = false;

            return Terminal(tokens, position, expected, discarded);
        }
        

        SyntaxParseResult<TestLexer> PrimaryGroup(IList<Token<TestLexer>> tokens, int position)
        {
            SyntaxParseResult<TestLexer> result = null;
            var result1 = Terminal(tokens, position, TestLexer.LPAREN, true);
            if (result1.IsOk)
            {
                
            }
            
        }
    }
}