using sly.lexer;
using sly.parser.generator;

namespace ParserTests.Issue414;

[UseMemoization]
public class Issue414ExpressionParser
{
 #region boolean
    [Operand]
    [Production("boolean: TRUE")]
    public string BooleanTrue(Token<Issue414Token> trueToken)
    {
        return "true";
    }
    [Operand]
    [Production("boolean: FALSE")]
    public string BooleanFalse(Token<Issue414Token> falseToken)
    {
        return "false";
    }
    #endregion

    #region variable

    [Operand]
    [Production("variable: IDENTIFIER")]
    public string VariableIdentifier(Token<Issue414Token> idToken)
    {
        var varName = idToken.StringWithoutQuotes;
        return varName;
    }
    #endregion

    #region group
    [Operand]
    [Production("group: LPAREN Issue414ExpressionParser_expressions RPAREN")]
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

    [Operand]
    [Production("primary: INT")]
    public string PrimaryInt(Token<Issue414Token> intToken)
    {
        return intToken.IntValue.ToString();
    }
    
    [Operand]
    [Production("primary: DOUBLE")]
    public string PrimaryDouble(Token<Issue414Token> doubleToken)
    {
        return doubleToken.DoubleValue.ToString();
    }
    //
    // [Production("primary: variable")]
    // public string PrimaryIdentifier(string variable)
    // {
    //     return variable;
    // }
    //
    
    [Operand]
    [Production("primary: STRING")]
    public string PrimaryString(Token<Issue414Token> strToken)
    {
        var str = strToken.StringWithoutQuotes;
        return str;
    }

    // [Production("primary: group")]
    // public string PrimaryGroup(string group)
    // {
    //     return group;
    // }

    [Operand]
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
    [Production("statement : Issue414ExpressionParser_expressions SEMICOLON")]
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
    [Production("assign : variable ASSIGN Issue414ExpressionParser_expressions")]
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

    [Infix("AND",Associativity.Left,10)]
    [Infix("OR",Associativity.Left,10)]
    public string Boolean(string left, Token<Issue414Token> operatorToken, string right)
    {
        return left + operatorToken.Value + right;
    }


    [Infix("GREATER",Associativity.Left,20)]
    [Infix("LESSER",Associativity.Left,20)]
    [Infix("EQUALS",Associativity.Left,20)]
    [Infix("DIFFERENT",Associativity.Left,20)]
 public string Compare(string left, Token<Issue414Token> operatorToken, string right)
    {
        return left + operatorToken.Value + right;
    }

    

    [Infix("PLUS",Associativity.Left,30)]
    [Infix("MINUS",Associativity.Left,30)]
    
    public string Term(string left, Token<Issue414Token> operatorToken, string right)
    {
        return left + operatorToken.Value + right;
    }
    
    #endregion

    #region sumlevel (term)
    [Infix("TIMES",Associativity.Left,40)]
    [Infix("DIVIDE",Associativity.Left,40)]
    public string Factor(string left, Token<Issue414Token> operatorToken, string right)
    {
        return left + operatorToken.Value + right;
    }
    
    #endregion

    #region factor
    

    [Prefix("MINUS",Associativity.Right,50)]
    public string UnaryMinus(Token<Issue414Token> minus, string factorValue)
    {
        return "-" + factorValue;
    }
    #endregion

    #region functioncall
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

    [Production("callparameters : Issue414ExpressionParser_expressions COMMA callparameters")]
    public string Function(string paramExpression, Token<Issue414Token> comma, string parameters)
    {
        return paramExpression + ", " + parameters;
    }

    [Production("callparameters : Issue414ExpressionParser_expressions")]
    public string Function(string paramExpression)
    {
        return paramExpression;
    }
    #endregion
}