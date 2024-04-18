using sly.lexer;
using sly.parser.generator;

namespace ParserTests.Issue414;

[UseMemoization]
public class Issue414AltParser
{
  #region boolean
    [Production("boolean: TRUE")]
    public string BooleanTrue(Token<Issue414Token> trueToken)
    {
        return "true";
    }
    [Production("boolean: FALSE")]
    public string BooleanFalse(Token<Issue414Token> falseToken)
    {
        return "false";
    }
    #endregion

    #region variable

    [Production("variable: IDENTIFIER")]
    public string VariableIdentifier(Token<Issue414Token> idToken)
    {
        var varName = idToken.StringWithoutQuotes;
        return varName;
    }
    #endregion

    #region group
    [Production("group: LPAREN expression RPAREN")]
    public string Group(Token<Issue414Token> lparen, string expression, Token<Issue414Token> rparen)
    {
        return "(" + expression + ")";
    }

    #endregion

    #region primary
    [Production("primary: boolean")]
    public string PrimaryBoolean(string boolean)
    {
        return boolean;
    }

    [Production("primary: INT")]
    public string PrimaryInt(Token<Issue414Token> intToken)
    {
        return intToken.IntValue.ToString();
    }
    [Production("primary: DOUBLE")]
    public string PrimaryDouble(Token<Issue414Token> doubleToken)
    {
        return doubleToken.DoubleValue.ToString();
    }

    [Production("primary: variable")]
    public string PrimaryIdentifier(string variable)
    {
        return variable;
    }

    [Production("primary: STRING")]
    public string PrimaryString(Token<Issue414Token> strToken)
    {
        var str = strToken.StringWithoutQuotes;
        return str;
    }

    [Production("primary: group")]
    public string PrimaryGroup(string group)
    {
        return group;
    }

    [Production("primary : functioncall")]
    public string PrimaryFunctioncall(string functioncall)
    {
        return functioncall;
    }

    #endregion

    #region begin end

    [Production("begin_end : LCURLYBRACE block RCURLYBRACE")]
    public string BeginEnd(Token<Issue414Token> lcurlybraceToken, string block, Token<Issue414Token> rcurlybraceToken)
    {
        return "{" + "\r\n" + block + "\r\n" + "}";
    }

    #endregion


    #region block
    [Production("block : statement")]
    public string Block(string statement)
    {
        return statement;
    }

    [Production("block : statement block")]
    public string Block(string statement, string blockExpression)
    {
        return statement + "\r\n" + blockExpression;
    }

    #endregion

    #region statement
    [Production("statement : expression SEMICOLON")]
    public string Statementstring(string expression, Token<Issue414Token> semicolonToken)
    {
        return expression + ";";
    }

    // A := ...
    [Production("statement : assign SEMICOLON")]
    public string StatementAssign(string assign, Token<Issue414Token> semicolonToken)
    {
        return assign + ";";
    }

    //// begin ... end
    [Production("statement : begin_end")]
    public string StatementBeginEnd(string beginend)
    {
        return beginend;
    }

    #endregion

    #region assign
    // A := ...
    [Production("assign : variable ASSIGN expression")]
    public string AssignExpression(string variable, Token<Issue414Token> assignToken, string right)
    {
        return variable + ":=" + right;
    }
    #endregion


    #region expression
    //expression ->
    //  logiclevel (AND, OR) ->
    //    comparelevel (<, >, !=, ==) ->
    //      sumlevel (+, -) ->
    //        factorlevel (*, ) ->
    //          primary

    [Production("expression : logiclevel [AND|OR] expression")]
    
    public string LogicExpression(string left, Token<Issue414Token> operatorToken, string right)
    {
        return left + operatorToken.Value + right;
    }

    [Production("expression : logiclevel")]
    public string LogicExpression(string comparelevel)
    {
        return comparelevel;
    }


    [Production("logiclevel : comparelevel [GREATER|LESSER|EQUALS|DIFFERENT] logiclevel")]
    public string CompareExpression(string left, Token<Issue414Token> operatorToken, string right)
    {
        return left + operatorToken.Value + right;
    }

    [Production("logiclevel : comparelevel")]
    public string CompareExpression(string comparelevel)
    {
        return comparelevel;
    }

    [Production("comparelevel : sumlevel [PLUS|MINUS] comparelevel")]
    public string PlusExpression(string left, Token<Issue414Token> operatorToken, string right)
    {
        return left + operatorToken.Value + right;
    }
    // [Production("comparelevel : sumlevel MINUS comparelevel")]
    // public string MinusExpression(string left, Token<Issue414Token> operatorToken, string right)
    // {
    //     return left + "-" + right;
    // }

    [Production("comparelevel : sumlevel")]
    public string CalcExpression(string sumlevel)
    {
        return sumlevel;
    }
    #endregion

    #region sumlevel (term)
    [Production("sumlevel : factorlevel [TIMES|DIVIDE] sumlevel")]
    public string Times(string left, Token<Issue414Token> operatorToken, string right)
    {
        return left + operatorToken.Value + right;
    }
    // [Production("sumlevel : factorlevel DIVIDE sumlevel")]
    // public string Divide(string left, Token<Issue414Token> operatorToken, string right)
    // {
    //     return left + "/" + right;
    // }

    [Production("sumlevel : factorlevel")]
    public string Sumlevel_Factor(string factorValue)
    {
        return factorValue;
    }
    #endregion

    #region factor
    [Production("factorlevel : primary")]
    public string PrimaryFactor(string primValue)
    {
        return primValue;
    }

    [Production("factorlevel : MINUS factorlevel")]
    public string Factor(Token<Issue414Token> minus, string factorValue)
    {
        return "-" + factorValue;
    }
    #endregion

    #region functioncall
    // function : variable ( parameters )
    // function : variable ( )
    [Production("functioncall : variable LPAREN callparameters RPAREN")]
    public string Functioncall(string variableExpression, Token<Issue414Token> lparen, string parameters, Token<Issue414Token> rparen)
    {
        return variableExpression + "(" + parameters + ")";
    }

    [Production("functioncall : variable LPAREN RPAREN")]
    public string Functioncall(string variableExpression, Token<Issue414Token> lparen, Token<Issue414Token> rparen)
    {
        return variableExpression + "()";
    }

    [Production("callparameters : expression COMMA callparameters")]
    public string Function(string paramExpression, Token<Issue414Token> comma, string parameters)
    {
        return paramExpression + ", " + parameters;
    }

    [Production("callparameters : expression")]
    public string Function(string paramExpression)
    {
        return paramExpression;
    }
    #endregion  
}