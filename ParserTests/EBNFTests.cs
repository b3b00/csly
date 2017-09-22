using System.Linq;
using Xunit;
using sly.parser;
using sly.lexer;
using sly.parser.generator;
using System.Collections.Generic;
using expressionparser;
using sly.parser.llparser;
using sly.parser.syntax;
using jsonparser;
using jsonparser.JsonModel;

namespace ParserTests
{

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
            [Lexeme("[ \\t]+",true)]
            WS = 100,
            [Lexeme("\\n\\r]+", true, true)]
            EOL = 101
        }


        [LexerConfiguration]
        public ILexer<TokenType> BuildLexer(ILexer<TokenType> lexer)
        {
            lexer.AddDefinition(new TokenDefinition<TokenType>(TokenType.a, "a"));
            lexer.AddDefinition(new TokenDefinition<TokenType>(TokenType.b, "b"));
            lexer.AddDefinition(new TokenDefinition<TokenType>(TokenType.c, "c"));
            lexer.AddDefinition(new TokenDefinition<TokenType>(TokenType.e, "e"));
            lexer.AddDefinition(new TokenDefinition<TokenType>(TokenType.f, "f"));

            lexer.AddDefinition(new TokenDefinition<TokenType>(TokenType.WS, "[ \\t]+", true));
            lexer.AddDefinition(new TokenDefinition<TokenType>(TokenType.EOL, "[\\n\\r]+", true, true));
            return lexer;
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
            result += (string) astr
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

        private Parser<JsonToken, JSon> JsonParser;

        public EBNFTests()
        {

        }

        private Parser<TokenType, string> BuildParser()
        {
            EBNFTests parserInstance = new EBNFTests();
            ParserBuilder<TokenType, string> builder = new ParserBuilder<TokenType, string>();

            Parser = builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, "R");
            return Parser;
        }


        private Parser<JsonToken, JSon> BuildEbnfJsonParser()
        {
            EbnfJsonParser parserInstance = new EbnfJsonParser();
            ParserBuilder<JsonToken, JSon> builder = new ParserBuilder<JsonToken, JSon>();

            JsonParser =
                builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, "root");
            return JsonParser;
        }

        [Fact]
        public void TestParseBuild()
        {
            Parser = BuildParser();
            Assert.Equal(typeof(EBNFRecursiveDescentSyntaxParser<TokenType, string>), Parser.SyntaxParser.GetType());
            Assert.Equal(Parser.Configuration.NonTerminals.Count, 4);
            NonTerminal<TokenType> nt = Parser.Configuration.NonTerminals["R"];
            Assert.Equal(nt.Rules.Count, 2);
            nt = Parser.Configuration.NonTerminals["A"];
            Assert.Equal(nt.Rules.Count, 1);
            Rule<TokenType> rule = nt.Rules[0];
            Assert.Equal(rule.Clauses.Count, 1);
            Assert.IsType(typeof(OneOrMoreClause<TokenType>), rule.Clauses[0]);
            nt = Parser.Configuration.NonTerminals["B"];
            Assert.Equal(nt.Rules.Count, 1);
            rule = nt.Rules[0];
            Assert.Equal(rule.Clauses.Count, 1);
            Assert.IsType(typeof(ZeroOrMoreClause<TokenType>), rule.Clauses[0]);
            ;
        }


        [Fact]
        public void TestOneOrMoreNonTerminal()
        {
            Parser = BuildParser();
            ParseResult<TokenType, string> result = Parser.Parse("e f e f");
            Assert.False(result.IsError);
            Assert.Equal("R(G(e,f),G(e,f))", result.Result.ToString().Replace(" ", ""));
        }
    

    [Fact]
        public void TestOneOrMoreWithMany()
        {
            Parser = BuildParser();
            ParseResult<TokenType,string> result = Parser.Parse("aaa b c");
            Assert.False(result.IsError);
            Assert.Equal("R(A(a,a,a),B(b),c)",result.Result.ToString().Replace(" ",""));
        }

        [Fact]
        public void TestOneOrMoreWithOne()
        {
            Parser = BuildParser();
            ParseResult<TokenType,string> result = Parser.Parse(" b c");
            Assert.True(result.IsError);
        }

        [Fact]
        public void TestZeroOrMoreWithOne()
        {
            Parser = BuildParser();
            ParseResult<TokenType,string> result = Parser.Parse("a b c");
            Assert.False(result.IsError);
            Assert.Equal("R(A(a),B(b),c)", result.Result.ToString().Replace(" ", ""));
        }

        [Fact]
        public void TestZeroOrMoreWithMany()
        {
            Parser = BuildParser();
            ParseResult<TokenType,string> result = Parser.Parse("a bb c");
            Assert.False(result.IsError);
            Assert.Equal("R(A(a),B(b,b),c)", result.Result.ToString().Replace(" ", ""));
        }

        [Fact]
        public void TestZeroOrMoreWithNone()
        {
            Parser = BuildParser();
            ParseResult<TokenType,string> result = Parser.Parse("a  c");
            Assert.False(result.IsError);
            Assert.Equal("R(A(a),B(),c)", result.Result.ToString().Replace(" ", ""));
        }


        [Fact]
        public void TestJsonList()
        {
            Parser<JsonToken,JSon> jsonParser = BuildEbnfJsonParser();
            ParseResult<JsonToken,JSon> result = jsonParser.Parse("[1,2,3,4]");
            Assert.False(result.IsError);
            Assert.True(result.Result.IsList);
            JList list = (JList) result.Result;
            Assert.Equal(4, list.Count);
            AssertInt(list,0,1);
            AssertInt(list,1,2);
            AssertInt(list,2,3);
            AssertInt(list,3,4);
        }

        [Fact]
        public void TestJsonObject()
        {
            Parser<JsonToken,JSon> jsonParser = BuildEbnfJsonParser();
            ParseResult<JsonToken,JSon> result = jsonParser.Parse("{\"one\":1,\"two\":2,\"three\":\"trois\" }");
            Assert.False(result.IsError);
            Assert.True(result.Result.IsObject);
            JObject o = (JObject) result.Result;
            Assert.Equal(3, o.Count);
            AssertInt(o,"one",1);
            AssertInt(o,"two",2);
            AssertString(o,"three","trois");
        }
        
        private void AssertString(JObject obj, string key, string value)
        {
            Assert.True(obj.ContainsKey(key));
            Assert.True(obj[key].IsValue);
            JValue val = (JValue) obj[key];
            Assert.True(val.IsString);
            Assert.Equal(value, val.GetValue<string>() );
        }
        
        private void AssertInt(JObject obj, string key, int value)
        {
            Assert.True(obj.ContainsKey(key));
            Assert.True(obj[key].IsValue);
            JValue val = (JValue) obj[key];
            Assert.True(val.IsInt);
            Assert.Equal(value, val.GetValue<int>() );
        }
        
        
        private void AssertDouble(JObject obj, string key, double value)
        {
            Assert.True(obj.ContainsKey(key));
            Assert.True(obj[key].IsValue);
            JValue val = (JValue) obj[key];
            Assert.True(val.IsDouble);
            Assert.Equal(value, val.GetValue<double>() );
        }
        
        
        private void AssertString(JList list, int index, string value)
        {
            Assert.True(list[index].IsValue);
            JValue val = (JValue) list[index];
            Assert.True(val.IsString);
            Assert.Equal(value, val.GetValue<string>() );
        }
        
        private void AssertInt(JList list, int index, int value)
        {
            Assert.True(list[index].IsValue);
            JValue val = (JValue) list[index];
            Assert.True(val.IsInt);
            Assert.Equal(value, val.GetValue<int>() );
        }
        
        
        private void AssertDouble(JList list, int index, double value)
        {
            Assert.True(list[index].IsValue);
            JValue val = (JValue) list[index];
            Assert.True(val.IsDouble);
            Assert.Equal(value, val.GetValue<double>() );
        }
        
        
        private void AssertBool(JList list, int index, bool value)
        {
            Assert.True(list[index].IsValue);
            JValue val = (JValue) list[index];
            Assert.True(val.IsBool);
            Assert.Equal(value, val.GetValue<bool>() );
        }
        
        
        private void AssertObject(JList list, int index, int count)
        {
            Assert.True(list[index].IsObject);
            JObject val = (JObject) list[index];
            Assert.Equal(count, val.Count);
        }




    }
}
