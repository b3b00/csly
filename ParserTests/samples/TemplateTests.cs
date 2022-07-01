using System.Collections.Generic;
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
            Assert.False(build.IsError);
            Assert.NotNull(build.Result);
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
            Assert.False(result.IsError);
            Assert.NotNull(result.Result);
            Assert.IsAssignableFrom<Template>(result.Result);
            var templated = result.Result.GetValue(context);
            Assert.Equal("hello-mùndo-billy--bob-this is the end",templated);
        }
    }
}