using System;
using System.Collections.Generic;
using System.Linq;
using expressionparser;
using expressionparser.model;
using GenericLexerWithCallbacks;
using indented;
using NFluent;
using simpleExpressionParser;
using sly.buildresult;
using sly.i18n;
using sly.lexer;
using sly.parser;
using sly.parser.generator;
using Xunit;
using ExpressionToken = simpleExpressionParser.ExpressionToken;

namespace ParserTests.lexer
{

    public enum HexaTokenDefault
    {
        [Hexa]HEXA,
    }
    
      public enum HexaToken6x
        {
            [Hexa("6x")]HEXA,
            
            [Int] INT
        }
    
    public enum HexaTokenConflictId
    {
        EOS= 0,
        
        
        [Hexa("hexa")]HEXA,
        
        [AlphaId] ID
    }
    
    public enum HexaTokenConflictIdAndInt
    {
        [Hexa("0x")]HEXA,
        
        [AlphaId] ID,
        
        [Int] INT,
    }
    
    
    public enum DateTokenEnglishDashed
    {
        [Date(DateFormat.YYYYMMDD, '-')] DATE,
    }

    public enum DateTokenWithFrenchSlashed
    {
        [Date(DateFormat.DDMMYYYY, '/')] DATE,

        [Int] INT
    }

    public enum DateAndDoubleToken
    {
        [Double(".")] DOUBLE,

        [Date(DateFormat.DDMMYYYY, '.')] FRENCH_DATE,

        [Date(DateFormat.YYYYMMDD, '.')] ENGLISH_DATE,

    }

    public enum ManyDateToken
    {
        [Date(DateFormat.DDMMYYYY, '/')] FRENCH_DATE,

        [Date(DateFormat.YYYYMMDD, '-')] ENGLISH_DATE,
    }

    public enum DateAndExpressions
    {

        [Int] INT,

        [Sugar("-")] MINUS,

        [Date(DateFormat.YYYYMMDD, '-')] DATE,
    }

    public enum DoubleQuotedString
    {
        [Lexeme(GenericToken.String, "\"")] DoubleString
    }

    public enum SingleQuotedString
    {
        [Lexeme(GenericToken.String, "'")] SingleString
    }

    public enum DefaultQuotedString
    {
        [Lexeme(GenericToken.String)] DefaultString
    }

    public enum SelfEscapedString
    {
        [Lexeme(GenericToken.String, "'", "'")]
        STRING
    }

    public enum ManyString
    {
        [Lexeme(GenericToken.String, "'", "'")] [Lexeme(GenericToken.String)]
        STRING
    }

    public enum AlphaId
    {
        [Lexeme(GenericToken.Identifier, IdentifierType.Alpha)]
        ID
    }

    public enum AlphaNumId
    {
        [AlphaNumId] ID
    }

    public enum AlphaNumDashId
    {
        [Lexeme(GenericToken.Identifier, IdentifierType.AlphaNumericDash)]
        ID
    }

    public enum CustomId
    {
        EOS,

        [CustomId("A-Za-z", "-_0-9A-Za-z")]
        // [Lexeme(GenericToken.Identifier, IdentifierType.Custom, "A-Za-z", "-_0-9A-Za-z")]
        ID,

        [Lexeme(GenericToken.SugarToken, "-", "_")]
        OTHER
    }

    public enum CustomIdReverseRange
    {
        EOS,

        [CustomId("Z-Az-a", "-_9-0A-Za-z")] ID,

        [Lexeme(GenericToken.SugarToken, "-", "_")]
        OTHER
    }

    [Lexer(IgnoreWS = false)]
    public enum IgnoreWS
    {
        [Lexeme(GenericToken.SugarToken, " ")] WS
    }

    [Lexer(IgnoreEOL = false)]
    public enum IgnoreEOL
    {
        [Lexeme(GenericToken.SugarToken, "\n")]
        EOL
    }

    [Lexer(WhiteSpace = new[] { ' ' })]
    public enum WhiteSpace
    {
        [Lexeme(GenericToken.SugarToken, "\t")]
        TAB
    }

    public enum Empty
    {
        EOS,

        [Lexeme(GenericToken.Identifier)] ID
    }

    public enum KeyWord
    {
        [Lexeme(GenericToken.KeyWord, "keyword")]
        KEYWORD = 1
    }

    [Lexer(KeyWordIgnoreCase = true)]
    public enum KeyWordIgnoreCase
    {
        [Lexeme(GenericToken.KeyWord, "keyword")]
        KEYWORD = 1
    }

    public enum Issue106
    {
        [Lexeme(GenericToken.Int)] Integer = 5,

        [Lexeme(GenericToken.Double)] Double = 6,

        [Lexeme(GenericToken.SugarToken, ".")] Period
    }

    public enum Issue114
    {
        [Lexeme(GenericToken.SugarToken, "//")]
        First = 1,

        [Lexeme(GenericToken.SugarToken, "/*")]
        Second = 2
    }

    // Test that the FSMLexer properly backtracks.
    public enum Issue137
    {
        [Lexeme(GenericToken.SugarToken, ".")] A = 1,
        [Lexeme(GenericToken.SugarToken, "-")] B,

        [Lexeme(GenericToken.SugarToken, "-+")]
        C,

        [Lexeme(GenericToken.SugarToken, "---")]
        E
    }

    // Test that the FSMLexer properly terminates, without skipping tokens.
    public enum Issue138
    {
        [Lexeme(GenericToken.SugarToken, "..")]
        A = 1,
        [Lexeme(GenericToken.SugarToken, "-")] B,

        [Lexeme(GenericToken.SugarToken, "---")]
        C
    }

    [Lexer(IgnoreEOL = true)]
    public enum Issue177Generic
    {

        [Lexeme(GenericToken.Int)] INT = 2,

        EOS = 0

    }

    public enum Issue348
    {
        [Lexeme(GenericToken.String, "\"", "\\")]
        STriNG
    }

    public enum Issue348Bis
    {
        [Lexeme(GenericToken.String, "\"", "^")]
        STriNG
    }

    [Lexer]
    public enum Issue186MixedGenericAndRegexLexer
    {
        [Lexeme(GenericToken.Identifier, IdentifierType.Alpha)]
        ID = 1,

        [Lexeme("[0-9]+")] INT = 2
    }

    public class Issue186MixedGenericAndRegexParser
    {
        [Production("root : INT")]
        public object root(Token<Issue186MixedGenericAndRegexLexer> integer)
        {
            return null;
        }
    }

    [Lexer(IgnoreEOL = false)]
    public enum Issue177Regex
    {
        [Lexeme("\r\n", IsLineEnding = true)] EOL = 1,

        [Lexeme("\\d+")] INT = 2,

        EOS = 0

    }



    public class GenericLexerTests
    {

        public GenericLexerTests()
        {
        }

        [Fact]
        public void TestEmptyInput()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<Empty>>());
            Check.That(lexerRes.IsError).IsFalse();
            var lexer = lexerRes.Result;
            var r = lexer.Tokenize("");
            Check.That(r.IsOk).IsTrue();
            Check.That(r.Tokens).IsSingle();
            var tok = r.Tokens[0];
            Check.That(tok.TokenID).IsEqualTo(Empty.EOS);
            Check.That(tok.Value).IsEqualTo("");
        }

        [Fact]
        public void TestIgnoredInput()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<Empty>>());
            Check.That(lexerRes.IsError).IsFalse();
            var lexer = lexerRes.Result;
            var r = lexer.Tokenize(" \t\n ");
            Check.That(r.IsOk).IsTrue();
            Check.That(r.Tokens).IsSingle();
            var tok = r.Tokens[0];
            Check.That(tok.TokenID).IsEqualTo(Empty.EOS);
            Check.That(tok.Value).IsEqualTo("");
        }

        [Fact]
        public void TestAlphaId()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<AlphaId>>());
            Check.That(lexerRes.IsError).IsFalse();
            var lexer = lexerRes.Result;

            var r = lexer.Tokenize("alpha");
            Check.That(r.IsOk).IsTrue();
            Check.That(r.Tokens).CountIs(2);
            var tok = r.Tokens[0];
            Check.That(tok.TokenID).IsEqualTo(AlphaId.ID);
            Check.That(tok.StringWithoutQuotes).IsEqualTo("alpha");
        }

        [Fact]
        public void TestAlphaNumDashId()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<AlphaNumDashId>>());
            Check.That(lexerRes.IsError).IsFalse();
            var lexer = lexerRes.Result;
            var r = lexer.Tokenize("alpha-123_");
            Check.That(r.IsOk).IsTrue();
            Check.That(r.Tokens).CountIs(2);
            var tok = r.Tokens[0];
            Check.That(tok.TokenID).IsEqualTo(AlphaNumDashId.ID);
            Check.That(tok.StringWithoutQuotes).IsEqualTo("alpha-123_");
        }

        [Fact]
        public void TestAlphaNumDashIdStartsWithUnderscore()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<AlphaNumDashId>>());
            Check.That(lexerRes.IsError).IsFalse();
            var lexer = lexerRes.Result;
            var r = lexer.Tokenize("_alpha-123_");
            Check.That(r.IsOk).IsTrue();
            Check.That(r.Tokens).CountIs(2);
            var tok = r.Tokens[0];
            Check.That(tok.TokenID).IsEqualTo(AlphaNumDashId.ID);
            Check.That(tok.StringWithoutQuotes).IsEqualTo("_alpha-123_");
        }

        [Fact]
        public void TestAlphaNumId()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<AlphaNumId>>());
            Check.That(lexerRes.IsError).IsFalse();
            var lexer = lexerRes.Result;
            var r = lexer.Tokenize("alpha123");
            Check.That(r.IsOk).IsTrue();
            Check.That(r.Tokens).CountIs(2);
            var tok = r.Tokens[0];
            Check.That(tok.TokenID).IsEqualTo(AlphaNumId.ID);
            Check.That(tok.StringWithoutQuotes).IsEqualTo("alpha123");
        }

        [Fact]
        public void TestCustomId()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<CustomId>>());
            Check.That(lexerRes.IsError).IsFalse();
            var lexer = lexerRes.Result;
            var r = lexer.Tokenize("a_-Bc ZyX-_");
            Check.That(r.IsOk).IsTrue();
            Check.That(r.Tokens).CountIs(3);
            var tok1 = r.Tokens[0];
            Check.That(tok1).IsEqualTo(CustomId.ID, "a_-Bc");

            var tok2 = r.Tokens[1];
            Check.That(tok2).IsEqualTo(CustomId.ID, "ZyX-_");
        }

        [Fact]
        public void TestCustomIdReverseRange()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<CustomIdReverseRange>>());
            Check.That(lexerRes.IsError).IsFalse();
            var lexer = lexerRes.Result;
            var r = lexer.Tokenize("a_-Bc ZyX-_");
            Check.That(r.IsOk).IsTrue();
            Check.That(r.Tokens).CountIs(3);
            var tok1 = r.Tokens[0];
            Check.That(tok1).IsEqualTo(CustomIdReverseRange.ID, "a_-Bc");
            var tok2 = r.Tokens[1];
            Check.That(tok2).IsEqualTo(CustomIdReverseRange.ID, "ZyX-_");
        }

        [Fact]
        public void TestCustomIdWithOther()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<CustomId>>());
            Check.That(lexerRes.IsError).IsFalse();
            var lexer = lexerRes.Result;
            var r = lexer.Tokenize("_b_ -C-");
            Check.That(r.IsOk).IsTrue();
            Check.That(r.Tokens).CountIs(5);
            var tok1 = r.Tokens[0];
            Check.That(tok1).IsEqualTo(CustomId.OTHER, "_");
            var tok2 = r.Tokens[1];
            Check.That(tok2).IsEqualTo(CustomId.ID, "b_");
            var tok3 = r.Tokens[2];
            Check.That(tok3).IsEqualTo(CustomId.OTHER, "-");
            var tok4 = r.Tokens[3];
            Check.That(tok4).IsEqualTo(CustomId.ID, "C-");
        }

        [Fact]
        public void TestIgnoreWS()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<IgnoreWS>>());
            Check.That(lexerRes.IsError).IsFalse();
            var lexer = lexerRes.Result;
            var r = lexer.Tokenize("\n \n");
            Check.That(r.IsOk).IsTrue();
            Check.That(r.Tokens).CountIs(2);
            var tok = r.Tokens[0];
            Check.That(tok).IsEqualTo(IgnoreWS.WS, " ");
        }

        [Fact]
        public void TestIgnoreEOL()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<IgnoreEOL>>());
            Check.That(lexerRes.IsError).IsFalse();
            var lexer = lexerRes.Result;
            var r = lexer.Tokenize(" \n ");
            Check.That(r.IsOk).IsTrue();
            Check.That(r.Tokens).CountIs(2);
            var tok = r.Tokens[0];
            Check.That(tok).IsEqualTo(IgnoreEOL.EOL, "\n");
        }

        [Fact]
        public void TestWhiteSpace()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<WhiteSpace>>());
            Check.That(lexerRes.IsError).IsFalse();
            var lexer = lexerRes.Result;
            var r = lexer.Tokenize(" \t ");
            Check.That(r.IsOk).IsTrue();
            Check.That(r.Tokens).CountIs(2);
            var tok = r.Tokens[0];
            Check.That(tok).IsEqualTo(WhiteSpace.TAB, "\t");
        }

        [Fact]
        public void TestKeyWord()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<KeyWord>>());
            Check.That(lexerRes.IsError).IsFalse();
            var lexer = lexerRes.Result;
            var r = lexer.Tokenize("keyword KeYwOrD");
            Check.That(r.IsOk).IsTrue();
            Check.That(r.Tokens).CountIs(3);
            var tok1 = r.Tokens[0];
            Check.That(tok1).IsEqualTo(KeyWord.KEYWORD, "keyword");
            var tok2 = r.Tokens[1];
            Check.That(tok2).IsEqualTo(default(KeyWord), "KeYwOrD");
        }

        [Fact]
        public void TestKeyWordIgnoreCase()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<KeyWordIgnoreCase>>());
            Check.That(lexerRes.IsError).IsFalse();
            var lexer = lexerRes.Result;
            var r = lexer.Tokenize("keyword KeYwOrD");
            Check.That(r.IsOk).IsTrue();
            Check.That(r.Tokens).CountIs(3);
            var tok1 = r.Tokens[0];
            Check.That(tok1).IsEqualTo(KeyWordIgnoreCase.KEYWORD, "keyword");
            var tok2 = r.Tokens[1];
            Check.That(tok2).IsEqualTo(KeyWordIgnoreCase.KEYWORD, "KeYwOrD");
        }

        [Fact]
        public void TestStringDelimiters()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<StringDelimiters>>());
            Check.That(lexerRes.IsError).IsFalse();
            var lexer = lexerRes.Result;
            var r = lexer.Tokenize("'that''s it'");
            Check.That(r.IsOk).IsTrue();
            Check.That(r.Tokens).IsNotNull();
            Check.That(r.Tokens).Not.IsEmpty();
            Check.That(r.Tokens).CountIs(2);
            var tok = r.Tokens[0];
            Check.That(tok.Value).IsEqualTo("'that's it'");
            Check.That(tok.StringWithoutQuotes).IsEqualTo("that's it");
        }

        [Fact]
        public void TestBadEmptyStringDelimiter()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<BadEmptyStringDelimiter>>());
            Check.That(lexerRes.IsError).IsTrue();
            Check.That(lexerRes.Errors).IsSingle();
            var error = lexerRes.Errors[0];
            Check.That(error.Level).IsEqualTo(ErrorLevel.FATAL);
            Check.That(error.Code).IsEqualTo(ErrorCodes.LEXER_STRING_DELIMITER_MUST_BE_1_CHAR);
        }

        [Fact]
        public void TestBadLetterStringDelimiter()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<BadLetterStringDelimiter>>());
            Check.That(lexerRes.IsError).IsTrue();
            Check.That(lexerRes.Errors).IsSingle();
            var error = lexerRes.Errors[0];
            Check.That(error.Level).IsEqualTo(ErrorLevel.FATAL);
            Check.That(error.Code).IsEqualTo(ErrorCodes.LEXER_STRING_DELIMITER_CANNOT_BE_LETTER_OR_DIGIT);
        }

        [Fact]
        public void TestBadEscapeStringDelimiterTooLong()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<BadEscapeStringDelimiterTooLong>>());
            Check.That(lexerRes.IsError).IsTrue();
            Check.That(lexerRes.Errors).IsSingle();
            var error = lexerRes.Errors[0];
            Check.That(error.Level).IsEqualTo(ErrorLevel.FATAL);
            Check.That(error.Code).IsEqualTo(ErrorCodes.LEXER_STRING_ESCAPE_CHAR_MUST_BE_1_CHAR);
        }

        [Fact]
        public void TestBadEscapeStringDelimiterLetter()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<BadEscapeStringDelimiterLetter>>());
            Check.That(lexerRes.IsError).IsTrue();
            Check.That(lexerRes.Errors).IsSingle();
            var error = lexerRes.Errors[0];
            Check.That(error.Level).IsEqualTo(ErrorLevel.FATAL);
            Check.That(error.Code).IsEqualTo(ErrorCodes.LEXER_STRING_ESCAPE_CHAR_CANNOT_BE_LETTER_OR_DIGIT);
        }

        [Fact]
        public void TestDefaultQuotedString()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<DefaultQuotedString>>());
            Check.That(lexerRes.IsError).IsFalse();
            var lexer = lexerRes.Result;
            var source = "hello \\\"world ";
            var expected = "hello \"world ";
            var r = lexer.Tokenize($"\"{source}\"");
            Check.That(r.IsOk).IsTrue();
            Check.That(r.Tokens).CountIs(2);
            var tok = r.Tokens[0];
            Check.That(tok.TokenID).IsEqualTo(DefaultQuotedString.DefaultString);
            Check.That(tok.StringWithoutQuotes).IsEqualTo(expected);
        }

        [Fact]
        public void TestIssue348()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<Issue348>>());
            Check.That(lexerRes.IsError).IsFalse();
            var lexer = lexerRes.Result;
            string source = "\\\"te\\\\\\\\st\\\"";


            var r = lexer.Tokenize($"\"{source}\"");
            Check.That(r.IsOk).IsTrue();
            Check.That(r.Tokens).CountIs(2);
            var tok = r.Tokens[0];
            Check.That(tok.TokenID).IsEqualTo(Issue348.STriNG);
            Check.That(tok.Value).IsEqualTo("\"\"te\\\\st\"\"");
            Check.That(tok.StringWithoutQuotes).IsEqualTo("\"te\\\\st\"");
        }

        [Fact]
        public void TestIssue348Bis()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<Issue348Bis>>());
            Check.That(lexerRes.IsError).IsFalse();
            var lexer = lexerRes.Result;
            string source = "^\"te\\\\st^\"";
            string expected = "\"\"te\\\\st\"\"";

            var glex = lexer as GenericLexer<Issue348Bis>;
            var g = glex.FSMBuilder.Fsm.ToGraphViz();
            var r = lexer.Tokenize($"\"{source}\"");
            Check.That(r.IsOk).IsTrue();
            Check.That(r.Tokens).CountIs(2);
            var tok = r.Tokens[0];
            Check.That(tok.TokenID).IsEqualTo(Issue348Bis.STriNG);
            Check.That(tok.Value).IsEqualTo(expected);
            Check.That(tok.StringWithoutQuotes).IsEqualTo("\"te\\\\st\"");

        }

        [Fact]
        public void TestDoubleQuotedString()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<DoubleQuotedString>>());
            Check.That(lexerRes.IsError).IsFalse();
            var lexer = lexerRes.Result;
            var source = "hello \\\"world ";
            var expected = "hello \"world ";
            var r = lexer.Tokenize($"\"{source}\"");
            Check.That(r.IsOk).IsTrue();
            Check.That(r.Tokens).CountIs(2);
            var tok = r.Tokens[0];
            Check.That(tok.TokenID).IsEqualTo(DoubleQuotedString.DoubleString);
            Check.That(tok.StringWithoutQuotes).IsEqualTo(expected);
        }




        [Fact]
        public void TestParserWithLexerExtensions()
        {
            ParserUsingLexerExtensions parserInstance = new ParserUsingLexerExtensions();
            var builder = new ParserBuilder<Extensions, object>();
            var parserResult = builder.BuildParser(parserInstance, ParserType.LL_RECURSIVE_DESCENT, "root",
                ExtendedGenericLexer.AddExtension);
            Check.That(parserResult).IsOk();
            var parser = parserResult.Result;
            var result = parser.Parse("15.01.2020");
            Check.That(result).IsOkParsing();

            Check.That(result.Result).IsEqualTo(new DateTime(2020, 01, 15));
            result = parser.Parse("3.14");
            Check.That(result).IsOkParsing();
            Check.That(result.Result).IsEqualTo(3.14);
        }

        [Fact]
        public void TestExtensions()
        {
            var lexerRes =
                LexerBuilder.BuildLexer(new BuildResult<ILexer<Extensions>>(), ExtendedGenericLexer.AddExtension);
            Check.That(lexerRes.IsError).IsFalse();
            var lexer = lexerRes.Result as GenericLexer<Extensions>;
            Check.That(lexer).IsNotNull();

            var r = lexer.Tokenize("20.02.2018 3.14");
            Check.That(r.IsOk).IsTrue();

            Check.That(r.Tokens).CountIs(3);
            Check.That(r.Tokens[0]).IsEqualTo(Extensions.DATE, "20.02.2018");
            Check.That(r.Tokens[1]).IsEqualTo(Extensions.DOUBLE, "3.14");
        }

        [Fact]
        public void TestShortExtensions()
        {
            var lexerRes =
                LexerBuilder.BuildLexer(new BuildResult<ILexer<ShortExtensions>>(),
                    ExtendedGenericLexer.AddShortExtension);
            Check.That(lexerRes.IsError).IsFalse();
            var lexer = lexerRes.Result as GenericLexer<ShortExtensions>;
            Check.That(lexer).IsNotNull();

            var r = lexer.Tokenize("20.02.2018 3.14 #***#");
            Check.That(r.IsOk).IsTrue();

            Check.That(r.Tokens).CountIs(4);
            Check.That(r.Tokens[0]).IsEqualTo(ShortExtensions.DATE, "20.02.2018");
            Check.That(r.Tokens[1]).IsEqualTo(ShortExtensions.DOUBLE, "3.14");
            Check.That(r.Tokens[2]).IsEqualTo(ShortExtensions.TEST, "#***#");
            
            r = lexer.Tokenize("#***");
            Check.That(r).Not.IsOkLexing();
            Check.That(r.Error).IsNotNull();
            Check.That(r.Error.UnexpectedChar).IsEqualTo('*');
            Check.That(r.Error.Column).IsEqualTo(3);
        }

        [Fact]
        public void TestExtensionsPreconditionFailure()
        {
            var lexerRes =
                LexerBuilder.BuildLexer(new BuildResult<ILexer<Extensions>>(), ExtendedGenericLexer.AddExtension);
            Check.That(lexerRes.IsError).IsFalse();
            var lexer = lexerRes.Result as GenericLexer<Extensions>;
            Check.That(lexer).IsNotNull();

            var r = lexer.Tokenize("0.0.2018");
            Check.That(r.IsError).IsTrue();
            Check.That(r.Error.Line).IsEqualTo(0);
            Check.That(r.Error.Column).IsEqualTo(3);
            Check.That(r.Error.UnexpectedChar).IsEqualTo('.');
        }

        [Fact]
        public void TestLexerError()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<AlphaNumDashId>>());
            Check.That(lexerRes.IsError).IsFalse();
            var lexer = lexerRes.Result;
            var source = "hello world  2 + 2 ";
            var r = lexer.Tokenize(source);
            Check.That(r.IsError).IsTrue();
            var error = r.Error;
            Check.That(error.Line).IsEqualTo(0);
            Check.That(error.Column).IsEqualTo(13);
            Check.That(error.UnexpectedChar).IsEqualTo('2');
            Check.That(error.ErrorType).IsEqualTo(ErrorType.UnexpectedChar);
        }

        [Fact]
        public void TestManyString()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<ManyString>>());
            Check.That(lexerRes.IsError).IsFalse();
            var lexer = lexerRes.Result;
            var string1 = "\"hello \\\"world \"";
            var expectString1 = "\"hello \"world \"";
            var string2 = "'that''s it'";
            var expectString2 = "'that's it'";
            var source1 = $"{string1} {string2}";
            var r = lexer.Tokenize(source1);
            Check.That(r.IsOk).IsTrue();
            Check.That(r.Tokens).CountIs(3);

            var tok1 = r.Tokens[0];
            Check.That(tok1).IsEqualTo(ManyString.STRING, expectString1);
            Check.That(tok1.StringDelimiter).IsEqualTo('"');

            var tok2 = r.Tokens[1];
            Check.That(tok2).IsEqualTo(ManyString.STRING, expectString2);
            Check.That(tok2.StringDelimiter).IsEqualTo('\'');

        }

        [Fact]
        public void TestSelfEscapedString()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<SelfEscapedString>>());
            Check.That(lexerRes.IsError).IsFalse();
            var lexer = lexerRes.Result as GenericLexer<SelfEscapedString>;
            Check.That(lexer).IsNotNull();
            var r = lexer.Tokenize("'that''s it'");

            Check.That(r.IsOk).IsTrue();
            var tokens = r.Tokens;
            Check.That(tokens).CountIs(2);
            var tok = tokens[0];
            Check.That(tok).IsEqualTo(SelfEscapedString.STRING, "'that's it'");



            r = lexer.Tokenize("'et voilà'");
            Check.That(r.IsOk).IsTrue();
            tokens = r.Tokens;
            Check.That(tokens).CountIs(2);
            tok = tokens[0];
            Check.That(tok).IsEqualTo(SelfEscapedString.STRING, "'et voilà'");

        }

        [Fact]
        public void TestSingleQuotedString()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<SingleQuotedString>>());
            Check.That(lexerRes.IsError).IsFalse();
            var lexer = lexerRes.Result;
            var source = "hello \\'world ";
            var expected = "'hello 'world '";
            var r = lexer.Tokenize($"'{source}'");
            Check.That(r.IsOk).IsTrue();
            Check.That(r.Tokens).CountIs(2);
            var tok = r.Tokens[0];
            Check.That(tok).IsEqualTo(SingleQuotedString.SingleString, expected);
        }

        [Fact]
        public void TestTokenCallbacks()
        {
            var res = LexerBuilder.BuildLexer(new BuildResult<ILexer<CallbackTokens>>());
            Check.That(res.IsError).IsFalse();
            var lexer = res.Result as GenericLexer<CallbackTokens>;
            CallBacksBuilder.BuildCallbacks(lexer);


            var r = lexer.Tokenize("aaa bbb");
            Check.That(r.IsOk).IsTrue();
            var tokens = r.Tokens;
            Check.That(tokens).CountIs(3);
            Check.That(tokens[0]).IsEqualTo(CallbackTokens.IDENTIFIER, "AAA");
            Check.That(tokens[1]).IsEqualTo(CallbackTokens.SKIP, "BBB");
        }

        [Fact]
        public void TestCharTokens()
        {
            var res = LexerBuilder.BuildLexer(new BuildResult<ILexer<CharTokens>>());
            Check.That(res.IsError).IsFalse();
            var lexer = res.Result as GenericLexer<CharTokens>;
            var dump = lexer.ToString();
            var grpah = lexer.ToGraphViz();

            Check.That(lexer).IsNotNull();
            var res1 = lexer.Tokenize("'c'");
            Check.That(res1.IsError).IsFalse();
            Check.That(res1.Tokens).CountIs(2);
            Token<CharTokens> token = res1.Tokens[0];
            Check.That(token.CharValue).IsEqualTo('c');
            Check.That(token.TokenID).IsEqualTo(CharTokens.MyChar);
            var lastToken = res1.Tokens.Last();

            var source = "'\\''";
            var res2 = lexer.Tokenize(source);
            Check.That(res2.IsError).IsFalse();
            Check.That(res2.Tokens).CountIs(2);
            token = res2.Tokens[0];
            Check.That(token).IsEqualTo(CharTokens.MyChar, source);

            var sourceU = "'\\u0066'";
            var res3 = lexer.Tokenize(sourceU);
            Check.That(res3.IsError).IsFalse();
            Check.That(res3.Tokens).CountIs(2);
            token = res3.Tokens[0];
            Check.That(token).IsEqualTo(CharTokens.MyChar, sourceU);
        }

        [Fact]
        public void TestCharTokenDelimiterConflict()
        {
            var res = LexerBuilder.BuildLexer(new BuildResult<ILexer<CharTokensConflicts>>());
            Check.That(res.IsError).IsTrue();
            Check.That(res.Errors).CountIs(2);
        }

        [Fact]
        public void TestIssue106()
        {
            var res = LexerBuilder.BuildLexer(new BuildResult<ILexer<Issue106>>());
            Check.That(res.IsError).IsFalse();
            var lexer = res.Result as GenericLexer<Issue106>;
            var r = lexer.Tokenize("1.");
            Check.That(r.IsOk).IsTrue();
            var tokens = r.Tokens;
            Check.That(tokens).IsNotNull();
            Check.That(tokens).CountIs(3);
            Check.That(tokens.Extracting(x => x.TokenID)).Contains(new[] { Issue106.Integer, Issue106.Period });
            var token = tokens[0];
            Check.That(token.TokenID).IsEqualTo(Issue106.Integer);
            Check.That(token.IntValue).IsEqualTo(1);
            token = tokens[1];
            Check.That(token.TokenID).IsEqualTo(Issue106.Period);
        }

        [Fact]
        public void TestIssue114()
        {
            var res = LexerBuilder.BuildLexer(new BuildResult<ILexer<Issue114>>());
            Check.That(res.IsError).IsFalse();
            var lexer = res.Result as GenericLexer<Issue114>;

            var r = lexer?.Tokenize("// /&");
            Check.That(r.IsError).IsTrue();
            Check.That(r.Error.UnexpectedChar).IsEqualTo('&');

            r = lexer?.Tokenize("/&");
            Check.That(r.Error.UnexpectedChar).IsEqualTo('&');

            r = lexer?.Tokenize("&/");
            Check.That(r.IsError).IsTrue();
            Check.That(r.Error.UnexpectedChar).IsEqualTo('&');

            r = lexer?.Tokenize("// &");
            Check.That(r.IsError).IsTrue();
            Check.That(r.Error.UnexpectedChar).IsEqualTo('&');
        }

        [Fact]
        public void TestIssue137()
        {
            var res = LexerBuilder.BuildLexer(new BuildResult<ILexer<Issue137>>());
            Check.That(res.IsError).IsFalse();
            var lexer = res.Result;

            var result = lexer.Tokenize("--+");
            Check.That(result.IsOk).IsTrue();
            Check.That(result.Tokens.Extracting(x => x.TokenID))
                .ContainsExactly(new[] { Issue137.B, Issue137.C, default(Issue137) });

            result = lexer.Tokenize("--.");
            Check.That(result.IsOk).IsTrue();
            Check.That(result.Tokens.Extracting(x => x.TokenID))
                .ContainsExactly(new[] { Issue137.B, Issue137.B, Issue137.A, default(Issue137) });
        }

        [Fact]
        public void TestIssue138()
        {
            var res = LexerBuilder.BuildLexer(new BuildResult<ILexer<Issue138>>());
            Check.That(res.IsError).IsFalse();
            var lexer = res.Result;

            var result = lexer.Tokenize(".");
            Check.That(result.IsError).IsTrue();
            Check.That(result.Error.ErrorType).IsEqualTo(ErrorType.UnexpectedChar);

            result = lexer.Tokenize("--");
            Check.That(result.IsOk).IsTrue();
            Check.That(result.Tokens.Extracting(x => x.TokenID))
                .ContainsExactly(new[] { Issue138.B, Issue138.B, default(Issue138) });
        }

        [Fact]
        public void TestIssue177()
        {
            var res = LexerBuilder.BuildLexer(new BuildResult<ILexer<Issue177Generic>>());
            Check.That(res.IsError).IsFalse();
            var lexer = res.Result;

            var result = lexer.Tokenize(@"1 2 
2 3
4 5");
            Check.That(result.IsOk).IsTrue();
            Check.That(result.Tokens).CountIs(7);

            var expectations = new (int value, int line, int column)[]
            {
                (0, 0, 1),
                (0, 2, 2),
                (1, 0, 2),
                (1, 2, 3),
                (2, 0, 4),
                (2, 2, 5)
            };

            Check.That(result.Tokens.Take(6).Extracting(x => (x.Position.Line, x.Position.Column, x.IntValue)))
                .ContainsExactly(expectations);
        }

        [Fact]
        public void TestSameIntValuesError()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<SameIntValuesError>>());
            Check.That(lexerRes.IsError).IsTrue();
            Check.That(lexerRes.Result).IsNull();
            Check.That(lexerRes.Errors).IsSingle();
            Check.That(lexerRes.Errors.First().Code).IsEqualTo(ErrorCodes.LEXER_SAME_VALUE_USED_MANY_TIME);


        }

        [Fact]
        public void TestIssue210()
        {
            var lexResult = LexerBuilder.BuildLexer<Issue210Token>(Issue210Extensions.AddExtensions);
            Check.That(lexResult.IsOk).IsTrue();
            var lexer = lexResult.Result;
            Check.That(lexer).IsNotNull();
            var r = lexer.Tokenize("?");
            Check.That(r.IsOk).IsTrue();
            r = lexer.Tokenize("?special?");
            Check.That(r.IsOk).IsTrue();
            Check.That(r.Tokens).CountIs(2);
            Check.That(r.Tokens[0]).IsEqualTo(Issue210Token.SPECIAL, "special");
            Check.That(r.Tokens[1]).IsEqualTo(Issue210Token.EOF, "");

            r = lexer.Tokenize("x?x");
            Check.That(r.IsOk).IsTrue();
            Check.That(r.Tokens).CountIs(4);
            Check.That(r.Tokens[0]).IsEqualTo(Issue210Token.X, "x");
            Check.That(r.Tokens[1]).IsEqualTo(Issue210Token.QMARK, "?");
            Check.That(r.Tokens[2]).IsEqualTo(Issue210Token.X, "x");
            Check.That(r.Tokens[3]).IsEqualTo(Issue210Token.EOF, "");


            r = lexer.Tokenize("??");
            Check.That(r.IsOk).IsTrue();
            Check.That(r.Tokens).CountIs(2);
            Check.That(r.Tokens[0]).IsEqualTo(Issue210Token.SPECIAL, "");
            Check.That(r.Tokens[1]).IsEqualTo(Issue210Token.EOF, "");
        }

        [Fact]
        public void TestIndented()
        {
            var source = @"if truc == 1
    un = 1
    deux = 2
else
    trois = 3
    quatre = 4

";

            var lexRes = LexerBuilder.BuildLexer<IndentedLangLexer>();
            Check.That(lexRes.IsOk).IsTrue();
            var lexer = lexRes.Result;
            Check.That(lexer).IsNotNull();
            var tokResult = lexer.Tokenize(source);
            Check.That(tokResult.IsOk).IsTrue();
            var tokens = tokResult.Tokens;
            Check.That(tokens).IsNotNull();
            Check.That(tokens).CountIs(22);
            Check.That(tokens.Where(x => x.IsIndent)).CountIs(2);
            Check.That(tokens.Where(x => x.IsUnIndent)).CountIs(2);
        }

        [Fact]
        public void TestIndented2()
        {
            var source = @"if truc == 1
    un = 1
    deux = 2
else
    trois = 3
    quatre = 4

";

            var lexRes = LexerBuilder.BuildLexer<IndentedLangLexer2>();
            Check.That(lexRes.IsOk).IsTrue();
            var lexer = lexRes.Result;
            Check.That(lexer).IsNotNull();
            var tokResult = lexer.Tokenize(source);
            Check.That(tokResult.IsOk).IsTrue();
            var tokens = tokResult.Tokens;
            Check.That(tokens).IsNotNull();
            Check.That(tokens).CountIs(29);
            Check.That(tokens.Where(x => x.IsIndent)).CountIs(2);
            Check.That(tokens.Where(x => x.IsUnIndent)).CountIs(2);
        }

        [Fact]
        public void TestGenericShortCode()
        {
            var build = LexerBuilder.BuildLexer<GenericShortAttributes>();
            Check.That(build.IsOk).IsTrue();
            Check.That(build.Result).IsNotNull();
            var lexer = build.Result;
            var lexResult = lexer.Tokenize(@"1 + 2 + a + b * 8.3 hello / 'b\'jour'");

            Check.That(lexResult.IsOk).IsTrue();
            var tokens = lexResult.Tokens;
            Check.That(tokens).CountIs(13);
            Check.That(tokens.Extracting(x => x.TokenID)).ContainsExactly(
                new[]
                {
                    GenericShortAttributes.INT,
                    GenericShortAttributes.PLUS,
                    GenericShortAttributes.INT,
                    GenericShortAttributes.PLUS,
                    GenericShortAttributes.IDENTIFIER,
                    GenericShortAttributes.PLUS,
                    GenericShortAttributes.IDENTIFIER,
                    GenericShortAttributes.TIMES,
                    GenericShortAttributes.DOUBLE,
                    GenericShortAttributes.HELLO,
                    GenericShortAttributes.DIVIDE,
                    GenericShortAttributes.STRING,
                    GenericShortAttributes.EOF
                });

        }


        [Fact]
        public void TestTokensAndError()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<SimpleExpressionToken>>());
            Check.That(lexerRes.IsError).IsFalse();
            var lexer = lexerRes.Result;

            var errorSource = "1 + 2 @";

            var lexingResult = lexer.Tokenize(errorSource);
            Check.That(lexingResult.IsError).IsTrue();
            Check.That(lexingResult.Tokens).IsNotNull();
            Check.That(lexingResult.Tokens).CountIs(3);
            Check.That(lexingResult.Error.UnexpectedChar).IsEqualTo('@');


        }

        [Fact]
        public void TestDate()
        {
            DateTime date = new DateTime(2023, 08, 03);

            var englishDateLexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<DateTokenEnglishDashed>>());
            Check.That(englishDateLexerRes.IsError).IsFalse();
            var englishDateLexer = englishDateLexerRes.Result;

            var source = "2023-08-03";

            var englishDateLexingResult = englishDateLexer.Tokenize(source);
            Check.That(englishDateLexingResult).IsOkLexing();
            Check.That(englishDateLexingResult.Tokens).IsNotNull();
            Check.That(englishDateLexingResult.Tokens).CountIs(2);
            Check.That(englishDateLexingResult.Tokens[0].TokenID).IsEqualTo(DateTokenEnglishDashed.DATE);
            Check.That(englishDateLexingResult.Tokens[0].Value).IsEqualTo(source);
            Check.That(englishDateLexingResult.Tokens[0].DateTimeValue).IsEqualTo(date);


            var frenchDateLexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<DateTokenWithFrenchSlashed>>());
            Check.That(frenchDateLexerRes.IsError).IsFalse();
            var franchDateLexer = frenchDateLexerRes.Result;

            source = "03/08/2023";

            var frenchDateLexingResult = franchDateLexer.Tokenize(source);
            Check.That(frenchDateLexingResult).IsOkLexing();
            Check.That(frenchDateLexingResult.Tokens).IsNotNull();
            Check.That(frenchDateLexingResult.Tokens).CountIs(2);
            Check.That(frenchDateLexingResult.Tokens[0].TokenID).IsEqualTo(DateTokenWithFrenchSlashed.DATE);
            Check.That(frenchDateLexingResult.Tokens[0].Value).IsEqualTo(source);
            Check.That(frenchDateLexingResult.Tokens[0].DateTimeValue).IsEqualTo(date);

        }

        [Fact]
        public void TestDateAndDouble()
        {
            DateTime date = new DateTime(2023, 08, 03);

            var englishDateLexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<DateAndDoubleToken>>());
            Check.That(englishDateLexerRes.IsError).IsFalse();
            var englishDateLexer = englishDateLexerRes.Result;

            var source = $"{date.ToString("yyyy.MM.dd")} 3.14 {date.ToString("dd.MM.yyyy")}";

            var lexingResult = englishDateLexer.Tokenize(source);
            Check.That(lexingResult).IsOkLexing();
            Check.That(lexingResult.Tokens).IsNotNull();
            Check.That(lexingResult.Tokens).CountIs(4);
            Check.That(lexingResult.Tokens.Extracting(x => x.TokenID).Take(3)).IsEqualTo(new List<DateAndDoubleToken>()
            {
                DateAndDoubleToken.ENGLISH_DATE, DateAndDoubleToken.DOUBLE, DateAndDoubleToken.FRENCH_DATE
            });
            Check.That(lexingResult.Tokens[0].DateTimeValue).IsEqualTo(date);
            Check.That(lexingResult.Tokens[1].DoubleValue).IsEqualTo(3.14);
            Check.That(lexingResult.Tokens[2].DateTimeValue).IsEqualTo(date);

        }

        [Fact]
        public void TestManyDate()
        {
            DateTime date = new DateTime(2023, 08, 08);

            var englishDateLexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<ManyDateToken>>());
            Check.That(englishDateLexerRes.IsError).IsFalse();
            var manyDateLexer = englishDateLexerRes.Result;

            var source = @"
08/08/2023
2023-08-08
";
            var mannyDateLexerResult = manyDateLexer.Tokenize(source);
            Check.That(mannyDateLexerResult).IsOkLexing();
            Check.That(mannyDateLexerResult.Tokens).IsNotNull();
            Check.That(mannyDateLexerResult.Tokens).CountIs(3);
            Check.That(mannyDateLexerResult.Tokens[0].TokenID).IsEqualTo(ManyDateToken.FRENCH_DATE);
            Check.That(mannyDateLexerResult.Tokens[0].Value).IsEqualTo("08/08/2023");
            Check.That(mannyDateLexerResult.Tokens[0].DateTimeValue).IsEqualTo(date);
            Check.That(mannyDateLexerResult.Tokens[1].TokenID).IsEqualTo(ManyDateToken.ENGLISH_DATE);
            Check.That(mannyDateLexerResult.Tokens[1].Value).IsEqualTo("2023-08-08");
            Check.That(mannyDateLexerResult.Tokens[1].DateTimeValue).IsEqualTo(date);
        }


        [Fact]
        public void TestDateAndExpressions()
        {


            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<DateAndExpressions>>());
            Check.That(lexerRes.IsError).IsFalse();
            var lexer = lexerRes.Result;

            var lexerResult = lexer.Tokenize("1978-12- 2023-08-08");
            Check.That(lexerResult).IsOkLexing();
            Check.That(lexerResult.Tokens).IsNotNull();
            Check.That(lexerResult.Tokens).CountIs(6);
            Check.That(lexerResult.Tokens.Extracting(x => x.TokenID).Take(5)).IsEqualTo(new List<DateAndExpressions>()
            {
                DateAndExpressions.INT, DateAndExpressions.MINUS, DateAndExpressions.INT, DateAndExpressions.MINUS,
                DateAndExpressions.DATE
            });


        }

        [Fact]
        public void TestDefaultHexa()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<HexaTokenDefault>>());
            Check.That(lexerRes.IsError).IsFalse();
            var lexer = lexerRes.Result;

            var lexerResult = lexer.Tokenize("0xAA 0x123 0xAb89CF");
            Check.That(lexerResult).IsOkLexing();
            Check.That(lexerResult.Tokens).IsNotNull();
            Check.That(lexerResult.Tokens).CountIs(4);
            Check.That(lexerResult.Tokens.Extracting(x => (x.TokenID,x.HexaIntValue)).Take(3)).IsEqualTo(new List<(HexaTokenDefault,long)>()
            {
                (HexaTokenDefault.HEXA,Convert.ToInt32("AA",16)),
                (HexaTokenDefault.HEXA,Convert.ToInt32("123",16)),
                (HexaTokenDefault.HEXA,Convert.ToInt32("AB89CF",16))
            });
        }
        
        [Fact]
        public void Test6xHexa()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<HexaToken6x>>());
            Check.That(lexerRes.IsError).IsFalse();
            var lexer = lexerRes.Result;

            var lexerResult = lexer.Tokenize("6xAA 6x123 6xAb89CF");
            Check.That(lexerResult).IsOkLexing();
            Check.That(lexerResult.Tokens).IsNotNull();
            Check.That(lexerResult.Tokens).CountIs(4);
            Check.That(lexerResult.Tokens.Extracting(x => (x.TokenID,x.HexaIntValue)).Take(3)).IsEqualTo(new List<(HexaToken6x,long)>()
            {
                (HexaToken6x.HEXA,Convert.ToInt32("AA",16)),
                (HexaToken6x.HEXA,Convert.ToInt32("123",16)),
                (HexaToken6x.HEXA,Convert.ToInt32("AB89CF",16))
            });
            
            lexerResult = lexer.Tokenize("0xAA");
            Check.That(lexerResult).Not.IsOkLexing();
        }
        
        [Fact]
        public void TestHexaConflictId()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<HexaTokenConflictId>>());
            Check.That(lexerRes.IsError).IsFalse();
            var lexer = lexerRes.Result;

            var lexerResult = lexer.Tokenize("hexaAA hexa123 hexaAb89CF hello");
            Check.That(lexerResult).IsOkLexing();
            Check.That(lexerResult.Tokens).IsNotNull();
            Check.That(lexerResult.Tokens).CountIs(5);
            
            
            
            Check.That(lexerResult.Tokens.Extracting(x => (x.TokenID)).Take(4)).IsEqualTo(new List<HexaTokenConflictId>()
            { HexaTokenConflictId.ID, HexaTokenConflictId.HEXA, HexaTokenConflictId.HEXA, HexaTokenConflictId.ID });
            
        }
        
        [Fact]
        public void TestHexaConflictIdAndInt()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<HexaTokenConflictIdAndInt>>());
            Check.That(lexerRes.IsError).IsFalse();
            var lexer = lexerRes.Result;

            var lexerResult = lexer.Tokenize("0xAA 0x123 0xAb89CF hello 42");
            Check.That(lexerResult).IsOkLexing();
            Check.That(lexerResult.Tokens).IsNotNull();
            Check.That(lexerResult.Tokens).CountIs(6);
            Check.That(lexerResult.Tokens.Extracting(x => (x.TokenID,x.HexaIntValue)).Take(3)).IsEqualTo(new List<(HexaTokenConflictIdAndInt,long)>()
            {
                (HexaTokenConflictIdAndInt.HEXA,Convert.ToInt32("AA",16)),
                (HexaTokenConflictIdAndInt.HEXA,Convert.ToInt32("123",16)),
                (HexaTokenConflictIdAndInt.HEXA,Convert.ToInt32("AB89CF",16))
            });
            Check.That(lexerResult.Tokens[3].TokenID).IsEqualTo(HexaTokenConflictIdAndInt.ID);
            Check.That(lexerResult.Tokens[3].Value).IsEqualTo("hello");
            
            Check.That(lexerResult.Tokens[4].TokenID).IsEqualTo(HexaTokenConflictIdAndInt.INT);
            Check.That(lexerResult.Tokens[4].Value).IsEqualTo("42");
        }

        [Fact]
        public void TestI18nLexerError() // issue #380
        {
            var StartingRule = $"{nameof(SimpleExpressionParserWithContext)}_expressions";
            var parserInstance = new SimpleExpressionParserWithContext();

            // fr
            var frBuilder = new ParserBuilder<ExpressionToken, int>("fr");
            var frParser = frBuilder.BuildParser(parserInstance, ParserType.LL_RECURSIVE_DESCENT, StartingRule);
            Check.That(frParser).IsOk();
            var frParseResult = frParser.Result.Parse("1 + + 2");

            Check.That(frParseResult).Not.IsOkParsing();
            Check.That(frParseResult.Errors).IsSingle();
            var frError = frParseResult.Errors[0];
            Check.That(frError).IsNotNull();
            Check.That(frError.ErrorMessage).Contains("plus inattendu");

            // en
            var enBuilder = new ParserBuilder<ExpressionToken, int>("en");
            var enParser = enBuilder.BuildParser(parserInstance, ParserType.LL_RECURSIVE_DESCENT, StartingRule);
            Check.That(enParser).IsOk();
            var enParseResult = enParser.Result.Parse("1 + + 2");

            Check.That(enParseResult).Not.IsOkParsing();
            Check.That(enParseResult.Errors).IsSingle();
            var enError = enParseResult.Errors[0];
            Check.That(enError).IsNotNull();
            Check.That(enError.ErrorMessage).Contains("unexpected plus sign");
        }

        [Fact]
        public void TestI18nDuplicateLabel()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<DuplicateLabels>>());
            Check.That(lexerRes).IsOk(); // not a blocking error, lexer should still be able to lex correctly.
            Check.That(lexerRes.Errors).IsSingle();
            var error = lexerRes.Errors[0];
            Check.That(error).IsNotNull();
            Check.That(error.Level).IsEqualTo(ErrorLevel.WARN);
            Check.That(error.Code).IsEqualTo(ErrorCodes.LEXER_MANY_LEXEM_WITH_SAME_LABEL);
            Check.That(error.Message).Contains("left paranthesis").And.Contains("paranthèse ouvrante");
        }

    }
}
