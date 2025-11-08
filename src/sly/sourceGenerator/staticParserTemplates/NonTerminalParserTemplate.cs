namespace sly.sourceGenerator.staticParserTemplates;

public class NonTerminalParserTemplate
{
    // STATIC this is not the right template. 
    public const string MainTemplate =
        @"public SyntaxParseResult<<#LEXER#>, <#OUTPUT#>> ParseNonTerminal_<#NAME#>(List<Token<<#LEXER#>>> tokens, int position)
    {
        var result = new SyntaxParseResult<<#LEXER#>, <#OUTPUT#>>();
        var token = tokens[position];
        var results = new List<SyntaxParseResult<<#LEXER#>, <#OUTPUT#>>>();

        <#CALLS#>

        result.IsError = true;
        result.AddErrors(results.SelectMany(x => x.Errors != null ? x.GetErrors() : new List<UnexpectedTokenSyntaxError<<#LEXER#>>>()).ToList());
        return result;
    }";

    public const string RuleCallTemplate = @"
        var r1Leadings = new[]
        {
            <#LEADINGS#>
        };
        if (r<#INDEX#>Leadings.Any(x => x.Match(token))) {
            var r<#INDEX#> = ParseRule_<#NAME#>_<#INDEX#>(tokens, position);
            if (r<#INDEX#>.IsOk)
            {
                return r<#INDEX#>;
            }
            results.Add(r<#INDEX#>);
        }";
}