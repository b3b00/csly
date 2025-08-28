using sly.lexer;
using sly.parser.generator;


namespace ParserTests.Issue574;

public partial class Issue574Parser
{
    #region ComparisonExpressions
    const int EqualToPrecedence = PlusEqualsExprPrecedence + 1;
    const int NotEqualToPrecedence = EqualToPrecedence;
    const int GreaterThanPrecedence = NotEqualToPrecedence;
    const int LessThanPrecedence = GreaterThanPrecedence;
    const int GreaterThanOrEqualToPrecedence = LessThanPrecedence;
    const int LessThanOrEqualToPrecedence = GreaterThanOrEqualToPrecedence;

    [Infix((int)Issue574Token.EqualTo, Associativity.Right, EqualToPrecedence)]
    [Infix((int)Issue574Token.NotEqualTo, Associativity.Right, NotEqualToPrecedence)]
    [Infix((int)Issue574Token.GreaterThan, Associativity.Right, GreaterThanPrecedence)]
    [Infix((int)Issue574Token.LessThan, Associativity.Right, LessThanPrecedence)]
    [Infix((int)Issue574Token.GreaterThanOrEqualTo, Associativity.Right, GreaterThanOrEqualToPrecedence)]
    [Infix((int)Issue574Token.LessThanOrEqualTo, Associativity.Right, LessThanOrEqualToPrecedence)]
    public object ComparisonExpressions(object Left, Token<Issue574Token> Operator, object Right) => null;
    
    #endregion
    #region OtherExpressions
    const int AssignmentExprPrecedence = 1;
    const int ImpliesExprPrecedence = AssignmentExprPrecedence + 1;
    const int OrExprPrecedence = ImpliesExprPrecedence + 1;

    const int XorExprPrecedence = OrExprPrecedence + 1;

    const int AndExprPrecedence = XorExprPrecedence + 1;

    const int NotExprPrecedence = AndExprPrecedence + 1;
    const int PlusEqualsExprPrecedence = NotExprPrecedence + 1;
    const int MinusEqualsExprPrecedence = PlusEqualsExprPrecedence;
    const int MultiplicationEqualsExprPrecedence = MinusEqualsExprPrecedence;
    const int DivisionEqualsExprPrecedence = MultiplicationEqualsExprPrecedence;
    const int BitwiseAndEqualsPrecedence = DivisionEqualsExprPrecedence;
    const int BitwiseOrEqualsPrecedence = BitwiseAndEqualsPrecedence;
    const int BitwiseXorEqualsPrecedence = BitwiseOrEqualsPrecedence;
    const int BitwiseNotEqualsPrecedence = BitwiseXorEqualsPrecedence;

    const int BitwiseLeftShiftEqualsPrecedence = BitwiseNotEqualsPrecedence;
    const int BitwiseRightShiftEqualsPrecedence = BitwiseLeftShiftEqualsPrecedence;
    const int AdditionPrecedence = EqualToPrecedence + 1;
    const int SubtractionPrecedence = AdditionPrecedence;

    const int MultiplicationPrecedence = SubtractionPrecedence + 1;
    const int DivisionPrecedence = MultiplicationPrecedence;

    const int PowerPrecedence = DivisionPrecedence + 1;

    const int NegationPrecedence = PowerPrecedence + 1;

    const int FactorialPrecedence = NegationPrecedence + 1;
    const int RShiftPrecedence = FactorialPrecedence + 1;
    const int LShiftPrecedence = RShiftPrecedence + 1;
    const int BitwiseOrExprPrecedence = LShiftPrecedence + 1;

    const int BitwiseXorExprPrecedence = BitwiseOrExprPrecedence + 1;

    const int BitwiseAndExprPrecedence = BitwiseXorExprPrecedence + 1;

    const int BitwiseNotExprPrecedence = BitwiseAndExprPrecedence + 1;
    const int IncrementExprPrecedence = BitwiseNotExprPrecedence + 1;
    const int DecrementExprPrecedence = IncrementExprPrecedence;
    #endregion

    [Postfix((int)Issue574Token.Factorial, Associativity.Left, FactorialPrecedence)]
    public object Factorial(object Left, Token<Issue574Token> Op) => null;
    
    [Prefix((int)Issue574Token.LogicalNot, Associativity.Right, NotExprPrecedence)]
    [Prefix((int)Issue574Token.BitwiseNegation, Associativity.Right, BitwiseNotExprPrecedence)]
    [Prefix((int)Issue574Token.Subtraction, Associativity.Right, NegationPrecedence)]
    public object Prefix(Token<Issue574Token> Op, object Right) => null;

    [Infix((int)Issue574Token.Equals, Associativity.Right, AssignmentExprPrecedence)]
    [Infix((int)Issue574Token.LogicalOr, Associativity.Left, OrExprPrecedence)]
    [Infix((int)Issue574Token.LogicalXor, Associativity.Left, XorExprPrecedence)]
    [Infix((int)Issue574Token.LogicalAnd, Associativity.Left, AndExprPrecedence)]
    [Infix((int)Issue574Token.Addition, Associativity.Left, AdditionPrecedence)]
    [Infix((int)Issue574Token.Subtraction, Associativity.Left, SubtractionPrecedence)]
    [Infix((int)Issue574Token.Division, Associativity.Left, DivisionPrecedence)]
    [Infix((int)Issue574Token.Multiplication, Associativity.Left, MultiplicationPrecedence)]
    [Infix((int)Issue574Token.Exponentiation, Associativity.Right, PowerPrecedence)]
    [Infix((int)Issue574Token.Subtraction, Associativity.Left, NegationPrecedence)]
    [Infix((int)Issue574Token.BitwiseOr, Associativity.Left, BitwiseOrExprPrecedence)]
    [Infix((int)Issue574Token.BitwiseXor, Associativity.Left, BitwiseXorExprPrecedence)]
    [Infix((int)Issue574Token.BitwiseAnd, Associativity.Left, BitwiseAndExprPrecedence)]
    [Infix((int)Issue574Token.BitwiseLeftShift, Associativity.Left, LShiftPrecedence)]
    [Infix((int)Issue574Token.BitwiseRightShift, Associativity.Left, RShiftPrecedence)]
    public object BinOp(object left, Token<Issue574Token> Op, object right) => null;

    [Infix((int)Issue574Token.PlusEquals, Associativity.Right, PlusEqualsExprPrecedence)]
    [Infix((int)Issue574Token.MinusEquals, Associativity.Right, MinusEqualsExprPrecedence)]
    [Infix((int)Issue574Token.MultiplicationEquals, Associativity.Right, MultiplicationEqualsExprPrecedence)]
    [Infix((int)Issue574Token.DivideEquals, Associativity.Right, DivisionEqualsExprPrecedence)]
    [Infix((int)Issue574Token.PowerEquals, Associativity.Right, PowerPrecedence)]
    [Infix((int)Issue574Token.BitwiseAndEquals, Associativity.Right, BitwiseAndEqualsPrecedence)]
    [Infix((int)Issue574Token.BitwiseOrEquals, Associativity.Right, BitwiseOrEqualsPrecedence)]
    [Infix((int)Issue574Token.BitwiseXorEquals, Associativity.Right, BitwiseXorEqualsPrecedence)]
    [Infix((int)Issue574Token.BitwiseNegateEquals, Associativity.Right, BitwiseNotEqualsPrecedence)]
    [Infix((int)Issue574Token.LeftShiftEquals, Associativity.Right, BitwiseLeftShiftEqualsPrecedence)]
    [Infix((int)Issue574Token.RightShiftEquals, Associativity.Right, BitwiseRightShiftEqualsPrecedence)]
    public object AssignmentOp(object left, Token<Issue574Token> Op, object right) => null;

    [Prefix((int)Issue574Token.Increment, Associativity.Left, IncrementExprPrecedence)]
    [Prefix((int)Issue574Token.Decrement, Associativity.Right, DecrementExprPrecedence)]
    public object PreCrementOp(Token<Issue574Token> left, object right) => null;
}