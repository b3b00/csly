using System.Linq;
using Xunit;
using sly.parser;
using sly.lexer;
using sly.parser.generator;
using System.Collections.Generic;
using expressionparser;
    

namespace ParserTests
{
    
    public class EBNFTests
    {

        public enum TokenType
        {
            a = 1,
            b = 2,
            c = 3,
            WS = 100,
            EOL = 101
        }

        [Reduction("R : A B c ")]
        public object R(string A, Token<TokenType> b, Token<TokenType> c)
        {
            string result = "R(";
            result += A + ",";
            result += b.Value + ",";
            result += c.Value;
            result += ")";
            return result;
        }

        [Reduction("A : a + ")]
        public object A(List<Token<TokenType>> astr)
        {
            string result = "A(";
            result +=(string) astr
                .Select(a => a.Value)
                .Aggregate<string>((a1, a2) => a1 + ", " + a2);
            result += ")";
            return result;
        }

        [Reduction("B : b * ")]
        public object B(List<Token<TokenType>> bstr)
        {
            if (bstr.Any())
            {
                string result = "B(";
                result += bstr
                    .Select(b => b.Value)
                    .Aggregate<string>((b1, b2) => b1 + ", " + b2);
                result += ")";
                return result;
            }
            return "B()";
        }

        private Parser<TokenType> Parser;
      
        public EBNFTests()
        {
            
        }


        [Fact]
        public void TestParseBuild()
        {
            EBNFTests parserInstance = new EBNFTests();
            EBNFParserBuilder<TokenType> builder = new EBNFParserBuilder<TokenType>();

            EBNFTests parserTest = new EBNFTests();

            Parser = builder.BuildParser(parserTest, ParserType.LL_RECURSIVE_DESCENT, "R");
        }

      

        

      
    }
}
