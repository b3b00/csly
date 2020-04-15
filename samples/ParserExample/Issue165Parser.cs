using expressionparser;
using sly.lexer;
using sly.parser.generator;

namespace ParserExample
{
    public class Issue165Parser
    {
        
        [Production("test_stmt :  ID EQ [d] arith_expr ")]
        public object assign(Token<Issue165Lexer> id, object expr)
        {
            return null;
        }
        
        
        
        [Operation((int) Issue165Lexer.PLUS, Affix.InFix, Associativity.Right, 10)]
        [Operation("MINUS", Affix.InFix, Associativity.Left, 10)]
        public object BinaryTermExpression(object left, Token<Issue165Lexer> operation, object right)
        {
            return null;
        }


        [Operation((int) Issue165Lexer.TIMES, Affix.InFix, Associativity.Right, 50)]
        [Operation("DIVIDE", Affix.InFix, Associativity.Left, 50)]
        public object BinaryFactorExpression(double left, Token<Issue165Lexer> operation, object right)
        {
 
            return null;
        }



        [Production("arith_expr : Issue165Parser_expressions")]
        [Production("arith_expr : ternary_expr")]
        public object arith(object h)
        {
            return h;
        }

        [Production(
            "ternary_expr : Issue165Parser_expressions QUESTIONMARK [d] Issue165Parser_expressions COLON [d] Issue165Parser_expressions")]
        public object ternary(object j, object k, object l)
        {
            return null;
        }

        [Production("sqbr_expr : ID LBR [d] Issue165Parser_expressions RBR [d]")]
        public object sqrbrack(Token<Issue165Lexer> token, object expr)
        {
            return null;
        }

        [Production("group_expr : LPAR [d] Issue165Parser_expressions RPAR [d]")]
        public object group(object subexpr)
        {
            return subexpr;
        }

        [Operand]
        [Production("atom : ID")]
        public object id(Token<Issue165Lexer> i)
        {
            return null;
        }
        [Operand]
        [Production("atom : INT")]
        public object integer(Token<Issue165Lexer> i)
        {
            return i;
        }
        [Operand]
        [Production("atom : sqbr_expr")]
        public object sq(object i)
        {
            return i;
        }
        [Operand]
        [Production("atom : group_expr")]
        public object id(object g)
        {
            return g;
        }
        
    }
}