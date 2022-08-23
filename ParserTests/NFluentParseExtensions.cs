using System.Collections.Generic;
using System.Linq;
using NFluent;
using NFluent.Extensibility;
using sly.buildresult;
using sly.lexer;
using sly.parser;

namespace ParserTests
{
    public static class NFluentParseExtensions
    {
        public static ICheckLink<ICheck<ParseResult<IN,OUT>>> ParseIsOk<IN,OUT>(this ICheck<ParseResult<IN,OUT>> context) where IN : struct
        {
            ExtensibilityHelper.BeginCheck(context)
                .FailWhen(sut => sut.IsError, "parse failed")
                .FailWhen(sut => sut.Result == null, "parse result is null")
                .OnNegate("parse expected to fail.")
                .EndCheck();
            return ExtensibilityHelper.BuildCheckLink(context);
        }

        public static ICheckLink<ICheck<BuildResult<T>>> IsOkParser<T>(this ICheck<BuildResult<T>> context) 
        {
            ExtensibilityHelper.BeginCheck(context)
                .FailWhen(sut => sut.IsError, "parse failed")
                .FailWhen(sut => sut.Result == null, "parser result is null")
                .OnNegate("parser expected to fail.")
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

        public static ICheckLink<ICheck<IEnumerable<T>>> IsSingle<T>(this ICheck<IEnumerable<T>> context)
        {
            ExtensibilityHelper.BeginCheck(context)
                .FailWhen(sut => !sut.Any() && sut.Count() != 1,
                    "{expected} is expected to have 1 and only 1 element.");
            return ExtensibilityHelper.BuildCheckLink(context);
        }
    }
}