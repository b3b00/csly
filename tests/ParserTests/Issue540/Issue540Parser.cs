
using System;
using sly.parser.generator;
using sly.parser.parser;
using System.Collections.Generic;
using ParserTests.Issue540;
using sly.lexer;


namespace ParserTests.Issue540
{
    public partial class Issue540Parser
    {
        void AppendIfNotEmpty(List<object> nodeList, ValueOption<object> Considered)
        {
            if (GetFromValOp(Considered) is object node)
            {
                nodeList.Add(node);
            }
        }

        List<object> BuildChildren(params object[] Vals)
        {
            return new();

        }

        object? GetFromValOp(ValueOption<object> value)
        {
            if (value.IsSome)
            {
                return value.Match(x => x, () => throw new Exception("IsSome was true when no value was found"));
            }

            return null;
        }

        object FromToken(Issue540Token t) => null;

        [Production($"NTSection: NTStatement*")]
        public object NTSection(List<object> Statements)
        {
            return null;
        }

        [Production(
            $"NTStatement: [NTSCExpr | NTLoop | NTCond | NTFunction | NTBlock | NTReturnStatement | NTLoopControlStatement]")]
        public object NTStatement(object SubStatement) => SubStatement;

        [Production($"NTSCExpr: NTExpression")]

        public object NTSCExpr(object Expression) => Expression;

        [Production($"NTReturnStatement: Return [d] NTSCExpr")]
        public object NTReturnStatement(object SCExpr) => null;

        [Production($"NTLoopControlStatement: [Break | Continue] NTNestedValueInLoopControl?")]
        public object NTLoopControlStatement(Issue540Token Operator, ValueOption<object> NestedVal) => null;

        [Production($"NTNestedValueInLoopControl: Identifier")]
        public object NTNestedValueInLoopControl(Token<Issue540Token> val) => null;

        [Production($"NTLoop: NTForLoopHeader NTLoopLabel? NTStatement NTElse?")]
        [Production($"NTLoop: NTWhileLoopHeader NTLoopLabel? NTStatement NTElse?")]
        public object NTLoop(object LoopHeader, ValueOption<object> Label, object StatementExpr,
            ValueOption<object> ElseExpr) => null;

        [Production($"NTLoopLabel: As [d] Identifier")]
        public object NTLoopLabel(Token<Issue540Token> ident) => null;

        [Production(
            $"NTForLoopHeader: For [d] OpenParen [d] NTExpression Semicolon [d] NTExpression Semicolon [d] NTExpression CloseParen [d]")]
        public object NTForLoopHeader(object Init, object Condition, object Step) => null;

        [Production($"NTWhileLoopHeader: While [d] OpenParen [d] NTExpression CloseParen [d]")]
        public object NTWhileLoopHeader(object Condition) => null;

        [Production($"NTCond: [NTSwitch | NTIf]")]
        public object NTCond(object C) => null;

        [Production($"NTIf: If [d] OpenParen [d] NTExpression CloseParen [d] NTStatement NTElse?")]
        public object NTIf(object Cond, object StatementExpr, ValueOption<object> ElseExpr) => null;

        [Production($"NTElse: Else [d] NTStatement")]
        public object NTElse(object AStatement) => null;

        [Production(
            $"NTSwitch: Switch [d] OpenParen [d] NTExpression CloseParen [d] OpenCurly [d] NTSwitchBody* CloseCurly [d]")]
        public object NTSwitch(object AExpression, List<object> ASwitchBody) => null;

        [Production($"NTSwitchBody: NTExpression Colon [d] NTStatement")]
        public object NTSwitchBody(object AExpr, object AStatement) => null;

        [Production($"NTFunction: NTType Identifier OpenParen [d] NTTypeAndIdentifierCSV? CloseParen [d] NTStatement")]
        public object NTFunction(object AType, Token<Issue540Token> Ident, ValueOption<object> TICSV, object Statement) =>
            null;

        [Production($"NTTypeAndIdentifierCSV: NTTypeAndIdentifierCSVElement (Comma [d] NTTypeAndIdentifierCSV)*")]
        public object NTTypeAndIdentifierCSV(object Element, List<Group<Issue540Token, object>> Prime) => null;

        [Production($"NTTypeAndIdentifierCSVElement: NTFunctionArgDeclModifiersCombined? NTType Identifier")]
        public object NTTypeAndIdentifierCSVElement(ValueOption<object> Modifiers, object AType, Token<Issue540Token> Ident) =>
            null;

        [Production($"NTBlock: OpenCurly [d] NTSection CloseCurly [d]")]
        public object NTBlock(object ASection) => ASection;

        [Production($"NTExpression: NTAliasExpr")]
        public object NTExpression(object pass) => pass;

        [Production($"NTAliasExpr: [NTAliasExpr1 | NTAliasExpr2 | NTAliasExpr3 | NTDeclarationExpr]")]
        public object NTAliasExpr(object Node) => Node;

        [Production($"NTAliasExpr1: Identifier As [d] Identifier")]
        public object NTAliasExpr1(Token<Issue540Token> Ident, Token<Issue540Token> Ident2) => null;

        [Production($"NTAliasExpr2: Identifier As [d] NTType Identifier")]
        public object NTAliasExpr2(Token<Issue540Token> Ident, object AType, Token<Issue540Token> Ident2) => null;

        [Production($"NTAliasExpr3: Identifier As [d] NTType")]
        public object NTAliasExpr3(Token<Issue540Token> Ident, object Type) => null;

        [Production($"NTDeclarationExpr: [NTDeclarationExpr1 | NTAssignmentExpr]")]
        public object NTDeclarationExpr(object Node) => Node;

        [Production($"NTDeclarationExpr1: NTDeclarationModifiersCombined? NTType Identifier NTAssignmentPrime?")]
        public object NTDeclarationExpr1(ValueOption<object> Modifiers, object AType, Token<Issue540Token> Ident,
            ValueOption<object> AAssignmentPrime) => null;

        [Production($"NTDeclarationModifiersCombined: NTDeclarationModifier+")]
        public object NTDeclarationModifiersCombined(List<object> Modifiers) => null;

        [Production($"NTDeclarationModifier: [Ref | Readonly | Frozen | Immut]")]
        public object NTDeclarationModifier(Token<Issue540Token> Mod) => null;

        [Production($"NTFunctionArgDeclModifier: [Ref | Readonly | Frozen | Immut | Copy]")]
        public object NTFunctionArgDeclModifier(Token<Issue540Token> Mod) => null;

        [Production($"NTFunctionArgDeclModifiersCombined: NTFunctionArgDeclModifier+")]
        public object NTFunctionArgDeclModifiersCombined(List<object> Modifiers) => null;

        [Production($"NTAssignmentPrime: Equals NTExpression")]
        public object NTAssignmentPrime(Token<Issue540Token> EQ, object Expr) => null;

        [Production($"NTAssignmentExpr: NTAssignmentExpr1")]
        public object NTAssignmentExpr(object Node) => null;

        [Production($"NTAssignmentExpr1: Issue540Parser_expressions")]
        public object NTAssignmentExpr1(object P1) => null;

        [Operand]
        [Production($"NTPrimary: NTLPrimary NTPrimaryPrime?")]
        public object NTPrimary(object APrimary, ValueOption<object> Prime) => null;

        [Production($"NTPrimaryPrime: [NTIndexPrime | NTFunctionCallPrime]")]
        public object NTPrimaryPrime(object Node) => null;

        [Production($"NTIndexPrime: OpenSquare [d] NTExpression CloseSquare [d]")]
        public object NTIndexPrime(object Expr) => null;

        [Production($"NTLPrimary: [NTNewExpr | NTLPrimary1 | NTLPrimary2 | NTLPrimary3]")]
        public object NTLPrimary(object Node) => null;

        [Production($"NTLPrimary1: OpenParen [d] NTExpression CloseParen [d]")]
        public object NTLPrimary1(object Expr) => null;

        [Production($"NTLPrimary2: Copy [d] NTExpression")]
        public object NTLPrimary2(object Expr) => null;

        [Production($"NTLPrimary3: [Identifier | Number | String | TrueLiteral | FalseLiteral]")]
        public object NTLPrimary3(Token<Issue540Token> Token) => null;

        [Production($"NTNewExpr: New [d] NTType OpenParen [d] NTArgList?")]
        public object NTNewExpr(object AType, ValueOption<object> AArgList) => null;

        [Production($"NTFunctionCallPrime:OpenParen [d] NTArgList? CloseParen [d]")]
        public object NTFunctionCallPrime(ValueOption<object> AArgList) => null;

        [Production($"NTArgList: NTArgListElement NTArgListPrime*")]
        public object NTArgList(object Element, List<object> Elements) => null;

        [Production($"NTArgListElement: NTArgumentLabel? NTExpression")]
        public object NTArgListElement(ValueOption<object> Label, object Expr) => null;

        [Production($"NTArgListPrime: Comma [d] NTArgListElement")]
        public object NTArgListPrime(object Element) => null;

        [Production($"NTArgumentLabel: Identifier Colon [d]")]
        public object NTArgumentLabel(Token<Issue540Token> Ident) => null;

        [Production($"NTTypeCSV: NTType (Comma [d] NTType)*")]
        public object NTTypeCSV(object AType, List<Group<Issue540Token, object>> OtherTypes) => null;

        [Production($"NTType: [NTBaseType | NTGenericType]")]
        public object NTType(object Node) => null;

        [Production(
            $"NTGenericType: [TypeArray | TypeList | TypeSet | TypeDict | TypeCollection] OpenAngleSquare [d] NTTypeCSV CloseAngleSquare [d]")]
        public object NTGenericType(Token<Issue540Token> TypeToken, object TypeArgs) => null;

        [Production(
            $"NTBaseType: [TypeByte | TypeShort | TypeInt | TypeLong | TypeLongInt | TypeFloat | TypeDouble | TypeRational | TypeNumber | TypeString | TypeChar | TypeVoid]")]
        public object NTBaseType(Token<Issue540Token> TypeToken) => null;
    }


    public partial class Issue540Parser
    {
        #region ComparisonExpressions

        const int EqualToPrecedence = NotExprPrecedence + 1;
        const int NotEqualToPrecedence = EqualToPrecedence;
        const int GreaterThanPrecedence = NotEqualToPrecedence;
        const int LessThanPrecedence = GreaterThanPrecedence;
        const int GreaterThanOrEqualToPrecedence = LessThanPrecedence;
        const int LessThanOrEqualToPrecedence = GreaterThanOrEqualToPrecedence;

        [Infix((int)Issue540Token.EqualTo, Associativity.Right, EqualToPrecedence)]
        [Infix((int)Issue540Token.NotEqualTo, Associativity.Right, NotEqualToPrecedence)]
        [Infix((int)Issue540Token.GreaterThan, Associativity.Right, GreaterThanPrecedence)]
        [Infix((int)Issue540Token.LessThan, Associativity.Right, LessThanPrecedence)]
        [Infix((int)Issue540Token.GreaterThanOrEqualTo, Associativity.Right, GreaterThanOrEqualToPrecedence)]
        [Infix((int)Issue540Token.LessThanOrEqualTo, Associativity.Right, LessThanOrEqualToPrecedence)]
        public object ComparisonExpressions(object Left, Token<Issue540Token> Operator, object Right) => null;

        #endregion

        #region OtherExpressions

        const int AssignmentExprPrecedence = 1;
        const int ImpliesExprPrecedence = AssignmentExprPrecedence + 1;
        const int OrExprPrecedence = ImpliesExprPrecedence + 1;

        const int XorExprPrecedence = OrExprPrecedence + 1;

        const int AndExprPrecedence = XorExprPrecedence + 1;

        const int NotExprPrecedence = AndExprPrecedence + 1;
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

        #endregion

        [Postfix((int)Issue540Token.Factorial, Associativity.Left, FactorialPrecedence)]
        public object Factorial(object Left, Token<Issue540Token> Op) => null;

        [Prefix((int)Issue540Token.LogicalNot, Associativity.Right, NotExprPrecedence)]
        [Prefix((int)Issue540Token.BitwiseNegation, Associativity.Right, BitwiseNotExprPrecedence)]
        [Prefix((int)Issue540Token.Subtraction, Associativity.Right, NegationPrecedence)]
        public object Prefix(Token<Issue540Token> Op, object Right) => null;


        [Infix((int)Issue540Token.Equals, Associativity.Right, 1)]
        [Infix((int)Issue540Token.LogicalOr, Associativity.Left, 1)]
        [Infix((int)Issue540Token.LogicalXor, Associativity.Left, 1)]
        [Infix((int)Issue540Token.LogicalAnd, Associativity.Left, 1)]
        [Infix((int)Issue540Token.Addition, Associativity.Left, 1)]
        [Infix((int)Issue540Token.Subtraction, Associativity.Left, 1)]
        [Infix((int)Issue540Token.Division, Associativity.Left, 1)]
        [Infix((int)Issue540Token.Multiplication, Associativity.Left, 1)]
        [Infix((int)Issue540Token.Exponentiation, Associativity.Right, 1)]
        [Infix((int)Issue540Token.Subtraction, Associativity.Left, 1)]
        [Infix((int)Issue540Token.BitwiseOr, Associativity.Left, 1)]
        [Infix((int)Issue540Token.BitwiseXor, Associativity.Left, 1)]
        [Infix((int)Issue540Token.BitwiseAnd, Associativity.Left, 1)]
        [Infix((int)Issue540Token.BitwiseLeftShift, Associativity.Left, 1)]
        [Infix((int)Issue540Token.BitwiseRightShift, Associativity.Left, 1)]
        public object BinOp(object left, Token<Issue540Token> Op, object right) => null;
    }
}