

using System.Collections.Generic;
using NFluent;
using ParserTests.lexer.genericlexers;
using sly.buildresult;
using sly.lexer;
using sly.parser.generator;
using XML;
using Xunit;

namespace ParserTests.lexer
{
    public class LexerModesTest
    {

        [Fact]
        public static void TestModesAndComments()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<ModesAndCommentsLexer>>());
            Check.That(lexerRes.IsError).IsFalse();
            var result = lexerRes.Result.Tokenize(@"
hello
# comment
>>>
toto
<<<
world");
            Check.That(result.IsOk).IsTrue();
            var expectedTokens = new List<ModesAndCommentsLexer>()
            {
                ModesAndCommentsLexer.ID,
                ModesAndCommentsLexer.IN,
                ModesAndCommentsLexer.TOTO,
                ModesAndCommentsLexer.OUT,
                ModesAndCommentsLexer.ID
            };
            var tokens = result.Tokens.GetChannel(Channels.Main).NotNullOrEosTokens;
            Check.That(expectedTokens).CountIs(tokens.Count);
            Check.That(tokens.Extracting("TokenID")).Contains(expectedTokens);
            
            expectedTokens = new List<ModesAndCommentsLexer>()
            {
                ModesAndCommentsLexer.SINGLELINE
            };
            tokens = result.Tokens.GetChannel(Channels.Comments).NotNullOrEosTokens;
            Check.That(expectedTokens).CountIs(tokens.Count);
            Check.That(tokens.Extracting("TokenID")).Contains(expectedTokens);

        }
        
        [Fact]
        public static void TestModesAndCommentFailure()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<ModesAndCommentsLexer>>());
            Check.That(lexerRes.IsError).IsFalse();
            var result = lexerRes.Result.Tokenize(@"
hello
# comment
>>>
# single line comments can not be in IN mode
toto
<<<
world");
            Check.That(result).Not.IsOkLexing();
            

        }
        
        [Fact]
        public static void TestModesAndCommentInManyMode()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<ModesAndCommentsLexer>>());
            Check.That(lexerRes.IsError).IsFalse();
            var result = lexerRes.Result.Tokenize(@"
hello
# comment
/* 
multi line comments can be in default mode 
/*
>>>
/* 
multi line comments can be in IN mode 
/*
toto
<<<
world");
            Check.That(result).IsOkLexing();
            

        }
        
        [Fact]
        public static void TestLexerModes()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<MinimalXmlLexer2>>());
            Check.That(lexerRes.IsError).IsFalse();
            var result = lexerRes.Result.Tokenize(@"hello
<tag attr=""value"">inner text</tag>
<!-- this is a comment -->
<? PI attr=""test""?>");
            Check.That(result.IsOk).IsTrue();
            var expectedTokens = new List<MinimalXmlLexer2>()
            {
                MinimalXmlLexer2.CONTENT,
                MinimalXmlLexer2.OPEN,
                MinimalXmlLexer2.ID,
                MinimalXmlLexer2.ID,
                MinimalXmlLexer2.EQUALS,
                MinimalXmlLexer2.VALUE,
                MinimalXmlLexer2.CLOSE,
                MinimalXmlLexer2.CONTENT,
                MinimalXmlLexer2.OPEN,
                MinimalXmlLexer2.SLASH,
                MinimalXmlLexer2.ID,
                MinimalXmlLexer2.CLOSE,
                MinimalXmlLexer2.COMMENT,
                MinimalXmlLexer2.OPEN_PI,
                MinimalXmlLexer2.ID,
                MinimalXmlLexer2.ID,
                MinimalXmlLexer2.EQUALS,
                MinimalXmlLexer2.VALUE,
                MinimalXmlLexer2.CLOSE_PI
            };
            var tokens = result.Tokens.MainTokens();
            Check.That(expectedTokens).CountIs(tokens.Count-1);

            Check.That(tokens.Extracting("TokenID")).Contains(expectedTokens);
            
        }

        [Fact]
        public void TestXmlParserWithLexerModes()
        {
            ParserBuilder<MinimalXmlLexer, string> builder = new ParserBuilder<MinimalXmlLexer, string>();
            var parser = new MinimalXmlParser();
            var r = builder.BuildParser(parser, ParserType.EBNF_LL_STACK, "document");
            Check.That(r.IsError).IsFalse();
            var pr = r.Result.Parse(@"
<?xml version=""1.0""?>
<!-- starting doc -->
<root name=""root"">
    <autoInner name=""autoinner1""/>
    <inner name=""inner"">
         <?PI name=""pi""?> 
        <innerinner name=""innerinner"">
            inner inner content
        </innerinner>
    </inner>                      
</root>
");
            Check.That(pr.IsOk).IsTrue();
            Check.That(pr.Result).IsNotNull();
            Check.That(pr.Result).IsNotEmpty();
            
        }
    }
}