using sly.parser.generator;
using sly.lexer;
using System.Text;
using System.Collections.Generic;

namespace ParserTests.Issue588;

[AutoCloseIndentations]
[ParserRoot("root")]
public class Parse
{
    [Production("root : instr+")]
    public string Root(List<string> instr)
    {
        return string.Join("\n\n", instr);
    }

    [Production("instr : ID SET[d] INT")]
    public string SetInstr(Token<Issue588Lexer> id, Token<Issue588Lexer> val)
    {
        return $"{id.Value} = {val.Value}";
    }

    [Production("instr:IF[d] ID EQ[d] INT INDENT[d] instr+ UINDENT[d]")]
    public string ifInstr(Token<Issue588Lexer> id, Token<Issue588Lexer> val, List<string> instrs)
    {
        StringBuilder b = new StringBuilder();
        b.AppendLine($"si {id.Value}=={val.Value} ALORS {{");
        foreach (var i in instrs)
        {
            b.AppendLine("\t" + i + ";");
        }
        b.AppendLine("\t}");
        return b.ToString();
    }

}