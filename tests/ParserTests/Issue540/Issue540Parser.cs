
using System.Data;
using Common.AST;
using Common.Tokens;
using sly.lexer;
using sly.parser.generator;
using sly.parser.parser;
using LyToken = sly.lexer.Token<Common.Tokens.TokenType>;
using NodeType = Common.AST.DynamicASTNode<SmallLang.ASTNodeType, SmallLang.Attributes>;
namespace ParserTests.Issue540
{
    public partial class SmallLangParser
    {
        void AppendIfNotEmpty(List<NodeType> nodeList, ValueOption<NodeType> Considered)
        {
            if (GetFromValOp(Considered) is NodeType node)
            {
                nodeList.Add(node);
            }
        }
        List<NodeType> BuildChildren(params object[] Vals)
        {
            List<NodeType> rc = new();
            foreach (var val in Vals)
            {
                if (val is NodeType node)
                {
                    rc.Add(node);
                }
                else if (val is ValueOption<NodeType> valOp)
                {
                    AppendIfNotEmpty(rc, valOp);
                }
                else if (val is LyToken TVal)
                {
                    var Token = FromToken(TVal);
                    if (Token == null) continue;
                    if (Token.TT == TokenType.Identifier)
                    {
                        rc.Add(new NodeType(Token, [], ASTNodeType.Identifier));
                    }
                    else
                    {
                        rc.Add(new NodeType(Token, [], ASTNodeType.Terminal));
                    }
                }
                else
                {
                    throw new Exception($"Unexpected type: {val.GetType().Name}");
                }
            }
            return rc;
        }
        NodeType? GetFromValOp(ValueOption<NodeType> value)
        {
            if (value.IsSome)
            {
                return value.Match(x => x, () => throw new Exception("IsSome was true when no value was found"));
            }
            return null;
        }
        IToken? FromToken(LyToken t) => t.IsEmpty ? null : IToken.NewToken(t.TokenID, t.Value, t.Position.Index, null);
        [Production($"{nameof(NTSection)}: {nameof(NTStatement)}*")]
        public NodeType NTSection(List<NodeType> Statements)
        {
            return new NodeType(null, Statements, ASTNodeType.Section);
        }
        [Production($"{nameof(NTStatement)}: [{nameof(NTSCExpr)} | {nameof(NTLoop)} | {nameof(NTCond)} | {nameof(NTFunction)} | {nameof(NTBlock)} | {nameof(NTReturnStatement)} | {nameof(NTLoopControlStatement)}]")]
        public NodeType NTStatement(NodeType SubStatement) => SubStatement;
        [Production($"{nameof(NTSCExpr)}: {nameof(NTExpression)}")]

        public NodeType NTSCExpr(NodeType Expression) => Expression;
        [Production($"{nameof(NTReturnStatement)}: Return [d] {nameof(NTSCExpr)}")]
        public NodeType NTReturnStatement(NodeType SCExpr) => SCExpr with { NodeType = ASTNodeType.Return };
        [Production($"{nameof(NTLoopControlStatement)}: [Break | Continue] {nameof(NTNestedValueInLoopControl)}?")]
        public NodeType NTLoopControlStatement(LyToken Operator, ValueOption<NodeType> NestedVal)
        {
            List<NodeType> rc = [];
            AppendIfNotEmpty(rc, NestedVal);
            return new(FromToken(Operator), rc, ASTNodeType.LoopCTRL);
        }
        [Production($"{nameof(NTNestedValueInLoopControl)}: Identifier")]
        public NodeType NTNestedValueInLoopControl(LyToken val) => new NodeType(FromToken(val), [], ASTNodeType.ValInLCTRL);

        [Production($"{nameof(NTLoop)}: {nameof(NTForLoopHeader)} {nameof(NTLoopLabel)}? {nameof(NTStatement)} {nameof(NTElse)}?")]
        [Production($"{nameof(NTLoop)}: {nameof(NTWhileLoopHeader)} {nameof(NTLoopLabel)}? {nameof(NTStatement)} {nameof(NTElse)}?")]
        public NodeType NTLoop(NodeType LoopHeader, ValueOption<NodeType> Label, NodeType StatementExpr, ValueOption<NodeType> ElseExpr)
        {
            var rt = LoopHeader.NodeType;
            List<NodeType> rc = LoopHeader.Children;
            AppendIfNotEmpty(rc, Label);
            rc.Add(StatementExpr);
            AppendIfNotEmpty(rc, ElseExpr);
            return new NodeType(null, rc, rt);
        }
        [Production($"{nameof(NTLoopLabel)}: As [d] Identifier")]
        public NodeType NTLoopLabel(LyToken ident) => new NodeType(FromToken(ident), [], ASTNodeType.LoopLabel);
        [Production($"{nameof(NTForLoopHeader)}: For [d] OpenParen [d] {nameof(NTExpression)} Semicolon [d] {nameof(NTExpression)} Semicolon [d] {nameof(NTExpression)} CloseParen [d]")]
        public NodeType NTForLoopHeader(NodeType Init, NodeType Condition, NodeType Step) => new NodeType(null, [Init, Condition, Step], ASTNodeType.For);
        [Production($"{nameof(NTWhileLoopHeader)}: While [d] OpenParen [d] {nameof(NTExpression)} CloseParen [d]")]
        public NodeType NTWhileLoopHeader(NodeType Condition) => new NodeType(null, [Condition], ASTNodeType.While);
        [Production($"{nameof(NTCond)}: [{nameof(NTSwitch)} | {nameof(NTIf)}]")]
        public NodeType NTCond(NodeType C) => C;
        [Production($"{nameof(NTIf)}: If [d] OpenParen [d] {nameof(NTExpression)} CloseParen [d] {nameof(NTStatement)} {nameof(NTElse)}?")]
        public NodeType NTIf(NodeType Cond, NodeType StatementExpr, ValueOption<NodeType> ElseExpr)
        {
            var ThisCombined = new NodeType(null, [Cond, StatementExpr], ASTNodeType.ExprStatementCombined);
            List<NodeType> rc = [ThisCombined];
            if (GetFromValOp(ElseExpr) is NodeType node)
            {
                if (node.NodeType == ASTNodeType.If)
                {
                    rc.AddRange(node.Children); //should automatically add the remaining elifs and else as well
                }
                else
                {
                    rc.Add(node); //node is statement or child thereof (the latter is probably what it is but we don't care)
                }
            }
            return new(null, rc, ASTNodeType.If);
        }
        [Production($"{nameof(NTElse)}: Else [d] {nameof(NTStatement)}")]
        public NodeType NTElse(NodeType AStatement) => AStatement; //passthrough
        [Production($"{nameof(NTSwitch)}: Switch [d] OpenParen [d] {nameof(NTExpression)} CloseParen [d] OpenCurly [d] {nameof(NTSwitchBody)}* CloseCurly [d]")]
        public NodeType NTSwitch(NodeType AExpression, List<NodeType> ASwitchBody) => new NodeType(null, [AExpression, .. ASwitchBody], ASTNodeType.Switch);

        [Production($"{nameof(NTSwitchBody)}: {nameof(NTExpression)} Colon [d] {nameof(NTStatement)}")]
        public NodeType NTSwitchBody(NodeType AExpr, NodeType AStatement) => new(null, [AExpr, AStatement], ASTNodeType.ExprStatementCombined);
        [Production($"{nameof(NTFunction)}: {nameof(NTType)} Identifier OpenParen [d] {nameof(NTTypeAndIdentifierCSV)}? CloseParen [d] {nameof(NTStatement)}")]
        public NodeType NTFunction(NodeType AType, LyToken Ident, ValueOption<NodeType> TICSV, NodeType Statement) => new(FromToken(Ident), BuildChildren(AType, TICSV, Statement), ASTNodeType.Function);
        [Production($"{nameof(NTTypeAndIdentifierCSV)}: {nameof(NTTypeAndIdentifierCSVElement)} (Comma [d] {nameof(NTTypeAndIdentifierCSV)})*")]
        public NodeType NTTypeAndIdentifierCSV(NodeType Element, List<Group<TokenType, NodeType>> Prime) => new(null, [Element, .. Prime.Select(x => x.Items.First().Value)], ASTNodeType.TypeAndIdentifierCSV);
        [Production($"{nameof(NTTypeAndIdentifierCSVElement)}: {nameof(NTFunctionArgDeclModifiersCombined)}? {nameof(NTType)} Identifier")]
        public NodeType NTTypeAndIdentifierCSVElement(ValueOption<NodeType> Modifiers, NodeType AType, LyToken Ident) => new(FromToken(Ident), BuildChildren(Modifiers, AType), ASTNodeType.TypeAndIdentifierCSVElement);
        [Production($"{nameof(NTBlock)}: OpenCurly [d] {nameof(NTSection)} CloseCurly [d]")]
        public NodeType NTBlock(NodeType ASection) => ASection;
        [Production($"{nameof(NTExpression)}: {nameof(NTAliasExpr)}")]
        public NodeType NTExpression(NodeType pass) => pass;
        [Production($"{nameof(NTAliasExpr)}: [{nameof(NTAliasExpr1)} | {nameof(NTAliasExpr2)} | {nameof(NTAliasExpr3)} | {nameof(NTDeclarationExpr)}]")]
        public NodeType NTAliasExpr(NodeType Node) => Node;
        [Production($"{nameof(NTAliasExpr1)}: Identifier As [d] Identifier")]
        public NodeType NTAliasExpr1(LyToken Ident, LyToken Ident2) => new(FromToken(Ident), BuildChildren(Ident2), ASTNodeType.AliasExpr);
        [Production($"{nameof(NTAliasExpr2)}: Identifier As [d] {nameof(NTType)} Identifier")]
        public NodeType NTAliasExpr2(LyToken Ident, NodeType AType, LyToken Ident2) => new(FromToken(Ident), BuildChildren(AType, Ident2), ASTNodeType.ReTypingAlias);
        [Production($"{nameof(NTAliasExpr3)}: Identifier As [d] {nameof(NTType)}")]
        public NodeType NTAliasExpr3(LyToken Ident, NodeType Type) => new(FromToken(Ident), BuildChildren(Type), ASTNodeType.ReTypeOriginal);
        [Production($"{nameof(NTDeclarationExpr)}: [{nameof(NTDeclarationExpr1)} | {nameof(NTAssignmentExpr)}]")]
        public NodeType NTDeclarationExpr(NodeType Node) => Node;
        [Production($"{nameof(NTDeclarationExpr1)}: {nameof(NTDeclarationModifiersCombined)}? {nameof(NTType)} Identifier {nameof(NTAssignmentPrime)}?")]
        public NodeType NTDeclarationExpr1(ValueOption<NodeType> Modifiers, NodeType AType, LyToken Ident, ValueOption<NodeType> AAssignmentPrime) => new(FromToken(Ident), BuildChildren(Modifiers, AType, AAssignmentPrime), ASTNodeType.Declaration);
        [Production($"{nameof(NTDeclarationModifiersCombined)}: {nameof(NTDeclarationModifier)}+")]
        public NodeType NTDeclarationModifiersCombined(List<NodeType> Modifiers) => new(null, Modifiers, ASTNodeType.DeclarationModifiersCombined);
        [Production($"{nameof(NTDeclarationModifier)}: [Ref | Readonly | Frozen | Immut]")]
        public NodeType NTDeclarationModifier(LyToken Mod) => new(FromToken(Mod), [], ASTNodeType.DeclarationModifier);
        [Production($"{nameof(NTFunctionArgDeclModifier)}: [Ref | Readonly | Frozen | Immut | Copy]")]
        public NodeType NTFunctionArgDeclModifier(LyToken Mod) => NTDeclarationModifier(Mod) with { NodeType = ASTNodeType.FunctionArgDeclModifiers };
        [Production($"{nameof(NTFunctionArgDeclModifiersCombined)}: {nameof(NTFunctionArgDeclModifier)}+")]
        public NodeType NTFunctionArgDeclModifiersCombined(List<NodeType> Modifiers) => new(null, Modifiers, ASTNodeType.FunctionArgDeclModifiersCombined);
        [Production($"{nameof(NTAssignmentPrime)}: Equals {nameof(NTExpression)}")]
        public NodeType NTAssignmentPrime(LyToken EQ, NodeType Expr) => new(FromToken(EQ), [Expr], ASTNodeType.AssignmentPrime);
        [Production($"{nameof(NTAssignmentExpr)}: {nameof(NTAssignmentExpr1)}")]
        public NodeType NTAssignmentExpr(NodeType Node) => Node;
        [Production($"{nameof(NTAssignmentExpr1)}: SmallLangParser_expressions")]
        public NodeType NTAssignmentExpr1(NodeType P1) => P1;
        [Operand]
        [Production($"{nameof(NTPrimary)}: {nameof(NTLPrimary)} {nameof(NTPrimaryPrime)}?")]
        public NodeType NTPrimary(NodeType APrimary, ValueOption<NodeType> Prime)
        {
            if (GetFromValOp(Prime) is NodeType NodePrime)
            {
                return new(NodePrime.Data, [APrimary, .. NodePrime.Children], NodePrime.NodeType);
            }
            else return APrimary;
        }
        [Production($"{nameof(NTPrimaryPrime)}: [{nameof(NTIndexPrime)} | {nameof(NTFunctionCallPrime)}]")]
        public NodeType NTPrimaryPrime(NodeType Node) => Node;
        [Production($"{nameof(NTIndexPrime)}: OpenSquare [d] {nameof(NTExpression)} CloseSquare [d]")]
        public NodeType NTIndexPrime(NodeType Expr) => new(null, [Expr], ASTNodeType.Index);
        [Production($"{nameof(NTLPrimary)}: [{nameof(NTNewExpr)} | {nameof(NTLPrimary1)} | {nameof(NTLPrimary2)} | {nameof(NTLPrimary3)}]")]
        public NodeType NTLPrimary(NodeType Node) => Node;
        [Production($"{nameof(NTLPrimary1)}: OpenParen [d] {nameof(NTExpression)} CloseParen [d]")]
        public NodeType NTLPrimary1(NodeType Expr) => Expr;
        [Production($"{nameof(NTLPrimary2)}: Copy [d] {nameof(NTExpression)}")]
        public NodeType NTLPrimary2(NodeType Expr) => new(null, [Expr], ASTNodeType.CopyExpr);
        [Production($"{nameof(NTLPrimary3)}: [Identifier | Number | String | TrueLiteral | FalseLiteral]")]
        public NodeType NTLPrimary3(LyToken Token)
        {
            IToken oT = FromToken(Token)!;
            return new(oT, [], oT.TT == TokenType.Identifier ? ASTNodeType.Identifier : ASTNodeType.Primary);
        }
        [Production($"{nameof(NTNewExpr)}: New [d] {nameof(NTType)} OpenParen [d] {nameof(NTArgList)}?")]
        public NodeType NTNewExpr(NodeType AType, ValueOption<NodeType> AArgList) => new(null, BuildChildren(AType, AArgList), ASTNodeType.NewExpr);
        [Production($"{nameof(NTFunctionCallPrime)}:OpenParen [d] {nameof(NTArgList)}? CloseParen [d]")]
        public NodeType NTFunctionCallPrime(ValueOption<NodeType> AArgList) => new(null, BuildChildren(AArgList), ASTNodeType.FunctionCall);
        [Production($"{nameof(NTArgList)}: {nameof(NTArgListElement)} {nameof(NTArgListPrime)}*")]
        public NodeType NTArgList(NodeType Element, List<NodeType> Elements) => new(null, [Element, .. Elements], ASTNodeType.ArgList);
        [Production($"{nameof(NTArgListElement)}: {nameof(NTArgumentLabel)}? {nameof(NTExpression)}")]
        public NodeType NTArgListElement(ValueOption<NodeType> Label, NodeType Expr) => new(null, BuildChildren(Label, Expr), ASTNodeType.ArgListElement);
        [Production($"{nameof(NTArgListPrime)}: Comma [d] {nameof(NTArgListElement)}")]
        public NodeType NTArgListPrime(NodeType Element) => Element;
        [Production($"{nameof(NTArgumentLabel)}: Identifier Colon [d]")]
        public NodeType NTArgumentLabel(LyToken Ident) => new(FromToken(Ident), [], ASTNodeType.Identifier);
        [Production($"{nameof(NTTypeCSV)}: {nameof(NTType)} (Comma [d] {nameof(NTType)})*")]
        public NodeType NTTypeCSV(NodeType AType, List<Group<TokenType, NodeType>> OtherTypes)
        {
            return new(null, [AType, .. OtherTypes.Select(x => x.Items.First().Value)], ASTNodeType.TypeCSV);
        }
        [Production($"{nameof(NTType)}: [{nameof(NTBaseType)} | {nameof(NTGenericType)}]")]
        public NodeType NTType(NodeType Node) => Node;
        [Production($"{nameof(NTGenericType)}: [TypeArray | TypeList | TypeSet | TypeDict | TypeCollection] OpenAngleSquare [d] {nameof(NTTypeCSV)} CloseAngleSquare [d]")]
        public NodeType NTGenericType(LyToken TypeToken, NodeType TypeArgs) => new(FromToken(TypeToken), [TypeArgs], ASTNodeType.GenericType);
        [Production($"{nameof(NTBaseType)}: [TypeByte | TypeShort | TypeInt | TypeLong | TypeLongInt | TypeFloat | TypeDouble | TypeRational | TypeNumber | TypeString | TypeChar | TypeVoid]")]
        public NodeType NTBaseType(LyToken TypeToken) => new(FromToken(TypeToken), [], ASTNodeType.BaseType);
    }```

    ```cs
    using Common.Tokens;
    using sly.parser.generator;
    using LyToken = sly.lexer.Token<Common.Tokens.TokenType>;
    using NodeType = Common.AST.DynamicASTNode<SmallLang.ASTNodeType, SmallLang.Attributes>;
}

namespace SmallLang.Parser;
public partial class SmallLangParser
{
    #region ComparisonExpressions
    const int EqualToPrecedence = NotExprPrecedence + 1;
    const int NotEqualToPrecedence = EqualToPrecedence;
    const int GreaterThanPrecedence = NotEqualToPrecedence;
    const int LessThanPrecedence = GreaterThanPrecedence;
    const int GreaterThanOrEqualToPrecedence = LessThanPrecedence;
    const int LessThanOrEqualToPrecedence = GreaterThanOrEqualToPrecedence;
    [Infix((int)TokenType.EqualTo, Associativity.Right, EqualToPrecedence)]
    [Infix((int)TokenType.NotEqualTo, Associativity.Right, NotEqualToPrecedence)]
    [Infix((int)TokenType.GreaterThan, Associativity.Right, GreaterThanPrecedence)]
    [Infix((int)TokenType.LessThan, Associativity.Right, LessThanPrecedence)]
    [Infix((int)TokenType.GreaterThanOrEqualTo, Associativity.Right, GreaterThanOrEqualToPrecedence)]
    [Infix((int)TokenType.LessThanOrEqualTo, Associativity.Right, LessThanOrEqualToPrecedence)]
    public NodeType ComparisonExpressions(NodeType Left, LyToken Operator, NodeType Right)
    {
        var OpToken = FromToken(Operator);
        List<NodeType> rc = [Left];
        if (Right.NodeType == ASTNodeType.ComparisionExpression)
        {
            rc.AddRange([
                new(OpToken, [Right.Children[0]], ASTNodeType.OperatorExpressionPair),
                ..Right.Children.Skip(1)
            ]);
        }
        else
        {
            rc.Add(new(OpToken, [Right], ASTNodeType.OperatorExpressionPair));
        }
        return new(null, rc, ASTNodeType.ComparisionExpression);
    }
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

    [Postfix((int)TokenType.Factorial, Associativity.Left, FactorialPrecedence)]
    public NodeType Factorial(NodeType Left, LyToken Op)
    {
        if (Left.NodeType == ASTNodeType.FactorialExpression)
        {
            Left.Children.AddRange(BuildChildren(Op));
            return Left;
        }
        return new(null, BuildChildren(Op, Left), ASTNodeType.FactorialExpression);
    }
    [Prefix((int)TokenType.LogicalNot, Associativity.Right, NotExprPrecedence)]
    [Prefix((int)TokenType.BitwiseNegation, Associativity.Right, BitwiseNotExprPrecedence)]
    [Prefix((int)TokenType.Subtraction, Associativity.Right, NegationPrecedence)]
    public NodeType Prefix(LyToken Op, NodeType Right) => new(FromToken(Op), [Right], ASTNodeType.UnaryExpression);
    [Infix((int)TokenType.Equals, Associativity.Right, AssignmentExprPrecedence)]
    [Infix((int)TokenType.LogicalOr, Associativity.Left, OrExprPrecedence)]
    [Infix((int)TokenType.LogicalXor, Associativity.Left, XorExprPrecedence)]
    [Infix((int)TokenType.LogicalAnd, Associativity.Left, AndExprPrecedence)]
    [Infix((int)TokenType.Addition, Associativity.Left, AdditionPrecedence)]
    [Infix((int)TokenType.Subtraction, Associativity.Left, SubtractionPrecedence)]
    [Infix((int)TokenType.Division, Associativity.Left, DivisionPrecedence)]
    [Infix((int)TokenType.Multiplication, Associativity.Left, MultiplicationPrecedence)]
    [Infix((int)TokenType.Exponentiation, Associativity.Right, PowerPrecedence)]
    [Infix((int)TokenType.Subtraction, Associativity.Left, NegationPrecedence)]
    [Infix((int)TokenType.BitwiseOr, Associativity.Left, BitwiseOrExprPrecedence)]
    [Infix((int)TokenType.BitwiseXor, Associativity.Left, BitwiseXorExprPrecedence)]
    [Infix((int)TokenType.BitwiseAnd, Associativity.Left, BitwiseAndExprPrecedence)]
    [Infix((int)TokenType.BitwiseLeftShift, Associativity.Left, LShiftPrecedence)]
    [Infix((int)TokenType.BitwiseRightShift, Associativity.Left, RShiftPrecedence)]
    public NodeType BinOp(NodeType left, LyToken Op, NodeType right)
    {
        return new NodeType(FromToken(Op), [left, right], ASTNodeType.BinaryExpression);
    }
}