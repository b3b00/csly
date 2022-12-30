using System.Collections.Generic;
using sly.lexer;
using sly.parser.generator;
using sly.parser.parser;

namespace ParserTests.Issue332;


[ParserRoot("root")]
public class Issue332Parser
{
    [Production("root: statement*")]
public object Root(List<object> statement) => "foo";

[Production("statement: LPAREN statement RPAREN")]
public object BOLCK1(Token<Issue332Token> l, object statement, Token<Issue332Token> r) =>
    null;



#region expr

[Operation((int) Issue332Token.LESSER, Affix.InFix, Associativity.Right, 50)]
[Operation((int) Issue332Token.GREATER, Affix.InFix, Associativity.Right, 50)]
[Operation((int) Issue332Token.EQUALS, Affix.InFix, Associativity.Right, 50)]
[Operation((int) Issue332Token.DIFFERENT, Affix.InFix, Associativity.Right, 50)]
public object binaryComparisonExpression(object left, Token<Issue332Token> operatorToken,
    object right) => null;

[Operation((int)Issue332Token.CONCAT, Affix.InFix, Associativity.Right, 100)]
public object DotExpr(object left, Token<Issue332Token> oper, object right) => null
;

[Operation((int)Issue332Token.PLUS, Affix.InFix, Associativity.Right, 20)]
[Operation((int)Issue332Token.MINUS, Affix.InFix, Associativity.Right, 20)]
public object BE1(object left, Token<Issue332Token> oper, object right) => null
;

[Operation((int)Issue332Token.TIMES, Affix.InFix, Associativity.Right, 70)]
[Operation((int)Issue332Token.DIVIDE, Affix.InFix, Associativity.Right, 70)]
public object BE2(object left, Token<Issue332Token> oper, object right) => null;

[Operation((int)Issue332Token.AND, Affix.InFix, Associativity.Right, 50)]
[Operation((int)Issue332Token.OR, Affix.InFix, Associativity.Right, 50)]
[Operation((int)Issue332Token.XOR, Affix.InFix, Associativity.Right, 50)]
public object Bool1(object left, Token<Issue332Token> oper, object right) => null;


[Operation((int)Issue332Token.NOT, Affix.PreFix, Associativity.Right, 100)]
public object Bool2(Token<Issue332Token> oper, object expr) => null;

[Operation((int)Issue332Token.MINUS, Affix.PreFix, Associativity.Right, 100)]
public object MINUS(Token<Issue332Token> oper, object expr) => null;


#endregion

#region primany

[Operand]
[Production("operand: primary")]
public object Operand(object prim) => prim;

[Production("primary: LPAREN primary RPAREN")]
public object LR(Token<Issue332Token> l, object prim, Token<Issue332Token> r) =>
    prim as object;

[Production("primary: STRING")]
public object STRING(Token<Issue332Token> token) => null;

[Production("primary: INT")]
public object INT(Token<Issue332Token> token) => null;

[Production("primary: CHAR")]
public object CHAR(Token<Issue332Token> token) => null;

[Production("primary: DOUBLE")]
public object DOUBLE(Token<Issue332Token> token) => null ;

[Production("primary: Issue332Parser_expressions")]
public object Bool(object expr) => null;


[Production("primary: IDENTFIER")]
public object IDENTIFIER(Token<Issue332Token> id) => null;

[Production("primary: TRUE")]
public object BoolTrue(Token<Issue332Token> token) => null;

[Production("primary: FALSE")]
public object BoolFalse(Token<Issue332Token> token) => null;

#endregion



[Production("set: IDENTFIER SET[d] Issue332Parser_expressions")]
public object Set( Token<Issue332Token> id, object value) => null;

[Production("statement: set")]
public object SET(object a) => null;

[Production("statement : IF[d] ifblock (ELIF ifblock)* (ELSE  block)?")]
public object IF(object ifBlock, List<Group<Issue332Token, object>> elif,
    ValueOption<Group<Issue332Token, object>> Else) => null;

[Production("ifblock: Issue332Parser_expressions block")]
public object IFBLOCK(object a, object block) => null;

[Production("block: INDENT[d] statement* UINDENT[d]")]
public object Block(List<object> statements) => null;

[Production("statement: FOR set Issue332Parser_expressions  statement  block")]
public object FOR(Token<Issue332Token> a, object set, object expr, object statement, object block) =>null;

[Production("statement: WHILE Issue332Parser_expressions block")]
public object WHILE(Token<Issue332Token> a, object expr, object block) => null;

[Production("statement: IDENTFIER DIRECT[d] IDENTFIER")]
public object DIRECT(Token<Issue332Token> id1, Token<Issue332Token> id2) => null;

[Production("statement: FUNC[d] IDENTFIER LPAREN[d] RPAREN[d] block")]
public object STAT_FUNC( Token<Issue332Token> id, object block) => null;

[Production("statement: CLASS[d] IDENTFIER set*")]
public object CLASS(Token<Issue332Token> id, List<object> sets) => null;

[Production("statement: IDENTFIER SET[d] IDENTFIER LPAREN RPAREN")]
public object INSTANTIATE(Token<Issue332Token> id, Token<Issue332Token> a, Token<Issue332Token> otherid, Token<Issue332Token> b,
    Token<Issue332Token> c) => null;
}