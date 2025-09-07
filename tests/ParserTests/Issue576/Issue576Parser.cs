using sly.lexer;
using sly.parser.generator;
using System.Collections.Generic;
using sly.parser.parser;

namespace issue576
{
    [ParserRoot("NTSection")]
    public class Issue576Parser
    {
        [Production("NTSection : NTStatement *")]
        public object NTSection_NTStatement_(List<object> p0)
        {
            return default(object);
        }

        [Production("NTStatement : [ NTSCExpr | NTLoop | NTCond | NTFunction | NTBlock | NTReturnStatement | NTLoopControlStatement ]")]
        public object NTStatement_NTSCExpr_NTLoop_NTCond_NTFunction_NTBlock_NTReturnStatement_NTLoopControlStatement_(object p0)
        {
            return default(object);
        }

        [Production("NTSCExpr : NTExpression Semicolon")]
        public object NTSCExpr_NTExpression_Semicolon(object p0, Token<Issue576Lexer> p1)
        {
            return default(object);
        }

        [Production("NTReturnStatement : Return NTExpression Semicolon")]
        public object NTReturnStatement_Return_NTExpression_Semicolon(Token<Issue576Lexer> p0, object p1, Token<Issue576Lexer> p2)
        {
            return default(object);
        }

        [Production("NTLoopControlStatement : [ Break | Continue ] NTNestedValueInLoopControl? Semicolon")]
        public object NTLoopControlStatement_Break_Continue_NTNestedValueInLoopControl_Semicolon(Token<Issue576Lexer> p0, ValueOption<object> p1, Token<Issue576Lexer> p2)
        {
            return default(object);
        }

        [Production("NTNestedValueInLoopControl : Identifier")]
        public object NTNestedValueInLoopControl_Identifier(Token<Issue576Lexer> p0)
        {
            return default(object);
        }

        [Production("NTLoop : NTForLoop")]
        public object NTLoop_NTForLoop(object p0)
        {
            return default(object);
        }

        [Production("NTLoop : NTWhileLoop")]
        public object NTLoop_NTWhileLoop(object p0)
        {
            return default(object);
        }

        [Production("NTLoopLabel : As Identifier")]
        public object NTLoopLabel_As_Identifier(Token<Issue576Lexer> p0, Token<Issue576Lexer> p1)
        {
            return default(object);
        }

        [Production("NTForLoop : For OpenParen NTExpression Semicolon NTExpression Semicolon NTExpression CloseParen NTLoopLabel? NTStatement NTElse?")]
        public object NTForLoop_For_OpenParen_NTExpression_Semicolon_NTExpression_Semicolon_NTExpression_CloseParen_NTLoopLabel_NTStatement_NTElse_(Token<Issue576Lexer> p0, Token<Issue576Lexer> p1, object p2, Token<Issue576Lexer> p3, object p4, Token<Issue576Lexer> p5, object p6, Token<Issue576Lexer> p7, ValueOption<object> p8, object p9, ValueOption<object> p10)
        {
            return default(object);
        }

        [Production("NTWhileLoop : While OpenParen NTExpression CloseParen NTLoopLabel? NTStatement NTElse?")]
        public object NTWhileLoop_While_OpenParen_NTExpression_CloseParen_NTLoopLabel_NTStatement_NTElse_(Token<Issue576Lexer> p0, Token<Issue576Lexer> p1, object p2, Token<Issue576Lexer> p3, ValueOption<object> p4, object p5, ValueOption<object> p6)
        {
            return default(object);
        }

        [Production("NTCond : [ NTSwitch | NTIf ]")]
        public object NTCond_NTSwitch_NTIf_(object p0)
        {
            return default(object);
        }

        [Production("NTIf : If OpenParen NTExpression CloseParen NTStatement NTElse?")]
        public object NTIf_If_OpenParen_NTExpression_CloseParen_NTStatement_NTElse_(Token<Issue576Lexer> p0, Token<Issue576Lexer> p1, object p2, Token<Issue576Lexer> p3, object p4, ValueOption<object> p5)
        {
            return default(object);
        }

        [Production("NTElse : Else NTStatement")]
        public object NTElse_Else_NTStatement(Token<Issue576Lexer> p0, object p1)
        {
            return default(object);
        }

        [Production("NTSwitch : Switch OpenParen NTExpression CloseParen OpenCurly NTSwitchBody * CloseCurly")]
        public object NTSwitch_Switch_OpenParen_NTExpression_CloseParen_OpenCurly_NTSwitchBody_CloseCurly(Token<Issue576Lexer> p0, Token<Issue576Lexer> p1, object p2, Token<Issue576Lexer> p3, Token<Issue576Lexer> p4, List<object> p5, Token<Issue576Lexer> p6)
        {
            return default(object);
        }

        [Production("NTSwitchBody : NTExpression Colon NTStatement")]
        public object NTSwitchBody_NTExpression_Colon_NTStatement(object p0, Token<Issue576Lexer> p1, object p2)
        {
            return default(object);
        }

        [Production("NTFunction : NTType Identifier OpenParen NTTypeAndIdentifierCSVElement * CloseParen NTStatement")]
        public object NTFunction_NTType_Identifier_OpenParen_NTTypeAndIdentifierCSVElement_CloseParen_NTStatement(object p0, Token<Issue576Lexer> p1, Token<Issue576Lexer> p2, List<object> p3, Token<Issue576Lexer> p4, object p5)
        {
            return default(object);
        }

        [Production("NTTypeAndIdentifierCSVElement : NTFunctionArgDeclModifiersCombined NTType Identifier Comma?")]
        public object NTTypeAndIdentifierCSVElement_NTFunctionArgDeclModifiersCombined_NTType_Identifier_Comma_(object p0, object p1, Token<Issue576Lexer> p2, Token<Issue576Lexer> p3)
        {
            return default(object);
        }

        [Production("NTBlock : OpenCurly NTSection CloseCurly")]
        public object NTBlock_OpenCurly_NTSection_CloseCurly(Token<Issue576Lexer> p0, object p1, Token<Issue576Lexer> p2)
        {
            return default(object);
        }

        [Production("NTExpression : NTAliasExpr")]
        public object NTExpression_NTAliasExpr(object p0)
        {
            return default(object);
        }

        [Production("NTAliasExpr : [ NTAliasExpr1 | NTAliasExpr2 | NTAliasExpr3 | NTDeclarationExpr ]")]
        public object NTAliasExpr_NTAliasExpr1_NTAliasExpr2_NTAliasExpr3_NTDeclarationExpr_(object p0)
        {
            return default(object);
        }

        [Production("NTAliasExpr1 : Identifier As Identifier")]
        public object NTAliasExpr1_Identifier_As_Identifier(Token<Issue576Lexer> p0, Token<Issue576Lexer> p1, Token<Issue576Lexer> p2)
        {
            return default(object);
        }

        [Production("NTAliasExpr2 : Identifier As NTType Identifier")]
        public object NTAliasExpr2_Identifier_As_NTType_Identifier(Token<Issue576Lexer> p0, Token<Issue576Lexer> p1, object p2, Token<Issue576Lexer> p3)
        {
            return default(object);
        }

        [Production("NTAliasExpr3 : Identifier As NTType")]
        public object NTAliasExpr3_Identifier_As_NTType(Token<Issue576Lexer> p0, Token<Issue576Lexer> p1, object p2)
        {
            return default(object);
        }

        [Production("NTDeclarationExpr : [ NTDeclarationExpr1 | NTAssignmentExpr ]")]
        public object NTDeclarationExpr_NTDeclarationExpr1_NTAssignmentExpr_(object p0)
        {
            return default(object);
        }

        [Production("NTDeclarationExpr1 : NTDeclarationModifiersCombined? NTType Identifier NTAssignmentPrime?")]
        public object NTDeclarationExpr1_NTDeclarationModifiersCombined_NTType_Identifier_NTAssignmentPrime_(ValueOption<object> p0, object p1, Token<Issue576Lexer> p2, ValueOption<object> p3)
        {
            return default(object);
        }

        [Production("NTDeclarationModifiersCombined : NTDeclarationModifier *")]
        public object NTDeclarationModifiersCombined_NTDeclarationModifier_(List<object> p0)
        {
            return default(object);
        }

        [Production("NTDeclarationModifier : [ Ref | Readonly | Frozen | Immut ]")]
        public object NTDeclarationModifier_Ref_Readonly_Frozen_Immut_(Token<Issue576Lexer> p0)
        {
            return default(object);
        }

        [Production("NTFunctionArgDeclModifier : [ Ref | Readonly | Frozen | Immut | Copy ]")]
        public object NTFunctionArgDeclModifier_Ref_Readonly_Frozen_Immut_Copy_(Token<Issue576Lexer> p0)
        {
            return default(object);
        }

        [Production("NTFunctionArgDeclModifiersCombined : NTFunctionArgDeclModifier *")]
        public object NTFunctionArgDeclModifiersCombined_NTFunctionArgDeclModifier_(List<object> p0)
        {
            return default(object);
        }

        [Production("NTAssignmentPrime : Equals NTExpression")]
        public object NTAssignmentPrime_Equals_NTExpression(Token<Issue576Lexer> p0, object p1)
        {
            return default(object);
        }

        [Production("NTAssignmentExpr : NTAssignmentExpr1")]
        public object NTAssignmentExpr_NTAssignmentExpr1(object p0)
        {
            return default(object);
        }

        [Production("NTAssignmentExpr1 : Issue576Parser_expressions")]
        public object NTAssignmentExpr1_Issue576Parserexpressions(object p0)
        {
            return default(object);
        }

        [Operand]
        [Production("NTPrimary : NTLPrimary")]
        public object NTPrimary_NTLPrimary(object p0)
        {
            return default(object);
        }

        [Operand]
        [Production("NTPrimary : NTLPrimary OpenSquare NTExpression CloseSquare")]
        public object NTPrimary_NTLPrimary_OpenSquare_NTExpression_CloseSquare(object p0, Token<Issue576Lexer> p1, object p2, Token<Issue576Lexer> p3)
        {
            return default(object);
        }

        [Operand]
        [Production("NTPrimary : NTLPrimary OpenParen NTArgListElement * CloseParen")]
        public object NTPrimary_NTLPrimary_OpenParen_NTArgListElement_CloseParen(object p0, Token<Issue576Lexer> p1, List<object> p2, Token<Issue576Lexer> p3)
        {
            return default(object);
        }

        [Production("NTLPrimary : [ NTNewExpr | NTLPrimary1 | NTLPrimary2 | NTLPrimary3 ]")]
        public object NTLPrimary_NTNewExpr_NTLPrimary1_NTLPrimary2_NTLPrimary3_(object p0)
        {
            return default(object);
        }

        [Production("NTLPrimary1 : OpenParen NTExpression CloseParen")]
        public object NTLPrimary1_OpenParen_NTExpression_CloseParen(Token<Issue576Lexer> p0, object p1, Token<Issue576Lexer> p2)
        {
            return default(object);
        }

        [Production("NTLPrimary2 : Copy NTExpression")]
        public object NTLPrimary2_Copy_NTExpression(Token<Issue576Lexer> p0, object p1)
        {
            return default(object);
        }

        [Production("NTLPrimary3 : [ Identifier | Number | String | TrueLiteral | FalseLiteral ]")]
        public object NTLPrimary3_Identifier_Number_String_TrueLiteral_FalseLiteral_(Token<Issue576Lexer> p0)
        {
            return default(object);
        }

        [Production("NTNewExpr : New NTType OpenParen NTArgListElement * CloseParen")]
        public object NTNewExpr_New_NTType_OpenParen_NTArgListElement_CloseParen(Token<Issue576Lexer> p0, object p1, Token<Issue576Lexer> p2, List<object> p3, Token<Issue576Lexer> p4)
        {
            return default(object);
        }

        [Production("NTArgListElement : NTArgumentLabel? NTExpression Comma?")]
        public object NTArgListElement_NTArgumentLabel_NTExpression_Comma_(ValueOption<object> p0, object p1, Token<Issue576Lexer> p2)
        {
            return default(object);
        }

        [Production("NTArgumentLabel : Identifier Colon")]
        public object NTArgumentLabel_Identifier_Colon(Token<Issue576Lexer> p0, Token<Issue576Lexer> p1)
        {
            return default(object);
        }

        [Production("NTTypeCSV : NTType (Comma NTType) *")]
        public object NTTypeCSV_NTType_Comma_NTType_(object p0, List<Group<Issue576Lexer, object>> p1)
        {
            return default(object);
        }

        [Production("NTType : [ NTBaseType | NTGenericType ]")]
        public object NTType_NTBaseType_NTGenericType_(object p0)
        {
            return default(object);
        }

        [Production("NTGenericType : [ TypeArray | TypeList | TypeSet | TypeDict | TypeCollection ] OpenAngleSquare NTTypeCSV CloseAngleSquare")]
        public object NTGenericType_TypeArray_TypeList_TypeSet_TypeDict_TypeCollection_OpenAngleSquare_NTTypeCSV_CloseAngleSquare(Token<Issue576Lexer> p0, Token<Issue576Lexer> p1, object p2, Token<Issue576Lexer> p3)
        {
            return default(object);
        }

        [Production("NTBaseType : [ TypeBool | TypeByte | TypeShort | TypeInt | TypeLong | TypeLongInt | TypeFloat | TypeDouble | TypeRational | TypeNumber | TypeString | TypeChar | TypeVoid ]")]
        public object NTBaseType_TypeBool_TypeByte_TypeShort_TypeInt_TypeLong_TypeLongInt_TypeFloat_TypeDouble_TypeRational_TypeNumber_TypeString_TypeChar_TypeVoid_(Token<Issue576Lexer> p0)
        {
            return default(object);
        }

        [Postfix("Factorial", Associativity.Left, 13)]
        public object Factorial(object value, Token<Issue576Lexer> oper)
        {
            return value;
        }

        [Prefix("LogicalNot", Associativity.Left, 6)]
        public object LogicalNot(Token<Issue576Lexer> oper, object value)
        {
            return value;
        }

        [Prefix("BitwiseNegation", Associativity.Left, 18)]
        public object BitwiseNegation(Token<Issue576Lexer> oper, object value)
        {
            return value;
        }

        [Prefix("Subtraction", Associativity.Left, 12)]
        public object Subtraction(Token<Issue576Lexer> oper, object value)
        {
            return value;
        }

        [Infix("Equals", Associativity.Right, 1)]
        public object Equals(object left, Token<Issue576Lexer> oper, object right)
        {
            return left;
        }

        [Infix("LogicalOr", Associativity.Left, 3)]
        public object LogicalOr(object left, Token<Issue576Lexer> oper, object right)
        {
            return left;
        }

        [Infix("LogicalXor", Associativity.Left, 4)]
        public object LogicalXor(object left, Token<Issue576Lexer> oper, object right)
        {
            return left;
        }

        [Infix("LogicalAnd", Associativity.Left, 5)]
        public object LogicalAnd(object left, Token<Issue576Lexer> oper, object right)
        {
            return left;
        }

        [Infix("Addition", Associativity.Left, 9)]
        public object Addition(object left, Token<Issue576Lexer> oper, object right)
        {
            return left;
        }

        [Infix("Subtraction", Associativity.Left, 9)]
        [Infix("Subtraction", Associativity.Left, 9)]
        public object Subtraction(object left, Token<Issue576Lexer> oper, object right)
        {
            return left;
        }

        [Infix("Division", Associativity.Left, 10)]
        public object Division(object left, Token<Issue576Lexer> oper, object right)
        {
            return left;
        }

        [Infix("Multiplication", Associativity.Left, 10)]
        public object Multiplication(object left, Token<Issue576Lexer> oper, object right)
        {
            return left;
        }

        [Infix("Exponentiation", Associativity.Right, 11)]
        public object Exponentiation(object left, Token<Issue576Lexer> oper, object right)
        {
            return left;
        }

        // [Infix("Subtraction", Associativity.Left, 12)]
        // public object Subtraction(object left, Token<Issue576Lexer> oper, object right)
        // {
        //     return left;
        // }

        [Infix("BitwiseOr", Associativity.Left, 15)]
        public object BitwiseOr(object left, Token<Issue576Lexer> oper, object right)
        {
            return left;
        }

        [Infix("BitwiseXor", Associativity.Left, 16)]
        public object BitwiseXor(object left, Token<Issue576Lexer> oper, object right)
        {
            return left;
        }

        [Infix("BitwiseAnd", Associativity.Left, 17)]
        public object BitwiseAnd(object left, Token<Issue576Lexer> oper, object right)
        {
            return left;
        }

        [Infix("BitwiseLeftShift", Associativity.Left, 14)]
        public object BitwiseLeftShift(object left, Token<Issue576Lexer> oper, object right)
        {
            return left;
        }

        [Infix("BitwiseRightShift", Associativity.Left, 14)]
        public object BitwiseRightShift(object left, Token<Issue576Lexer> oper, object right)
        {
            return left;
        }

        [Infix("PlusEquals", Associativity.Right, 7)]
        public object PlusEquals(object left, Token<Issue576Lexer> oper, object right)
        {
            return left;
        }

        [Infix("MinusEquals", Associativity.Right, 7)]
        public object MinusEquals(object left, Token<Issue576Lexer> oper, object right)
        {
            return left;
        }

        [Infix("MultiplicationEquals", Associativity.Right, 7)]
        public object MultiplicationEquals(object left, Token<Issue576Lexer> oper, object right)
        {
            return left;
        }

        [Infix("DivideEquals", Associativity.Right, 7)]
        public object DivideEquals(object left, Token<Issue576Lexer> oper, object right)
        {
            return left;
        }

        [Infix("PowerEquals", Associativity.Right, 11)]
        public object PowerEquals(object left, Token<Issue576Lexer> oper, object right)
        {
            return left;
        }

        [Infix("BitwiseAndEquals", Associativity.Right, 7)]
        public object BitwiseAndEquals(object left, Token<Issue576Lexer> oper, object right)
        {
            return left;
        }

        [Infix("BitwiseOrEquals", Associativity.Right, 7)]
        public object BitwiseOrEquals(object left, Token<Issue576Lexer> oper, object right)
        {
            return left;
        }

        [Infix("BitwiseXorEquals", Associativity.Right, 7)]
        public object BitwiseXorEquals(object left, Token<Issue576Lexer> oper, object right)
        {
            return left;
        }

        [Infix("BitwiseNegateEquals", Associativity.Right, 7)]
        public object BitwiseNegateEquals(object left, Token<Issue576Lexer> oper, object right)
        {
            return left;
        }

        [Infix("LeftShiftEquals", Associativity.Right, 7)]
        public object LeftShiftEquals(object left, Token<Issue576Lexer> oper, object right)
        {
            return left;
        }

        [Infix("RightShiftEquals", Associativity.Right, 7)]
        public object RightShiftEquals(object left, Token<Issue576Lexer> oper, object right)
        {
            return left;
        }

        [Prefix("Increment", Associativity.Left, 19)]
        public object Increment(Token<Issue576Lexer> oper, object value)
        {
            return value;
        }

        [Prefix("Decrement", Associativity.Left, 20)]
        public object Decrement(Token<Issue576Lexer> oper, object value)
        {
            return value;
        }

        [Infix("EqualTo", Associativity.Right, 8)]
        public object EqualTo(object left, Token<Issue576Lexer> oper, object right)
        {
            return left;
        }

        [Infix("NotEqualTo", Associativity.Right, 8)]
        public object NotEqualTo(object left, Token<Issue576Lexer> oper, object right)
        {
            return left;
        }

        [Infix("GreaterThan", Associativity.Right, 8)]
        public object GreaterThan(object left, Token<Issue576Lexer> oper, object right)
        {
            return left;
        }

        [Infix("LessThan", Associativity.Right, 8)]
        public object LessThan(object left, Token<Issue576Lexer> oper, object right)
        {
            return left;
        }

        [Infix("GreaterThanOrEqualTo", Associativity.Right, 8)]
        public object GreaterThanOrEqualTo(object left, Token<Issue576Lexer> oper, object right)
        {
            return left;
        }

        [Infix("LessThanOrEqualTo", Associativity.Right, 8)]
        public object LessThanOrEqualTo(object left, Token<Issue576Lexer> oper, object right)
        {
            return left;
        }
    }
}