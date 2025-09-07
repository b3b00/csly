using System;
using System.Collections.Generic;
using System.Diagnostics;
using sly.lexer;
using sly.parser.generator;
using sly.parser.parser;

namespace Common.Tokens;

public partial class SmallLangParser
{
    


    [Production($"{nameof(NTSection)}: {nameof(NTStatement)}*")]
    public int NTSection(List<int> Statements)
    {
        return 1;
    }

    [Production(
        $"{nameof(NTStatement)}: [{nameof(NTSCExpr)} | {nameof(NTLoop)} | {nameof(NTCond)} | {nameof(NTFunction)} | {nameof(NTBlock)} | {nameof(NTReturnStatement)} | {nameof(NTLoopControlStatement)}]")]
    public int NTStatement(int SubStatement)
    {
        return SubStatement;
    }

    [Production($"{nameof(NTSCExpr)}: {nameof(NTExpression)} Semicolon [d]")]
    public int NTSCExpr(int Expression)
    {
        return Expression;
    }

    [Production($"{nameof(NTReturnStatement)}: Return [d] {nameof(NTExpression)} Semicolon [d]")]
    public int NTReturnStatement(int SCExpr)
    {
        return 1;
    }

    [Production(
        $"{nameof(NTLoopControlStatement)}: [Break | Continue] {nameof(NTNestedValueInLoopControl)}? Semicolon [d]")]
    public int NTLoopControlStatement(Token<SmallLangToken> Operator, ValueOption<int> NestedVal)
    {
        return 1;
    }

    [Production($"{nameof(NTNestedValueInLoopControl)}: Identifier")]
    public int NTNestedValueInLoopControl(Token<SmallLangToken> val)
    {
        return 1;
    }

    [Production($"{nameof(NTLoop)}: {nameof(NTForLoop)}")]
    [Production($"{nameof(NTLoop)}: {nameof(NTWhileLoop)}")]
    public int NTLoop(int Loop)
    {
        return Loop;
    }

    [Production($"{nameof(NTLoopLabel)}: As [d] Identifier")]
    public int NTLoopLabel(Token<SmallLangToken> ident)
    {
        return 1;
    }

    [Production(
        $"{nameof(NTForLoop)}: For [d] OpenParen [d] {nameof(NTExpression)} Semicolon [d] {nameof(NTExpression)} Semicolon [d] {nameof(NTExpression)} CloseParen [d] {nameof(NTLoopLabel)}? {nameof(NTStatement)} {nameof(NTElse)}?")]
    public int NTForLoop(int Init, int Condition, int Step, ValueOption<int> LoopLabel,
        int Statement, ValueOption<int> Else)
    {
        return 1;
    }

    [Production(
        $"{nameof(NTWhileLoop)}: While [d] OpenParen [d] {nameof(NTExpression)} CloseParen [d] {nameof(NTLoopLabel)}? {nameof(NTStatement)} {nameof(NTElse)}?")]
    public int NTWhileLoop(int Condition, ValueOption<int> LoopLabel, int Statement,
        ValueOption<int> Else)
    {
        return 1;
    }

    [Production($"{nameof(NTCond)}: [{nameof(NTSwitch)} | {nameof(NTIf)}]")]
    public int NTCond(int C)
    {
        return C;
    }

    [Production(
        $"{nameof(NTIf)}: If [d] OpenParen [d] {nameof(NTExpression)} CloseParen [d] {nameof(NTStatement)} {nameof(NTElse)}?")]
    public int NTIf(int Cond, int StatementExpr, ValueOption<int> ElseExpr)
    {
        return 1;
    }


    [Production($"{nameof(NTElse)}: Else [d] {nameof(NTStatement)}")]
    public int NTElse(int AStatement)
    {
        return 1;
        //passthrough
    }

    [Production(
        $"{nameof(NTSwitch)}: Switch [d] OpenParen [d] {nameof(NTExpression)} CloseParen [d] OpenCurly [d] {nameof(NTSwitchBody)}* CloseCurly [d]")]
    public int NTSwitch(int AExpression, List<int> ASwitchBody)
    {
        return 1;
    }

    [Production($"{nameof(NTSwitchBody)}: {nameof(NTExpression)} Colon [d] {nameof(NTStatement)}")]
    public int NTSwitchBody(int AExpr, int AStatement)
    {
        return 1;
    }

    [Production(
        $"{nameof(NTFunction)}: {nameof(NTType)} Identifier OpenParen [d] {nameof(NTTypeAndIdentifierCSVElement)}* CloseParen [d] {nameof(NTStatement)}")]
    public int NTFunction(int AType, Token<SmallLangToken> Ident, List<int> TICSV, int Statement)
    {
        return 1;
    }

    [Production(
        $"{nameof(NTTypeAndIdentifierCSVElement)}: {nameof(NTFunctionArgDeclModifiersCombined)} {nameof(NTType)} Identifier Comma?")]
    public int NTTypeAndIdentifierCSVElement(int Modifiers, int AType, Token<SmallLangToken> Ident, Token<SmallLangToken> _)
    {
        return 1;
    }

    [Production($"{nameof(NTBlock)}: OpenCurly [d] {nameof(NTSection)} CloseCurly [d]")]
    public int NTBlock(int ASection)
    {
        return ASection;
    }

    [Production($"{nameof(NTExpression)}: {nameof(NTAliasExpr)}")]
    public int NTExpression(int pass)
    {
        return pass;
    }

    [Production(
        $"{nameof(NTAliasExpr)}: [{nameof(NTAliasExpr1)} | {nameof(NTAliasExpr2)} | {nameof(NTAliasExpr3)} | {nameof(NTDeclarationExpr)}]")]
    public int NTAliasExpr(int Node)
    {
        return Node;
    }

    [Production($"{nameof(NTAliasExpr1)}: Identifier As [d] Identifier")]
    public int NTAliasExpr1(Token<SmallLangToken> Ident, Token<SmallLangToken> Ident2)
    {
        return 1;
    }

    [Production($"{nameof(NTAliasExpr2)}: Identifier As [d] {nameof(NTType)} Identifier")]
    public int NTAliasExpr2(Token<SmallLangToken> Ident, int AType, Token<SmallLangToken> Ident2)
    {
        return 1;
    }

    [Production($"{nameof(NTAliasExpr3)}: Identifier As [d] {nameof(NTType)}")]
    public int NTAliasExpr3(Token<SmallLangToken> Ident, int Type)
    {
        return 1;
    }

    [Production($"{nameof(NTDeclarationExpr)}: [{nameof(NTDeclarationExpr1)} | {nameof(NTAssignmentExpr)}]")]
    public int NTDeclarationExpr(int Node)
    {
        return Node;
    }

    [Production(
        $"{nameof(NTDeclarationExpr1)}: {nameof(NTDeclarationModifiersCombined)}? {nameof(NTType)} Identifier {nameof(NTAssignmentPrime)}?")]
    public int NTDeclarationExpr1(ValueOption<int> Modifiers, int AType, Token<SmallLangToken> Ident,
        ValueOption<int> AAssignmentPrime)
    {
        return 1;
    }

    [Production($"{nameof(NTDeclarationModifiersCombined)}: {nameof(NTDeclarationModifier)}*")]
    public int NTDeclarationModifiersCombined(List<int> Modifiers)
    {
        return 1;
    }

    [Production($"{nameof(NTDeclarationModifier)}: [Ref | Readonly | Frozen | Immut]")]
    public int NTDeclarationModifier(Token<SmallLangToken> Mod)
    {
        return 1;
    }

    [Production($"{nameof(NTFunctionArgDeclModifier)}: [Ref | Readonly | Frozen | Immut | Copy]")]
    public int NTFunctionArgDeclModifier(Token<SmallLangToken> Mod)
    {
        return 1;
    }

    [Production($"{nameof(NTFunctionArgDeclModifiersCombined)}: {nameof(NTFunctionArgDeclModifier)}*")]
    public int NTFunctionArgDeclModifiersCombined(List<int> Modifiers)
    {
        return 1;
    }

    [Production($"{nameof(NTAssignmentPrime)}: Equals {nameof(NTExpression)}")]
    public int NTAssignmentPrime(Token<SmallLangToken> EQ, int Expr)
    {
        return 1;
    }

    [Production($"{nameof(NTAssignmentExpr)}: {nameof(NTAssignmentExpr1)}")]
    public int NTAssignmentExpr(int Node)
    {
        return Node;
    }

    [Production($"{nameof(NTAssignmentExpr1)}: SmallLangParser_expressions")]
    public int NTAssignmentExpr1(int P1)
    {
        return P1;
    }

    [Operand]
    [Production($"{nameof(NTPrimary)}: {nameof(NTLPrimary)}")]
    public int NTPrimary(int Node)
    {
        return Node;
    }

    [Operand]
    [Production($"{nameof(NTPrimary)}: {nameof(NTLPrimary)} OpenSquare {nameof(NTExpression)} CloseSquare")]
    public int NTPrimary(int Node, Token<SmallLangToken> Open, int Expr, Token<SmallLangToken> Close)
    {
        return 1;
    }

    [Operand]
    [Production($"{nameof(NTPrimary)}: {nameof(NTLPrimary)}  OpenParen {nameof(NTArgListElement)}* CloseParen")]
    public int NTPrimary(int Node, Token<SmallLangToken> Open, List<int> Expression, Token<SmallLangToken> Close)
    {
        return 1;
    }


    [Production(
        $"{nameof(NTLPrimary)}: [{nameof(NTNewExpr)} | {nameof(NTLPrimary1)} | {nameof(NTLPrimary2)} | {nameof(NTLPrimary3)}]")]
    public int NTLPrimary(int Node)
    {
        return Node;
    }

    [Production($"{nameof(NTLPrimary1)}: OpenParen [d] {nameof(NTExpression)} CloseParen [d]")]
    public int NTLPrimary1(int Expr)
    {
        return Expr;
    }

    [Production($"{nameof(NTLPrimary2)}: Copy [d] {nameof(NTExpression)}")]
    public int NTLPrimary2(int Expr)
    {
        return 1;
    }

    [Production($"{nameof(NTLPrimary3)}: [Identifier | Number | String | TrueLiteral | FalseLiteral]")]
    public int NTLPrimary3(Token<SmallLangToken> Token)
    {
        return 1;
    }

    [Production(
        $"{nameof(NTNewExpr)}: New [d] {nameof(NTType)} OpenParen [d] {nameof(NTArgListElement)}* CloseParen [d]")]
    public int NTNewExpr(int AType, List<int> AArgList)
    {
        return 1;
    }

    [Production($"{nameof(NTArgListElement)}: {nameof(NTArgumentLabel)}? {nameof(NTExpression)} Comma?")]
    public int NTArgListElement(ValueOption<int> Label, int Expr, Token<SmallLangToken> _)
    {
        return 1;
    }

    [Production($"{nameof(NTArgumentLabel)}: Identifier Colon [d]")]
    public int NTArgumentLabel(Token<SmallLangToken> Ident)
    {
        return 1;
    }

    [Production($"{nameof(NTTypeCSV)}: {nameof(NTType)} (Comma [d] {nameof(NTType)})*")]
    public int NTTypeCSV(int AType, List<Group<SmallLangToken, int>> OtherTypes)
    {
        return 1;
    }

    [Production($"{nameof(NTType)}: [{nameof(NTBaseType)} | {nameof(NTGenericType)}]")]
    public int NTType(int Node)
    {
        return Node;
    }

    [Production(
        $"{nameof(NTGenericType)}: [TypeArray | TypeList | TypeSet | TypeDict | TypeCollection] OpenAngleSquare [d] {nameof(NTTypeCSV)} CloseAngleSquare [d]")]
    public int NTGenericType(Token<SmallLangToken> TypeToken, int TypeArgs)
    {
        return 1;
    }

    [Production(
        $"{nameof(NTBaseType)}: [TypeBool | TypeByte | TypeShort | TypeInt | TypeLong | TypeLongInt | TypeFloat | TypeDouble | TypeRational | TypeNumber | TypeString | TypeChar | TypeVoid]")]
    public int NTBaseType(Token<SmallLangToken> TypeToken)
    {
        return 1;
    }
 [Postfix((int)SmallLangToken.Factorial, Associativity.Left, FactorialPrecedence)]
    public int Factorial(int Left, Token<SmallLangToken> Op)
    {
        return 1;
    }

    [Prefix((int)SmallLangToken.LogicalNot, Associativity.Right, NotExprPrecedence)]
    [Prefix((int)SmallLangToken.BitwiseNegation, Associativity.Right, BitwiseNotExprPrecedence)]
    [Prefix((int)SmallLangToken.Subtraction, Associativity.Right, NegationPrecedence)]
    public int Prefix(Token<SmallLangToken> Op, int Right)
    {
        return 1;
    }

    [Infix((int)SmallLangToken.Equals, Associativity.Right, AssignmentExprPrecedence)]
    [Infix((int)SmallLangToken.LogicalOr, Associativity.Left, OrExprPrecedence)]
    [Infix((int)SmallLangToken.LogicalXor, Associativity.Left, XorExprPrecedence)]
    [Infix((int)SmallLangToken.LogicalAnd, Associativity.Left, AndExprPrecedence)]
    [Infix((int)SmallLangToken.Addition, Associativity.Left, AdditionPrecedence)]
    [Infix((int)SmallLangToken.Subtraction, Associativity.Left, SubtractionPrecedence)]
    [Infix((int)SmallLangToken.Division, Associativity.Left, DivisionPrecedence)]
    [Infix((int)SmallLangToken.Multiplication, Associativity.Left, MultiplicationPrecedence)]
    [Infix((int)SmallLangToken.Exponentiation, Associativity.Right, PowerPrecedence)]
    [Infix((int)SmallLangToken.Subtraction, Associativity.Left, NegationPrecedence)]
    [Infix((int)SmallLangToken.BitwiseOr, Associativity.Left, BitwiseOrExprPrecedence)]
    [Infix((int)SmallLangToken.BitwiseXor, Associativity.Left, BitwiseXorExprPrecedence)]
    [Infix((int)SmallLangToken.BitwiseAnd, Associativity.Left, BitwiseAndExprPrecedence)]
    [Infix((int)SmallLangToken.BitwiseLeftShift, Associativity.Left, LShiftPrecedence)]
    [Infix((int)SmallLangToken.BitwiseRightShift, Associativity.Left, RShiftPrecedence)]
    public int BinOp(int left, Token<SmallLangToken> Op, int right)
    {
        return 1;
    }

    [Infix((int)SmallLangToken.PlusEquals, Associativity.Right, PlusEqualsExprPrecedence)]
    [Infix((int)SmallLangToken.MinusEquals, Associativity.Right, MinusEqualsExprPrecedence)]
    [Infix((int)SmallLangToken.MultiplicationEquals, Associativity.Right, MultiplicationEqualsExprPrecedence)]
    [Infix((int)SmallLangToken.DivideEquals, Associativity.Right, DivisionEqualsExprPrecedence)]
    [Infix((int)SmallLangToken.PowerEquals, Associativity.Right, PowerPrecedence)]
    [Infix((int)SmallLangToken.BitwiseAndEquals, Associativity.Right, BitwiseAndEqualsPrecedence)]
    [Infix((int)SmallLangToken.BitwiseOrEquals, Associativity.Right, BitwiseOrEqualsPrecedence)]
    [Infix((int)SmallLangToken.BitwiseXorEquals, Associativity.Right, BitwiseXorEqualsPrecedence)]
    [Infix((int)SmallLangToken.BitwiseNegateEquals, Associativity.Right, BitwiseNotEqualsPrecedence)]
    [Infix((int)SmallLangToken.LeftShiftEquals, Associativity.Right, BitwiseLeftShiftEqualsPrecedence)]
    [Infix((int)SmallLangToken.RightShiftEquals, Associativity.Right, BitwiseRightShiftEqualsPrecedence)]
    public static int AssignmentOp(int left, Token<SmallLangToken> Op, int right)
    {
        return 1;
    }

    [Prefix((int)SmallLangToken.Increment, Associativity.Left, IncrementExprPrecedence)]
    [Prefix((int)SmallLangToken.Decrement, Associativity.Right, DecrementExprPrecedence)]
    public static int PreCrementOp(Token<SmallLangToken> left, int right)
    {
        return 1;
    }

    #region ComparisonExpressions

    private const int EqualToPrecedence = PlusEqualsExprPrecedence + 1;
    private const int NotEqualToPrecedence = EqualToPrecedence;
    private const int GreaterThanPrecedence = NotEqualToPrecedence;
    private const int LessThanPrecedence = GreaterThanPrecedence;
    private const int GreaterThanOrEqualToPrecedence = LessThanPrecedence;
    private const int LessThanOrEqualToPrecedence = GreaterThanOrEqualToPrecedence;

    [Infix((int)SmallLangToken.EqualTo, Associativity.Right, EqualToPrecedence)]
    [Infix((int)SmallLangToken.NotEqualTo, Associativity.Right, NotEqualToPrecedence)]
    [Infix((int)SmallLangToken.GreaterThan, Associativity.Right, GreaterThanPrecedence)]
    [Infix((int)SmallLangToken.LessThan, Associativity.Right, LessThanPrecedence)]
    [Infix((int)SmallLangToken.GreaterThanOrEqualTo, Associativity.Right, GreaterThanOrEqualToPrecedence)]
    [Infix((int)SmallLangToken.LessThanOrEqualTo, Associativity.Right, LessThanOrEqualToPrecedence)]
    public int ComparisonExpressions(int Left, Token<SmallLangToken> Operator, int Right)
    {
        return 1;
    }

    #endregion

    #region OtherExpressions

    private const int AssignmentExprPrecedence = 1;
    private const int ImpliesExprPrecedence = AssignmentExprPrecedence + 1;
    private const int OrExprPrecedence = ImpliesExprPrecedence + 1;

    private const int XorExprPrecedence = OrExprPrecedence + 1;

    private const int AndExprPrecedence = XorExprPrecedence + 1;

    private const int NotExprPrecedence = AndExprPrecedence + 1;
    private const int PlusEqualsExprPrecedence = NotExprPrecedence + 1;
    private const int MinusEqualsExprPrecedence = PlusEqualsExprPrecedence;
    private const int MultiplicationEqualsExprPrecedence = MinusEqualsExprPrecedence;
    private const int DivisionEqualsExprPrecedence = MultiplicationEqualsExprPrecedence;
    private const int BitwiseAndEqualsPrecedence = DivisionEqualsExprPrecedence;
    private const int BitwiseOrEqualsPrecedence = BitwiseAndEqualsPrecedence;
    private const int BitwiseXorEqualsPrecedence = BitwiseOrEqualsPrecedence;
    private const int BitwiseNotEqualsPrecedence = BitwiseXorEqualsPrecedence;

    private const int BitwiseLeftShiftEqualsPrecedence = BitwiseNotEqualsPrecedence;
    private const int BitwiseRightShiftEqualsPrecedence = BitwiseLeftShiftEqualsPrecedence;
    private const int AdditionPrecedence = EqualToPrecedence + 1;
    private const int SubtractionPrecedence = AdditionPrecedence;

    private const int MultiplicationPrecedence = SubtractionPrecedence + 1;
    private const int DivisionPrecedence = MultiplicationPrecedence;

    private const int PowerPrecedence = DivisionPrecedence + 1;

    private const int NegationPrecedence = PowerPrecedence + 1;

    private const int FactorialPrecedence = NegationPrecedence + 1;
    private const int RShiftPrecedence = FactorialPrecedence + 1;
    private const int LShiftPrecedence = RShiftPrecedence + 1;
    private const int BitwiseOrExprPrecedence = LShiftPrecedence + 1;

    private const int BitwiseXorExprPrecedence = BitwiseOrExprPrecedence + 1;

    private const int BitwiseAndExprPrecedence = BitwiseXorExprPrecedence + 1;

    private const int BitwiseNotExprPrecedence = BitwiseAndExprPrecedence + 1;
    private const int IncrementExprPrecedence = BitwiseNotExprPrecedence + 1;
    private const int DecrementExprPrecedence = IncrementExprPrecedence;

    #endregion
}



