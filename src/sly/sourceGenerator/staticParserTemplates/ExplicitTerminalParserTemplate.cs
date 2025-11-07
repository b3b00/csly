namespace sly.sourceGenerator.staticParserTemplates;

public class ExplicitTerminalParserTemplate
{
    public const string Template =
        @"    public SyntaxParseResult<<#LEXER#>, <#OUTPUT#>> ParseExplicitTerminal_<#NAME#>(List<Token<<#LEXER#>>> tokens, int position, bool discarded = false)
     => parseExplicitTerminal(tokens,""explicit"",position,discarded);";
}