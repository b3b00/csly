using SimpleTemplate.model;
using sly.sourceGenerator;

namespace SimpleTemplate;

[ParserGenerator(typeof(TemplateLexer), typeof(TemplateParser), typeof(ITemplate))]
public partial class TemplateParserGenerator : AbstractParserGenerator<TemplateLexer>
{
        
}