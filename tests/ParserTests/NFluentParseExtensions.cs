using System.Collections.Generic;
using System.Linq;
using NFluent;
using NFluent.Extensibility;
using NFluent.Kernel;
using sly.buildresult;
using sly.lexer;
using sly.parser;

namespace ParserTests
{
    public static class NFluentParseExtensions
    {
        public static ICheckLink<ICheck<ParseResult<IN,OUT>>> IsOkParsing<IN,OUT>(this ICheck<ParseResult<IN,OUT>> context) where IN : struct
        {
            ExtensibilityHelper.BeginCheck(context)
                .FailWhen(sut => sut.IsError, $"parse failed")
                .FailWhen(sut => sut.Result == null  && !sut.SyntaxTree.IsEpsilon, "parse result is null")
                .OnNegate("parse expected to fail.")
                .EndCheck();
            return ExtensibilityHelper.BuildCheckLink(context);
        }
        
        public static ICheckLink<ICheck<LexerResult<IN>>> IsOkLexing<IN>(this ICheck<LexerResult<IN>> context) where IN : struct
        {
            ExtensibilityHelper.BeginCheck(context)
                .FailWhen(sut => sut == null, "lexer result is null")
                .FailWhen(sut => sut.IsError, "lexing failed")
                .FailWhen(sut => sut.Tokens == null, "lexing result is null")
                .OnNegate("parse expected to fail.")
                .EndCheck();
            return ExtensibilityHelper.BuildCheckLink(context);
        }

        public static ICheckLink<ICheck<BuildResult<T>>> IsOk<T>(this ICheck<BuildResult<T>> context) 
        {
            ExtensibilityHelper.BeginCheck(context)
                .FailWhen(sut => sut.IsError, "parse failed")
                .FailWhen(sut => sut.Result == null, "parser result is null")
                .OnNegate("parser expected to fail.")
                .EndCheck();
            return ExtensibilityHelper.BuildCheckLink(context);
        }
        
        public static ICheckLink<ICheck<BuildResult<T>>> HasError<T>(this ICheck<BuildResult<T>> context, ErrorCodes expectedErrorCode, string expectedMessageNeedle) 
        {
            ExtensibilityHelper.BeginCheck(context)
                .FailWhen(sut => !sut.IsError, "parser has not failed.")
                .FailWhen(sut => !sut.Errors.Any() , "parser has no error")
                .FailWhen(sut => !sut.Errors.Exists(x => x.Code == expectedErrorCode && x.Message.Contains(expectedMessageNeedle)),"error {expected} not found in {checked}")
                .DefineExpectedValue($"{expectedErrorCode} : >{expectedMessageNeedle}<")
                .OnNegate("error {expected} found.")
                .EndCheck();
            return ExtensibilityHelper.BuildCheckLink(context);
        }

        public static ICheckLink<ICheck<Token<T>>> IsEqualTo<T>(this ICheck<Token<T>> context, T expectedTokenId, string expectedValue) where T: struct
        {
            ExtensibilityHelper.BeginCheck(context)
                .FailWhen(sut => !sut.TokenID.Equals(expectedTokenId), "expecting {expected} found {checked}.")
                .FailWhen(sut => !sut.Value.Equals(expectedValue), "expecting {expected} found {checked}.")
                .OnNegate("token is exact")
                .DefineExpectedValue($"{expectedTokenId} : >{expectedValue}<")
                .EndCheck();
            return ExtensibilityHelper.BuildCheckLink(context);
        }
        
        public static ICheckLink<ICheck<Token<T>>> IsEqualTo<T>(this ICheck<Token<T>> context, T expectedTokenId, string expectedValue, int expectedLine, int expectedColumn) where T: struct
        {
            ExtensibilityHelper.BeginCheck(context)
                .FailWhen(sut => !sut.TokenID.Equals(expectedTokenId), "expecting {expected} found {checked}.")
                .FailWhen(sut => !sut.Value.Equals(expectedValue), "expecting {expected} found {checked}.")
                .FailWhen(sut => !sut.Position.Line.Equals(expectedLine), "expecting {expected} found {checked}.")
                .FailWhen(sut => !sut.Position.Column.Equals(expectedColumn), "expecting {expected} found {checked}.")
                .OnNegate("token is exact")
                .DefineExpectedValue($"{expectedTokenId} : >{expectedValue}<")
                .EndCheck();
            return ExtensibilityHelper.BuildCheckLink(context);
        }

        public static ICheckLink<ICheck<IEnumerable<T>>> IsSingle<T>(this ICheck<IEnumerable<T>> context)
        {
            ExtensibilityHelper.BeginCheck(context)
                .FailWhen(sut => !sut.Any() && sut.Count() != 1,
                    "{expected} is expected to have 1 and only 1 element.");
            return ExtensibilityHelper.BuildCheckLink(context);
        }
    }
}