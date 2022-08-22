using NFluent;
using NFluent.Extensibility;
using sly.buildresult;
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
    }
}