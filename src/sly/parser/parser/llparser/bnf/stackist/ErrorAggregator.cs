using System;
using System.Collections.Generic;
using System.Linq;

namespace sly.parser.llparser.bnf.stackist;

public class ErrorAggregator
{
    public static List<UnexpectedTokenSyntaxError<IN>> Aggregate<IN>(List<UnexpectedTokenSyntaxError<IN>> errors) where IN : struct, Enum
    {
        List<UnexpectedTokenSyntaxError<IN>> errorList = new();
        var groups = errors.GroupBy(x => x.UnexpectedToken.ToString());
        foreach (IGrouping<string, UnexpectedTokenSyntaxError<IN>> g in groups)
        {
            var first = g.First();
            var expected = g.SelectMany(x => x.ExpectedTokens).Distinct().ToList();
            var error = new UnexpectedTokenSyntaxError<IN>(first.UnexpectedToken, first.I18n, expected);
            errorList.Add(error);
        }
        
        return errorList;
    }
}