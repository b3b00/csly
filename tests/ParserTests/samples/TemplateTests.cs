using System.Collections.Generic;
using NFluent;
using SimpleTemplate;
using SimpleTemplate.model;
using sly.parser;
using sly.parser.generator;
using Xunit;

namespace ParserTests.samples
{
    public class TemplateTests
    {

        public Parser<TemplateLexer, ITemplate> GetParser()
        {
            var builder = new ParserBuilder<TemplateLexer, ITemplate>();
            var instance = new TemplateParser();
            var build = builder.BuildParser(instance, ParserType.EBNF_LL_RECURSIVE_DESCENT, "template");
            Check.That(build.IsError).IsFalse();
            Check.That(build.Result).IsNotNull();
            return build.Result;
        }

        [Fact]
        public void BasicTemplateTest()
        {
            var parser = GetParser();
            var source = @"hello-{=world=}-billy-{% if (a == 1) %}-bob-{%else%}-boubou-{%endif%}this is the end";
            
            var context = new Dictionary<string, object>()
            {
                { "world", "mùndo" },
                { "a", 1 }
            };
            
            var result = parser.Parse(source);
            Check.That(result.IsError).IsFalse();
            Check.That(result.Result).IsNotNull();
            Check.That(result.Result).IsInstanceOf<Template>();
            var templated = result.Result.GetValue(context);
            Check.That(templated).IsEqualTo("hello-mùndo-billy--bob-this is the end");
        }
    }
}