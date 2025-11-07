namespace sly.sourceGenerator.staticParserTemplates;

public class TerminalClauseTemplate
{
    public const string Template = @"var r<#INDEX#> = ParseTerminal_<#NAME#>(tokens,position);
if (r1.IsError)
{
    return r<#INDEX#>;
}
position = r<#INDEX#>.EndingPosition;

";
}