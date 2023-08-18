using System.Collections.Generic;
using ParserTests.errorAccuracyIssue381;
using sly.lexer;
using sly.parser.generator;
using sly.parser.parser;

namespace ParserTests.Issue381ErrorAccuracy;

public class ErrorAccuracyIssue381Parser
{
        
        [Production("statements : statement*")]
        public object Action(List<object> statements)
        {
            return null;
        }

        [Production("statement : Id Set[d] ErrorAccuracyIssue381Parser_expressions")]
        public object StatementAssignment(Token<ErrorAccuracyIssue381Token> id, object expression)
        {
            return null;
        }


        // *******************************************************************************

        [Operation("Plus", Affix.InFix, Associativity.Right, 10)]
        // using token string representation
        [Operation("Minus", Affix.InFix, Associativity.Left, 10)]
        public object BinaryTermExpression(object left, Token<ErrorAccuracyIssue381Token> operation, object right)
        {
            return null;
        }


        [Operation("Times", Affix.InFix, Associativity.Right, 50)]
        [Operation("Div", Affix.InFix, Associativity.Left, 50)]
        public object BinaryFactorExpression(object left, Token<ErrorAccuracyIssue381Token> operation, object right)
        {
            return null;
        }

        [Operation("Minus", Affix.PreFix, Associativity.Left, 55)]
        public object MinusExpression(Token<ErrorAccuracyIssue381Token> minus, object expression)
        {
            return null;
        }


        [Operation("And", Affix.InFix, Associativity.Right, 90)]
        // using token string representation
        public object AndExpression(object left, Token<ErrorAccuracyIssue381Token> operation, object right)
        {
            return null;
        }

        [Operation("Or", Affix.InFix, Associativity.Right, 70)]
        public object OrExpression(object left, Token<ErrorAccuracyIssue381Token> operation, object right)
        {
            return null;
        }

        [Operation("Not", Affix.PreFix, Associativity.Left, 95)]
        public object UnaryNot(Token<ErrorAccuracyIssue381Token> not, object expression)
        {
            return null;
        }

        [Operation("Equal", Affix.InFix, Associativity.Right, 110)]
        [Operation("Diff", Affix.InFix, Associativity.Right, 110)]
        [Operation("Greater", Affix.InFix, Associativity.Right, 110)]
        [Operation("GreaterEqual", Affix.InFix, Associativity.Right, 110)]
        [Operation("Lesser", Affix.InFix, Associativity.Right, 110)]
        [Operation("LesserEqual", Affix.InFix, Associativity.Right, 110)]
        public object ComparisonExpression(object left, Token<ErrorAccuracyIssue381Token> operation, object right)
        {
            return null;
        }

        [Operand]
        [Production("inoperand : Id In[d] optionsoperand")]
        public object IsOperand(Token<ErrorAccuracyIssue381Token> id, object right)
        {
            return null;
        }

        [Operand]
        [Production("stringoperand : String")]
        public object StringOperand(Token<ErrorAccuracyIssue381Token> value)
        {
            return null;
        }

        [Operand]
        [Production("doubleoperand : Double")]
        public object DoubleOperand(Token<ErrorAccuracyIssue381Token> value)
        {
            return null;
        }

        [Operand]
        [Production("intoperand : Int")]
        public object IntOperand(Token<ErrorAccuracyIssue381Token> value)
        {
            return null;
        }

        [Operand]
        [Production("idoperand : Id")]
        public object IdOperand(Token<ErrorAccuracyIssue381Token> value)
        {
            return null;
        }

        [Operand]
        [Production("notempryoperand : Not Empty[d]")]
        public object NotEmpty(Token<ErrorAccuracyIssue381Token> not)
        {
            return null;
        }

        [Operand]
        [Production("BooleanOperand : [True|False]")]
        public object BoolOperand(Token<ErrorAccuracyIssue381Token> boolean)
        {
            return null;
        }

        [Operand]
        [Production("emptyoperand : Empty")]
        public object Empty(Token<ErrorAccuracyIssue381Token> empty)
        {
            return null;
        }

        [Operand]
        [Production("groupOperand : Lparen[d] ErrorAccuracyIssue381Parser_expressions Rparen[d]")]
        public object Group(object expression)
        {
            return null;
        }

        [Operand]
        [Production("optionsoperand : Lbrack[d] option ( Comma option)* Rbrack[d]")]
        public object Options(object head, List<Group<ErrorAccuracyIssue381Token, object>> tail)
        {
            return null;
        }

        [Production("option : Not Empty[d]")]
        public object OptionNonVide(Token<ErrorAccuracyIssue381Token> not)
        {
            return null;
        }

        [Production("option : Empty")]
        public object OptionVide(Token<ErrorAccuracyIssue381Token> empty)
        {
            return null;
        }

        [Production("option : Int")]
        public object OptionInt(Token<ErrorAccuracyIssue381Token> token)
        {
            return null;
        }

        [Production("option : String")]
        public object OptionString(Token<ErrorAccuracyIssue381Token> token)
        {
            return null;
        }

        [Operand]
        [Production("functioncall : Id Lparen[d] args Rparen[d]")]
        public object FunctionCall(Token<ErrorAccuracyIssue381Token> d, object args)
        {
            return null;
        }
        
        [Operand]
        [Production("statement : Lbrack[d] args2 Rbrack[d]")]
        public object Array(object args)
        {
            return null;
        }

        [Production("args : ErrorAccuracyIssue381Parser_expressions (Comma[d] ErrorAccuracyIssue381Parser_expressions)*")]
        public object Arguments(object head, List<Group<ErrorAccuracyIssue381Token, object>> tail)
        {
            return null;
        }
        
        [Production("args2 :  (Comma[d] ErrorAccuracyIssue381Parser_expressions)+")]
        public object Arguments2(List<Group<ErrorAccuracyIssue381Token, object>> tail)
        {
            return null;
        }
}