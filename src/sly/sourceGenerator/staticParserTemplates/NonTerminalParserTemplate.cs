namespace sly.sourceGenerator.staticParserTemplates;

public class NonTerminalParserTemplate
{
    // STATIC this is not the right template. 
    public const string Template =
        @"public SyntaxParseResult<<#LEXER#>,<#OUTPUT#>> ParseNonTerminal_<#NAME#>(List<Token<<#LEXER#>>> tokens ,int position, bool discarded = false) 
        => null; // TODO implement this";
}