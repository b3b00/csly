using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using sly.sourceGenerator.generated.ebnfparser.model;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace sly.sourceGenerator.generated.extractor;

public class RuleGenerator : IParserModelVisitor<string>
{
    private readonly string _lexerType;
    private readonly string _outputType;

    public RuleGenerator(string lexerType, string outputType)
    {
        _lexerType = lexerType;
        _outputType = outputType;
    }
    public string VisitTerminal(TerminalClause terminal)
    {
        // [Discarded]TerminalParser(expectedTokens:GenericExpressionToken.LPAREN);
        var result = $"{(terminal.IsDiscared ? "DiscardedTerminalParser" : "TerminalParser")}(expectedTokens:{_lexerType}.{terminal.Terminal})";
        return result;
    }

    public string VisitNonTerminal(NonTerminalClause nonTerminal)
    {
        return nonTerminal.NonTerminal.Capitalize();
    }

    public string VisitOption(OptionalClause option, string clause)
    {
        var method = option.Clause switch
        {
            TerminalClause terminal => "ZeroOrMoreToken",
            NonTerminalClause nonTerminal => "ZeroOrMoreValue",
            GroupClause groupClause => "ZeroOrMoreGroup",
            _ => throw new NotImplementedException()
        };
        return $"{method}({clause})";
    }

    public string VisitZeroOrMore(ZeroOrMoreClause zeroOrMore, string clause)
    {
        var method = zeroOrMore.Clause switch
        {
            TerminalClause terminal => "ZeroOrMoreToken",
            NonTerminalClause nonTerminal => "ZeroOrMoreValue",
            GroupClause groupClause => "ZeroOrMoreGroup",
            _ => throw new NotImplementedException()
        };
        return $"{method}({clause})";
    }

    public string VisitOneOrMore(OneOrMoreClause oneOrMore, string clause)
    {
        var method = oneOrMore.Clause switch
        {
            TerminalClause terminal => "OneOrMoreToken",
            NonTerminalClause nonTerminal => "OneOrMoreValue",
            GroupClause groupClause => "OneOrMoreGroup",
            _ => throw new NotImplementedException()
        };
        return $"{method}({clause})";
    }

    public string VisitGroup(GroupClause group, IList<string> clauses)
    {
        return $"SubGroup({string.Join(", ", clauses)})";
    }
  

    public string VisitAlternate(AlternateClause alternate, IList<string> clauses)
    {
        return $"Alternate({string.Join(", ", clauses)})";
    }
    
    public string VisitRule(Rule rule, IList<string> clauses)
    {
        StringBuilder builder = new StringBuilder();
        
        var methodName = rule.Number >= 0 ? $"{rule.NonTerminalName}_{rule.Number}" : $"{rule.NonTerminalName}";
        methodName = methodName.Capitalize();
        
        string visibility = rule.Number >= 0 ? "private" : "public";
        var visitor = GenerateVisitor(rule.Method);
        
        builder.AppendLine(
            $"    {visibility} Match<{_lexerType},{_outputType}> {methodName}(IList<Token<{_lexerType}>> tokens, int position) {{");
        builder.AppendLine(visitor);
        builder.AppendLine($"        var parser = Sequence({string.Join(", ", clauses)});");
        builder.AppendLine($"        var result = parser(tokens,position);");
        builder.AppendLine($"        if (result.Matched &&  result.Node is SyntaxNode<{_lexerType},{_outputType}> node) {{");
        builder.AppendLine($"            node.LambdaVisitor = visitor;");
        builder.AppendLine("         }");
        builder.AppendLine("        return result;");
        builder.AppendLine("    }");
        return builder.ToString();
    }

    private string GenerateVisitor(MethodDeclarationSyntax method)
    {
        var parameters = method.ParameterList.Parameters.ToList();
        
        StringBuilder builder = new StringBuilder(); 
        string methodName = method.Identifier.ToString();
        builder.AppendLine($"        Func<object[],{_outputType}> visitor = (object[] args) => {{");
        builder.Append($"            var result = _instance.{methodName}(");
        for (int i = 0; i < parameters.Count; i++)
        {
            if (i > 0)
            {
                builder.Append(", ");
            }
            var type = parameters[i].Type.ToString();
            builder.Append($"({type})args[{i}]");
        }
        builder.AppendLine(");");
        builder.AppendLine("            return result;");
        builder.Append("        };");
        return builder.ToString();
    }
}