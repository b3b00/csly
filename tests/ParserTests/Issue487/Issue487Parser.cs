using sly.lexer;
using sly.parser.generator;
using sly.parser.parser;

namespace ParserTests.Issue487;

public class Issue487Parser
{

    public Issue487Parser()
    {

    }

    [Production("paramexpr: OPS")]
    public string PExpr(Token<Issue487Token> argToken)               // just the parameters.  No particular inclining toward D365 or XL
    {


        return "1";
    }

    [Production("convertexpr: CONVERT ISNULLABLE? COLON [d]")]
    public string ConvertExpr(Token<Issue487Token> argToken, Token<Issue487Token> nullflag)
    {


        return "2";
    }


    [Production("ifnullExpr: IFNULLTHEN [d] BLOCK_START [d]")]      // Todo: "@appcapcalc ?? @+(fast_munibudget_appcapcalc) >> @appcapcalc
    public string ifNullExpr()
    {
        return "3";

    }

    [Production("insexpr: INSTALL [d] convertexpr? ccexpr")]
    [Production("insexpr: INSTALL [d] convertexpr? d365expr")]
    [Production("insexpr: INSTALL [d] convertexpr? lvarexpr")]
    public string IExpr(ValueOption<string> formatToken, string arg)
    {
        return "4";
    }

    [Production("getexpr: ccexpr")]
    [Production("getexpr: xlexpr")]
    [Production("getexpr: d365expr")]
    [Production("getexpr: lvarexpr")]
    public string GetExpr(string arg)
    {
        return "5";
    }

    [Production("lexpr: constexpr")]
    [Production("lexpr: getexpr")]
    public string LExpr(string arg)
    {
        return "6";
    }

    [Production("rexpr: ifnullExpr")]
    [Production("rexpr: insexpr")]
    public string RExpr(string arg)
    {
        return "7";
    }

    [Production("lvarexpr: VARSTART [d] OPS")]
    public string LVarExpr(Token<Issue487Token> token)
    {
        return "8";
    }

    [Production("constexpr: CONST_BLOCK")]
    public string ConstExpr(Token<Issue487Token> token)
    {
        return "9";
    }


    [Production("ccexpr: START_C paramexpr")]
    [Production("d365expr: START_D paramexpr")]
    [Production("xlexpr: START_X paramexpr")]
    [Production("lvarexpr: VARSTART paramexpr")]
    public string XFuncExpr(Token<Issue487Token> startToken, string arg)
    {
        return "10";
    }


    [Production("expression: lexpr rexpr?")]
    public string ExprOut(string lexpr, ValueOption<string> rexpr)
    {
        return "11";
    }
}