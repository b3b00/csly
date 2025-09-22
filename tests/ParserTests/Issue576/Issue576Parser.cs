using sly.lexer;
using sly.parser.generator;
using System.Collections.Generic;
using sly.parser.parser;


namespace issue576;


[UseMemoization]
[BroadenTokenWindow]
[ParserRoot("NTSection")]
public class Issue576Parser
{
    [Production("NTSection : NTStatement *")]
    public int NTSection_NTStatement_(List<int> p0)
    {
        return default(int);
    }

    [Production(
        "NTStatement : [ NTSCExpr | NTLoop | NTCond | NTFunction | NTBlock | NTReturnStatement | NTLoopControlStatement ]")]
    public int NTStatement_NTSCExpr_NTLoop_NTCond_NTFunction_NTBlock_NTReturnStatement_NTLoopControlStatement_(
        int p0)
    {
        return default(int);
    }

    [Production("NTSCExpr : NTExpression Semicolon")]
    public int NTSCExpr_NTExpression_Semicolon(int p0, Token<Issue576Lexer> p1)
    {
        return default(int);
    }

    [Production("NTReturnStatement : Return NTExpression Semicolon")]
    public int NTReturnStatement_Return_NTExpression_Semicolon(Token<Issue576Lexer> p0, int p1,
        Token<Issue576Lexer> p2)
    {
        return default(int);
    }

    [Production("NTLoopControlStatement : [ Break | Continue ] NTNestedValueInLoopControl? Semicolon")]
    public int NTLoopControlStatement_Break_Continue_NTNestedValueInLoopControl_Semicolon(Token<Issue576Lexer> p0,
        ValueOption<int> p1, Token<Issue576Lexer> p2)
    {
        return default(int);
    }

    [Production("NTNestedValueInLoopControl : Identifier")]
    public int NTNestedValueInLoopControl_Identifier(Token<Issue576Lexer> p0)
    {
        return default(int);
    }

    [Production("NTLoop : NTForLoop")]
    public int NTLoop_NTForLoop(int p0)
    {
        return default(int);
    }

    [Production("NTLoop : NTWhileLoop")]
    public int NTLoop_NTWhileLoop(int p0)
    {
        return default(int);
    }

    [Production("NTLoopLabel : As Identifier")]
    public int NTLoopLabel_As_Identifier(Token<Issue576Lexer> p0, Token<Issue576Lexer> p1)
    {
        return default(int);
    }

    [Production(
        "NTForLoop : For OpenParen NTExpression Semicolon NTExpression Semicolon NTExpression CloseParen NTLoopLabel? NTStatement NTElse?")]
    public int
        NTForLoop_For_OpenParen_NTExpression_Semicolon_NTExpression_Semicolon_NTExpression_CloseParen_NTLoopLabel_NTStatement_NTElse_(
            Token<Issue576Lexer> p0, Token<Issue576Lexer> p1, int p2, Token<Issue576Lexer> p3, int p4,
            Token<Issue576Lexer> p5, int p6, Token<Issue576Lexer> p7, ValueOption<int> p8, int p9,
            ValueOption<int> p10)
    {
        return default(int);
    }

    [Production("NTWhileLoop : While OpenParen NTExpression CloseParen NTLoopLabel? NTStatement NTElse?")]
    public int NTWhileLoop_While_OpenParen_NTExpression_CloseParen_NTLoopLabel_NTStatement_NTElse_(
        Token<Issue576Lexer> p0, Token<Issue576Lexer> p1, int p2, Token<Issue576Lexer> p3, ValueOption<int> p4,
        int p5, ValueOption<int> p6)
    {
        return default(int);
    }

    [Production("NTCond : [ NTSwitch | NTIf ]")]
    public int NTCond_NTSwitch_NTIf_(int p0)
    {
        return default(int);
    }

    [Production("NTIf : If OpenParen NTExpression CloseParen NTStatement NTElse?")]
    public int NTIf_If_OpenParen_NTExpression_CloseParen_NTStatement_NTElse_(Token<Issue576Lexer> p0,
        Token<Issue576Lexer> p1, int p2, Token<Issue576Lexer> p3, int p4, ValueOption<int> p5)
    {
        return default(int);
    }

    [Production("NTElse : Else NTStatement")]
    public int NTElse_Else_NTStatement(Token<Issue576Lexer> p0, int p1)
    {
        return default(int);
    }

    [Production("NTSwitch : Switch OpenParen NTExpression CloseParen OpenCurly NTSwitchBody * CloseCurly")]
    public int NTSwitch_Switch_OpenParen_NTExpression_CloseParen_OpenCurly_NTSwitchBody_CloseCurly(
        Token<Issue576Lexer> p0, Token<Issue576Lexer> p1, int p2, Token<Issue576Lexer> p3, Token<Issue576Lexer> p4,
        List<int> p5, Token<Issue576Lexer> p6)
    {
        return default(int);
    }

    [Production("NTSwitchBody : NTExpression Colon NTStatement")]
    public int NTSwitchBody_NTExpression_Colon_NTStatement(int p0, Token<Issue576Lexer> p1, int p2)
    {
        return default(int);
    }

    [Production("NTFunction : NTType Identifier OpenParen NTTypeAndIdentifierCSVElement * CloseParen NTStatement")]
    public int NTFunction_NTType_Identifier_OpenParen_NTTypeAndIdentifierCSVElement_CloseParen_NTStatement(int p0,
        Token<Issue576Lexer> p1, Token<Issue576Lexer> p2, List<int> p3, Token<Issue576Lexer> p4, int p5)
    {
        return default(int);
    }

    [Production("NTTypeAndIdentifierCSVElement : NTFunctionArgDeclModifiersCombined NTType Identifier Comma?")]
    public int NTTypeAndIdentifierCSVElement_NTFunctionArgDeclModifiersCombined_NTType_Identifier_Comma_(int p0,
        int p1, Token<Issue576Lexer> p2, Token<Issue576Lexer> p3)
    {
        return default(int);
    }

    [Production("NTBlock : OpenCurly NTSection CloseCurly")]
    public int NTBlock_OpenCurly_NTSection_CloseCurly(Token<Issue576Lexer> p0, int p1, Token<Issue576Lexer> p2)
    {
        return default(int);
    }

    [Production("NTExpression : NTAliasExpr")]
    public int NTExpression_NTAliasExpr(int p0)
    {
        return default(int);
    }

    [Production("NTAliasExpr : [ NTAliasExpr1 | NTAliasExpr2 | NTAliasExpr3 | NTDeclarationExpr ]")]
    public int NTAliasExpr_NTAliasExpr1_NTAliasExpr2_NTAliasExpr3_NTDeclarationExpr_(int p0)
    {
        return default(int);
    }

    [Production("NTAliasExpr1 : Identifier As Identifier")]
    public int NTAliasExpr1_Identifier_As_Identifier(Token<Issue576Lexer> p0, Token<Issue576Lexer> p1,
        Token<Issue576Lexer> p2)
    {
        return default(int);
    }

    [Production("NTAliasExpr2 : Identifier As NTType Identifier")]
    public int NTAliasExpr2_Identifier_As_NTType_Identifier(Token<Issue576Lexer> p0, Token<Issue576Lexer> p1,
        int p2, Token<Issue576Lexer> p3)
    {
        return default(int);
    }

    [Production("NTAliasExpr3 : Identifier As NTType")]
    public int NTAliasExpr3_Identifier_As_NTType(Token<Issue576Lexer> p0, Token<Issue576Lexer> p1, int p2)
    {
        return default(int);
    }

    [Production("NTDeclarationExpr : [ NTDeclarationExpr1 | NTAssignmentExpr ]")]
    public int NTDeclarationExpr_NTDeclarationExpr1_NTAssignmentExpr_(int p0)
    {
        return default(int);
    }

    [Production("NTDeclarationExpr1 : NTDeclarationModifiersCombined? NTType Identifier NTAssignmentPrime?")]
    public int NTDeclarationExpr1_NTDeclarationModifiersCombined_NTType_Identifier_NTAssignmentPrime_(
        ValueOption<int> p0, int p1, Token<Issue576Lexer> p2, ValueOption<int> p3)
    {
        return default(int);
    }

    [Production("NTDeclarationModifiersCombined : NTDeclarationModifier *")]
    public int NTDeclarationModifiersCombined_NTDeclarationModifier_(List<int> p0)
    {
        return default(int);
    }

    [Production("NTDeclarationModifier : [ Ref | Readonly | Frozen | Immut ]")]
    public int NTDeclarationModifier_Ref_Readonly_Frozen_Immut_(Token<Issue576Lexer> p0)
    {
        return default(int);
    }

    [Production("NTFunctionArgDeclModifier : [ Ref | Readonly | Frozen | Immut | Copy ]")]
    public int NTFunctionArgDeclModifier_Ref_Readonly_Frozen_Immut_Copy_(Token<Issue576Lexer> p0)
    {
        return default(int);
    }

    [Production("NTFunctionArgDeclModifiersCombined : NTFunctionArgDeclModifier *")]
    public int NTFunctionArgDeclModifiersCombined_NTFunctionArgDeclModifier_(List<int> p0)
    {
        return default(int);
    }

    [Production("NTAssignmentPrime : Equals NTExpression")]
    public int NTAssignmentPrime_Equals_NTExpression(Token<Issue576Lexer> p0, int p1)
    {
        return default(int);
    }

    [Production("NTAssignmentExpr : NTAssignmentExpr1")]
    public int NTAssignmentExpr_NTAssignmentExpr1(int p0)
    {
        return default(int);
    }

    [Production("NTAssignmentExpr1 : Issue576Parser_expressions")]
    public int NTAssignmentExpr1_Issue576Parserexpressions(int p0)
    {
        return default(int);
    }

    [Operand]
    [Production("NTPrimary : NTLPrimary")]
    public int NTPrimary_NTLPrimary(int p0)
    {
        return default(int);
    }

    [Operand]
    [Production("NTPrimary : NTLPrimary OpenSquare NTExpression CloseSquare")]
    public int NTPrimary_NTLPrimary_OpenSquare_NTExpression_CloseSquare(int p0, Token<Issue576Lexer> p1,
        int p2, Token<Issue576Lexer> p3)
    {
        return default(int);
    }

    [Operand]
    [Production("NTPrimary : NTLPrimary OpenParen NTArgListElement * CloseParen")]
    public int NTPrimary_NTLPrimary_OpenParen_NTArgListElement_CloseParen(int p0, Token<Issue576Lexer> p1,
        List<int> p2, Token<Issue576Lexer> p3)
    {
        return default(int);
    }

    [Production("NTLPrimary : [ NTNewExpr | NTLPrimary1 | NTLPrimary2 | NTLPrimary3 ]")]
    public int NTLPrimary_NTNewExpr_NTLPrimary1_NTLPrimary2_NTLPrimary3_(int p0)
    {
        return default(int);
    }

    [Production("NTLPrimary1 : OpenParen NTExpression CloseParen")]
    public int NTLPrimary1_OpenParen_NTExpression_CloseParen(Token<Issue576Lexer> p0, int p1,
        Token<Issue576Lexer> p2)
    {
        return default(int);
    }

    [Production("NTLPrimary2 : Copy NTExpression")]
    public int NTLPrimary2_Copy_NTExpression(Token<Issue576Lexer> p0, int p1)
    {
        return default(int);
    }

    [Production("NTLPrimary3 : [ Identifier | Number | String | TrueLiteral | FalseLiteral ]")]
    public int NTLPrimary3_Identifier_Number_String_TrueLiteral_FalseLiteral_(Token<Issue576Lexer> p0)
    {
        return default(int);
    }

    [Production("NTNewExpr : New NTType OpenParen NTArgListElement * CloseParen")]
    public int NTNewExpr_New_NTType_OpenParen_NTArgListElement_CloseParen(Token<Issue576Lexer> p0, int p1,
        Token<Issue576Lexer> p2, List<int> p3, Token<Issue576Lexer> p4)
    {
        return default(int);
    }

    [Production("NTArgListElement : NTArgumentLabel? NTExpression Comma?")]
    public int NTArgListElement_NTArgumentLabel_NTExpression_Comma_(ValueOption<int> p0, int p1,
        Token<Issue576Lexer> p2)
    {
        return default(int);
    }

    [Production("NTArgumentLabel : Identifier Colon")]
    public int NTArgumentLabel_Identifier_Colon(Token<Issue576Lexer> p0, Token<Issue576Lexer> p1)
    {
        return default(int);
    }

    [Production("NTTypeCSV : NTType (Comma NTType) *")]
    public int NTTypeCSV_NTType_Comma_NTType_(int p0, List<Group<Issue576Lexer, int>> p1)
    {
        return default(int);
    }

    [Production("NTType : [ NTBaseType | NTGenericType ]")]
    public int NTType_NTBaseType_NTGenericType_(int p0)
    {
        return default(int);
    }

    [Production(
        "NTGenericType : [ TypeArray | TypeList | TypeSet | TypeDict | TypeCollection ] OpenAngleSquare NTTypeCSV CloseAngleSquare")]
    public int
        NTGenericType_TypeArray_TypeList_TypeSet_TypeDict_TypeCollection_OpenAngleSquare_NTTypeCSV_CloseAngleSquare(
            Token<Issue576Lexer> p0, Token<Issue576Lexer> p1, int p2, Token<Issue576Lexer> p3)
    {
        return default(int);
    }

    [Production(
        "NTBaseType : [ TypeBool | TypeByte | TypeShort | TypeInt | TypeLong | TypeLongInt | TypeFloat | TypeDouble | TypeRational | TypeNumber | TypeString | TypeChar | TypeVoid ]")]
    public int
        NTBaseType_TypeBool_TypeByte_TypeShort_TypeInt_TypeLong_TypeLongInt_TypeFloat_TypeDouble_TypeRational_TypeNumber_TypeString_TypeChar_TypeVoid_(
            Token<Issue576Lexer> p0)
    {
        return default(int);
    }

    [Postfix("Factorial", Associativity.Left, 13)]
    public int Factorial(int value, Token<Issue576Lexer> oper)
    {
        return value;
    }

    [Prefix("LogicalNot", Associativity.Left, 6)]
    public int LogicalNot(Token<Issue576Lexer> oper, int value)
    {
        return value;
    }

    [Prefix("BitwiseNegation", Associativity.Left, 18)]
    public int BitwiseNegation(Token<Issue576Lexer> oper, int value)
    {
        return value;
    }

    [Prefix("Subtraction", Associativity.Left, 12)]
    public int Subtraction(Token<Issue576Lexer> oper, int value)
    {
        return value;
    }

    [Infix("Equals", Associativity.Right, 1)]
    public int Equals(int left, Token<Issue576Lexer> oper, int right)
    {
        return left;
    }

    [Infix("LogicalOr", Associativity.Left, 3)]
    public int LogicalOr(int left, Token<Issue576Lexer> oper, int right)
    {
        return left;
    }

    [Infix("LogicalXor", Associativity.Left, 4)]
    public int LogicalXor(int left, Token<Issue576Lexer> oper, int right)
    {
        return left;
    }

    [Infix("LogicalAnd", Associativity.Left, 5)]
    public int LogicalAnd(int left, Token<Issue576Lexer> oper, int right)
    {
        return left;
    }

    [Infix("Addition", Associativity.Left, 9)]
    public int Addition(int left, Token<Issue576Lexer> oper, int right)
    {
        return left;
    }

    [Infix("Subtraction", Associativity.Left, 9)]
    [Infix("Subtraction", Associativity.Left, 9)]
    public int Subtraction(int left, Token<Issue576Lexer> oper, int right)
    {
        return left;
    }

    [Infix("Division", Associativity.Left, 10)]
    public int Division(int left, Token<Issue576Lexer> oper, int right)
    {
        return left;
    }

    [Infix("Multiplication", Associativity.Left, 10)]
    public int Multiplication(int left, Token<Issue576Lexer> oper, int right)
    {
        return left;
    }

    [Infix("Exponentiation", Associativity.Right, 11)]
    public int Exponentiation(int left, Token<Issue576Lexer> oper, int right)
    {
        return left;
    }

    // [Infix("Subtraction", Associativity.Left, 12)]
    // public int Subtraction(int left, Token<Issue576Lexer> oper, int right)
    // {
    //     return left;
    // }

    [Infix("BitwiseOr", Associativity.Left, 15)]
    public int BitwiseOr(int left, Token<Issue576Lexer> oper, int right)
    {
        return left;
    }

    [Infix("BitwiseXor", Associativity.Left, 16)]
    public int BitwiseXor(int left, Token<Issue576Lexer> oper, int right)
    {
        return left;
    }

    [Infix("BitwiseAnd", Associativity.Left, 17)]
    public int BitwiseAnd(int left, Token<Issue576Lexer> oper, int right)
    {
        return left;
    }

    [Infix("BitwiseLeftShift", Associativity.Left, 14)]
    public int BitwiseLeftShift(int left, Token<Issue576Lexer> oper, int right)
    {
        return left;
    }

    [Infix("BitwiseRightShift", Associativity.Left, 14)]
    public int BitwiseRightShift(int left, Token<Issue576Lexer> oper, int right)
    {
        return left;
    }

    [Infix("PlusEquals", Associativity.Right, 7)]
    public int PlusEquals(int left, Token<Issue576Lexer> oper, int right)
    {
        return left;
    }

    [Infix("MinusEquals", Associativity.Right, 7)]
    public int MinusEquals(int left, Token<Issue576Lexer> oper, int right)
    {
        return left;
    }

    [Infix("MultiplicationEquals", Associativity.Right, 7)]
    public int MultiplicationEquals(int left, Token<Issue576Lexer> oper, int right)
    {
        return left;
    }

    [Infix("DivideEquals", Associativity.Right, 7)]
    public int DivideEquals(int left, Token<Issue576Lexer> oper, int right)
    {
        return left;
    }

    [Infix("PowerEquals", Associativity.Right, 11)]
    public int PowerEquals(int left, Token<Issue576Lexer> oper, int right)
    {
        return left;
    }

    [Infix("BitwiseAndEquals", Associativity.Right, 7)]
    public int BitwiseAndEquals(int left, Token<Issue576Lexer> oper, int right)
    {
        return left;
    }

    [Infix("BitwiseOrEquals", Associativity.Right, 7)]
    public int BitwiseOrEquals(int left, Token<Issue576Lexer> oper, int right)
    {
        return left;
    }

    [Infix("BitwiseXorEquals", Associativity.Right, 7)]
    public int BitwiseXorEquals(int left, Token<Issue576Lexer> oper, int right)
    {
        return left;
    }

    [Infix("BitwiseNegateEquals", Associativity.Right, 7)]
    public int BitwiseNegateEquals(int left, Token<Issue576Lexer> oper, int right)
    {
        return left;
    }

    [Infix("LeftShiftEquals", Associativity.Right, 7)]
    public int LeftShiftEquals(int left, Token<Issue576Lexer> oper, int right)
    {
        return left;
    }

    [Infix("RightShiftEquals", Associativity.Right, 7)]
    public int RightShiftEquals(int left, Token<Issue576Lexer> oper, int right)
    {
        return left;
    }

    [Prefix("Increment", Associativity.Left, 19)]
    public int Increment(Token<Issue576Lexer> oper, int value)
    {
        return value;
    }

    [Prefix("Decrement", Associativity.Left, 20)]
    public int Decrement(Token<Issue576Lexer> oper, int value)
    {
        return value;
    }

    [Infix("EqualTo", Associativity.Right, 8)]
    public int EqualTo(int left, Token<Issue576Lexer> oper, int right)
    {
        return left;
    }

    [Infix("NotEqualTo", Associativity.Right, 8)]
    public int NotEqualTo(int left, Token<Issue576Lexer> oper, int right)
    {
        return left;
    }

    [Infix("GreaterThan", Associativity.Right, 8)]
    public int GreaterThan(int left, Token<Issue576Lexer> oper, int right)
    {
        return left;
    }

    [Infix("LessThan", Associativity.Right, 8)]
    public int LessThan(int left, Token<Issue576Lexer> oper, int right)
    {
        return left;
    }

    [Infix("GreaterThanOrEqualTo", Associativity.Right, 8)]
    public int GreaterThanOrEqualTo(int left, Token<Issue576Lexer> oper, int right)
    {
        return left;
    }

    [Infix("LessThanOrEqualTo", Associativity.Right, 8)]
    public int LessThanOrEqualTo(int left, Token<Issue576Lexer> oper, int right)
    {
        return left;
    }
}
