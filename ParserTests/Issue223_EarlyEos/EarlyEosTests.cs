using System;
using System.Linq;
using sly.parser.generator;
using sly.parser.generator.visitor;
using Xunit;

namespace ParserTests.Issue223_EarlyEos
{
    public class EarlyEosTests
    {
        [Fact]
        public void Early_EOS_NRE()
        {
            var queryExpression = Parse("( java AND dotnet )");

            
            Assert.IsType<GroupExpression>(queryExpression);
            Assert.Equal("(java AND dotnet)", queryExpression.ToString());
        }

        private static Expression Parse(string query)
        {
            var parserInstance = new EarlyEosParser();
            var builder = new ParserBuilder<EarlyEosToken, Expression>();

            var buildResult =
                builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, "expression");
            if (buildResult.IsError)
                throw new AggregateException(
                    buildResult.Errors
                        .Select(e => new Exception($"{e.Level} {e.Code} {e.Message}"))
                );
            var parser = buildResult.Result;

            var queryExpression = parser.Parse(query.Trim());
            
            GraphVizEBNFSyntaxTreeVisitor<EarlyEosToken> graphVisitor =
                new GraphVizEBNFSyntaxTreeVisitor<EarlyEosToken>();
            var graph = graphVisitor.VisitTree(queryExpression.SyntaxTree);
            string gviz = graphVisitor.Graph.Compile();
            
            ;
            
            if (queryExpression.IsError)
                throw new AggregateException(
                    queryExpression.Errors
                        .Select(e => new Exception(e.ErrorMessage))
                );

            return queryExpression.Result;
        }
    }
}