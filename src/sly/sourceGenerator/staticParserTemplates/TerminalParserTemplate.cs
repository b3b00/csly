namespace sly.sourceGenerator.staticParserTemplates;

public class TerminalParserTemplate
{
    public const string Template =
        @"public SyntaxParseResult<<#LEXER#>,<#OUTPUT#>> ParseTerminal_<#NAME#>(List<Token<<#LEXER#>>> tokens ,int position, bool discarded = false) 
        => parseTerminal(tokens,<#LEXER#>.<#NAME#>,position,discarded);";
}