using System;
using System.Linq;
using NFluent;
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
            Check.That(queryExpression).IsInstanceOf<GroupExpression>();
            Check.That(queryExpression.ToString()).IsEqualTo("(java AND dotnet)");
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
            
            var graphviz = new GraphVizEBNFSyntaxTreeVisitor<EarlyEosToken>();
            var root = graphviz.VisitTree(queryExpression.SyntaxTree);
            string graph = graphviz.Graph.Compile();
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