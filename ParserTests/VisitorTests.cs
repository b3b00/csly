using Xunit;
using System.Collections.Generic;
using sly.parser.generator;
using sly.parser;
using sly.parser.syntax;
using sly.lexer;

namespace ParserTests
{
    public enum TokenType
    {
        a = 1,
        b = 2,
        c = 3,
        WS = 100,
        EOL = 101
    }


   
    public class VisitorTests
    {

        [Reduction("R : A b c ")]
        public  object R(string A, Token<TokenType> b, Token<TokenType> c)
        {
            string result = "R(";
            result += A + ",";
            result += b.Value+",";
            result += c.Value;
            result += ")";
            return result;
        }

        [Reduction("A : a ")]
        public  object A(Token<TokenType> a)
        {
            string result = "A(";            
            result += a.Value;            
            result += ")";
            return result;
        }

        public ConcreteSyntaxLeaf<TokenType> leaf(TokenType typ, string value,TokenPosition position)
        {
            Token<TokenType> tok = new Token<TokenType>(typ, value, position);
            tok.TokenID = typ;
            tok.Value = value;
            return new ConcreteSyntaxLeaf<TokenType>(tok);
        }

        public ConcreteSyntaxNode<TokenType> node(string name, params IConcreteSyntaxNode<TokenType>[] leaves)
        {
            List<IConcreteSyntaxNode<TokenType>> subNodes = new List<IConcreteSyntaxNode<TokenType>>();
            subNodes.AddRange(leaves);
            ConcreteSyntaxNode<TokenType> n = new ConcreteSyntaxNode<TokenType>(name, subNodes);
            return n;
        }

        [Fact]
        public void testVisitor()
        {
            VisitorTests visitorInstance = new VisitorTests();
            Parser<TokenType> parser = ParserBuilder.BuildParser<TokenType>(visitorInstance, ParserType.LL_RECURSIVE_DESCENT, "R");
            ConcreteSyntaxTreeVisitor<TokenType> visitor = parser.Visitor;

            // build a syntax tree

            TokenPosition position = new TokenPosition(0, 1, 1);
            ConcreteSyntaxLeaf<TokenType> aT = leaf(TokenType.a, "a", position);
            ConcreteSyntaxLeaf<TokenType> bT = leaf(TokenType.b, "b", position);
            ConcreteSyntaxLeaf<TokenType> cT = leaf(TokenType.c, "c", position);

            ConcreteSyntaxNode<TokenType> aN = node("A__a_", aT);
            ConcreteSyntaxNode<TokenType> rN = node("R__A_b_c_", aN,bT,cT);

            string r = visitor.VisitSyntaxTree(rN).ToString();
            ;
            Assert.Equal("R(A(a),b,c)", r);            


        }
    }
}
