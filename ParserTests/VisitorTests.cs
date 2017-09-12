using System.Collections.Generic;
using NUnit.Framework;
using sly.lexer;
using sly.parser;
using sly.parser.generator;
using sly.parser.syntax;

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

    [TestFixture]
    public class VisitorTests
    {

        [Production("R : A b c ")]
        public object R(string A, Token<TokenType> b, Token<TokenType> c)
        {
            string result = "R(";
            result += A + ",";
            result += b.Value + ",";
            result += c.Value;
            result += ")";
            return result;
        }

        [Production("A : a ")]
        public object A(Token<TokenType> a)
        {
            string result = "A(";
            result += a.Value;
            result += ")";
            return result;
        }

        public SyntaxLeaf<TokenType> Leaf(TokenType typ, string value, TokenPosition position)
        {
            Token<TokenType> tok = new Token<TokenType>(typ, value, position)
            {
                TokenID = typ,
                Value = value
            };

            return new SyntaxLeaf<TokenType>(tok);
        }

        public SyntaxNode<TokenType> Node(string name, params ISyntaxNode<TokenType>[] leaves)
        {
            List<ISyntaxNode<TokenType>> subNodes = new List<ISyntaxNode<TokenType>>();
            subNodes.AddRange(leaves);
            SyntaxNode<TokenType> n = new SyntaxNode<TokenType>(name, subNodes);
            return n;
        }

        [Test]
        public void TestVisitor()
        {
            VisitorTests visitorInstance = new VisitorTests();
            ParserBuilder builder = new ParserBuilder();
            Parser<TokenType> parser = builder.BuildParser<TokenType>(visitorInstance, ParserType.LL_RECURSIVE_DESCENT, "R");
            SyntaxTreeVisitor<TokenType> visitor = parser.Visitor;

            // build a syntax tree

            TokenPosition position = new TokenPosition(0, 1, 1);
            SyntaxLeaf<TokenType> aT = Leaf(TokenType.a, "a", position);
            SyntaxLeaf<TokenType> bT = Leaf(TokenType.b, "b", position);
            SyntaxLeaf<TokenType> cT = Leaf(TokenType.c, "c", position);

            SyntaxNode<TokenType> aN = Node("A__a_", aT);
            SyntaxNode<TokenType> rN = Node("R__A_b_c", aN, bT, cT);

            string r = visitor.VisitSyntaxTree(rN).ToString();

            Assert.AreEqual("R(A(a),b,c)", r);
        }
    }
}
