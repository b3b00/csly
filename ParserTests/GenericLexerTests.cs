using sly.parser;
using expressionparser;
using jsonparser;
using jsonparser.JsonModel;
using sly.lexer;
using sly.parser.generator;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System;
using Xunit;
using sly.buildresult;
using sly.lexer.fsm;

namespace ParserTests
{

    public enum Extensions
    {
        [Lexeme(GenericToken.Extension)]
        DATE,

        [Lexeme(GenericToken.Extension)]
        CHAINE,

        [Lexeme(GenericToken.Double)]
        DOUBLE,

        
    }


    


    public static class ExtendedGenericLexer
    {



        public static bool CheckDate(string value)
        {
            bool ok = false;
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
                NodeCallback<GenericToken> callback = (FSMMatch<GenericToken> match) =>
                {
                    match.Properties[GenericLexer<Extensions>.DerivedToken] = Extensions.DATE;
                    return match;
                };

                var fsmBuilder = lexer.FSMBuilder;

                // TODO
                fsmBuilder.GoTo(GenericLexer<Extensions>.in_double)
                .Transition('.', CheckDate)
                .Mark("start_date")
                .RepetitionTransition(4, "[0-9]")                
                .End(GenericToken.Extension)
                .CallBack(callback);
            }
            else if (token == Extensions.CHAINE) {
                NodeCallback<GenericToken> callback = (FSMMatch<GenericToken> match) =>
                {
                    match.Properties[GenericLexer<Extensions>.DerivedToken] = Extensions.CHAINE;
                    return match;
                };

                char quote = '\'';
                NodeAction collapseDelimiter = (string value) => {
                    if (value.EndsWith(""+quote+quote)) {
                        return value.Substring(0,value.Length-2)+quote;
                    }
                    return value;
                };
                
                var exceptQuote = new char[]{quote};
                string in_string = "in_string_same";
                string escaped = "escaped_same";
                string delim = "delim_same";

                var fsmBuilder = lexer.FSMBuilder;

                fsmBuilder.GoTo(GenericLexer<Extensions>.start)                
                .Transition(quote)
                .Mark(in_string)
                .ExceptTransitionTo(exceptQuote,in_string)
                .Transition(quote)

                .Mark(escaped)
                .End(GenericToken.String)
                .CallBack(callback)
                .Transition(quote)

                .Mark(delim)
                .Action(collapseDelimiter)                
                .ExceptTransitionTo(exceptQuote,in_string);
                fsmBuilder.GoTo(delim)
                .TransitionTo(quote,escaped)

                .ExceptTransitionTo(exceptQuote,in_string);
                
            }
        }

    }

    public enum BadLetterStringDelimiter
    {
        [Lexeme(GenericToken.String, "a")]
        Letter
    }

    public enum BadEmptyStringDelimiter
    {
        [Lexeme(GenericToken.String, "")]
        Empty
    }

    public enum DoubleQuotedString
    {

        [Lexeme(GenericToken.String, "\"")]
        DoubleString
    }

    public enum SingleQuotedString
    {

        [Lexeme(GenericToken.String, "'")]
        SingleString
    }

    public enum DefaultQuotedString
    {

        [Lexeme(GenericToken.String)]
        DefaultString
    }

    public enum SelfEscapedString {
        [Lexeme(GenericToken.String,"'","'")]
        STRING
    }

    public enum ManyString {
        [Lexeme(GenericToken.String,"'","'")]
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

    public class GenericLexerTests
    {


        [Fact]
        public void TestExtensions()
        {
            var lexerRes = LexerBuilder.BuildLexer<Extensions>(new BuildResult<ILexer<Extensions>>(), ExtendedGenericLexer.AddExtension);
            Assert.False(lexerRes.IsError);
            var lexer = lexerRes.Result as GenericLexer<Extensions>;
            Assert.NotNull(lexer);

            List<Token<Extensions>> tokens = lexer.Tokenize("20.02.2018 3.14").ToList();
            Assert.Equal(2, tokens.Count);
            Assert.Equal(Extensions.DATE, tokens[0].TokenID);
            Assert.Equal("20.02.2018", tokens[0].Value);
            Assert.Equal(Extensions.DOUBLE, tokens[1].TokenID);
            Assert.Equal("3.14", tokens[1].Value);

            tokens = lexer.Tokenize("'that''s it'").ToList();
            Assert.Equal(1,tokens.Count);
            Token<Extensions> tok = tokens[0];
            Assert.Equal(Extensions.CHAINE,tok.TokenID);
            Assert.Equal("'that's it'",tokens[0].Value);

            tokens = lexer.Tokenize("'et voilà'").ToList();
            Assert.Equal(1,tokens.Count);
            tok = tokens[0];
            Assert.Equal(Extensions.CHAINE,tok.TokenID);
            Assert.Equal("'et voilà'",tokens[0].Value);
        }

       

        [Fact]
        public void TestAlphaId()
        {
            var lexerRes = LexerBuilder.BuildLexer<AlphaId>(new BuildResult<ILexer<AlphaId>>());
            Assert.False(lexerRes.IsError);
            var lexer = lexerRes.Result;
            var r = lexer.Tokenize("alpha").ToList();
            Assert.Equal(1, r.Count);
            Token<AlphaId> tok = r[0];
            Assert.Equal(AlphaId.ID, tok.TokenID);
            Assert.Equal("alpha", tok.StringWithoutQuotes);
            ;
        }



        [Fact]
        public void TestAlphaNumId()
        {
            var lexerRes = LexerBuilder.BuildLexer<AlphaNumId>(new BuildResult<ILexer<AlphaNumId>>());
            Assert.False(lexerRes.IsError);
            var lexer = lexerRes.Result;
            var r = lexer.Tokenize("alpha123").ToList();
            Assert.Equal(1, r.Count);
            Token<AlphaNumId> tok = r[0];
            Assert.Equal(AlphaNumId.ID, tok.TokenID);
            Assert.Equal("alpha123", tok.StringWithoutQuotes);
            ;
        }

        [Fact]
        public void TestAlphaNumDashId()
        {
            var lexerRes = LexerBuilder.BuildLexer<AlphaNumDashId>(new BuildResult<ILexer<AlphaNumDashId>>());
            Assert.False(lexerRes.IsError);
            var lexer = lexerRes.Result;
            var r = lexer.Tokenize("alpha-123_").ToList();
            Assert.Equal(1, r.Count);
            Token<AlphaNumDashId> tok = r[0];
            Assert.Equal(AlphaNumDashId.ID, tok.TokenID);
            Assert.Equal("alpha-123_", tok.StringWithoutQuotes);
            ;
        }

        [Fact]
        public void TestAlphaNumDashIdStartsWithUnderscore()
        {
            var lexerRes = LexerBuilder.BuildLexer<AlphaNumDashId>(new BuildResult<ILexer<AlphaNumDashId>>());
            Assert.False(lexerRes.IsError);
            var lexer = lexerRes.Result;
            var r = lexer.Tokenize("_alpha-123_").ToList();
            Assert.Equal(1, r.Count);
            Token<AlphaNumDashId> tok = r[0];
            Assert.Equal(AlphaNumDashId.ID, tok.TokenID);
            Assert.Equal("_alpha-123_", tok.StringWithoutQuotes);
            ;
        }

        [Fact]
        public void TestDoubleQuotedString()
        {
            var lexerRes = LexerBuilder.BuildLexer<DoubleQuotedString>(new BuildResult<ILexer<DoubleQuotedString>>());
            Assert.False(lexerRes.IsError);
            var lexer = lexerRes.Result;
            string source = "hello \\\"world ";
            var r = lexer.Tokenize($"\"{source}\"").ToList();
            Assert.Equal(1, r.Count);
            Token<DoubleQuotedString> tok = r[0];
            Assert.Equal(DoubleQuotedString.DoubleString, tok.TokenID);
            Assert.Equal(source, tok.StringWithoutQuotes);
        }

        [Fact]
        public void TestSingleQuotedString()
        {
            var lexerRes = LexerBuilder.BuildLexer<SingleQuotedString>(new BuildResult<ILexer<SingleQuotedString>>());
            Assert.False(lexerRes.IsError);
            var lexer = lexerRes.Result;
            string source = "hello \\'world ";
            var r = lexer.Tokenize($"'{source}'").ToList();
            Assert.Equal(1, r.Count);
            Token<SingleQuotedString> tok = r[0];
            Assert.Equal(SingleQuotedString.SingleString, tok.TokenID);
            Assert.Equal(source, tok.StringWithoutQuotes);
        }

        [Fact]
        public void TestDefaultQuotedString()
        {
            var lexerRes = LexerBuilder.BuildLexer<DefaultQuotedString>(new BuildResult<ILexer<DefaultQuotedString>>());
            Assert.False(lexerRes.IsError);
            var lexer = lexerRes.Result;
            string source = "hello \\\"world ";
            var r = lexer.Tokenize($"\"{source}\"").ToList();
            Assert.Equal(1, r.Count);
            Token<DefaultQuotedString> tok = r[0];
            Assert.Equal(DefaultQuotedString.DefaultString, tok.TokenID);
            Assert.Equal(source, tok.StringWithoutQuotes);
        }

         [Fact]
        public void TestSelfEscapedString()
        {
            var lexerRes = LexerBuilder.BuildLexer<SelfEscapedString>(new BuildResult<ILexer<SelfEscapedString>>());
            Assert.False(lexerRes.IsError);
            var lexer = lexerRes.Result as GenericLexer<SelfEscapedString>;
            Assert.NotNull(lexer);
            var tokens = lexer.Tokenize("'that''s it'").ToList();
            Assert.Equal(1,tokens.Count);
            Token<SelfEscapedString> tok = tokens[0];
            Assert.Equal(SelfEscapedString.STRING,tok.TokenID);
            Assert.Equal("'that's it'",tokens[0].Value);

            tokens = lexer.Tokenize("'et voilà'").ToList();
            Assert.Equal(1,tokens.Count);
            tok = tokens[0];
            Assert.Equal(SelfEscapedString.STRING,tok.TokenID);
            Assert.Equal("'et voilà'",tokens[0].Value);

        }

        [Fact]
        public void TestManyString()
        {
            var lexerRes = LexerBuilder.BuildLexer<ManyString>(new BuildResult<ILexer<ManyString>>());
            Assert.False(lexerRes.IsError);
            var lexer = lexerRes.Result;
            string string1 ="\"hello \\\"world \"";
            string string2 = "'that''s it'";
            string source1 = $"{string1} {string2}";
            var r = lexer.Tokenize(source1).ToList();
            Assert.Equal(2, r.Count);
            Token<ManyString> tok1 = r[0];
            Assert.Equal(ManyString.STRING, tok1.TokenID);
            Assert.Equal(string1, tok1.Value);

            Token<ManyString> tok2 = r[1];
            Assert.Equal(ManyString.STRING, tok2.TokenID);
            Assert.Equal(string2, tok2.Value);
        }

        [Fact]
        public void TestBadLetterStringDelimiter()
        {
            var lexerRes = LexerBuilder.BuildLexer<BadLetterStringDelimiter>(new BuildResult<ILexer<BadLetterStringDelimiter>>());
            Assert.True(lexerRes.IsError);
            Assert.Equal(1, lexerRes.Errors.Count);
            var error = lexerRes.Errors[0];
            Assert.Equal(ErrorLevel.FATAL, error.Level);
            Assert.True(error.Message.Contains("can not start with a letter"));
        }

        [Fact]
        public void TestBadEmptyStringDelimiter()
        {
            var lexerRes = LexerBuilder.BuildLexer<BadEmptyStringDelimiter>(new BuildResult<ILexer<BadEmptyStringDelimiter>>());
            Assert.True(lexerRes.IsError);
            Assert.Equal(1, lexerRes.Errors.Count);
            var error = lexerRes.Errors[0];
            Assert.Equal(ErrorLevel.FATAL, error.Level);
            Assert.True(error.Message.Contains("must be 1 character length"));
        }

        [Fact]
        public void TestLexerError()
        {
            var lexerRes = LexerBuilder.BuildLexer<AlphaNumDashId>(new BuildResult<ILexer<AlphaNumDashId>>());
            Assert.False(lexerRes.IsError);
            var lexer = lexerRes.Result;
            string source = "hello world  2 + 2 ";
            var errException = Assert.Throws<LexerException<GenericToken>>(() => lexer.Tokenize(source).ToList());
            var error = errException.Error;
            Assert.Equal(0, error.Line);
            Assert.Equal(13, error.Column);
            Assert.Equal('2', error.UnexpectedChar);
            Assert.Contains("Unrecognized symbol",error.ToString());




        }
    }
}
