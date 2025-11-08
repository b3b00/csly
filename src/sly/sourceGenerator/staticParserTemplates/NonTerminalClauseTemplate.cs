namespace sly.sourceGenerator.staticParserTemplates;

public class NonTerminalClauseTemplate
{
    public const string Template = @" 

// parse non terminal <#NAME#>
var r<#INDEX#> = ParseNonTerminal_<#NAME#>(tokens,position);
 if (r1.IsError)
 {
     return r<#INDEX#>;
 }
 position = r<#INDEX#>.EndingPosition;
 
 ";
}