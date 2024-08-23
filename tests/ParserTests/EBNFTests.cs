using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using expressionparser;
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
using sly.parser.syntax.grammar;
using Xunit;
using ExpressionToken = simpleExpressionParser.ExpressionToken;
using String = System.String;

namespace ParserTests
{
    
    [Lexer(IgnoreWS = true, IgnoreEOL = true)]
    public enum DoNotIgnoreCommentsTokenWithChannels
    {
        [MultiLineComment("/*","*/")]
        MULTILINECOMMENT = 1,
        
        [SingleLineComment("//")]
        SINGLELINECOMMENT = 2,

        [Lexeme(GenericToken.Identifier, IdentifierType.AlphaNumeric)]
        ID = 3,
        
        [Lexeme(GenericToken.Double, channel:101)]
        DOUBLE = 4
    }

    
    [Lexer(IgnoreWS = true, IgnoreEOL = true)]
    public enum DoNotIgnoreCommentsToken
    {
        [MultiLineComment("/*","*/",true,channel:0)]
        COMMENT = 1,

        [Lexeme(GenericToken.Identifier, IdentifierType.AlphaNumeric)]
        ID = 2
    }


    public interface DoNotIgnore
    {
        
    }

    public class IdentifierList : DoNotIgnore
    {

        public List<DoNotIgnoreCommentIdentifier> Ids;
        public IdentifierList(List<DoNotIgnore> ids)
        {
            Ids = ids.Cast<DoNotIgnoreCommentIdentifier>().ToList();
        }
    }
    
    public class DoNotIgnoreCommentIdentifier : DoNotIgnore
    {
        public string Name;

        public string Comment;

        public bool IsCommented => !string.IsNullOrEmpty(Comment);

        public DoNotIgnoreCommentIdentifier(string name)
        {
            Name = name;
        }

        public DoNotIgnoreCommentIdentifier(string name, string comment) : this(name)
        {
            Comment = comment;
        }
    }
    
    public class DoNotIgnoreCommentsParser
    {
        [Production("main : id *")]
        public DoNotIgnore Main(List<DoNotIgnore> ids)
        {
            return new IdentifierList(ids);
        }
        
        [Production("id : ID")]
        public DoNotIgnore SimpleId(Token<DoNotIgnoreCommentsToken> token)
        {
            return new DoNotIgnoreCommentIdentifier(token.Value);
        } 
        
        [Production("id : COMMENT ID")]
        public DoNotIgnore CommentedId(Token<DoNotIgnoreCommentsToken> comment, Token<DoNotIgnoreCommentsToken> token)
        {
            return new DoNotIgnoreCommentIdentifier(token.Value, comment.Value);
        } 
    }
    
    public class DoNotIgnoreCommentsWithChannelsParser
    {
        [Production("main : id *")]
        public DoNotIgnore Main(List<DoNotIgnore> ids)
        {
            return new IdentifierList(ids);
        }

        [Production("id : ID")]
        public DoNotIgnore SimpleId(Token<DoNotIgnoreCommentsTokenWithChannels> token)
        {
            // get previous tokens in channel 2 (COMMENT)
            var commentTokens = token.PreviousTokens(Channels.Comments);
            
            string comment = "";
            // previous tokens may not be a comment so we have to check if not null
            if (commentTokens.Any())
            {
                var previousComments = commentTokens
                    .Where(x => x.TokenID == DoNotIgnoreCommentsTokenWithChannels.MULTILINECOMMENT ||
                                x.TokenID == DoNotIgnoreCommentsTokenWithChannels.SINGLELINECOMMENT)
                    .Reverse()
                    .Select(x => x.Value.Trim());
                
                if (previousComments.Any())
                {
                    comment += string.Join("\n", previousComments);
                }
            }
            
            commentTokens = token.NextTokens(Channels.Comments);
           
            
            // previous tokens may not be a comment so we have to check if not null
            if (commentTokens.Any())
            {
                var nextComments = commentTokens
                    .Where(x => x.TokenID == DoNotIgnoreCommentsTokenWithChannels.MULTILINECOMMENT ||
                                x.TokenID == DoNotIgnoreCommentsTokenWithChannels.SINGLELINECOMMENT)
                    .Select(x => x.Value.Trim());
                if (nextComments.Any())
                {
                    comment += (string.IsNullOrEmpty(comment) ? "" : "\n")+ string.Join("\n", nextComments);
                }
            }
            
            return new DoNotIgnoreCommentIdentifier(token.Value, comment);
        }

    }
    
    public static class ListExtensions
    {
        public static bool ContainsAll<IN>(this IEnumerable<IN> list1, IEnumerable<IN> list2)
        {
            return list1.Intersect(list2).Count() == list1.Count();
        }
    }

    public enum OptionTestToken
    {
        [Lexeme("a")] a = 1,
        [Lexeme("b")] b = 2,
        [Lexeme("c")] c = 3,
        [Lexeme("e")] e = 4,
        [Lexeme("f")] f = 5,

        [Lexeme("[ \\t]+", true)] WS = 100,
        [Lexeme("\\n\\r]+", true, true)] EOF = 101
    }

    public enum GroupTestToken
    {
        [Lexeme("a")] A = 1,
        [Lexeme(",")] COMMA = 2,
        [Lexeme(";")] SEMICOLON = 3,


        [Lexeme("[ \\t]+", true)] WS = 100,
        [Lexeme("\\n\\r]+", true, true)] EOL = 101,
        EOF = 0
    }

    public class OptionTestParser
    {
        [Production("root2 : a B? c ")]
        public string Root2(Token<OptionTestToken> a, ValueOption<string> b, Token<OptionTestToken> c)
        {
            var r = new StringBuilder();
            r.Append("R(");
            r.Append(a.Value);
            r.Append(b.Match(v => $",{v}", () => ",<none>"));
            r.Append($",{c.Value}");
            r.Append(")");
            return r.ToString();
        }

        [Production("root3 : a B c? ")]
        public string root(Token<OptionTestToken> a, string b, Token<OptionTestToken> c)
        {
            var r = $"R({a.StringWithoutQuotes},{b}";
            if (c.IsEmpty)
                r = $"{r},<none>)";
            else
                r = $"{r},{c.Value})";
            return r;
        }

        [Production("root : a b? c ")]
        public string root(Token<OptionTestToken> a, Token<OptionTestToken> b, Token<OptionTestToken> c)
        {
            var result = new StringBuilder();
            result.Append("R(");
            result.Append(a.StringWithoutQuotes);
            result.Append(",");
            if (b.IsEmpty)
                result.Append("<none>");
            else
                result.Append(b.StringWithoutQuotes);
            result.Append(",");
            result.Append(c.StringWithoutQuotes);
            result.Append(")");

            return result.ToString();
        }


        [Production("B : b ")]
        public string bee(Token<OptionTestToken> b)
        {
            return $"B({b.Value})";
        }
    }

    public class GroupTestParser
    {
        [Production("rootGroup : A ( COMMA A ) ")]
        public string root(Token<GroupTestToken> a, Group<GroupTestToken, string> group)
        {
            var r = new StringBuilder();
            r.Append("R(");
            r.Append(a.Value);
            r.Append("; {");
            group.Items.ForEach(item =>
            {
                r.Append(",");
                r.Append(item.IsValue ? item.Value : item.Token.Value);
            });
            r.Append("}");
            r.Append(")");
            return r.ToString();
        }
        
        [Production("rootGroupChoice : A ( [SEMICOLON | COMMA ] A ) ")]
        public string rootGroupChoice(Token<GroupTestToken> a, Group<GroupTestToken, string> group)
        {
            var r = new StringBuilder();
            r.Append("R(");
            r.Append(a.Value);
            r.Append(",").Append(group.Token(1).Value).Append(")");
            return r.ToString();
        }
        [Production("rootGroupChoiceMany : A ( [SEMICOLON | COMMA ] A )* ")]
        public string rootGroupChoiceMany(Token<GroupTestToken> a, List<Group<GroupTestToken, string>> groups)
        {
            var r = new StringBuilder();
            r.Append("R(");
            r.Append(a.Value);
            groups.ForEach(group =>
            {
                r.Append(",").Append(group.Token(1).Value);
            });
            r.Append(")");
            return r.ToString();
        }

        [Production("rootMany : A ( COMMA [d] A )* ")]
        public string rootMany(Token<GroupTestToken> a, List<Group<GroupTestToken, string>> groups)
        {
            var r = new StringBuilder();
            r.Append("R(");
            r.Append(a.Value);
            groups.ForEach(group =>
            {
                group.Items.ForEach(item =>
                {
                    r.Append(",");
                    r.Append(item.Match(
                        (name, token) => token.Value,
                        (name, val) => val)
                    );
                });
            });
            r.Append(")");
            return r.ToString();
        }

        [Production("rootOption : A ( SEMICOLON [d] A )? ")]
        public string rootOption(Token<GroupTestToken> a, ValueOption<Group<GroupTestToken, string>> option)
        {
            var builder = new StringBuilder();
            builder.Append("R(");
            builder.Append(a.Value);
            option.Match(
                group =>
                {
                    var aToken = group.Token("A").Value;
                    builder.Append($";{aToken}");
                    return null;
                },
                () =>
                {
                    builder.Append($";<none>");
                    return null;
                });
            builder.Append(")");
            return builder.ToString();
        }

        [Production("root3 : A ( COMMA [d] A )* ")]
        public string root3(Token<GroupTestToken> a, List<Group<GroupTestToken, string>> groups)
        {
            var r = new StringBuilder();
            r.Append("R(");
            r.Append(a.Value);
            groups.ForEach(group =>
            {
                group.Items.ForEach(item =>
                {
                    r.Append(",");
                    r.Append(item.Value);
                });
            });
            return r.ToString();
        }
    }


    public class Bugfix100Test
    {
        [Production("testNonTerm : sub* COMMA ")]
        public int TestNonTerminal(List<int> options, Token<GroupTestToken> token)
        {
            return 1;
        }

        [Production("sub : A")]
        public int sub(Token<GroupTestToken> token)
        {
            return 1;
        }

        [Production("testTerm : A* COMMA")]
        public int TestTerminal(List<Token<GroupTestToken>> options, Token<GroupTestToken> token)
        {
            return 1;
        }
    }


    public class AlternateChoiceTestTerminal
    {
        [Production("choice : [ a | b | c]")]
        public string Choice(Token<OptionTestToken> c)
        {
            return c.Value;
        }
    }
    
    public class AlternateChoiceTestZeroOrMoreTerminal
    {
        [Production("choice : [ a | b | c]*")]
        public string Choice(List<Token<OptionTestToken>> list)
        {
            return string.Join(",",list.Select(x => x.Value));
        }
    }

    public class AlternateChoiceTestOneOrMoreTerminal
    {
        [Production("choice : [ a | b | c]+")]
        public string Choice(List<Token<OptionTestToken>> list)
        {
            return string.Join(",", list.Select(x => x.Value));
        }
    }

    public class AlternateChoiceTestOptionTerminal
    {
        [Production("choice : [ a | b | c] [ b | c]?")]
        public string Choice(Token<OptionTestToken> first, Token<OptionTestToken> next)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(first.Value);

            if (next.IsEmpty)
            {
                builder.Append($",<none>");
            }
            else
            {
                builder.Append($",{next.Value}");
            }
            
            return builder.ToString();
        }
    }
    
    public class AlternateChoiceTestOptionDiscardedTerminal
    {
        [Production("choice : [ a | b | c] [ b | c] [d]")]
        public string Choice(Token<OptionTestToken> first)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(first.Value);

            
            return builder.ToString();
        }
    }

    
    public class LeftRecWithChoiceInGroup
    {
        [Production("leftrec: ([leftrec | right])")]
        public string leftrec(Group<OptionTestToken, string> opt)
        {
            return "";
        }

        [Production("right : a")]
        public string right(Token<OptionTestToken> a)
        {
            return a.Value;
        }
    }
    
    public class AlternateChoiceInGroupTestError
    {

        [Production("choiceInGroup : a ([ a | b | C | D] a)")]
        public string ChoiceInGroup(Token<OptionTestToken> c, Group<OptionTestToken, string> opt)
        {
            return c.Value;
        }
    }

    public class AlternateChoiceTestError
    {
        [Production("choice : [ a | b | C | D]")]
        public string Choice(Token<OptionTestToken> c)
        {
            return "choice";
        }
        [Production("D : [ E | C] [d]")]
        public string D()
        {
            return "nothing here";
        }
        
        [Production("E : e")]
        public string E(Token<OptionTestToken> e)
        {
            return e.Value;
        }

        [Production("C : c")]
        public string C(Token<OptionTestToken> c)
        {
            return c.Value;
        }
        
    }
    
    public class AlternateChoiceTestNonTerminal
    {
        [Production("choice : [ A | B | C]")]
        public string Choice(string c)
        {
            return c;
        }

        [Production("C : c")]
        public string C(Token<OptionTestToken> t)
        {
            return $"C({t.Value})";
        }
        
        [Production("B : b")]
        public string B(Token<OptionTestToken> t)
        {
            return $"B({t.Value})";
        }
        
        [Production("A : a")]
        public string A(Token<OptionTestToken> t)
        {
            return $"A({t.Value})";
        }
        
    }

    public class AlternateChoiceTestOneOrMoreNonTerminal
    {
        [Production("choice : [ A | B | C]+")]
        public string Choice(List<String> choices)
        {
            return string.Join(" ", choices);
        }

        [Production("C : c")]
        public string C(Token<OptionTestToken> t)
        {
            return $"C({t.Value})";
        }

        [Production("B : b")]
        public string B(Token<OptionTestToken> t)
        {
            return $"B({t.Value})";
        }

        [Production("A : a")]
        public string A(Token<OptionTestToken> t)
        {
            return $"A({t.Value})";
        }

    }

    public class Bugfix104Test
    {
        [Production("testNonTerm : sub (COMMA[d] unreachable)? ")]
        public int TestNonTerminal(int sub, ValueOption<Group<GroupTestToken,int>> group)
        {
            return 1;
        }

        [Production("sub : A")]
        public int Sub(Token<GroupTestToken> token)
        {
            return 1;
        }


        [Production("unreachable : A")]
        public int Unreachable(Token<GroupTestToken> token)
        {
            return 1;
        }
    }

    [Lexer]
    public enum Issue190Token
    {
        EOF = 0,
        
        [Lexeme(GenericToken.Identifier,IdentifierType.Alpha)]
        ID = 1,
        
        [Lexeme(GenericToken.KeyWord,"not")]
        NOT = 2,
        
        [Lexeme(GenericToken.KeyWord,"true")]
        TRUE = 3,
        
        [Lexeme(GenericToken.KeyWord,"false")]
        FALSE = 4,
        
        [Lexeme(GenericToken.KeyWord,"yes")]
        YES = 5,
        
        [Lexeme(GenericToken.KeyWord,"no")]
        NO = 6
        
    }

    public class Issue190parser
    {
        [Production("root: NOT? [TRUE | FALSE | YES | NO]")]
        public bool BooleanValue(Token<Issue190Token> notToken, Token<Issue190Token> valueToken)
        {
            bool value = valueToken.TokenID == Issue190Token.YES || valueToken.TokenID == Issue190Token.TRUE;
            if (!notToken.IsEmpty)
            {
                value = !value;
            }

            return value;
        }

    }

    public class EBNFTests
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
            var result = "R(";
            result += gs
                .Select(g => g.ToString())
                .Aggregate((s1, s2) => s1 + "," + s2);
            result += ")";
            return result;
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

       


        private Parser<TokenType, string> Parser;

        private BuildResult<Parser<TokenType, string>> BuildParser()
        {
            var parserInstance = new EBNFTests();
            var builder = new ParserBuilder<TokenType, string>();
            var result = builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, "R");
            return result;
        }


        private BuildResult<Parser<JsonToken, JSon>> BuildEbnfJsonParser()
        {
            var parserInstance = new EbnfJsonParser();
            var builder = new ParserBuilder<JsonToken, JSon>();

            var result =
                builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, "root");
            return result;
        }

        private BuildResult<Parser<OptionTestToken, string>> BuildOptionParser()
        {
            var parserInstance = new OptionTestParser();
            var builder = new ParserBuilder<OptionTestToken, string>();

            var result =
                builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, "root");
            return result;
        }

        private BuildResult<Parser<GroupTestToken, string>> BuildGroupParser()
        {
            var parserInstance = new GroupTestParser();
            var builder = new ParserBuilder<GroupTestToken, string>();

            var result =
                builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, "rootGroup");
            return result;
        }

        

        [Fact]
        public void TestBuildGroupParser()
        {
            var buildResult = BuildGroupParser();
            Check.That(buildResult.IsError).IsFalse();
        }

        [Fact]
        public void TestEmptyOptionalNonTerminal()
        {
            var buildResult = BuildOptionParser();
            Check.That(buildResult.IsError).IsFalse();
            var optionParser = buildResult.Result;

            var result = optionParser.Parse("a c", "root2");
            Check.That(result.IsError).IsFalse();
            Check.That(result.Result).IsEqualTo("R(a,<none>,c)");
        }

        [Fact]
        public void TestEmptyOptionTerminalInMiddle()
        {
            var buildResult = BuildOptionParser();
            Check.That(buildResult.IsError).IsFalse();
            var optionParser = buildResult.Result;

            var result = optionParser.Parse("a c", "root2");
            Check.That(result.IsError).IsFalse();
            Check.That(result.Result).IsEqualTo("R(a,<none>,c)");
        }


        [Fact]
        public void TestEmptyTerminalOption()
        {
            var buildResult = BuildOptionParser();
            Check.That(buildResult.IsError).IsFalse();
            var optionParser = buildResult.Result;

            var result = optionParser.Parse("a b", "root3");
            Check.That(result.IsError).IsFalse();
            Check.That(result.Result).IsEqualTo("R(a,B(b),<none>)");
        }

        [Fact]
        public void TestErrorMissingClosingBracket()
        {
            var jsonParser = new EbnfJsonGenericParser();
            var builder = new ParserBuilder<JsonTokenGeneric, JSon>();
            var build = builder.BuildParser(jsonParser, ParserType.EBNF_LL_RECURSIVE_DESCENT, "root");
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

            Check.That(r.IsError).IsTrue();
        }

        [Fact]
        public void TestGroupSyntaxManyParser()
        {
            var buildResult = BuildGroupParser();
            Check.That(buildResult.IsError).IsFalse();
            var groupParser = buildResult.Result;
            var res = groupParser.Parse("a ,a , a ,a,a", "rootMany");
            Check.That(res.IsError).IsFalse();
            Check.That(res.Result).IsEqualTo("R(a,a,a,a,a)");
        }
        
        [Fact]
        public void TestGroupSyntaxChoicesParser()
        {
            var buildResult = BuildGroupParser();
            Check.That(buildResult.IsError).IsFalse();
            var groupParser = buildResult.Result;
            var res = groupParser.Parse("a ;a ", "rootGroupChoice");

            Check.That(res.IsError).IsFalse();
            Check.That(res.Result).IsEqualTo("R(a,a)"); 
            
            res = groupParser.Parse("a ,a ", "rootGroupChoice");

            Check.That(res.IsError).IsFalse();
            Check.That(res.Result).IsEqualTo("R(a,a)");
        }
        
        [Fact]
        public void TestGroupSyntaxChoicesManyParser()
        {
            var buildResult = BuildGroupParser();
            Check.That(buildResult.IsError).IsFalse();
            var groupParser = buildResult.Result;
            var res = groupParser.Parse("a ;a,a  ; a,a ", "rootGroupChoiceMany");
            Check.That(res.IsError).IsFalse();
            Check.That(res.Result).IsEqualTo("R(a,a,a,a,a)"); // rootMany
        }

        [Fact]
        public void TestGroupSyntaxOptionIsSome()
        {
            var buildResult = BuildGroupParser();
            Check.That(buildResult.IsError).IsFalse();
            var groupParser = buildResult.Result;
            var res = groupParser.Parse("a ; a ", "rootOption");
            Check.That(res.IsError).IsFalse();
            Check.That(res.Result).IsEqualTo("R(a;a)");
        }

        [Fact]
        public void TestGroupSyntaxOptionIsNone()
        {
            var buildResult = BuildGroupParser();
            Check.That(buildResult.IsError).IsFalse();
            var groupParser = buildResult.Result;
            var res = groupParser.Parse("a ", "rootOption");
            Check.That(res.IsError).IsFalse();
            Check.That(res.Result).IsEqualTo("R(a;<none>)");
        }

        [Fact]
        public void TestGroupSyntaxParser()
        {
            var buildResult = BuildGroupParser();
            Check.That(buildResult.IsError).IsFalse();
            var groupParser = buildResult.Result;
            var res = groupParser.Parse("a ,a");

            Check.That(res.IsError).IsFalse();
            Check.That(res.Result).IsEqualTo("R(a; {,,,a})");
        }


        [Fact]
        public void TestJsonList()
        {
            var buildResult = BuildEbnfJsonParser();
            Check.That(buildResult.IsError).IsFalse();
            var jsonParser = buildResult.Result;

            var result = jsonParser.Parse("[1,2,3,4]");
            Check.That(result.IsError).IsFalse();
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
            Check.That(buildResult.IsError).IsFalse();
            var jsonParser = buildResult.Result;
            var result = jsonParser.Parse("{\"one\":1,\"two\":2,\"three\":\"trois\" }");
            Check.That(result.IsError).IsFalse();
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
            Check.That(buildResult.IsError).IsFalse();
            var optionParser = buildResult.Result;

            var result = optionParser.Parse("a b c", "root2");
            Check.That(result.IsError).IsFalse();
            Check.That(result.Result).IsEqualTo("R(a,B(b),c)");
        }


        [Fact]
        public void TestNonEmptyTerminalOption()
        {
            var buildResult = BuildOptionParser();
            Check.That(buildResult.IsError).IsFalse();
            var optionParser = buildResult.Result;

            var result = optionParser.Parse("a b c", "root");
            Check.That(result.IsError).IsFalse();
            Check.That(result.Result).IsEqualTo("R(a,b,c)");
        }


        [Fact]
        public void TestOneOrMoreNonTerminal()
        {
            var buildResult = BuildParser();
            Check.That(buildResult.IsError).IsFalse();
            Parser = buildResult.Result;
            var result = Parser.Parse("e f e f");
            Check.That(result.IsError).IsFalse();
            Check.That(result.Result).IsEqualTo("R(G(e,f),G(e,f))");
        }


        [Fact]
        public void TestOneOrMoreWithMany()
        {
            var buildResult = BuildParser();
            Check.That(buildResult.IsError).IsFalse();
            Parser = buildResult.Result;
            var result = Parser.Parse("aaa b c");
            Check.That(result.IsError).IsFalse();
            Check.That(result.Result).IsEqualTo("R(A(a, a, a),B(b),c)");
        }

        [Fact]
        public void TestOneOrMoreWithOne()
        {
            var buildResult = BuildParser();
            Check.That(buildResult.IsError).IsFalse();
            Parser = buildResult.Result;
            var result = Parser.Parse(" b c");
            Check.That(result.IsError).IsTrue();
            
        }

        [Fact]
        public void TestParseBuild()
        {
            var buildResult = BuildParser();
            Check.That(buildResult.IsError).IsFalse();
            Parser = buildResult.Result;
            Check.That(Parser.SyntaxParser).IsInstanceOf<EBNFRecursiveDescentSyntaxParser<TokenType, string>>();
            Check.That(Parser.Configuration.NonTerminals).CountIs(4);
            
            var nt = Parser.Configuration.NonTerminals["R"];
            Check.That(nt.Rules).CountIs(2);
            nt = Parser.Configuration.NonTerminals["A"];
            Check.That(nt.Rules).CountIs(1);
            var rule = nt.Rules[0];
            Check.That(rule.Clauses).CountIs(1);
            Check.That(rule.Clauses[0]).IsInstanceOf<OneOrMoreClause<TokenType>>();
            nt = Parser.Configuration.NonTerminals["B"];
            Check.That((nt.Rules)).CountIs(1);
            rule = nt.Rules[0];
            Check.That((rule.Clauses)).CountIs(1);
            Check.That(rule.Clauses[0]).IsInstanceOf<ZeroOrMoreClause<TokenType>>();
            
        }

        [Fact]
        public void TestZeroOrMoreWithMany()
        {
            var buildResult = BuildParser();
            Check.That(buildResult.IsError).IsFalse();
            Parser = buildResult.Result;
            var result = Parser.Parse("a bb c");
            Check.That(result.IsError).IsFalse();
            Check.That(result.Result).IsEqualTo("R(A(a),B(b, b),c)");            
        }

        [Fact]
        public void TestZeroOrMoreWithNone()
        {
            var buildResult = BuildParser();
            Check.That(buildResult.IsError).IsFalse();
            Parser = buildResult.Result;
            var result = Parser.Parse("a  c");
            Check.That(result.IsError).IsFalse();
            Check.That(result.Result).IsEqualTo("R(A(a),B(),c)");
        }

        [Fact]
        public void TestZeroOrMoreWithOne()
        {
            var buildResult = BuildParser();
            Check.That(buildResult.IsError).IsFalse();
            Parser = buildResult.Result;
            var result = Parser.Parse("a b c");
            Check.That(result.IsError).IsFalse();
            Check.That(result.Result).IsEqualTo("R(A(a),B(b),c)");
        }


        #region CONTEXTS

        private BuildResult<Parser<ExpressionToken, int>> buildSimpleExpressionParserWithContext(ParserType parserType = ParserType.EBNF_LL_RECURSIVE_DESCENT)
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

            Check.That(buildResult.IsError).IsFalse();
            var parser = buildResult.Result;
            var res = parser.ParseWithContext("2 + a", new Dictionary<string, int> {{"a", 2}});
            Check.That(res.IsOk).IsTrue();
            Check.That(res.Result).IsEqualTo(4);
        }

        [Fact]
        public void TestContextualParsing2()
        {
            var buildResult = buildSimpleExpressionParserWithContext();

            Check.That(buildResult.IsError).IsFalse();
            var parser = buildResult.Result;
            var res = parser.ParseWithContext("2 + a * b", new Dictionary<string, int> {{"a", 2}, {"b", 3}});
            Check.That(res.IsOk).IsTrue();
            Check.That(res.Result).IsEqualTo(8);
        }

        [Fact]
        public void TestContextualParsingWithEbnf()
        {
            var buildResult = buildSimpleExpressionParserWithContext(ParserType.EBNF_LL_RECURSIVE_DESCENT);

            Check.That(buildResult.IsError).IsFalse();
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
            var builtParser = builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, startingRule);
            Check.That(builtParser.IsError).IsFalse();
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
            var builtParser = builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, startingRule);
            Check.That(builtParser.IsError).IsFalse();
            Check.That(builtParser.Errors).IsEmpty();
        }

        [Fact]
        public void TestAlternateChoiceTerminal()
        {
            var startingRule = $"choice";
            var parserInstance = new AlternateChoiceTestTerminal();
            var builder = new ParserBuilder<OptionTestToken, string>();
            var builtParser = builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, startingRule);
            Check.That(builtParser.IsError).IsFalse();
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
            var builtParser = builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, startingRule);
            Check.That(builtParser.IsError).IsFalse();
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
            var builtParser = builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, startingRule);
            Check.That(builtParser.IsError).IsFalse();
            Check.That(builtParser.Errors).IsEmpty();
            var parseResult = builtParser.Result.Parse("a b", "choice");
            Check.That(parseResult.IsOk).IsTrue();
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
            var builtParser = builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, startingRule);
            Check.That(builtParser.IsError).IsFalse();
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
            var builtParser = builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, startingRule);
            Check.That(builtParser.IsError).IsFalse();
            Check.That(builtParser.Errors).IsEmpty();
            var parseResult = builtParser.Result.Parse("a b c", "choice");
            Check.That(parseResult.IsOk).IsTrue();
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
            var builtParser = builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, startingRule);
            Check.That(builtParser.IsError).IsFalse();
            Check.That(builtParser.Errors).IsEmpty();
            var parseResult = builtParser.Result.Parse("a b", "choice");
            Check.That(parseResult.IsOk).IsTrue();
            Check.That(parseResult.Result).IsEqualTo("a,b");
            parseResult = builtParser.Result.Parse("a", "choice");
            Check.That(parseResult.IsOk).IsTrue();
            Check.That(parseResult.Result).IsEqualTo("a,<none>");
        }
        
        [Fact]
        public void TestAlternateChoiceOptionDiscardedTerminal()
        {
            var startingRule = $"choice";
            var parserInstance = new AlternateChoiceTestOptionDiscardedTerminal();
            var builder = new ParserBuilder<OptionTestToken, string>();
            var builtParser = builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, startingRule);
            Check.That(builtParser.IsError).IsFalse();
            Check.That(builtParser.Errors).IsEmpty();
            var parseResult = builtParser.Result.Parse("a b", "choice");
            Check.That(parseResult.IsOk).IsTrue();
            Check.That(parseResult.Result).IsEqualTo("a");
            parseResult = builtParser.Result.Parse("a", "choice");
            Check.That(parseResult.IsError).IsTrue();
            Check.That(parseResult.Errors).CountIs(1);
            Check.That(parseResult.Errors[0].ErrorType).IsEqualTo(ErrorType.UnexpectedEOS);
        }

        [Fact]
        public void TestAlternateChoiceErrorMixedTerminalAndNonTerminal()
        {
            var startingRule = $"choice";
            var parserInstance = new AlternateChoiceTestError();
            var builder = new ParserBuilder<OptionTestToken, string>();
            var builtParser = builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, startingRule);
            Check.That(builtParser.IsError).IsTrue();
            Check.That(builtParser.Errors).CountIs(2);
            Check.That(builtParser.Errors.Select(x => x.Code)).Contains(ErrorCodes.PARSER_MIXED_CHOICES,
                ErrorCodes.PARSER_NON_TERMINAL_CHOICE_CANNOT_BE_DISCARDED);
        }
        
        
        
        [Fact]
        public void TestAlternateChoiceInGroupLeftRecursion()
        {
            var startingRule = $"choiceInGroup";
            var parserInstance = new LeftRecWithChoiceInGroup();
            var builder = new ParserBuilder<OptionTestToken, string>();
            var builtParser = builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, startingRule);
            Check.That(builtParser.IsError).IsTrue();
            Check.That(builtParser.Errors).CountIs(1);
            Check.That(builtParser.Errors.First().Code).IsEqualTo(ErrorCodes.PARSER_LEFT_RECURSIVE);
        }


        [Fact]
        public void TestIssue190()
        {
            var startingRule = $"root";
            var parserInstance = new Issue190parser();
            var builder = new ParserBuilder<Issue190Token, bool>();
            var builtParser = builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, startingRule);
            Check.That(builtParser.IsError).IsFalse();
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
            var builtParser = BuildParser();
            Check.That(builtParser.IsError).IsFalse();
            Check.That(builtParser.Result).IsNotNull();
            var parser = builtParser.Result;

            var test = parser.Parse("a b");

            Check.That(test.IsError).IsTrue();
            Check.That(test.Errors).CountIs(1);
            var error = test.Errors[0] as UnexpectedTokenSyntaxError<TokenType>;
            Check.That(error).IsNotNull();
            Check.That(error.UnexpectedToken.IsEOS).IsTrue();
        }
        
        [Fact]
        public void TestIssue213()
        {
            var parserInstance = new DoNotIgnoreCommentsParser();
            var builder = new ParserBuilder<DoNotIgnoreCommentsToken, DoNotIgnore>();
            var builtParser = builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, "main");

            Check.That(builtParser.IsOk).IsTrue();
            Check.That(builtParser.Result).IsNotNull();
            
            var parser = builtParser.Result;

            var test = parser.Parse("a /*commented b*/b");
            Check.That(test.IsOk).IsTrue();
            Check.That(test.Result).IsNotNull();
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
            var parserRes = builder.BuildParser(instance, ParserType.EBNF_LL_RECURSIVE_DESCENT, "root");
            Check.That(parserRes.IsOk).IsTrue();
            
            var parser = parserRes.Result;
            Check.That(parser).IsNotNull();
            var parseResult = parser.Parse(source);
            Check.That(parseResult.IsOk).IsTrue();
            
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
            var parserRes = builder.BuildParser(instance, ParserType.EBNF_LL_RECURSIVE_DESCENT, "root");
            Check.That(parserRes.IsOk).IsTrue();
            var parser = parserRes.Result;
            Check.That(parser).IsNotNull();
            var parseResult = parser.Parse(source);
            Check.That(parseResult.IsOk).IsTrue();
            
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
            var parserRes = builder.BuildParser(instance, ParserType.EBNF_LL_RECURSIVE_DESCENT, "root");
            Check.That(parserRes.IsOk).IsTrue();
            var parser = parserRes.Result;
            Check.That(parser).IsNotNull();
            var parseResult = parser.Parse(source);
            Check.That(parseResult.IsOk).IsTrue();
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
            var parserRes = builder.BuildParser(instance, ParserType.EBNF_LL_RECURSIVE_DESCENT, "root");
            Check.That(parserRes.IsOk).IsTrue();
            var parser = parserRes.Result;
            Check.That(parser).IsNotNull();
            
            var parseResult = parser.Parse(source);
            Check.That(parseResult.IsOk).IsTrue();
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
            var builtParser = builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, "main");
            
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

            Check.That(test.IsOk).IsTrue();
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
            var parserRes = builder.BuildParser(instance, ParserType.EBNF_LL_RECURSIVE_DESCENT, "root");
            Check.That(parserRes.IsOk).IsTrue();
            
            var parser = parserRes.Result;
            Check.That(parser).IsNotNull();
            var parseResult = parser.Parse(source);
            Check.That(parseResult.IsOk).Not.IsTrue();
            Check.That(parseResult.Errors).CountIs(1);
            var error = parseResult.Errors[0];
            Check.That(error.ErrorType).IsEqualTo(ErrorType.UnexpectedEOS);
        }
    }
}
