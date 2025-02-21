
using sly.parser.generator;
using sly.parser.parser;
using System.Collections.Generic;
using sly.lexer;


namespace ParserTests.Issue540
{
    public class Issue540Parser
    {

        [Production($"NtSection: NtStatement*")]
        public object NTSection(List<object> a) => null;

        [Production(
            $"NtStatement: [NtSCExpr | NtLoop | NtCond | NtFunction | NtBlock | NtReturnStatement | NtLoopControlStatement]")]
        public object NtStatement(object subStatement) => null;

        [Production($"NtSCExpr: NtExpression Semicolon [d]")]

        public object NtScExpr(object expression) => null;

        [Production($"NtReturnStatement: Return [d] NtSCExpr")]
        public object NtReturnStatement(object expr) => null;

        [Production($"NtLoopControlStatement: [Break | Continue] NtNestedValueInLoopControl?")]
        public object NtLoopControlStatement(Issue540Token op, ValueOption<object> nestedVal) => null;

        [Production($"NtNestedValueInLoopControl: Identifier")]
        public object NtNestedValueInLoopControl(Token<Issue540Token> val) => null;

        [Production($"NtLoop: NtForLoopHeader NtLoopLabel? NtStatement NtElse?")]
        [Production($"NtLoop: NtWhileLoopHeader NtLoopLabel? NtStatement NtElse?")]
        public object NtLoop(object loopHeader, ValueOption<object> label, object statementExpr,
            ValueOption<object> slseExpr) => null;

        [Production($"NtLoopLabel: As [d] Identifier")]
        public object NtLoopLabel(Token<Issue540Token> ident) => null;

        [Production(
            $"NtForLoopHeader: For [d] OpenParen [d] NtExpression Semicolon [d] NtExpression Semicolon [d] NtExpression CloseParen [d]")]
        public object NtForLoopHeader(object init, object condition, object step) => null;

        [Production($"NtWhileLoopHeader: While [d] OpenParen [d] NtExpression CloseParen [d]")]
        public object NtWhileLoopHeader(object condition) => null;

        [Production($"NtCond: [NtSwitch | NtIf]")]
        public object NtCond(object _) => null;

        [Production($"NtIf: If [d] OpenParen [d] NtExpression CloseParen [d] NtStatement NtElse?")]
        public object NtIf(object a, object b, ValueOption<object> c) => null;

        [Production($"NtElse: Else [d] NtStatement")]
        public object NtElse(object _) => null;

        [Production(
            $"NtSwitch: Switch [d] OpenParen [d] NtExpression CloseParen [d] OpenCurly [d] NtSwitchBody* CloseCurly [d]")]
        public object NtSwitch(object a, List<object> b) => null;

        [Production($"NtSwitchBody: NtExpression Colon [d] NtStatement")]
        public object NtSwitchBody(object a, object b) => null;

        [Production($"NtFunction: NtType Identifier OpenParen [d] NtTypeAndIdentifierCSV? CloseParen [d] NtStatement")]
        public object NtFunction(object a, Token<Issue540Token> b, ValueOption<object> c, object d) =>
            null;

        [Production($"NtTypeAndIdentifierCSV: NtTypeAndIdentifierCSVElement (Comma [d] NtTypeAndIdentifierCSV)*")]
        public object NtTypeAndIdentifierCsv(object a, List<Group<Issue540Token, object>> b) => null;

        [Production($"NtTypeAndIdentifierCSVElement: NtFunctionArgDeclModifiersCombined? NtType Identifier")]
        public object NtTypeAndIdentifierCsvElement(ValueOption<object> a, object b, Token<Issue540Token> c) =>
            null;

        [Production($"NtBlock: OpenCurly [d] NtSection CloseCurly [d]")]
        public object NtBlock(object a) => null;

        [Production($"NtExpression: NtAliasExpr")]
        public object NtExpression(object pass) => pass;

        [Production($"NtAliasExpr: [NtAliasExpr1 | NtAliasExpr2 | NtAliasExpr3 | NtDeclarationExpr]")]
        public object NtAliasExpr(object a) => null;

        [Production($"NtAliasExpr1: Identifier As [d] Identifier")]
        public object NtAliasExpr1(Token<Issue540Token> a, Token<Issue540Token> b) => null;

        [Production($"NtAliasExpr2: Identifier As [d] NtType Identifier")]
        public object NtAliasExpr2(Token<Issue540Token> a, object b, Token<Issue540Token> v) => null;

        [Production($"NtAliasExpr3: Identifier As [d] NtType")]
        public object NtAliasExpr3(Token<Issue540Token> a, object b) => null;

        [Production($"NtDeclarationExpr: [NtDeclarationExpr1 | NtAssignmentExpr]")]
        public object NtDeclarationExpr(object a) => null;

        [Production($"NtDeclarationExpr1: NtDeclarationModifiersCombined? NtType Identifier NtAssignmentPrime?")]
        public object NtDeclarationExpr1(ValueOption<object> a, object b, Token<Issue540Token> c,
            ValueOption<object> d) => null;

        [Production($"NtDeclarationModifiersCombined: NtDeclarationModifier+")]
        public object NtDeclarationModifiersCombined(List<object> a) => null;

        [Production($"NtDeclarationModifier: [Ref | Readonly | Frozen | Immut]")]
        public object NtDeclarationModifier(Token<Issue540Token> a) => null;

        [Production($"NtFunctionArgDeclModifier: [Ref | Readonly | Frozen | Immut | Copy]")]
        public object NtFunctionArgDeclModifier(Token<Issue540Token> a) => null;

        [Production($"NtFunctionArgDeclModifiersCombined: NtFunctionArgDeclModifier+")]
        public object NtFunctionArgDeclModifiersCombined(List<object> a) => null;

        [Production($"NtAssignmentPrime: Equals NtExpression")]
        public object NtAssignmentPrime(Token<Issue540Token> a, object b) => null;

        [Production($"NtAssignmentExpr: NtAssignmentExpr1")]
        public object NtAssignmentExpr(object a) => null;

        [Production($"NtAssignmentExpr1: Issue540Parser_expressions")]
        public object NtAssignmentExpr1(object a) => null;

        [Operand]
        [Production($"NtPrimary: NtLPrimary NtPrimaryPrime?")]
        public object NtPrimary(object a, ValueOption<object> b) => null;

        [Production($"NtPrimaryPrime: [NtIndexPrime | NtFunctionCallPrime]")]
        public object NtPrimaryPrime(object a) => null;

        [Production($"NtIndexPrime: OpenSquare [d] NtExpression CloseSquare [d]")]
        public object NtIndexPrime(object a) => null;

        [Production($"NtLPrimary: [NtNewExpr | NtLPrimary1 | NtLPrimary2 | NtLPrimary3]")]
        public object NtLPrimary(object a) => null;

        [Production($"NtLPrimary1: OpenParen [d] NtExpression CloseParen [d]")]
        public object NtLPrimary1(object a) => null;

        [Production($"NtLPrimary2: Copy [d] NtExpression")]
        public object NtLPrimary2(object a) => null;

        [Production($"NtLPrimary3: [Identifier | Number | String | TrueLiteral | FalseLiteral]")]
        public object NtLPrimary3(Token<Issue540Token> a) => null;

        [Production($"NtNewExpr: New [d] NtType OpenParen [d] NtArgList?")]
        public object NtNewExpr(object a, ValueOption<object> b) => null;

        [Production($"NtFunctionCallPrime:OpenParen [d] NtArgList? CloseParen [d]")]
        public object NtFunctionCallPrime(ValueOption<object> a) => null;

        [Production($"NtArgList: NtArgListElement NtArgListPrime*")]
        public object NtArgList(object a, List<object> b) => null;

        [Production($"NtArgListElement: NtArgumentLabel? NtExpression")]
        public object NtArgListElement(ValueOption<object> a, object b) => null;

        [Production($"NtArgListPrime: Comma [d] NtArgListElement")]
        public object NtArgListPrime(object a) => null;

        [Production($"NtArgumentLabel: Identifier Colon [d]")]
        public object NtArgumentLabel(Token<Issue540Token> ident) => null;

        [Production($"NtTypeCSV: NtType (Comma [d] NtType)*")]
        public object NtTypeCsv(object aType, List<Group<Issue540Token, object>> otherTypes) => null;

        [Production($"NtType: [NtBaseType | NtGenericType]")]
        public object NtType(object node) => null;

        [Production(
            $"NtGenericType: [TypeArray | TypeList | TypeSet | TypeDict | TypeCollection] OpenAngleSquare [d] NtTypeCSV CloseAngleSquare [d]")]
        public object NtGenericType(Token<Issue540Token> typeToken, object typeArgs) => null;

        [Production(
            $"NtBaseType: [TypeByte | TypeShort | TypeInt | TypeLong | TypeLongInt | TypeFloat | TypeDouble | TypeRational | TypeNumber | TypeString | TypeChar | TypeVoid]")]
        public object NtBaseType(Token<Issue540Token> typeToken) => null;

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
        public object ComparisonExpressions(object a, Token<Issue540Token> b, object c) => null;

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
        public object Factorial(object a, Token<Issue540Token> b) => null;

        [Prefix((int)Issue540Token.LogicalNot, Associativity.Right, NotExprPrecedence)]
        [Prefix((int)Issue540Token.BitwiseNegation, Associativity.Right, BitwiseNotExprPrecedence)]
        [Prefix((int)Issue540Token.Subtraction, Associativity.Right, NegationPrecedence)]
        public object Prefix(Token<Issue540Token> a, object b) => null;


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
        public object BinOp(object left, Token<Issue540Token> a, object b) => null;
    }
}