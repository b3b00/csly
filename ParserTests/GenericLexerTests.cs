using sly.parser;
using expressionparser;
using jsonparser;
using jsonparser.JsonModel;
using sly.lexer;
using sly.parser.generator;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Xunit;
using sly.buildresult;

namespace ParserTests
{

    public enum DoubleQuotedString {

         [Lexeme(GenericToken.String,"\"")]
        DoubleString
    }

    public enum SingleQuotedString {

         [Lexeme(GenericToken.String,"'")]
        SingleString
    }

    public enum DefaultQuotedString {

         [Lexeme(GenericToken.String)]
        DefaultString
    }

    public enum AlphaId
    {
        [Lexeme(GenericToken.Identifier,IdentifierType.Alpha)]
        ID
    }

    public enum AlphaNumId
    {
        [Lexeme(GenericToken.Identifier,IdentifierType.AlphaNumeric)]
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
        public void TestDoubleQuotedString() {
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
        public void TestSingleQuotedString() {
            var lexerRes = LexerBuilder.BuildLexer<SingleQuotedString>(new BuildResult<ILexer<SingleQuotedString>>());
            Assert.False(lexerRes.IsError);
            var lexer = lexerRes.Result;
            string source = "hello \\\"world ";
            var r = lexer.Tokenize($"\"{source}\"").ToList();
            Assert.Equal(1, r.Count);
            Token<SingleQuotedString> tok = r[0];
            Assert.Equal(SingleQuotedString.SingleString, tok.TokenID);
            Assert.Equal(source, tok.StringWithoutQuotes);
        }

[Fact]
        public void TestDefaultQuotedString() {
            var lexerRes = LexerBuilder.BuildLexer<DefaultQuotedString>(new BuildResult<ILexer<DefaultQuotedString>>());
            Assert.False(lexerRes.IsError);
            var lexer = lexerRes.Result;
            string source = "hello \\\"world ";
            var r = lexer.Tokenize($"'{source}'").ToList();
            Assert.Equal(1, r.Count);
            Token<DefaultQuotedString> tok = r[0];
            Assert.Equal(DefaultQuotedString.DefaultString, tok.TokenID);
            Assert.Equal(source, tok.StringWithoutQuotes);
        }
    }
}
