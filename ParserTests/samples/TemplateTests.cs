using System.Collections.Generic;
using SimpleTemplate;
using sly.parser;
using sly.parser.generator;
using Xunit;

namespace ParserTests.samples
{
    public class TemplateTests
    {

        public Parser<TemplateLexer, string> GetParser()
        {
            var builder = new ParserBuilder<TemplateLexer, string>();
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
            var source = @"hello-{=world=}-billy-{% if (a == 1.0) %}-bob-{%else%}-boubou-{%endif%}this is the end";
            
            var context = new Dictionary<string, string>()
            {
                { "world", "monde" },
                { "a", "1.0" }
            };
            
            var result = parser.ParseWithContext(source,context);
            Assert.False(result.IsError);
            Assert.Equal("hello-monde-billy--bob-this is the end",result.Result);
        }
    }
}