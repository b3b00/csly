using System;
using System.Linq;
using sly.buildresult;
using sly.lexer;
using sly.lexer.fsm;
using Xunit;
using GenericLexerWithCallbacks;

namespace ParserTests.lexer
{
    public enum Extensions
    {
        [Lexeme(GenericToken.Extension)] DATE,

        [Lexeme(GenericToken.Double)] DOUBLE
    }


    public static class ExtendedGenericLexer
    {
        public static bool CheckDate(string value)
        {
            var ok = false;
            if (value.Length == 5)
            {
                ok = char.IsDigit(value[0]);
                ok = ok && char.IsDigit(value[1]);
                ok = ok && value[2] == '.';
                ok = ok && char.IsDigit(value[3]);
                ok = ok && char.IsDigit(value[4]);
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

    public enum Issue106
    {
        [Lexeme(GenericToken.Int)]
        Integer = 5,

        [Lexeme(GenericToken.Double)]
        Double = 6,
    }

    public enum Issue114
    {
        [Lexeme(GenericToken.SugarToken, "//")]
        First = 1,

        [Lexeme(GenericToken.SugarToken, "/*")]
        Second = 2
    }

    public class GenericLexerTests
    {
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
            ;
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
            ;
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
            ;
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
            ;
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
            CallBacksBuilder.BuildCallbacks<CallbackTokens>(lexer);


            var r = lexer.Tokenize("aaa bbb");
            Assert.True(r.IsOk);
            var tokens = r.Tokens;
            Assert.Equal(3, tokens.Count);
            Assert.Equal("AAA", tokens[0].Value);
            Assert.Equal("BBB", tokens[1].Value);
            Assert.Equal(CallbackTokens.SKIP, tokens[1].TokenID);
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
            Assert.Equal(2, tokens.Count);
            var token = tokens[0];
            Assert.Equal(Issue106.Integer, token.TokenID);
            Assert.Equal(1, token.IntValue);
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
    }
}
