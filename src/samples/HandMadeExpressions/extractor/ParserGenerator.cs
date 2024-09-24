using System.Collections.Immutable;
using System.Text;
using handExpressions.ebnfparser.model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using sly.sourceGenerator;

namespace handExpressions.extractor;

public class ParserGenerator
{
    private readonly ClassDeclarationSyntax _parserClass;

    private readonly EnumDeclarationSyntax _lexerEnumDeclarationSyntax;

    private readonly string _outputType;

    private readonly string _lexerType;

    public ParserGenerator(EnumDeclarationSyntax lexerEnum, ClassDeclarationSyntax parserClass, string outputType)
    {
        _lexerEnumDeclarationSyntax = lexerEnum;
        _lexerType = lexerEnum.Identifier.ToString();
        _parserClass = parserClass;
        _outputType = outputType;
    }

    private List<string> ExtractTokens()
    {
        var tokens = _lexerEnumDeclarationSyntax.Members.Cast<EnumMemberDeclarationSyntax>()
            .Select(x => x.Identifier.Text).ToList();
        return tokens;
    }

    private void NumberRules(Dictionary<string, List<Rule>> nonTerminals)
    {
        foreach (var nonTerminal in nonTerminals)
        {
            if (nonTerminal.Value.Count > 1)
            {
                for (int i = 0; i < nonTerminal.Value.Count; i++)
                {
                    nonTerminal.Value[i].Number = i;
                }
            }
        }
    }

    public string Generate()
    {
        var tokens = ExtractTokens();
        var extractor = new ParserConfigurationExtractor(tokens);
        var rules = extractor.ExtractRules(_parserClass);
        var nonTerminals = rules.GroupBy(x => x.NonTerminalName).ToDictionary(x => x.Key, x => x.ToList());
        NumberRules(nonTerminals);

        RuleGenerator ruleGenerator = new RuleGenerator(_lexerEnumDeclarationSyntax.Identifier.Text, _outputType);
        ParserModelWalker<string> modelWalker = new ParserModelWalker<string>(ruleGenerator);

        StringBuilder builder = new StringBuilder();

        var usings = _parserClass.GetCompilationUnit().Usings.Select(x => x.ToString()).ToList();

        builder.AppendLine("using System;");
        builder.AppendLine("using sly.lexer;");
        builder.AppendLine("using handExpressions;");
        builder.AppendLine("using sly.parser.generator.visitor;");
        builder.Append("using sly.parser.syntax.tree;");
        builder.AppendLine($"using {_parserClass.GetNameSpace()};");
        foreach (var usingStatement in usings)
        {
            builder.AppendLine(usingStatement);
        }

        builder.AppendLine("");
        builder.AppendLine($"namespace {_parserClass.GetNameSpace()};");
        builder.AppendLine("");
        builder.AppendLine(
            $"public class Generated{_parserClass.Identifier.Text} : BaseParser<{_lexerEnumDeclarationSyntax.Identifier.Text},{_outputType}> {{");

        builder.AppendLine("");
        builder.AppendLine($"    {_parserClass.Identifier.Text} _instance;");
        builder.AppendLine($"    ILexer<{_lexerType}> _lexer;");
        builder.AppendLine("");

        builder.AppendLine(
            $"    public Generated{_parserClass.Identifier.Text}({_parserClass.Identifier.Text} instance) {{");
        builder.AppendLine($"       var build = LexerBuilder.BuildLexer<{_lexerType}>();");
        builder.AppendLine($"       _lexer = build.Result;");
        builder.AppendLine("        _instance = instance;");
        builder.AppendLine("    }");

        foreach (var rule in rules)
        {
            builder.AppendLine("");
            var ruleSource = modelWalker.Visit(rule);
            builder.AppendLine(ruleSource);
        }
        
        var nts = nonTerminals.Where(x => x.Value.Count > 1).ToList();
        
        foreach (var nt in nts)
        {
            List<string> clauses = new List<string>();
            for (int i = 0; i < nt.Value.Count; i++)
            {
                clauses.Add($"{nt.Key.Capitalize()}_{i}");
            }

            builder.AppendLine("");
            builder.AppendLine(
                $"    public Match<{_lexerType},{_outputType}> {nt.Key.Capitalize()}(IList<Token<{_lexerType}>> tokens, int position) {{");
            builder.AppendLine($"        var parser = Alternate({string.Join(", ", clauses)});");
            builder.AppendLine($"        var result = parser(tokens,position);");
            builder.AppendLine("        return result;");
            builder.AppendLine("    }");


            
            
        }


        foreach (var nonTerm in nonTerminals.Keys)
        {
            builder.AppendLine(
                $@"    public {_outputType} Parse{nonTerm.Capitalize()}(string source) => Parse(""{nonTerm}"", source);");
            builder.AppendLine("");
        }

        builder.AppendLine($"    public {_outputType} Parse(string rule, string input) {{");
        //builder.AppendLine($"        var parser = new Generated{parserType}(_instance); 
        builder.AppendLine("        var tokens = _lexer.Tokenize(input).Tokens.MainTokens();");
        builder.AppendLine($"        Match<{_lexerType},{_outputType}> result;");
        builder.AppendLine("        switch(rule) {");
        foreach (var nonTerm in nonTerminals.Keys)
        {
            builder.AppendLine($@"            case ""{nonTerm}"" : {{ ");
            builder.AppendLine($"                 result = {nonTerm.Capitalize()}(tokens, 0);");
            builder.AppendLine($"                 break;");
            builder.AppendLine("              }");
        }
        builder.AppendLine($@"            default : {{ ");
        builder.AppendLine("                  result = null;");
        builder.AppendLine("                  break;");
        builder.AppendLine("              } ");

        builder.AppendLine("        }");
        builder.AppendLine("");
        builder.AppendLine("        if (result != null && result.Matched) {");
        builder.AppendLine($"            EBNFSyntaxTreeVisitor<{_lexerType}, {_outputType}> visitor =new EBNFSyntaxTreeVisitor<{_lexerType}, {_outputType}>(null, _instance);");
        builder.AppendLine("            var r = visitor.VisitSyntaxTree(result.Node);");
        builder.AppendLine("            return r;");
        builder.AppendLine("        }");
        builder.AppendLine($"        return default({_outputType});");
        // builder.AppendLine("        var o = parser.Parse(rule,tokens);");
        // builder.AppendLine("        return o");
        builder.AppendLine("    }");

        builder.AppendLine("");


        builder.AppendLine("}");

        
        

        return builder.ToString();
    }
}