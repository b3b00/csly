using System;
using System.Collections.Generic;
using System.Linq;
using rules.model;
using sly.lexer;
using sly.parser.generator;
using sly.parser.parser;

namespace rules;

public class RulesParser
{
    private static List<RuleComment> ExtractPreviousComments(Token<RulesToken> token)
    {
        var commentTokens = token.GetPreviousTokens(Channels.Comments);
        //var commentTokens = token.PreviousTokens(Channels.Comments);

        if (commentTokens == null || !commentTokens.Any())
            return null;

        var comments = commentTokens
            .Where(t => t.TokenID == RulesToken.SingleComment || t.TokenID == RulesToken.MultiComment)
            .Select(t => new RuleComment(t.Value, t.TokenID == RulesToken.MultiComment))
            .ToList();

        return comments.Any() ? comments : null;
    }

    

    [Production(
        "rule : Rule Hash[d] Int  If RulesParser_expressions Then[d] statements EndIf[d] End[d] Rule[d]")]
    public IRule Rule(Token<RulesToken> ruleKeyword, Token<RulesToken> number, Token<RulesToken> ifKeyword,
         RuleExpression condition,
         Statements action)
    {
        List<RuleComment> precedingComments;
        
            precedingComments = ExtractPreviousComments(ruleKeyword);
        

        var rule = new Rule( number.IntValue, condition, action, number.SourcePosition());
        rule.PrecedingComments = precedingComments;
        rule.ConditionPrecedingComments = ExtractPreviousComments(ifKeyword);
        return rule;
    }

    

    [Production("statements : statement*")]
    public IRule Action(List<IRule> statements)
    {
        var statementList = statements.Cast<IStatement>();
        var position = statements.Any() ? statements.First().Position : new Position();
        return new Statements(statementList, position);
    }

    [Production("identifier: Id")]
    public IRule Identifier(Token<RulesToken> id)
    {
        var identifier = new Identifier(id.Value, id.SourcePosition());
        identifier.PrecedingComments = ExtractPreviousComments(id);
        return identifier;
    }

    [Production("statement : [identifier|identifierArray] Set[d] RulesParser_expressions")]
    public IRule StatementAssignment(RuleExpression left, IRule expression)
    {
        var statement = new AssignmentStatement(left, expression, left.Position);
        statement.PrecedingComments = left.PrecedingComments;
        return statement;
    }

    [Production("statement : If RulesParser_expressions Then[d] statements (Else statements)? EndIf[d] ")]
    public IRule StatementIf(Token<RulesToken> ifKeyword, RuleExpression condition, Statements thenBlock,
        ValueOption<Group<RulesToken, IRule>> elseBlock)
    {
        var elseBlk = elseBlock.Match(x => x, () => null);
        var elseStatements = elseBlk?.Value(1) as Statements;
        var statement = new IfStatement(condition, thenBlock, elseStatements, condition.Position);
        statement.PrecedingComments = ExtractPreviousComments(ifKeyword);
        return statement;
    }


    // *******************************************************************************

    [Operation("Plus", Affix.InFix, Associativity.Right, 10)]
    // using token string representation
    [Operation("Minus", Affix.InFix, Associativity.Left, 10)]
    public IRule BinaryTermExpression(IRule left, Token<RulesToken> operation, IRule right)
    {
        IRule result = null;
        switch (operation.TokenID)
        {
            case RulesToken.Plus:
                {
                    result = new ArithmExpr(ArithmOperation.PLUS, left as RuleExpression, right as RuleExpression,
                        operation.SourcePosition());
                    break;
                }
            case RulesToken.Minus:
                {
                    result = new ArithmExpr(ArithmOperation.MINUS, left as RuleExpression, right as RuleExpression,
                        operation.SourcePosition());
                    break;
                }
        }

        return result;
    }


    [Operation("Times", Affix.InFix, Associativity.Right, 50)]
    [Operation("Div", Affix.InFix, Associativity.Left, 50)]
    public IRule BinaryFactorExpression(RuleExpression left, Token<RulesToken> operation, RuleExpression right)
    {
        IRule result = null;
        switch (operation.TokenID)
        {
            case RulesToken.Times:
                {
                    result = new ArithmExpr(ArithmOperation.MULT, left, right, operation.SourcePosition());
                    break;
                }
            case RulesToken.Div:
                {
                    result = new ArithmExpr(ArithmOperation.DIV, left, right, operation.SourcePosition());
                    break;
                }
        }

        return result;
    }

    [Operation("Minus", Affix.PreFix, Associativity.Left, 55)]
    public IRule MinusExpression(Token<RulesToken> minus, RuleExpression expression)
    {
        return new UnaryMinusExpression(expression, minus.SourcePosition());
    }


    [Operation("And", Affix.InFix, Associativity.Right, 90)]
    // using token string representation
    public IRule AndExpression(RuleExpression left, Token<RulesToken> operation, RuleExpression right)
    {
        return new BooleanExpr(BooleanOperation.AND, left, right, operation.SourcePosition());
    }

    [Operation("Or", Affix.InFix, Associativity.Right, 70)]
    public IRule OrExpression(RuleExpression left, Token<RulesToken> operation, RuleExpression right)
    {
        return new BooleanExpr(BooleanOperation.OR, left, right, operation.SourcePosition());
    }

    [Operation("Not", Affix.PreFix, Associativity.Left, 95)]
    public IRule UnaryNot(Token<RulesToken> not, RuleExpression expression)
    {
        return new NotExprpression(expression, not.SourcePosition());
    }

    [Operation("Equal", Affix.InFix, Associativity.Right, 110)]
    [Operation("Diff", Affix.InFix, Associativity.Right, 110)]
    [Operation("Greater", Affix.InFix, Associativity.Right, 110)]
    [Operation("GreaterEqual", Affix.InFix, Associativity.Right, 110)]
    [Operation("Lesser", Affix.InFix, Associativity.Right, 110)]
    [Operation("LesserEqual", Affix.InFix, Associativity.Right, 110)]
    public IRule ComparisonExpression(RuleExpression left, Token<RulesToken> operation, RuleExpression right)
    {
        var compOperation = default(ComparisonOperation);
        switch (operation.TokenID)
        {
            case RulesToken.Equal:
                {
                    compOperation = ComparisonOperation.EQ;
                    break;
                }
            case RulesToken.Diff:
                {
                    compOperation = ComparisonOperation.NEQ;
                    break;
                }
            case RulesToken.Greater:
                {
                    compOperation = ComparisonOperation.GT;
                    break;
                }
            case RulesToken.GreaterEqual:
                {
                    compOperation = ComparisonOperation.GTE;
                    break;
                }
            case RulesToken.Lesser:
                {
                    compOperation = ComparisonOperation.LT;
                    break;
                }
            case RulesToken.LesserEqual:
                {
                    compOperation = ComparisonOperation.LTE;
                    break;
                }
        }

        return new ComparisonExpr(compOperation, left, right, left.Position);
    }

    [Operand]
    [Production("inoperand : Id In[d] optionsoperand")]
    public IRule IsOperand(Token<RulesToken> id, Choices right)
    {
        return new In(new Identifier(id.Value, id.SourcePosition()), right, id.SourcePosition());
    }

    [Operand]
    [Production("stringoperand : String")]
    public IRule StringOperand(Token<RulesToken> value)
    {
        return new StringLiteral(value.StringWithoutQuotes, value.SourcePosition());
    }

    [Operand]
    [Production("doubleoperand : Double")]
    public IRule DoubleOperand(Token<RulesToken> value)
    {
        return new DoubleLiteral(Convert.ToDecimal(value.DoubleValue), value.SourcePosition());
    }

    [Operand]
    [Production("intoperand : Int")]
    public IRule IntOperand(Token<RulesToken> value)
    {
        return new IntLiteral(value.IntValue, value.SourcePosition());
    }

    [Operand]
    [Production("idoperand : Id")]
    public IRule IdOperand(Token<RulesToken> value)
    {
        return new Identifier(value.Value, value.SourcePosition());
    }

    [Operand]
    [Production("notempryoperand : Not Empty[d]")]
    public IRule NotEmpty(Token<RulesToken> not)
    {
        return new Emptyness(false, not.SourcePosition());
    }

    [Operand]
    [Production("BooleanOperand : [True|False]")]
    public IRule BoolOperand(Token<RulesToken> boolean)
    {
        return new BoolLiteral(boolean.Value == "VRAI", boolean.SourcePosition());
    }

    [Operand]
    [Production("emptyoperand : Empty")]
    public IRule Empty(Token<RulesToken> empty)
    {
        return new Emptyness(true, empty.SourcePosition());
    }

    [Operand]
    [Production("groupOperand : Lparen[d] RulesParser_expressions Rparen[d]")]
    public IRule Group(RuleExpression expression)
    {
        return new GroupExprpression(expression, expression.Position);
    }

    [Operand]
    [Production("optionsoperand : LBrack[d] option ( Comma option)* RBrack[d]")]
    public IRule Options(Literal head, List<Group<RulesToken, IRule>> tail)
    {
        var t = new Choices(head, tail.Select(x => x.Value(1)).Cast<Literal>(), head.Position);
        return t;
    }

    [Operand]
    [Production("stringArrayOperand: LSquareBracket[d] String (Comma[d] String)* RSquareBracket[d]")]
    public IRule StringArrayOperand(Token<RulesToken> head, List<Group<RulesToken, IRule>> tail)
    {
        var list = new List<StringLiteral>();
        list.Add(new StringLiteral(head.StringWithoutQuotes,head.SourcePosition()));
        list.AddRange(tail.Select(x => new StringLiteral(x.Token(0).StringWithoutQuotes,x.Token(0).SourcePosition())));
        return new StringArray(list);
    }
    
    [Operand]
    [Production("identifierArray: LSquareBracket[d] Id (Comma[d] Id)* RSquareBracket[d]")]
    public IRule IdentifierArray(Token<RulesToken> head, List<Group<RulesToken, IRule>> tail)
    {
        var list = new List<Identifier>();
        list.Add(new Identifier(head.StringWithoutQuotes,head.SourcePosition()));
        list.AddRange(tail.Select(x => new Identifier(x.Token(0).StringWithoutQuotes,x.Token(0).SourcePosition())));
        var identifierArray = new IdentifierArray(list);
        identifierArray.PrecedingComments = ExtractPreviousComments(head);
        return identifierArray;
    }

    [Production("option : Not Empty[d]")]
    public IRule OptionNonVide(Token<RulesToken> not)
    {
        return new Emptyness(false, not.SourcePosition());
    }

    [Production("option : Empty")]
    public IRule OptionVide(Token<RulesToken> empty)
    {
        return new Emptyness(true, empty.SourcePosition());
    }

    [Production("option : Int")]
    public IRule OptionInt(Token<RulesToken> token)
    {
        return new IntLiteral(token.IntValue, token.SourcePosition());
    }

    [Production("option : String")]
    public IRule OptionString(Token<RulesToken> token)
    {
        return new StringLiteral(token.StringWithoutQuotes, token.SourcePosition());
    }

    [Operand]
    [Production("functioncall : Id Lparen[d] args Rparen[d]")]
    public IRule FunctionCall(Token<RulesToken> d, ArgList args)
    {
        return new FunctionCall(d.Value, args.Arguments, d.SourcePosition());
    }

    [Production("args : RulesParser_expressions (Comma[d] RulesParser_expressions)*")]
    public IRule Arguments(RuleExpression head, List<Group<RulesToken, IRule>> tail)
    {
        var args = new List<RuleExpression> { head };
        foreach (var arg in tail)
        {
            if (arg.Value(0) != null)
            {
                args.Add(arg.Value(0) as RuleExpression);
            }
        }

        return new ArgList(args);
    }
}
