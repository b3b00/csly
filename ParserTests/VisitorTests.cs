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

        [Production("R : A b c ")]
        public  object R(string A, Token<TokenType> b, Token<TokenType> c)
        {
            string result = "R(";
            result += A + ",";
            result += b.Value+",";
            result += c.Value;
            result += ")";
            return result;
        }

        [Production("A : a ")]
        public  object A(Token<TokenType> a)
        {
            string result = "A(";            
            result += a.Value;            
            result += ")";
            return result;
        }

        public SyntaxLeaf<TokenType> leaf(TokenType typ, string value,TokenPosition position)
        {
            Token<TokenType> tok = new Token<TokenType>(typ, value, position);
            tok.TokenID = typ;
            tok.Value = value;
            return new SyntaxLeaf<TokenType>(tok);
        }

        public SyntaxNode<TokenType> node(string name, params ISyntaxNode<TokenType>[] leaves)
        {
            List<ISyntaxNode<TokenType>> subNodes = new List<ISyntaxNode<TokenType>>();
            subNodes.AddRange(leaves);
            SyntaxNode<TokenType> n = new SyntaxNode<TokenType>(name, subNodes);
            return n;
        }

        [Fact]
        public void testVisitor()
        {
            VisitorTests visitorInstance = new VisitorTests();
            ParserBuilder builder = new ParserBuilder();
            Parser<TokenType,string> parser = builder.BuildParser<TokenType,string>(visitorInstance, ParserType.LL_RECURSIVE_DESCENT, "R");
            SyntaxTreeVisitor<TokenType,string> visitor = parser.Visitor;

            // build a syntax tree

            TokenPosition position = new TokenPosition(0, 1, 1);
            SyntaxLeaf<TokenType> aT = leaf(TokenType.a, "a", position);
            SyntaxLeaf<TokenType> bT = leaf(TokenType.b, "b", position);
            SyntaxLeaf<TokenType> cT = leaf(TokenType.c, "c", position);

            SyntaxNode<TokenType> aN = node("A__a_", aT);
            SyntaxNode<TokenType> rN = node("R__A_b_c", aN,bT,cT);

            string r = visitor.VisitSyntaxTree(rN).ToString();
            ;
            Assert.Equal("R(A(a),b,c)", r);            


        }
    }
}
