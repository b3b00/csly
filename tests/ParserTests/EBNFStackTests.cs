using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using indented;
using jsonparser;
using jsonparser.JsonModel;
using NFluent;
using simpleExpressionParser;
using sly.buildresult;
using sly.lexer;
using sly.parser;
using sly.parser.generator;
using sly.parser.llparser.ebnf;
using sly.parser.parser;
using sly.parser.parser.llparser.ebnf.stackist;
using sly.parser.syntax.grammar;
using Xunit;
using ExpressionToken = simpleExpressionParser.ExpressionToken;
using String = System.String;

namespace ParserTests
{
    

    [Collection("stack")]
    public class EBNFStackTests
    {
        public enum TokenType
        {
            [Lexeme("a")] a = 1,
            [Lexeme("b")] b = 2,
            [Lexeme("c")] c = 3,
            [Lexeme("e")] e = 4,
            [Lexeme("f")] f = 5,
            [Lexeme("[ \\t]+", true)] WS = 100,
            [Lexeme("\\n\\r]+", true, true)] EOL = 101
        }


        [Production("R : A B c ")]
        public string R(string A, string B, Token<TokenType> c)
        {
            var result = "R(";
            result += A + ",";
            result += B + ",";
            result += c.Value;
            result += ")";
            return result;
        }

        [Production("R : G+ ")]
        public string RManyNT(List<string> gs)
        {
            if (gs.Any())
            {
                var result = "R(";
                result += gs
                    .Select(g => g.ToString())
                    .Aggregate((s1, s2) => s1 + "," + s2);
                result += ")";
                return result;
            }

            return "";
        }

        [Production("G : e f ")]
        public string RManyNT(Token<TokenType> e, Token<TokenType> f)
        {
            var result = $"G({e.Value},{f.Value})";
            return result;
        }

        [Production("A : a + ")]
        public string A(List<Token<TokenType>> astr)
        {
            var result = "A(";
            result += astr
                .Select(a => a.Value)
                .Aggregate((a1, a2) => a1 + ", " + a2);
            result += ")";
            return result;
        }

        [Production("B : b * ")]
        public string B(List<Token<TokenType>> bstr)
        {
            if (bstr.Any())
            {
                var result = "B(";
                result += bstr
                    .Select(b => b.Value)
                    .Aggregate((b1, b2) => b1 + ", " + b2);
                result += ")";
                return result;
            }

            return "B()";
        }
        
        [Production("Ba : b* a")]
        public string Ba(List<Token<TokenType>> bstr, Token<TokenType> a) {
            var result = "Ba(";
            if (bstr.Any())
            {
                result += bstr
                    .Select(b => b.Value)
                    .Aggregate((b1, b2) => b1 + ", " + b2);
                result += ", ";
            }

            result += a.Value;
            result += ")";
            return result;
        }
        [Production("BA : b* A")]
        public string BA(List<Token<TokenType>> bstr, string a) {
            var result = "BA(";
            if (bstr.Any())
            {
                result += bstr
                    .Select(b => b.Value)
                    .Aggregate((b1, b2) => b1 + ", " + b2);
                result += ", ";
            }
            result += a;
            result += ")";
            return result;
        }

       


        private Parser<TokenType, string> Parser;

        private BuildResult<Parser<TokenType, string>> BuildParser()
        {
            var parserInstance = new EBNFStackTests();
            var builder = new ParserBuilder<TokenType, string>();
            var result = builder.BuildParser(parserInstance, ParserType.EBNF_LL_STACK, "R");
            return result;
        }


        private BuildResult<Parser<JsonToken, JSon>> BuildEbnfJsonParser()
        {
            var parserInstance = new EbnfJsonParser();
            var builder = new ParserBuilder<JsonToken, JSon>();

            var result =
                builder.BuildParser(parserInstance, ParserType.EBNF_LL_STACK, "root");
            return result;
        }

        private BuildResult<Parser<OptionTestToken, string>> BuildOptionParser()
        {
            var parserInstance = new OptionTestParser();
            var builder = new ParserBuilder<OptionTestToken, string>();

            var result =
                builder.BuildParser(parserInstance, ParserType.EBNF_LL_STACK, "root");
            return result;
        }

        private BuildResult<Parser<GroupTestToken, string>> BuildGroupParser()
        {
            var parserInstance = new GroupTestParser();
            var builder = new ParserBuilder<GroupTestToken, string>();

            var result =
                builder.BuildParser(parserInstance, ParserType.EBNF_LL_STACK, "rootGroup");
            return result;
        }

        

        [Fact]
        public void TestBuildGroupParser()
        {
            var buildResult = BuildGroupParser();
            Check.That(buildResult).IsOk();
        }

        [Fact]
        public void TestEmptyOptionalNonTerminal()
        {
            var buildResult = BuildOptionParser();
            Check.That(buildResult).IsOk();
            var optionParser = buildResult.Result;

            var result = optionParser.Parse("a c", "root2");
            Check.That(result).IsOkParsing();
            Check.That(result.Result).IsEqualTo("R(a,<none>,c)");
        }

        [Fact]
        public void TestEmptyOptionTerminalInMiddle()
        {
            var buildResult = BuildOptionParser();
            Check.That(buildResult).IsOk();
            var optionParser = buildResult.Result;

            var result = optionParser.Parse("a c", "root2");
            Check.That(result).IsOkParsing();
            Check.That(result.Result).IsEqualTo("R(a,<none>,c)");
        }


        [Fact]
        public void TestEmptyTerminalOption()
        {
            var buildResult = BuildOptionParser();
            Check.That(buildResult).IsOk();
            var optionParser = buildResult.Result;

            var result = optionParser.Parse("a b", "root3");
            Check.That(result).IsOkParsing();
            Check.That(result.Result).IsEqualTo("R(a,B(b),<none>)");
        }

        [Fact]
        public void TestErrorMissingClosingBracket()
        {
            var jsonParser = new EbnfJsonGenericParser();
            var builder = new ParserBuilder<JsonTokenGeneric, JSon>();
            var build = builder.BuildParser(jsonParser, ParserType.EBNF_LL_STACK, "root");
            var parserTest = build.Result;
            ParseResult<JsonTokenGeneric, JSon> r = null;
            try
            {
                r = parserTest.Parse("{");
            }
            catch (Exception e)
            {
                var stack = e.StackTrace;
                var message = e.Message;
            }

            Check.That(r).Not.IsOkParsing();
        }

        [Fact]
        public void TestGroupSyntaxManyParser()
        {
            var buildResult = BuildGroupParser();
            Check.That(buildResult).IsOk();
            var groupParser = buildResult.Result;
            var res = groupParser.Parse("a ,a , a ,a,a", "rootMany");
            Check.That(res).IsOkParsing();
            Check.That(res.Result).IsEqualTo("R(a,a,a,a,a)");
        }
        
        [Fact]
        public void TestGroupSyntaxChoicesParser()
        {
            var buildResult = BuildGroupParser();
            Check.That(buildResult).IsOk();
            var groupParser = buildResult.Result;
            var res = groupParser.Parse("a ;a ", "rootGroupChoice");

            Check.That(res).IsOkParsing();
            Check.That(res.Result).IsEqualTo("R(a,a)"); 
            
            res = groupParser.Parse("a ,a ", "rootGroupChoice");

            Check.That(res).IsOkParsing();
            Check.That(res.Result).IsEqualTo("R(a,a)");
        }
        
        [Fact]
        public void TestGroupSyntaxChoicesManyParser()
        {
            var buildResult = BuildGroupParser();
            Check.That(buildResult).IsOk();
            var groupParser = buildResult.Result;
            var res = groupParser.Parse("a ;a,a  ; a,a ", "rootGroupChoiceMany");
            Check.That(res).IsOkParsing();
            Check.That(res.Result).IsEqualTo("R(a,a,a,a,a)"); // rootMany
        }

        [Fact]
        public void TestGroupSyntaxOptionIsSome()
        {
            var buildResult = BuildGroupParser();
            Check.That(buildResult).IsOk();
            var groupParser = buildResult.Result;
            var res = groupParser.Parse("a ; a ", "rootOption");
            Check.That(res).IsOkParsing();
            Check.That(res.Result).IsEqualTo("R(a;a)");
        }

        [Fact]
        public void TestGroupSyntaxOptionIsNone()
        {
            var buildResult = BuildGroupParser();
            Check.That(buildResult).IsOk();
            var groupParser = buildResult.Result;
            var res = groupParser.Parse("a ", "rootOption");
            Check.That(res).IsOkParsing();
            Check.That(res.Result).IsEqualTo("R(a;<none>)");
        }

        [Fact]
        public void TestGroupSyntaxParser()
        {
            var buildResult = BuildGroupParser();
            Check.That(buildResult).IsOk();
            var groupParser = buildResult.Result;
            var res = groupParser.Parse("a ,a");

            Check.That(res).IsOkParsing();
            Check.That(res.Result).IsEqualTo("R(a; {,,,a})");
        }


        [Fact]
        public void TestJsonList()
        {
            var buildResult = BuildEbnfJsonParser();
            Check.That(buildResult).IsOk();
            var jsonParser = buildResult.Result;

            var result = jsonParser.Parse("[1,2,3,4]");
            Check.That(result).IsOkParsing();
            Check.That(result.Result.IsList).IsTrue();
            
            var list = (JList) result.Result;
            Check.That(list.Count).IsEqualTo(4);
            
            Check.That(list).HasItem(0,1);
            Check.That(list).HasItem(1,2);
            Check.That(list).HasItem( 2,3);
            Check.That(list).HasItem( 3,4);
        }

        [Fact]
        public void TestJsonObject()
        {
            var buildResult = BuildEbnfJsonParser();
            Check.That(buildResult).IsOk();
            var jsonParser = buildResult.Result;
            var result = jsonParser.Parse("{\"one\":1,\"two\":2,\"three\":\"trois\" }");
            Check.That(result).IsOkParsing();
            Check.That(result.Result.IsObject).IsTrue();
            
            var o = (JObject) result.Result;
            Check.That(o.Count).IsEqualTo(3);
            Check.That(o.Count).IsEqualTo(3);
            Check.That(o).HasProperty("one", 1);
            Check.That(o).HasProperty("two", 2);
            Check.That(o).HasProperty("three", "trois");
        }

        [Fact]
        public void TestNonEmptyOptionalNonTerminal()
        {
            var buildResult = BuildOptionParser();
            Check.That(buildResult).IsOk();
            var optionParser = buildResult.Result;

            var result = optionParser.Parse("a b c", "root2");
            Check.That(result).IsOkParsing();
            Check.That(result.Result).IsEqualTo("R(a,B(b),c)");
        }


        [Fact]
        public void TestNonEmptyTerminalOption()
        {
            var buildResult = BuildOptionParser();
            Check.That(buildResult).IsOk();
            var optionParser = buildResult.Result;

            var result = optionParser.Parse("a b c", "root");
            Check.That(result).IsOkParsing();
            Check.That(result.Result).IsEqualTo("R(a,b,c)");
        }


        [Fact]
        public void TestOneOrMoreNonTerminal()
        {
            var buildResult = BuildParser();
            Check.That(buildResult).IsOk();
            Parser = buildResult.Result;
            var result = Parser.Parse("e f e f");
            Check.That(result).IsOkParsing();
            Check.That(result.Result).IsEqualTo("R(G(e,f),G(e,f))");
        }


        [Fact]
        public void TestOneOrMoreWithMany()
        {
            var buildResult = BuildParser();
            Check.That(buildResult).IsOk();
            Parser = buildResult.Result;
            var result = Parser.Parse("aaa b c");
            Check.That(result).IsOkParsing();
            Check.That(result.Result).IsEqualTo("R(A(a, a, a),B(b),c)");
        }

        [Fact]
        public void TestOneOrMoreWithOne()
        {
            var buildResult = BuildParser();
            Check.That(buildResult).IsOk();
            Parser = buildResult.Result;
            var result = Parser.Parse(" b c");
            Check.That(result).Not.IsOkParsing();
            
        }

        [Fact]
        public void TestParseBuild()
        {
            var buildResult = BuildParser();
            Check.That(buildResult).IsOk();
            Parser = buildResult.Result;
            Check.That(Parser.SyntaxParser).IsInstanceOf<EBNFStackDescentSyntaxParser<TokenType, string>>();
            Check.That(Parser.Configuration.NonTerminals).CountIs(6);
            
            var nt = Parser.Configuration.NonTerminals["R"];
            Check.That(nt.Rules).CountIs(2);
            nt = Parser.Configuration.NonTerminals["A"];
            Check.That(nt.Rules).CountIs(1);
            var rule = nt.Rules[0];
            Check.That(rule.Clauses).CountIs(1);
            Check.That(rule.Clauses[0]).IsInstanceOf<OneOrMoreClause<TokenType, string>>();
            nt = Parser.Configuration.NonTerminals["B"];
            Check.That((nt.Rules)).CountIs(1);
            rule = nt.Rules[0];
            Check.That((rule.Clauses)).CountIs(1);
            Check.That(rule.Clauses[0]).IsInstanceOf<ZeroOrMoreClause<TokenType, string>>();
        }

        [Fact]
        public void TestZeroOrMoreWithMany()
        {
            var buildResult = BuildParser();
            Check.That(buildResult).IsOk();
            Parser = buildResult.Result;
            var result = Parser.Parse("a bb c");
            Check.That(result).IsOkParsing();
            Check.That(result.Result).IsEqualTo("R(A(a),B(b, b),c)");            
        }
        
        [Fact]
        public void TestZeroOrMoreStarterFollowedByTerminal()
        {
            var buildResult = BuildParser();
            Check.That(buildResult).IsOk();
            Parser = buildResult.Result;
            var result = Parser.Parse("bbb a","Ba");
            Check.That(result).IsOkParsing();
            Check.That(result.Result).IsEqualTo("Ba(b, b, b, a)");
            result = Parser.Parse("a","Ba");
            Check.That(result).IsOkParsing();
            Check.That(result.Result).IsEqualTo("Ba(a)"); 
        }
        
        [Fact]
        public void TestZeroOrMoreStarterFollowedByNonTerminal()
        {
            var buildResult = BuildParser();
            Check.That(buildResult).IsOk();
            Parser = buildResult.Result;
            var result = Parser.Parse("bbb a","BA");
            Check.That(result).IsOkParsing();
            Check.That(result.Result).IsEqualTo("BA(b, b, b, A(a))");    
            result = Parser.Parse("a","BA");
            Check.That(result).IsOkParsing();
            Check.That(result.Result).IsEqualTo("BA(A(a))");    
        }

        [Fact]
        public void TestZeroOrMoreWithNone()
        {
            var buildResult = BuildParser();
            Check.That(buildResult).IsOk();
            Parser = buildResult.Result;
            var result = Parser.Parse("a  c");
            Check.That(result).IsOkParsing();
            Check.That(result.Result).IsEqualTo("R(A(a),B(),c)");
        }

        [Fact]
        public void TestZeroOrMoreWithOne()
        {
            var buildResult = BuildParser();
            Check.That(buildResult).IsOk();
            Parser = buildResult.Result;
            var result = Parser.Parse("a b c");
            Check.That(result).IsOkParsing();
            Check.That(result.Result).IsEqualTo("R(A(a),B(b),c)");
        }


        #region CONTEXTS

        private BuildResult<Parser<ExpressionToken, int>> buildSimpleExpressionParserWithContext(ParserType parserType = ParserType.EBNF_LL_STACK)
        {
            var startingRule = $"{nameof(SimpleExpressionParserWithContext)}_expressions";
            var parserInstance = new SimpleExpressionParserWithContext();
            var builder = new ParserBuilder<ExpressionToken, int>();
            var parser = builder.BuildParser(parserInstance, parserType, startingRule);
            return parser;
        }

        [Fact]
        public void TestContextualParsing()
        {
            var buildResult = buildSimpleExpressionParserWithContext();
            Check.That(buildResult).IsOk();
            //Check.That(buildResult).IsOkParsing();
            var parser = buildResult.Result;
            var res = parser.ParseWithContext("2 + a", new Dictionary<string, int> {{"a", 2}});
            Check.That(res).IsOkParsing();
            Check.That(res.Result).IsEqualTo(4);
        }

        [Fact]
        public void TestContextualParsing2()
        {
            var buildResult = buildSimpleExpressionParserWithContext();

            Check.That(buildResult).IsOk();
            var parser = buildResult.Result;
            var res = parser.ParseWithContext("2 + a * b", new Dictionary<string, int> {{"a", 2}, {"b", 3}});
            Check.That(res.IsOk).IsTrue();
            Check.That(res.Result).IsEqualTo(8);
        }
        
        [Fact]
        public void TestContextualParsingPrefixAndPostfix()
        {
            var buildResult = buildSimpleExpressionParserWithContext();

            Check.That(buildResult).IsOk();
            var parser = buildResult.Result;
            var res = parser.ParseWithContext("- a", new Dictionary<string, int> {{"a", 3}});
            Check.That(res.IsOk).IsTrue();
            Check.That(res.Result).IsEqualTo(-3);
        }

        [Fact]
        public void TestContextualParsingWithEbnf()
        {
            var buildResult = buildSimpleExpressionParserWithContext(ParserType.EBNF_LL_STACK);

            Check.That(buildResult).IsOk();
            var parser = buildResult.Result;
            var res = parser.ParseWithContext("2 + a * b", new Dictionary<string, int> {{"a", 2}, {"b", 3}});
            Check.That(res.IsOk).IsTrue();
            Check.That(res.Result).IsEqualTo(8);
        }

        [Fact]
        public void TestBug100()
        {
            var startingRule = $"testNonTerm";
            var parserInstance = new Bugfix100Test();
            var builder = new ParserBuilder<GroupTestToken, int>();
            var builtParser = builder.BuildParser(parserInstance, ParserType.EBNF_LL_STACK, startingRule);
            Check.That(builtParser).IsOk();
            Check.That(builtParser.Result).IsNotNull();
            var parser = builtParser.Result;
            var conf = parser.Configuration;
            var expected = new List<GroupTestToken>() {GroupTestToken.A, GroupTestToken.COMMA};

            var nonTerm = conf.NonTerminals["testNonTerm"];
            Check.That(nonTerm).IsNotNull();
            Check.That(nonTerm.GetPossibleLeadingTokens()).CountIs(2);
            Check.That(nonTerm.GetPossibleLeadingTokens().Select(x => x.TokenId)).Contains(expected);
            
            var term = conf.NonTerminals["testTerm"];
            Check.That(term).IsNotNull();
            Check.That(term.GetPossibleLeadingTokens()).CountIs(2);
            Check.That(term.GetPossibleLeadingTokens().Select(x => x.TokenId)).Contains(expected);
        }

        #endregion

        [Fact]
        public void TestBug104()
        {
            var startingRule = $"testNonTerm";
            var parserInstance = new Bugfix104Test();
            var builder = new ParserBuilder<GroupTestToken, int>();
            var builtParser = builder.BuildParser(parserInstance, ParserType.EBNF_LL_STACK, startingRule);
            Check.That(builtParser).IsOk();
            Check.That(builtParser.Errors).IsEmpty();
        }

        [Fact]
        public void TestAlternateChoiceTerminal()
        {
            var startingRule = $"choice";
            var parserInstance = new AlternateChoiceTestTerminal();
            var builder = new ParserBuilder<OptionTestToken, string>();
            var builtParser = builder.BuildParser(parserInstance, ParserType.EBNF_LL_STACK, startingRule);
            Check.That(builtParser).IsOk();
            Check.That(builtParser.Errors).IsEmpty();
            var parseResult = builtParser.Result.Parse("a", "choice");
            Check.That(parseResult.IsOk).IsTrue();
            Check.That(parseResult.Result).IsEqualTo("a");
            parseResult = builtParser.Result.Parse("b", "choice");
            Check.That(parseResult.IsOk).IsTrue();
            Check.That(parseResult.Result).IsEqualTo("b");
            parseResult = builtParser.Result.Parse("c", "choice");
            Check.That(parseResult.IsOk).IsTrue();
            Check.That(parseResult.Result).IsEqualTo("c");
            parseResult = builtParser.Result.Parse("d", "choice");
            Check.That(parseResult.IsOk).IsFalse();
        }
        
        [Fact]
        public void TestAlternateChoiceNonTerminal()
        {
            var startingRule = $"choice";
            var parserInstance = new AlternateChoiceTestNonTerminal();
            var builder = new ParserBuilder<OptionTestToken, string>();
            var builtParser = builder.BuildParser(parserInstance, ParserType.EBNF_LL_STACK, startingRule);
            Check.That(builtParser).IsOk();
            Check.That(builtParser.Errors).IsEmpty();
            var parseResult = builtParser.Result.Parse("a", "choice");
            Check.That(parseResult.IsOk).IsTrue();
            Check.That(parseResult.Result).IsEqualTo("A(a)");
            parseResult = builtParser.Result.Parse("b", "choice");
            Check.That(parseResult.IsOk).IsTrue();
            Check.That(parseResult.Result).IsEqualTo("B(b)");
            parseResult = builtParser.Result.Parse("c", "choice");
            Check.That(parseResult.IsOk).IsTrue();
            Check.That(parseResult.Result).IsEqualTo("C(c)");;
            parseResult = builtParser.Result.Parse("d", "choice");
            Check.That(parseResult.IsOk).IsFalse();
        }

        [Fact]
        public void TestAlternateChoiceOneOrMoreNonTerminal()
        {
            var startingRule = $"choice";
            var parserInstance = new AlternateChoiceTestOneOrMoreNonTerminal();
            var builder = new ParserBuilder<OptionTestToken, string>();
            var builtParser = builder.BuildParser(parserInstance, ParserType.EBNF_LL_STACK, startingRule);
            Check.That(builtParser).IsOk();
            Check.That(builtParser.Errors).IsEmpty();
            var parseResult = builtParser.Result.Parse("a b", "choice");
            Check.That(parseResult).IsOkParsing();
            Check.That(parseResult.Result).IsEqualTo("A(a) B(b)");
            
            parseResult = builtParser.Result.Parse("b", "choice");
            Check.That(parseResult.IsOk).IsTrue();
            Check.That(parseResult.Result).IsEqualTo("B(b)");
            parseResult = builtParser.Result.Parse("c", "choice");
            Check.That(parseResult.IsOk).IsTrue();
            Check.That(parseResult.Result).IsEqualTo("C(c)");
            parseResult = builtParser.Result.Parse("d", "choice");
            Check.That(parseResult.IsOk).IsFalse();
        }

        [Fact]
        public void TestAlternateChoiceZeroOrMoreTerminal()
        {
            var startingRule = $"choice";
            var parserInstance = new AlternateChoiceTestZeroOrMoreTerminal();
            var builder = new ParserBuilder<OptionTestToken, string>();
            var builtParser = builder.BuildParser(parserInstance, ParserType.EBNF_LL_STACK, startingRule);
            Check.That(builtParser).IsOk();
            Check.That(builtParser.Errors).IsEmpty();
            var parseResult = builtParser.Result.Parse("a b c", "choice");
            Check.That(parseResult.IsOk).IsTrue();
            Check.That(parseResult.Result).IsEqualTo("a,b,c");
            parseResult = builtParser.Result.Parse("b", "choice");
            Check.That(parseResult.IsOk).IsTrue();
        }

        [Fact]
        public void TestAlternateChoiceOneOrMoreTerminal()
        {
            var startingRule = $"choice";
            var parserInstance = new AlternateChoiceTestOneOrMoreTerminal();
            var builder = new ParserBuilder<OptionTestToken, string>();
            var builtParser = builder.BuildParser(parserInstance, ParserType.EBNF_LL_STACK, startingRule);
            Check.That(builtParser).IsOk();
            Check.That(builtParser.Errors).IsEmpty();
            var parseResult = builtParser.Result.Parse("a b c", "choice");
            Check.That(parseResult).IsOkParsing();
            Check.That(parseResult.Result).IsEqualTo("a,b,c");
            parseResult = builtParser.Result.Parse("b", "choice");
            Check.That(parseResult.IsOk).IsTrue();
        }

        [Fact]
        public void TestAlternateChoiceOptionTerminal()
        {
            var startingRule = $"choice";
            var parserInstance = new AlternateChoiceTestOptionTerminal();
            var builder = new ParserBuilder<OptionTestToken, string>();
            var builtParser = builder.BuildParser(parserInstance, ParserType.EBNF_LL_STACK, startingRule);
            Check.That(builtParser).IsOk();
            Check.That(builtParser.Errors).IsEmpty();
            var parseResult = builtParser.Result.Parse("a b", "choice");
            Check.That(parseResult.IsOk).IsTrue();
            Check.That(parseResult.Result).IsEqualTo("a,b");
            parseResult = builtParser.Result.Parse("a", "choice");
            Check.That(parseResult.IsOk).IsTrue();
            Check.That(parseResult.Result).IsEqualTo("a,<none>");
        }
        
        [Fact]
        public void TestAlternateChoiceOptionNonTerminal()
        {
            var startingRule = $"choice";
            var parserInstance = new AlternateChoiceTestOptionNonTerminal();
            var builder = new ParserBuilder<OptionTestToken, string>();
            var builtParser = builder.BuildParser(parserInstance, ParserType.EBNF_LL_STACK, startingRule);
            Check.That(builtParser).IsOk();
            Check.That(builtParser.Errors).IsEmpty();
            var parseResult = builtParser.Result.Parse("a b f", "choice");
            Check.That(parseResult.IsOk).IsTrue();
            Check.That(parseResult.Result).IsEqualTo("a,b,f");
            parseResult = builtParser.Result.Parse("a", "choice");
            Check.That(parseResult.IsOk).IsTrue();
            Check.That(parseResult.Result).IsEqualTo("a,<none>,<none>");
            parseResult = builtParser.Result.Parse("a b ", "choice");
            Check.That(parseResult.IsOk).IsTrue();
            Check.That(parseResult.Result).IsEqualTo("a,b,<none>");
            parseResult = builtParser.Result.Parse("a f", "choice");
            Check.That(parseResult.IsOk).IsTrue();
            Check.That(parseResult.Result).IsEqualTo("a,<none>,f");
            
        }
        
        [Fact]
        public void TestAlternateChoiceOptionDiscardedTerminal()
        {
            var startingRule = $"choice";
            var parserInstance = new AlternateChoiceTestOptionDiscardedTerminal();
            var builder = new ParserBuilder<OptionTestToken, string>();
            var builtParser = builder.BuildParser(parserInstance, ParserType.EBNF_LL_STACK, startingRule);
            Check.That(builtParser).IsOk();
            Check.That(builtParser.Errors).IsEmpty();
            var parseResult = builtParser.Result.Parse("a b", "choice");
            Check.That(parseResult.IsOk).IsTrue();
            Check.That(parseResult.Result).IsEqualTo("a");
            parseResult = builtParser.Result.Parse("a", "choice");
            Check.That(parseResult).Not.IsOkParsing();
            Check.That(parseResult.Errors).CountIs(1);
            Check.That(parseResult.Errors[0].ErrorType).IsEqualTo(ErrorType.UnexpectedEOS);
        }

        [Fact]
        public void TestAlternateChoiceErrorMixedTerminalAndNonTerminal()
        {
            var startingRule = $"choice";
            var parserInstance = new AlternateChoiceTestError();
            var builder = new ParserBuilder<OptionTestToken, string>("en");
            var builtParser = builder.BuildParser(parserInstance, ParserType.EBNF_LL_STACK, startingRule);
            Check.That(builtParser).Not.IsOk();
            Check.That(builtParser.Errors).CountIs(2);
            Check.That(builtParser.Errors.Select(x => x.Code)).Contains(ErrorCodes.PARSER_MIXED_CHOICES,
                ErrorCodes.PARSER_NON_TERMINAL_CHOICE_CANNOT_BE_DISCARDED);
            Check.That(builtParser.Errors.Select(x => x.Message)).Contains("choice : [ a | b | C | D] contains [ a(T) | b(T) | C(NT) | D(NT) ] with mixed terminal and nonterminal.");
            
        }
        
        
        
        [Fact]
        public void TestAlternateChoiceInGroupLeftRecursion()
        {
            var startingRule = $"choiceInGroup";
            var parserInstance = new LeftRecWithChoiceInGroup();
            var builder = new ParserBuilder<OptionTestToken, string>();
            var builtParser = builder.BuildParser(parserInstance, ParserType.EBNF_LL_STACK, startingRule);
            Check.That(builtParser).Not.IsOk();
            Check.That(builtParser.Errors).CountIs(1);
            Check.That(builtParser.Errors.First().Code).IsEqualTo(ErrorCodes.PARSER_LEFT_RECURSIVE);
        }


        [Fact]
        public void TestIssue507TransitiveEmptyStarter()
        {
            var startingRule = $"x";
            var parserInstance = new Issue507TransitiveEmptyStarterParser();
            var builder = new ParserBuilder<OptionTestToken, string>();
            var builtParser = builder.BuildParser(parserInstance, ParserType.EBNF_LL_STACK, startingRule);
            Check.That(builtParser).IsOk();
            var parser = builtParser.Result;
            var parserResultNotEmpty = parser.Parse("a a a");
            Check.That(parserResultNotEmpty).IsOkParsing();
            Check.That(parserResultNotEmpty.Result).IsEqualTo("a,a,a");
            
            var parserResultEmpty = parser.Parse("");
            Check.That(parserResultEmpty).IsOkParsing();
            Check.That(parserResultEmpty.Result).IsEqualTo("empty");
        }
        
        [Fact]
        public void TestIssue507MoreTransitiveEmptyStarter()
        {
            var startingRule = $"x";
            var parserInstance = new Issue507MoreTransitiveEmptyStarterParser();
            var builder = new ParserBuilder<OptionTestToken, string>();
            var builtParser = builder.BuildParser(parserInstance, ParserType.EBNF_LL_STACK, startingRule);
            Check.That(builtParser).IsOk();
            var parser = builtParser.Result;
            var parserResultNotEmpty = parser.Parse("a a a");
            Check.That(parserResultNotEmpty).IsOkParsing();
            Check.That(parserResultNotEmpty.Result).IsEqualTo("a,a,a");
            
            var parserResultEmpty = parser.Parse("");
            Check.That(parserResultEmpty).IsOkParsing();
            Check.That(parserResultEmpty.Result).IsEqualTo("empty");
        }

        [Fact]
        public void TestIssue190()
        {
            var startingRule = $"root";
            var parserInstance = new Issue190parser();
            var builder = new ParserBuilder<Issue190Token, bool>();
            var builtParser = builder.BuildParser(parserInstance, ParserType.EBNF_LL_STACK, startingRule);
            Check.That(builtParser).IsOk();
            var parser = builtParser.Result;
            var parserResultNotTrue = parser.Parse("not true");
            Check.That(parserResultNotTrue.IsOk).IsTrue();
            Check.That(parserResultNotTrue.Result).IsFalse();
            var parserResultTrue = parser.Parse("yes");
            Check.That(parserResultTrue.IsOk).IsTrue();
            Check.That(parserResultTrue.Result).IsTrue();
        }

        [Fact]
        public void TestIssue193()
        {
            RuleParserType.ParserType = ParserType.LL_RECURSIVE_DESCENT;
            var builtParser = BuildParser();
            Check.That(builtParser).IsOk();
            Check.That(builtParser.Result).IsNotNull();
            var parser = builtParser.Result;
            var test = parser.Parse("a b");

            Check.That(test).Not.IsOkParsing();
            Check.That(test.Errors).CountIs(1);
            var error = test.Errors[0] as UnexpectedTokenSyntaxError<TokenType>;
            // TODO : parser does not return furthest error. 
            Check.That(error).IsNotNull();
            Check.That(error.ErrorType).IsEqualTo(ErrorType.UnexpectedEOS);
        }
        
        [Fact]
        public void TestIssue213()
        {
            var parserInstance = new DoNotIgnoreCommentsParser();
            var builder = new ParserBuilder<DoNotIgnoreCommentsToken, DoNotIgnore>();
            var builtParser = builder.BuildParser(parserInstance, ParserType.EBNF_LL_STACK, "main");

            Check.That(builtParser.IsOk).IsTrue();
            Check.That(builtParser.Result).IsNotNull();
            
            var parser = builtParser.Result;

            var test = parser.Parse("a /*commented b*/b");
            Check.That(test).IsOkParsing();            
            Check.That(test.Result).IsInstanceOf<IdentifierList>();
            
            var list = test.Result as IdentifierList;
            Check.That(list.Ids).CountIs(2);
            Check.That(list.Ids[0].IsCommented).IsFalse();
            Check.That(list.Ids[0].Name).IsEqualTo("a");
            Check.That(list.Ids[1].IsCommented).IsTrue();
            Check.That(list.Ids[1].Name).IsEqualTo("b");
            Check.That(list.Ids[1].Comment).IsEqualTo("commented b");
        }


        [Fact]
        public void TestIndentedParser()
        {
            var source =@"if truc == 1
    un = 1
    deux = 2
else
    trois = 3
    quatre = 4

";
            ParserBuilder<IndentedLangLexer, Ast> builder = new ParserBuilder<IndentedLangLexer, Ast>();
            var instance = new IndentedParser();
            var parserRes = builder.BuildParser(instance, ParserType.EBNF_LL_STACK, "root");
            Check.That(parserRes).IsOk();
            
            var parser = parserRes.Result;
            Check.That(parser).IsNotNull();
            var parseResult = parser.Parse(source);
            Check.That(parseResult).IsOkParsing();
            
            var ast = parseResult.Result;
            Check.That(ast).IsNotNull();
            Check.That(ast).IsInstanceOf<Block>();
            
            Block root = ast as Block;
            Check.That(root.Statements).CountIs(1);
            Check.That(root.Statements.First()).IsInstanceOf<IfThenElse>();
            
            IfThenElse ifthenelse = root.Statements.First() as IfThenElse;
            Check.That(ifthenelse.Cond).IsNotNull();
            Check.That(ifthenelse.Then).IsNotNull();
            Check.That(ifthenelse.Else).IsNotNull();
            Check.That(ifthenelse.Then.Statements).CountIs(2);
            Check.That(ifthenelse.Else.Statements).CountIs(2);
        }
        
        [Fact]
        public void TestIndentedParserNestedBlocks()
        {

            var source =@"
// this is a informative comment
if truc == 1
  un = 1
  deux = 2
else  
  trois = 3
  quatre = 4
  if bidule ==89
     toto = 28
final = 9999
";
            ParserBuilder<IndentedLangLexer, Ast> builder = new ParserBuilder<IndentedLangLexer, Ast>();
            var instance = new IndentedParser();
            var parserRes = builder.BuildParser(instance, ParserType.EBNF_LL_STACK, "root");
            Check.That(parserRes).IsOk();
            var parser = parserRes.Result;
            Check.That(parser).IsNotNull();
            var parseResult = parser.Parse(source);
            Check.That(parseResult).IsOkParsing();
            
            var ast = parseResult.Result;
            Check.That(ast).IsNotNull();
            Check.That(ast).IsInstanceOf<Block>();
            
            Block root = ast as Block;
            Check.That(root.Statements).CountIs(2);
            Check.That(root.Statements.First()).IsInstanceOf<IfThenElse>();
            
            IfThenElse ifthenelse = root.Statements.First() as IfThenElse;
            Check.That(ifthenelse.Comment).IsNotNull();
            Check.That(ifthenelse.Comment.Trim()).IsEqualTo("this is a informative comment");
            
            Check.That(ifthenelse.Cond).IsNotNull();
            Check.That(ifthenelse.Then).IsNotNull();
            Check.That(ifthenelse.Else).IsNotNull();
            
            Check.That(ifthenelse.Then.Statements).CountIs(2);
            Check.That(ifthenelse.Else.Statements).CountIs(3);
            
            var lastelseStatement = ifthenelse.Else.Statements.Last();
            Check.That(lastelseStatement).IsInstanceOf<IfThenElse>();
            var nestedIf = lastelseStatement as IfThenElse;
            Check.That(nestedIf.Then).IsNotNull();
            Check.That(nestedIf.Cond).IsNotNull();
            
            var lastStatement = root.Statements.Last();
            Check.That(lastStatement).IsInstanceOf<Set>();
            
            var finalSet = lastStatement as Set;
            Check.That(finalSet.Id.Name).IsEqualTo("final");
            Check.That(finalSet.Value.Value).IsEqualTo(9999);
            
        }
        
        [Fact]
        public void TestIndentedParserWithEolAwareness()
        {
            var source =@"// information
if truc == 1
    un = 1
    deux = 2
else
    trois = 3
    quatre = 4

";
            ParserBuilder<IndentedLangLexer2, Ast> builder = new ParserBuilder<IndentedLangLexer2, Ast>();
            var instance = new IndentedParser2();
            var parserRes = builder.BuildParser(instance, ParserType.EBNF_LL_STACK, "root");
            Check.That(parserRes.IsOk).IsTrue();
            var parser = parserRes.Result;
            Check.That(parser).IsNotNull();
            var parseResult = parser.Parse(source);
            Check.That(parseResult).IsOkParsing();
            var ast = parseResult.Result;
            Check.That(ast).IsNotNull();
            Check.That(ast).IsInstanceOf<Block>();
            
            Block root = ast as Block;
            Check.That(root.Statements).CountIs(1);
            
            Check.That(root.Statements.First()).IsInstanceOf<IfThenElse>();
            IfThenElse ifthenelse = root.Statements.First() as IfThenElse;
            Check.That(ifthenelse.IsCommented).IsTrue();
            
            Check.That(ifthenelse.Comment.Trim()).IsEqualTo("information");
            Check.That(ifthenelse.Cond).IsNotNull();
            Check.That(ifthenelse.Then).IsNotNull();
            Check.That(ifthenelse.Then.Statements).CountIs(2);
            Check.That(ifthenelse.Else).IsNotNull();
            Check.That(ifthenelse.Else.Statements).CountIs(2);
        }
        
        [Fact]
        public void TestIndentedParserWithEolAwareness2()
        {
            var source =@"// information
if truc == 1
    un = 1
    deux = 2
else
    trois = 3
    quatre = 4

";
            ParserBuilder<IndentedLangLexer2, Ast> builder = new ParserBuilder<IndentedLangLexer2, Ast>();
            var instance = new IndentedParser2();
            var parserRes = builder.BuildParser(instance, ParserType.EBNF_LL_STACK, "root");
            Check.That(parserRes.IsOk).IsTrue();
            var parser = parserRes.Result;
            Check.That(parser).IsNotNull();
            
            var parseResult = parser.Parse(source);
            Check.That(parseResult).IsOkParsing();
            var ast = parseResult.Result;
            Check.That(ast).IsNotNull();
            Check.That(ast).IsInstanceOf<Block>();
            Block root = ast as Block;
            Check.That(root.Statements).CountIs(1);
            Check.That(root.Statements.First()).IsInstanceOf<IfThenElse>();
            IfThenElse ifthenelse = root.Statements.First() as IfThenElse;
            Check.That(ifthenelse.IsCommented).IsTrue();
            Check.That(ifthenelse.Comment).Contains("information");
            Check.That(ifthenelse.Cond).IsNotNull();
            Check.That(ifthenelse.Then).IsNotNull();
            Check.That(ifthenelse.Then.Statements).CountIs(2);
            Check.That(ifthenelse.Else).IsNotNull();
            Check.That(ifthenelse.Else.Statements).CountIs(2);
        }
        
        [Fact]
        public void TestIssue213WithChannels()
        {
            var parserInstance = new DoNotIgnoreCommentsWithChannelsParser();
            var builder = new ParserBuilder<DoNotIgnoreCommentsTokenWithChannels, DoNotIgnore>();
            var builtParser = builder.BuildParser(parserInstance, ParserType.EBNF_LL_STACK, "main");
            
            Check.That(builtParser.IsOk).IsTrue();
            Check.That(builtParser.Result).IsNotNull();
            var parser = builtParser.Result;

            var test = parser.Parse(@"
a

b1
// commented b [1] 
/*commented b [2]*/
b2

c
// comment c @1
// commented c @2
// commented c @3

test

// commented d before
d
// commented d after

");

            Check.That(test).IsOkParsing();
            Check.That(test.Result).IsNotNull();
            Check.That(test.Result).IsInstanceOf<IdentifierList>();
            var list = test.Result as IdentifierList;
            Check.That(list.Ids).CountIs(6);
            
            var id = list.Ids[0];
            Check.That(id.Name).IsEqualTo("a");
            Check.That(id.IsCommented).IsFalse();

            id = list.Ids[1];
            Check.That(id.IsCommented).IsTrue();
            Check.That(id.Name).IsEqualTo("b1");
            Check.That(id.Comment.Trim()).IsEqualTo("commented b [1]\ncommented b [2]");    
            
            id = list.Ids[2];
            Check.That(id.IsCommented).IsTrue();
            Check.That(id.Name).IsEqualTo("b2");
            Check.That(id.Comment.Trim()).IsEqualTo("commented b [1]\ncommented b [2]");    
            
            id = list.Ids[3];
            Check.That(id.IsCommented).IsTrue();
            Check.That(id.Name).IsEqualTo("c");
            var comments = id.Comment;
            Check.That(id.Comment.Trim()).IsEqualTo("comment c @1\ncommented c @2\ncommented c @3");
            
            id = list.Ids[4];
            Check.That(id.Name).IsEqualTo("test");
            Check.That(id.IsCommented).IsTrue(); // catches comment from c  and d
            Check.That(id.Comment.Trim()).IsEqualTo("comment c @1\ncommented c @2\ncommented c @3\ncommented d before");
            
            id = list.Ids[5];
            Check.That(id.IsCommented).IsTrue();
            Check.That(id.Name).IsEqualTo("d");
            comments = id.Comment;
            Check.That(id.Comment.Trim()).IsEqualTo("commented d before\ncommented d after");
            
            test = parser.Parse(@"a 
// commented b
b");

            Check.That(test.IsOk).IsTrue();
            Check.That(test.Result).IsNotNull();
            Check.That(test.Result).IsInstanceOf<IdentifierList>();
            list = test.Result as IdentifierList;
            Check.That(list.Ids).CountIs(2);
            Check.That(list.Ids[0].IsCommented).IsFalse();
            Check.That(list.Ids[0].Name).IsEqualTo("a");
            Check.That(list.Ids[1].IsCommented).IsTrue();
            Check.That(list.Ids[1].Name).IsEqualTo("b");
            Check.That(list.Ids[1].Comment.Trim()).IsEqualTo("commented b");    
            ;

        }

        [Fact]
        public void TestNotClosingIndents()
        {
            var source =@"
if truc == 1
    un = 1
    deux = 2";
            ParserBuilder<IndentedLangLexer, Ast> builder = new ParserBuilder<IndentedLangLexer, Ast>();
            var instance = new IndentedParser();
            var parserRes = builder.BuildParser(instance, ParserType.EBNF_LL_STACK, "root");
            Check.That(parserRes.IsOk).IsTrue();
            
            var parser = parserRes.Result;
            Check.That(parser).IsNotNull();
            var parseResult = parser.Parse(source);
            Check.That(parseResult).Not.IsOkParsing();
            Check.That(parseResult.Errors).CountIs(1);
            var error = parseResult.Errors[0];
            Check.That(error.ErrorType).IsEqualTo(ErrorType.UnexpectedEOS);
        }


        //[Fact] : repeat are not (yet) managed by stack parser
        public void TestRepeat()
        {
            ParserBuilder<BasicToken, string> builder = new ParserBuilder<BasicToken, string>();
            var instance = new RepeatParser();
            var parserRes = builder.BuildParser(instance, ParserType.EBNF_LL_STACK, "root");
            Check.That(parserRes).IsOk();
            
            var parser = parserRes.Result;
            Check.That(parser).IsNotNull();
            var parseResult = parser.Parse("a1b2c3d4e5.");
            Check.That(parseResult).IsOkParsing();
            Check.That(parseResult.Result).IsEqualTo("a(1),b(2),c(3),d(4),e(5)");
            parseResult = parser.Parse(".");
            Check.That(parseResult).IsOkParsing();
            Check.That(parseResult.Result).IsEqualTo("");
            parseResult = parser.Parse("a1b2c3d4e5f6g7");
            Check.That(parseResult).Not.IsOkParsing();
            Check.That(parseResult.Errors).CountIs(1);
            var error = parseResult.Errors[0] as UnexpectedTokenSyntaxError<BasicToken>;
            Check.That(error).IsNotNull();
            Check.That(error.ErrorType).IsEqualTo(ErrorType.UnexpectedToken);
            
            Check.That(error.UnexpectedToken.TokenID).IsEqualTo(BasicToken.ID);
            Check.That(error.UnexpectedToken.Value).IsEqualTo("g");
            
        }
    }
}
