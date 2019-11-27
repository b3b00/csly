using System;
using System.Linq;
using System.Text;
using GenericLexerWithCallbacks;
using sly.buildresult;
using sly.lexer;
using sly.lexer.fsm;
using Xunit;

namespace ParserTests.lexer
{
    public enum Extensions
    {
        [Lexeme(GenericToken.Extension)] DATE,

        [Lexeme(GenericToken.Double)] DOUBLE
    }


    public static class ExtendedGenericLexer
    {
        public static bool CheckDate(ReadOnlyMemory<char> value)
        {
            var ok = false;
            if (value.Length == 6)
            {
                ok = char.IsDigit(value.At(0));
                ok = ok && char.IsDigit(value.At(1));
                ok = ok && value.At(2) == '.';
                ok = ok && char.IsDigit(value.At(3));
                ok = ok && char.IsDigit(value.At(4));
                ok = ok && value.At(5) == '.';
            }

            return ok;
        }

        public static void AddExtension(Extensions token, LexemeAttribute lexem, GenericLexer<Extensions> lexer)
        {
            if (token == Extensions.DATE)
            {
                NodeCallback<GenericToken> callback = match =>
                {
                    match.Properties[GenericLexer<Extensions>.DerivedToken] = Extensions.DATE;
                    return match;
                };

                var fsmBuilder = lexer.FSMBuilder;

                fsmBuilder.GoTo(GenericLexer<Extensions>.in_double)
                    .Transition('.', CheckDate)
                    .Mark("start_date")
                    .RepetitionTransition(4, "[0-9]")
                    .End(GenericToken.Extension)
                    .CallBack(callback);
            }
        }
    }

    public enum CharTokens {
        [Lexeme(GenericToken.Char,"'","\\")]
        MyChar
    }

    public enum CharTokensConflicts{
        [Lexeme(GenericToken.Char,"'","\\")]
        [Lexeme(GenericToken.Char,"|","\\")]
        MyChar,

        [Lexeme(GenericToken.Char,"|","\\")]
        OtherChar,

        [Lexeme(GenericToken.String,"'","\\")]
        MyString
    }


    public enum StringDelimiters {
        [Lexeme(GenericToken.String,"'","'")]
        MyString
    }
    public enum BadLetterStringDelimiter
    {
        [Lexeme(GenericToken.String, "a")] Letter
    }

    public enum BadEscapeStringDelimiterTooLong
    {
        [Lexeme(GenericToken.String, "'", ";:")] toolong
    }

    public enum BadEscapeStringDelimiterLetter
    {
        [Lexeme(GenericToken.String, "'", "a")] toolong
    }

    public enum BadEmptyStringDelimiter
    {
        [Lexeme(GenericToken.String, "")] Empty
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
        [Lexeme(GenericToken.String, "'", "'")]
        [Lexeme(GenericToken.String)]
        STRING
    }

    public enum AlphaId
    {
        [Lexeme(GenericToken.Identifier, IdentifierType.Alpha)]
        ID
    }

    public enum AlphaNumId
    {
        [Lexeme(GenericToken.Identifier, IdentifierType.AlphaNumeric)]
        ID
    }

    public enum AlphaNumDashId
    {
        [Lexeme(GenericToken.Identifier, IdentifierType.AlphaNumericDash)]
        ID
    }

    public enum CustomId
    {
        EOS,

        [Lexeme(GenericToken.Identifier, IdentifierType.Custom, "A-Za-z", "-_0-9A-Za-z")]
        ID,

        [Lexeme(GenericToken.SugarToken, "-", "_")]
        OTHER
    }

    [Lexer(IgnoreWS = false)]
    public enum IgnoreWS
    {
        [Lexeme(GenericToken.SugarToken, " ")]
        WS
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

        [Lexeme(GenericToken.Identifier)]
        ID
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
        [Lexeme(GenericToken.Int)]
        Integer = 5,

        [Lexeme(GenericToken.Double)]
        Double = 6,

        [Lexeme(GenericToken.SugarToken, ".")]
        Period
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
        [Lexeme(GenericToken.SugarToken, ".")]
        A = 1,
        [Lexeme(GenericToken.SugarToken, "-")]
        B,
        [Lexeme(GenericToken.SugarToken, "-+")]
        C,
        [Lexeme(GenericToken.SugarToken, "---")]
        E,
    }

    // Test that the FSMLexer properly terminates, without skipping tokens.
    public enum Issue138
    {
        [Lexeme(GenericToken.SugarToken, "..")]
        A = 1,
        [Lexeme(GenericToken.SugarToken, "-")]
        B,
        [Lexeme(GenericToken.SugarToken, "---")]
        C,
    }

    public class GenericLexerTests
    {
        [Fact]
        public void TestEmptyInput()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<Empty>>());
            Assert.False(lexerRes.IsError);
            var lexer = lexerRes.Result;
            var r = lexer.Tokenize("");
            Assert.True(r.IsOk);
            Assert.Single(r.Tokens);
            var tok = r.Tokens[0];
            Assert.Equal(Empty.EOS, tok.TokenID);
            Assert.Equal("",        tok.Value);
        }

        [Fact]
        public void TestIgnoredInput()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<Empty>>());
            Assert.False(lexerRes.IsError);
            var lexer = lexerRes.Result;
            var r = lexer.Tokenize(" \t\n ");
            Assert.True(r.IsOk);
            Assert.Single(r.Tokens);
            var tok = r.Tokens[0];
            Assert.Equal(Empty.EOS, tok.TokenID);
            Assert.Equal("",        tok.Value);
        }

        [Fact]
        public void TestAlphaId()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<AlphaId>>());
            Assert.False(lexerRes.IsError);
            var lexer = lexerRes.Result;

            var r = lexer.Tokenize("alpha");
            Assert.True(r.IsOk);
            Assert.Equal(2, r.Tokens.Count);
            var tok = r.Tokens[0];
            Assert.Equal(AlphaId.ID, tok.TokenID);
            Assert.Equal("alpha", tok.StringWithoutQuotes);
        }

        [Fact]
        public void TestAlphaNumDashId()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<AlphaNumDashId>>());
            Assert.False(lexerRes.IsError);
            var lexer = lexerRes.Result;
            var r = lexer.Tokenize("alpha-123_");
            Assert.True(r.IsOk);
            Assert.Equal(2, r.Tokens.Count);
            var tok = r.Tokens[0];
            Assert.Equal(AlphaNumDashId.ID, tok.TokenID);
            Assert.Equal("alpha-123_", tok.StringWithoutQuotes);
        }

        [Fact]
        public void TestAlphaNumDashIdStartsWithUnderscore()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<AlphaNumDashId>>());
            Assert.False(lexerRes.IsError);
            var lexer = lexerRes.Result;
            var r = lexer.Tokenize("_alpha-123_");
            Assert.True(r.IsOk);
            Assert.Equal(2, r.Tokens.Count);
            var tok = r.Tokens[0];
            Assert.Equal(AlphaNumDashId.ID, tok.TokenID);
            Assert.Equal("_alpha-123_", tok.StringWithoutQuotes);
        }

        [Fact]
        public void TestAlphaNumId()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<AlphaNumId>>());
            Assert.False(lexerRes.IsError);
            var lexer = lexerRes.Result;
            var r = lexer.Tokenize("alpha123");
            Assert.True(r.IsOk);
            Assert.Equal(2, r.Tokens.Count);
            var tok = r.Tokens[0];
            Assert.Equal(AlphaNumId.ID, tok.TokenID);
            Assert.Equal("alpha123", tok.StringWithoutQuotes);
        }

        [Fact]
        public void TestCustomId()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<CustomId>>());
            Assert.False(lexerRes.IsError);
            var lexer = lexerRes.Result;
            var r = lexer.Tokenize("a_-Bc ZyX-_");
            Assert.True(r.IsOk);
            Assert.Equal(3, r.Tokens.Count);
            var tok1 = r.Tokens[0];
            Assert.Equal(CustomId.ID, tok1.TokenID);
            Assert.Equal("a_-Bc",     tok1.Value);
            var tok2 = r.Tokens[1];
            Assert.Equal(CustomId.ID, tok2.TokenID);
            Assert.Equal("ZyX-_",     tok2.Value);
        }

        [Fact]
        public void TestCustomIdWithOther()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<CustomId>>());
            Assert.False(lexerRes.IsError);
            var lexer = lexerRes.Result;
            var r = lexer.Tokenize("_b_ -C-");
            Assert.True(r.IsOk);
            Assert.Equal(5, r.Tokens.Count);
            var tok1 = r.Tokens[0];
            Assert.Equal(CustomId.OTHER, tok1.TokenID);
            Assert.Equal("_",            tok1.Value);
            var tok2 = r.Tokens[1];
            Assert.Equal(CustomId.ID, tok2.TokenID);
            Assert.Equal("b_",        tok2.Value);
            var tok3 = r.Tokens[2];
            Assert.Equal(CustomId.OTHER, tok3.TokenID);
            Assert.Equal("-",            tok3.Value);
            var tok4 = r.Tokens[3];
            Assert.Equal(CustomId.ID, tok4.TokenID);
            Assert.Equal("C-",        tok4.Value);
        }

        [Fact]
        public void TestIgnoreWS()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<IgnoreWS>>());
            Assert.False(lexerRes.IsError);
            var lexer = lexerRes.Result;
            var r = lexer.Tokenize("\n \n");
            Assert.True(r.IsOk);
            Assert.Equal(2, r.Tokens.Count);
            var tok = r.Tokens[0];
            Assert.Equal(IgnoreWS.WS, tok.TokenID);
            Assert.Equal(" ", tok.Value);
        }

        [Fact]
        public void TestIgnoreEOL()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<IgnoreEOL>>());
            Assert.False(lexerRes.IsError);
            var lexer = lexerRes.Result;
            var r = lexer.Tokenize(" \n ");
            Assert.True(r.IsOk);
            Assert.Equal(2, r.Tokens.Count);
            var tok = r.Tokens[0];
            Assert.Equal(IgnoreEOL.EOL, tok.TokenID);
            Assert.Equal("\n", tok.Value);
        }

        [Fact]
        public void TestWhiteSpace()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<WhiteSpace>>());
            Assert.False(lexerRes.IsError);
            var lexer = lexerRes.Result;
            var r = lexer.Tokenize(" \t ");
            Assert.True(r.IsOk);
            Assert.Equal(2, r.Tokens.Count);
            var tok = r.Tokens[0];
            Assert.Equal(WhiteSpace.TAB, tok.TokenID);
            Assert.Equal("\t",           tok.Value);
        }

        [Fact]
        public void TestKeyWord()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<KeyWord>>());
            Assert.False(lexerRes.IsError);
            var lexer = lexerRes.Result;
            var r = lexer.Tokenize("keyword KeYwOrD");
            Assert.True(r.IsOk);
            Assert.Equal(3, r.Tokens.Count);
            var tok1 = r.Tokens[0];
            Assert.Equal(KeyWord.KEYWORD, tok1.TokenID);
            Assert.Equal("keyword",       tok1.Value);
            var tok2 = r.Tokens[1];
            Assert.Equal(default(KeyWord), tok2.TokenID);
            Assert.Equal("KeYwOrD",        tok2.Value);
        }

        [Fact]
        public void TestKeyWordIgnoreCase()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<KeyWordIgnoreCase>>());
            Assert.False(lexerRes.IsError);
            var lexer = lexerRes.Result;
            var r = lexer.Tokenize("keyword KeYwOrD");
            Assert.True(r.IsOk);
            Assert.Equal(3, r.Tokens.Count);
            var tok1 = r.Tokens[0];
            Assert.Equal(KeyWordIgnoreCase.KEYWORD, tok1.TokenID);
            Assert.Equal("keyword",                 tok1.Value);
            var tok2 = r.Tokens[1];
            Assert.Equal(KeyWordIgnoreCase.KEYWORD, tok2.TokenID);
            Assert.Equal("KeYwOrD",                 tok2.Value);
        }

        [Fact]
        public void TestStringDelimiters()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<StringDelimiters>>());
            Assert.True(lexerRes.IsOk);
            var lexer = lexerRes.Result;
            var r = lexer.Tokenize("'that''s it'");
            Assert.True(r.IsOk);
            Assert.NotNull(r.Tokens);
            Assert.NotEmpty(r.Tokens);
            Assert.Equal(2,r.Tokens.Count);
            var tok = r.Tokens[0];

            Assert.Equal("'that's it'",tok.Value);
            Assert.Equal("that's it",tok.StringWithoutQuotes);

        }

        [Fact]
        public void TestBadEmptyStringDelimiter()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<BadEmptyStringDelimiter>>());
            Assert.True(lexerRes.IsError);
            Assert.Single(lexerRes.Errors);
            var error = lexerRes.Errors[0];
            Assert.Equal(ErrorLevel.FATAL, error.Level);
            Assert.Contains("must be 1 character length", error.Message);
        }

        [Fact]
        public void TestBadLetterStringDelimiter()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<BadLetterStringDelimiter>>());
            Assert.True(lexerRes.IsError);
            Assert.Single(lexerRes.Errors);
            var error = lexerRes.Errors[0];
            Assert.Equal(ErrorLevel.FATAL, error.Level);
            Assert.Contains("can not start with a letter", error.Message);
        }

        [Fact]
        public void TestBadEscapeStringDelimiterTooLong()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<BadEscapeStringDelimiterTooLong>>());
            Assert.True(lexerRes.IsError);
            Assert.Single(lexerRes.Errors);
            var error = lexerRes.Errors[0];
            Assert.Equal(ErrorLevel.FATAL, error.Level);
            Assert.Contains("must be 1 character length", error.Message);
        }

        [Fact]
        public void TestBadEscapeStringDelimiterLetter()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<BadEscapeStringDelimiterLetter>>());
            Assert.True(lexerRes.IsError);
            Assert.Single(lexerRes.Errors);
            var error = lexerRes.Errors[0];
            Assert.Equal(ErrorLevel.FATAL, error.Level);
            Assert.Contains("can not start with a letter", error.Message);
        }

        [Fact]
        public void TestDefaultQuotedString()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<DefaultQuotedString>>());
            Assert.False(lexerRes.IsError);
            var lexer = lexerRes.Result;
            var source = "hello \\\"world ";
            var expected = "hello \"world ";
            var r = lexer.Tokenize($"\"{source}\"");
            Assert.True(r.IsOk);
            Assert.Equal(2, r.Tokens.Count);
            var tok = r.Tokens[0];
            Assert.Equal(DefaultQuotedString.DefaultString, tok.TokenID);
            Assert.Equal(expected, tok.StringWithoutQuotes);
        }

        [Fact]
        public void TestDoubleQuotedString()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<DoubleQuotedString>>());
            Assert.False(lexerRes.IsError);
            var lexer = lexerRes.Result;
            var source = "hello \\\"world ";
            var expected = "hello \"world ";
            var r = lexer.Tokenize($"\"{source}\"");
            Assert.True(r.IsOk);
            Assert.Equal(2, r.Tokens.Count);
            var tok = r.Tokens[0];
            Assert.Equal(DoubleQuotedString.DoubleString, tok.TokenID);
            Assert.Equal(expected, tok.StringWithoutQuotes);
        }


        [Fact]
        public void TestExtensions()
        {
            var lexerRes =
                LexerBuilder.BuildLexer(new BuildResult<ILexer<Extensions>>(), ExtendedGenericLexer.AddExtension);
            Assert.False(lexerRes.IsError);
            var lexer = lexerRes.Result as GenericLexer<Extensions>;
            Assert.NotNull(lexer);

            var r = lexer.Tokenize("20.02.2018 3.14");
            Assert.True(r.IsOk);

            Assert.Equal(3, r.Tokens.Count);
            Assert.Equal(Extensions.DATE, r.Tokens[0].TokenID);
            Assert.Equal("20.02.2018", r.Tokens[0].Value);
            Assert.Equal(Extensions.DOUBLE, r.Tokens[1].TokenID);
            Assert.Equal("3.14", r.Tokens[1].Value);

        }

        [Fact]
        public void TestExtensionsPreconditionFailure()
        {
            var lexerRes =
                LexerBuilder.BuildLexer(new BuildResult<ILexer<Extensions>>(), ExtendedGenericLexer.AddExtension);
            Assert.False(lexerRes.IsError);
            var lexer = lexerRes.Result as GenericLexer<Extensions>;
            Assert.NotNull(lexer);

            var r = lexer.Tokenize("0.0.2018");
            Assert.Equal(0,r.Error.Line);
            Assert.Equal(3,r.Error.Column);
            Assert.Equal('.',r.Error.UnexpectedChar);
        }

        [Fact]
        public void TestLexerError()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<AlphaNumDashId>>());
            Assert.False(lexerRes.IsError);
            var lexer = lexerRes.Result;
            var source = "hello world  2 + 2 ";
             var r = lexer.Tokenize(source);
             Assert.True(r.IsError);
            var error = r.Error;
            Assert.Equal(0, error.Line);
            Assert.Equal(13, error.Column);
            Assert.Equal('2', error.UnexpectedChar);
            Assert.Contains("Unrecognized symbol", error.ToString());
        }

        [Fact]
        public void TestManyString()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<ManyString>>());
            Assert.False(lexerRes.IsError);
            var lexer = lexerRes.Result;
            var string1 = "\"hello \\\"world \"";
            var expectString1 = "\"hello \"world \"";
            var string2 = "'that''s it'";
            var expectString2 = "'that's it'";
            var source1 = $"{string1} {string2}";
            var r = lexer.Tokenize(source1);
            Assert.True(r.IsOk);
            Assert.Equal(3, r.Tokens.Count);
            var tok1 = r.Tokens[0];
            Assert.Equal(ManyString.STRING, tok1.TokenID);
            Assert.Equal(expectString1, tok1.Value);

            var tok2 = r.Tokens[1];
            Assert.Equal(ManyString.STRING, tok2.TokenID);
            Assert.Equal(expectString2, tok2.Value);
        }

        [Fact]
        public void TestSelfEscapedString()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<SelfEscapedString>>());
            Assert.False(lexerRes.IsError);
            var lexer = lexerRes.Result as GenericLexer<SelfEscapedString>;
            Assert.NotNull(lexer);
            var r = lexer.Tokenize("'that''s it'");

            Assert.True(r.IsOk);
            var tokens = r.Tokens;
            Assert.Equal(2, tokens.Count);
            var tok = tokens[0];
            Assert.Equal(SelfEscapedString.STRING, tok.TokenID);
            Assert.Equal("'that's it'", tokens[0].Value);

            r = lexer.Tokenize("'et voilà'");
            Assert.True(r.IsOk);
            tokens = r.Tokens;
            Assert.Equal(2, tokens.Count);
            tok = tokens[0];
            Assert.Equal(SelfEscapedString.STRING, tok.TokenID);
            Assert.Equal("'et voilà'", tokens[0].Value);
        }

        [Fact]
        public void TestSingleQuotedString()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<SingleQuotedString>>());
            Assert.False(lexerRes.IsError);
            var lexer = lexerRes.Result;
            var source = "hello \\'world ";
            var expected = "hello 'world ";
            var r = lexer.Tokenize($"'{source}'");
            Assert.True(r.IsOk);
            Assert.Equal(2, r.Tokens.Count);
            var tok = r.Tokens[0];
            Assert.Equal(SingleQuotedString.SingleString, tok.TokenID);
            Assert.Equal(expected, tok.StringWithoutQuotes);
        }

        [Fact]
        public void TestTokenCallbacks()
        {
            var res = LexerBuilder.BuildLexer(new BuildResult<ILexer<CallbackTokens>>());
            Assert.False(res.IsError);
            var lexer = res.Result as GenericLexer<CallbackTokens>;
            CallBacksBuilder.BuildCallbacks(lexer);


            var r = lexer.Tokenize("aaa bbb");
            Assert.True(r.IsOk);
            var tokens = r.Tokens;
            Assert.Equal(3, tokens.Count);
            Assert.Equal("AAA", tokens[0].Value);
            Assert.Equal("BBB", tokens[1].Value);
            Assert.Equal(CallbackTokens.SKIP, tokens[1].TokenID);
        }

        [Fact]
        public void TestCharTokens()
        {
            var res = LexerBuilder.BuildLexer(new BuildResult<ILexer<CharTokens>>());
            Assert.False(res.IsError);
            var lexer = res.Result as GenericLexer<CharTokens>;
            var dump = lexer.ToString();
            var grpah = lexer.ToGraphViz();

            Assert.NotNull(lexer);
            var res1 = lexer.Tokenize("'c'");
            Assert.False(res1.IsError);
            Assert.Equal(2, res1.Tokens.Count);
            Token<CharTokens> token = res1.Tokens[0];
            Assert.Equal('c', token.CharValue);
            Assert.Equal(CharTokens.MyChar, token.TokenID);
            var lastToken = res1.Tokens.Last();

            var source = "'\\''";
            var res2 = lexer.Tokenize(source);
            Assert.False(res2.IsError);
            Assert.Equal(2, res2.Tokens.Count);
            token = res2.Tokens[0];
            Assert.Equal(source, token.Value);
            Assert.Equal(CharTokens.MyChar, token.TokenID);

            var sourceU = "'\\u0066'";
            var res3 = lexer.Tokenize(sourceU);
            Assert.False(res3.IsError);
            Assert.Equal(2, res3.Tokens.Count);
            token = res3.Tokens[0];
            Assert.Equal(sourceU, token.Value);
            Assert.Equal(CharTokens.MyChar, token.TokenID);
        }

        [Fact]
        public void TestCharTokenDelimiterConflict()
        {
            var res = LexerBuilder.BuildLexer(new BuildResult<ILexer<CharTokensConflicts>>());
            Assert.True(res.IsError);
            Assert.Equal(2,res.Errors.Count);

        }

        [Fact]
        public void TestIssue106()
        {
            var res = LexerBuilder.BuildLexer(new BuildResult<ILexer<Issue106>>());
            Assert.False(res.IsError);
            var lexer = res.Result as GenericLexer<Issue106>;
            var r = lexer.Tokenize("1.");
            Assert.True(r.IsOk);
            var tokens = r.Tokens;
            Assert.NotNull(tokens);
            Assert.Equal(3, tokens.Count);
            var token = tokens[0];
            Assert.Equal(Issue106.Integer, token.TokenID);
            Assert.Equal(1, token.IntValue);
            token = tokens[1];
            Assert.Equal(Issue106.Period, token.TokenID);
        }

        [Fact]
        public void TestIssue114()
        {
            var res = LexerBuilder.BuildLexer(new BuildResult<ILexer<Issue114>>());
            Assert.False(res.IsError);
            var lexer = res.Result as GenericLexer<Issue114>;

            var r = lexer?.Tokenize("// /&");
            Assert.True(r.IsError);

            Assert.Equal('&', r.Error.UnexpectedChar);

            r = lexer?.Tokenize("/&");


            Assert.Equal('&', r.Error.UnexpectedChar);

            r = lexer?.Tokenize("&/");
            Assert.True(r.IsError);

            Assert.Equal('&', r.Error.UnexpectedChar);

            r = lexer?.Tokenize("// &");
            Assert.True(r.IsError);
            Assert.Equal('&', r.Error.UnexpectedChar);
        }

        [Fact]
        public void TestIssue137()
        {
            var res = LexerBuilder.BuildLexer(new BuildResult<ILexer<Issue137>>());
            Assert.False(res.IsError);
            var lexer = res.Result;

            var result = lexer.Tokenize("--+");
            Assert.True(result.IsOk);
            Assert.Equal("BC0", ToTokens(result));

            result = lexer.Tokenize("--.");
            Assert.True(result.IsOk);
            Assert.Equal("BBA0", ToTokens(result));
        }

        [Fact]
        public void TestIssue138()
        {
            var res = LexerBuilder.BuildLexer(new BuildResult<ILexer<Issue138>>());
            Assert.False(res.IsError);
            var lexer = res.Result;

            var result = lexer.Tokenize(".");
            Assert.True(result.IsError);
            Assert.Equal("Lexical Error : Unrecognized symbol '.' at  (line 0, column 0).", result.Error.ErrorMessage);

            result = lexer.Tokenize("--");
            Assert.True(result.IsOk);
            Assert.Equal("BB0", ToTokens(result));
        }

        private static string ToTokens<T>(LexerResult<T> result) where T : struct
        {
            return result.Tokens.Aggregate(new StringBuilder(), (buf, token) => buf.Append(token.TokenID)).ToString();
        }
    }
}
