using System.Collections.Generic;
using System.IO;
using System.Reflection;
using SharpFileSystem.FileSystems;
using sly.sourceGenerator.staticParserTemplates;

namespace sly.sourceGenerator;

public class TemplateEngine
{
    // private readonly EmbeddedResourceFileSystem _resourceFileSystem = null;

    private readonly string _lexerName;
    
    private readonly string _parserName;
    
    private readonly string _outputType;

    private const string LEXER_PLACEHOLDER = "<#LEXER#>";
    private const string PARSER_PLACEHOLDER = "<#PARSER#>";
    private const string OUTPUT_PLACEHOLDER = "<#OUTPUT#>";
    private const string NAME_PLACEHOLDER = "<#NAME#>";
    
    public TemplateEngine(string lexerName, string parserName, string outputType)
    {
        _lexerName = lexerName;
        _parserName = parserName;
        _outputType = outputType;
        //_resourceFileSystem =  new EmbeddedResourceFileSystem(typeof(TemplateEngine).Assembly);
    }
    
    public string GetTemplate(string name)
    {
        if (Templates.TryGetValue(name, out var template))
        {
            return template;
        }

        return "";
        return "GOODBYE";
                 Assembly assembly = GetType().Assembly;
        using (var stream = assembly.GetManifestResourceStream($"sly.sourceGenerator.TemplateEngine.{name}"))
        {
            using (var reader = new StreamReader(stream))
            {
                var content = reader.ReadToEnd();
                return content;
            }
        }
    }

    private Dictionary<string, string> Templates = new Dictionary<string, string>()
    {
        { "helpers.txt", HelpersTemplate.Template },
        {"explicitterminalParser.txt", ExplicitTerminalParserTemplate.Template},
        {"nonTerminalClause.txt", NonTerminalClauseTemplate.Template},
        {"nonTerminalParser.txt", NonTerminalParserTemplate.MainTemplate},
        {"ruleCall.txt", NonTerminalParserTemplate.RuleCallTemplate},
        {"parser.txt", ParserTemplate.Template},
        {"ruleParser.txt", RuleParserTemplate.Template},
        {"terminalClause.txt", TerminalClauseTemplate.Template},
        {"terminalParser.txt", TerminalParserTemplate.Template}
    };

    public string ApplyTemplate(string templateName, string name = null, bool substitute = true, Dictionary<string, string> additional = null)
    {
        var template = GetTemplate(templateName);
        if (substitute)
        {
            template = Substitute(template, LEXER_PLACEHOLDER, _lexerName);
            template = Substitute(template, PARSER_PLACEHOLDER, _parserName);
            template = Substitute(template, OUTPUT_PLACEHOLDER, _outputType);
            if (name != null)
            {
                template = Substitute(template, NAME_PLACEHOLDER, name);
            }

            if (additional != null)
            {
                foreach (var pair in additional)
                {
                    template = Substitute(template, pair.Key, pair.Value);
                }
            }
        }
        return template;
    }

    private string Substitute(string content, string placeholder, string value)
    {
        return content.Replace(placeholder, value);
    }
}