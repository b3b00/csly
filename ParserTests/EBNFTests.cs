using System.Linq;
using Xunit;
using System.Text;
using sly.parser;
using sly.lexer;
using sly.parser.generator;
using System.Collections.Generic;

using sly.parser.llparser;
using sly.parser.syntax;
using jsonparser;
using jsonparser.JsonModel;
using sly.buildresult;
using sly.parser.parser;

namespace ParserTests
{

    public enum OptionTestToken
    {

        [Lexeme("a")]
        a = 1,
        [Lexeme("b")]
        b = 2,
        [Lexeme("c")]
        c = 3,
        [Lexeme("e")]
        e = 4,
        [Lexeme("f")]
        f = 5,

        [Lexeme("[ \\t]+", true)]
        WS = 100,
        [Lexeme("\\n\\r]+", true, true)]
        EOF = 101
    }

    public class OptionTestParser{

        [Production("root2 : a B? c ")]
        public string root2(Token<OptionTestToken> a, ValueOption<string> b, Token<OptionTestToken> c)
        {
            StringBuilder r = new StringBuilder();
            r.Append($"R(");
            r.Append(a.Value);
            r.Append(b.Match(v => $",{v}", () => ",<none>"));
            r.Append($",{c.Value}");
            r.Append($")");
            return r.ToString();
        }

         [Production("root : a B c? ")]
         public string root(Token<OptionTestToken> a, string b, Token<OptionTestToken> c) {
             string r = $"R({a.StringWithoutQuotes},{b}";
             if (c.IsEmpty) {
                   r =  $"{r},<none>)";
             }  
             else {
                    r =  $"{r},{c.Value})";
             }  
             return r;
         }

        [Production("root : a b? c ")]
        public string root2(Token<OptionTestToken> a, Token<OptionTestToken> b, Token<OptionTestToken> c)
        {
            StringBuilder result = new StringBuilder();
            result.Append("R(");
            result.Append(a.StringWithoutQuotes);
            result.Append(",");
            if (b.IsEmpty)
            {
                result.Append("<none>");
            }
            else
            {
                result.Append(b.StringWithoutQuotes);
            }
            result.Append(",");
            result.Append(c.StringWithoutQuotes);
            result.Append(")");

            return result.ToString();
        }

        [Production("B : b ")]
         public string bee(Token<OptionTestToken> b) {
             return $"B({b.Value})";
         }
    }

    public class EBNFTests
    {

        public enum TokenType
        {
            [Lexeme("a")]
            a = 1,
            [Lexeme("b")]
            b = 2,
            [Lexeme("c")]
            c = 3,
            [Lexeme("e")]
            e = 4,
            [Lexeme("f")]
            f = 5,
            [Lexeme("[ \\t]+", true)]
            WS = 100,
            [Lexeme("\\n\\r]+", true, true)]
            EOL = 101
        }





        [Production("R : A B c ")]
        public string R(string A, string B, Token<TokenType> c)
        {
            string result = "R(";
            result += A + ",";
            result += B + ",";
            result += c.Value;
            result += ")";
            return result;
        }

        [Production("R : G+ ")]
        public string RManyNT(List<string> gs)
        {
            string result = "R(";
            result += gs
                .Select(g => g.ToString())
                .Aggregate((string s1, string s2) => s1 + "," + s2);
            result += ")";
            return result;
        }

        [Production("G : e f ")]
        public string RManyNT(Token<TokenType> e, Token<TokenType> f)
        {
            string result = $"G({e.Value},{f.Value})";
            return result;
        }

        [Production("A : a + ")]
        public string A(List<Token<TokenType>> astr)
        {
            string result = "A(";
            result += (string)astr
                .Select(a => a.Value)
                .Aggregate<string>((a1, a2) => a1 + ", " + a2);
            result += ")";
            return result;
        }

        [Production("B : b * ")]
        public string B(List<Token<TokenType>> bstr)
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



        private Parser<TokenType, string> Parser;


        public EBNFTests()
        {

        }

        private BuildResult<Parser<TokenType, string>> BuildParser()
        {
            EBNFTests parserInstance = new EBNFTests();
            ParserBuilder<TokenType, string> builder = new ParserBuilder<TokenType, string>();
            var result = builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, "R");
            return result;
        }


        private BuildResult<Parser<JsonToken, JSon>> BuildEbnfJsonParser()
        {
            EbnfJsonParser parserInstance = new EbnfJsonParser();
            ParserBuilder<JsonToken, JSon> builder = new ParserBuilder<JsonToken, JSon>();

            var result =
                builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, "root");
            return result;
        }

        private BuildResult<Parser<OptionTestToken, string>> BuildOptionParser()
        {
            OptionTestParser parserInstance = new OptionTestParser();
            ParserBuilder<OptionTestToken, string> builder = new ParserBuilder<OptionTestToken, string>();

            var result =
                builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, "root");
            return result;
        }

        [Fact]
        public void TestParseBuild()
        {
            var buildResult = BuildParser();
            Assert.False(buildResult.IsError);
            Parser = buildResult.Result;
            Assert.Equal(typeof(EBNFRecursiveDescentSyntaxParser<TokenType, string>), Parser.SyntaxParser.GetType());
            Assert.Equal(4, Parser.Configuration.NonTerminals.Count);
            NonTerminal<TokenType> nt = Parser.Configuration.NonTerminals["R"];
            Assert.Equal(2, nt.Rules.Count);
            nt = Parser.Configuration.NonTerminals["A"];
            Assert.Single(nt.Rules);
            Rule<TokenType> rule = nt.Rules[0];
            Assert.Single(rule.Clauses);
            Assert.IsType<OneOrMoreClause<TokenType>>(rule.Clauses[0]);
            nt = Parser.Configuration.NonTerminals["B"];
            Assert.Single(nt.Rules);
            rule = nt.Rules[0];
            Assert.Single(rule.Clauses);
            Assert.IsType<ZeroOrMoreClause<TokenType>>(rule.Clauses[0]);
            ;
        }


        [Fact]
        public void TestOneOrMoreNonTerminal()
        {
            var buildResult = BuildParser();
            Assert.False(buildResult.IsError);
            Parser = buildResult.Result;
            ParseResult<TokenType, string> result = Parser.Parse("e f e f");
            Assert.False(result.IsError);
            Assert.Equal("R(G(e,f),G(e,f))", result.Result.ToString().Replace(" ", ""));
        }


        [Fact]
        public void TestOneOrMoreWithMany()
        {
            var buildResult = BuildParser();
            Assert.False(buildResult.IsError);
            Parser = buildResult.Result;
            ParseResult<TokenType, string> result = Parser.Parse("aaa b c");
            Assert.False(result.IsError);
            Assert.Equal("R(A(a,a,a),B(b),c)", result.Result.ToString().Replace(" ", ""));
        }

        [Fact]
        public void TestOneOrMoreWithOne()
        {
            var buildResult = BuildParser();
            Assert.False(buildResult.IsError);
            Parser = buildResult.Result;
            ParseResult<TokenType, string> result = Parser.Parse(" b c");
            Assert.True(result.IsError);
        }

        [Fact]
        public void TestZeroOrMoreWithOne()
        {
            var buildResult = BuildParser();
            Assert.False(buildResult.IsError);
            Parser = buildResult.Result;
            ParseResult<TokenType, string> result = Parser.Parse("a b c");
            Assert.False(result.IsError);
            Assert.Equal("R(A(a),B(b),c)", result.Result.ToString().Replace(" ", ""));
        }

        [Fact]
        public void TestZeroOrMoreWithMany()
        {
            var buildResult = BuildParser();
            Assert.False(buildResult.IsError);
            Parser = buildResult.Result;
            ParseResult<TokenType, string> result = Parser.Parse("a bb c");
            Assert.False(result.IsError);
            Assert.Equal("R(A(a),B(b,b),c)", result.Result.ToString().Replace(" ", ""));
        }

        [Fact]
        public void TestZeroOrMoreWithNone()
        {
            var buildResult = BuildParser();
            Assert.False(buildResult.IsError);
            Parser = buildResult.Result;
            ParseResult<TokenType, string> result = Parser.Parse("a  c");
            Assert.False(result.IsError);
            Assert.Equal("R(A(a),B(),c)", result.Result.ToString().Replace(" ", ""));
        }


        [Fact]
        public void TestJsonList()
        {
            var buildResult = BuildEbnfJsonParser();
            Assert.False(buildResult.IsError);
            Parser<JsonToken, JSon> jsonParser = buildResult.Result;

            ParseResult<JsonToken, JSon> result = jsonParser.Parse("[1,2,3,4]");
            Assert.False(result.IsError);
            Assert.True(result.Result.IsList);
            JList list = (JList)result.Result;
            Assert.Equal(4, list.Count);
            AssertInt(list, 0, 1);
            AssertInt(list, 1, 2);
            AssertInt(list, 2, 3);
            AssertInt(list, 3, 4);
        }

        [Fact]
        public void TestJsonObject()
        {
            var buildResult = BuildEbnfJsonParser();
            Assert.False(buildResult.IsError);
            Parser<JsonToken, JSon> jsonParser = buildResult.Result;
            ParseResult<JsonToken, JSon> result = jsonParser.Parse("{\"one\":1,\"two\":2,\"three\":\"trois\" }");
            Assert.False(result.IsError);
            Assert.True(result.Result.IsObject);
            JObject o = (JObject)result.Result;
            Assert.Equal(3, o.Count);
            AssertInt(o, "one", 1);
            AssertInt(o, "two", 2);
            AssertString(o, "three", "trois");
        }


        [Fact]
        public void TestNonEmptyTerminalOption() {
            var buildResult = BuildOptionParser();
            Assert.False(buildResult.IsError);
            var optionParser = buildResult.Result;

            var result = optionParser.Parse("a b c","root");
            Assert.Equal("R(a,B(b),c)", result.Result);
        }

        
        [Fact]
        public void TestEmptyTerminalOption() {
            var buildResult = BuildOptionParser();
            Assert.False(buildResult.IsError);
            var optionParser = buildResult.Result;

            var result = optionParser.Parse("a b","root");
            Assert.Equal("R(a,B(b),<none>)", result.Result);
        }

        [Fact]
        public void TestEmptyOptionTerminalInMiddle()
        {
            var buildResult = BuildOptionParser();
            Assert.False(buildResult.IsError);
            var optionParser = buildResult.Result;

            var result = optionParser.Parse("a c","root");
            Assert.Equal("R(a,<none>,c)", result.Result);
        }

        [Fact]
        public void TestEmptyOptionalNonTerminal()
        {
            var buildResult = BuildOptionParser();
            Assert.False(buildResult.IsError);
            var optionParser = buildResult.Result;

            var result = optionParser.Parse("a c","root2");
            Assert.False(result.IsError);
            Assert.Equal("R(a,<none>,c)", result.Result);
        }

        [Fact]
        public void TestNonEmptyOptionalNonTerminal()
        {
            var buildResult = BuildOptionParser();
            Assert.False(buildResult.IsError);
            var optionParser = buildResult.Result;

            var result = optionParser.Parse("a b c", "root2");
            Assert.False(result.IsError);
            Assert.Equal("R(a,B(b),c)", result.Result);
        }

        private void AssertString(JObject obj, string key, string value)
        {
            Assert.True(obj.ContainsKey(key));
            Assert.True(obj[key].IsValue);
            JValue val = (JValue)obj[key];
            Assert.True(val.IsString);
            Assert.Equal(value, val.GetValue<string>());
        }

        private void AssertInt(JObject obj, string key, int value)
        {
            Assert.True(obj.ContainsKey(key));
            Assert.True(obj[key].IsValue);
            JValue val = (JValue)obj[key];
            Assert.True(val.IsInt);
            Assert.Equal(value, val.GetValue<int>());
        }





        private void AssertInt(JList list, int index, int value)
        {
            Assert.True(list[index].IsValue);
            JValue val = (JValue)list[index];
            Assert.True(val.IsInt);
            Assert.Equal(value, val.GetValue<int>());
        }





    }
}
