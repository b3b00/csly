using System;
using System.Linq;
using System.Text.RegularExpressions;
using NFluent;
using sly.parser.generator;
using Xunit;

namespace ParserTests.Issue225_IndexOutOfRangeException
{
    public class IndexOutOfRangeTests
    {
        [Fact]
        public void Should_Not_IndexOutOfRange()
        {
            var queryExpression = Parse("(java AND dotnet)");
            Check.That(queryExpression).IsInstanceOf<GroupExpression>();
            Check.That(queryExpression.ToString()).IsEqualTo("(java AND dotnet)");
        }

        private static Expression Parse(string query)
        {
            var parserInstance = new IndexOutOfRangeParser();
            var builder = new ParserBuilder<IndexOutOfRangeToken, Expression>();

            var buildResult =
                builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, "expression");
            // if (buildResult.IsError)
            //     throw new AggregateException(
            //         buildResult.Errors
            //             .Select(e => new Exception($"{e.Level} {e.Code} {e.Message}"))
            //     );
            var parser = buildResult.Result;

            var queryExpression = parser.Parse(query.Trim());
            if (queryExpression.IsError)
                throw new AggregateException(
                    queryExpression.Errors
                        .Select(e => new Exception(e.ErrorMessage))
                );

            return queryExpression.Result;
        }
    }
}
