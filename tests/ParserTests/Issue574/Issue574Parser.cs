using System.Collections.Generic;
using sly.lexer;
using sly.parser.generator;
using sly.parser.parser;

namespace ParserTests.Issue574;



public partial class Issue574Parser
{
    
    
    [Production($"{nameof(NTSection)}: {nameof(NTStatement)}*")]
    public object NTSection(List<object> Statements)
    {
        return null;
    }
    
    [Production($"{nameof(NTStatement)}: [{nameof(NTSCExpr)} | {nameof(NTLoop)} | {nameof(NTCond)} | {nameof(NTFunction)} | {nameof(NTBlock)} | {nameof(NTReturnStatement)} | {nameof(NTLoopControlStatement)}]")]
    public object NTStatement(object SubStatement) => SubStatement;
    [Production($"{nameof(NTSCExpr)}: {nameof(NTExpression)} Semicolon [d]")]

    public object NTSCExpr(object Expression) => Expression;
    
    
    [Production($"{nameof(NTReturnStatement)}: Return [d] {nameof(NTExpression)} Semicolon [d]")]
    public object NTReturnStatement(object SCExpr) => null;
    
    [Production($"{nameof(NTLoopControlStatement)}: [Break | Continue] {nameof(NTNestedValueInLoopControl)}? Semicolon [d]")]
    public object NTLoopControlStatement(Token<Issue574Token> Operator, ValueOption<object> NestedVal)
    {
        return null;
    }
    [Production($"{nameof(NTNestedValueInLoopControl)}: Identifier")]
    public object NTNestedValueInLoopControl(Token<Issue574Token> val) => null;

    [Production($"{nameof(NTLoop)}: {nameof(NTForLoop)}")]
    [Production($"{nameof(NTLoop)}: {nameof(NTWhileLoop)}")]
    public object NTLoop(object Loop) => Loop;
    
    [Production($"{nameof(NTLoopLabel)}: As [d] Identifier")]
    public object NTLoopLabel(Token<Issue574Token> ident) => null;
    
    [Production($"{nameof(NTForLoop)}: For [d] OpenParen [d] {nameof(NTExpression)} Semicolon [d] {nameof(NTExpression)} Semicolon [d] {nameof(NTExpression)} CloseParen [d] {nameof(NTLoopLabel)}? {nameof(NTStatement)} {nameof(NTElse)}?")]
    public object NTForLoop(object Init, object Condition, object Step, ValueOption<object> LoopLabel, object Statement, ValueOption<object> Else) => null;
    
    [Production($"{nameof(NTWhileLoop)}: While [d] OpenParen [d] {nameof(NTExpression)} CloseParen [d] {nameof(NTLoopLabel)}? {nameof(NTStatement)} {nameof(NTElse)}?")]
    public object NTWhileLoop(object Condition, ValueOption<object> LoopLabel, object Statement, ValueOption<object> Else) => null;
    
    [Production($"{nameof(NTCond)}: [{nameof(NTSwitch)} | {nameof(NTIf)}]")]
    public object NTCond(object C) => C;

    [Production(
        $"{nameof(NTIf)}: If [d] OpenParen [d] {nameof(NTExpression)} CloseParen [d] {nameof(NTStatement)} {nameof(NTElse)}?")]
    public object NTIf(object Cond, object StatementExpr, ValueOption<object> ElseExpr) => null;


    [Production($"{nameof(NTElse)}: Else [d] {nameof(NTStatement)}")]
    public object NTElse(object AStatement) => null; 
    
    [Production($"{nameof(NTSwitch)}: Switch [d] OpenParen [d] {nameof(NTExpression)} CloseParen [d] OpenCurly [d] {nameof(NTSwitchBody)}* CloseCurly [d]")]
    public object NTSwitch(object AExpression, List<object> ASwitchBody) => null;

    [Production($"{nameof(NTSwitchBody)}: {nameof(NTExpression)} Colon [d] {nameof(NTStatement)}")]
    public object NTSwitchBody(object AExpr, object AStatement) => null;

    [Production(
        $"{nameof(NTFunction)}: {nameof(NTType)} Identifier OpenParen [d] {nameof(NTTypeAndIdentifierCSVElement)}* CloseParen [d] {nameof(NTStatement)}")]
    public object NTFunction(object AType, Token<Issue574Token> Ident, List<object> TICSV, object Statement) => null;
    
    [Production($"{nameof(NTTypeAndIdentifierCSVElement)}: {nameof(NTFunctionArgDeclModifiersCombined)} {nameof(NTType)} Identifier Comma?")]
    public object NTTypeAndIdentifierCSVElement(object Modifiers, object AType, Token<Issue574Token> Ident, Token<Issue574Token> _) => null;
    
    [Production($"{nameof(NTBlock)}: OpenCurly [d] {nameof(NTSection)} CloseCurly [d]")]
    public object NTBlock(object ASection) => null;
    
    [Production($"{nameof(NTExpression)}: {nameof(NTAliasExpr)}")]
    public object NTExpression(object pass) => null;
    
    [Production($"{nameof(NTAliasExpr)}: [{nameof(NTAliasExpr1)} | {nameof(NTAliasExpr2)} | {nameof(NTAliasExpr3)} | {nameof(NTDeclarationExpr)}]")]
    public object NTAliasExpr(object Node) => null;
    
    [Production($"{nameof(NTAliasExpr1)}: Identifier As [d] Identifier")]
    public object NTAliasExpr1(Token<Issue574Token> Ident, Token<Issue574Token> Ident2) => null;
    
    [Production($"{nameof(NTAliasExpr2)}: Identifier As [d] {nameof(NTType)} Identifier")]
    public object NTAliasExpr2(Token<Issue574Token> Ident, object AType, Token<Issue574Token> Ident2) => null;
    
    [Production($"{nameof(NTAliasExpr3)}: Identifier As [d] {nameof(NTType)}")]
    public object NTAliasExpr3(Token<Issue574Token> Ident, object Type) => null;
    
    [Production($"{nameof(NTDeclarationExpr)}: [{nameof(NTDeclarationExpr1)} | {nameof(NTAssignmentExpr)}]")]
    public object NTDeclarationExpr(object Node) => null;
    [Production($"{nameof(NTDeclarationExpr1)}: {nameof(NTDeclarationModifiersCombined)}? {nameof(NTType)} Identifier {nameof(NTAssignmentPrime)}?")]
    public object NTDeclarationExpr1(ValueOption<object> Modifiers, object AType, Token<Issue574Token> Ident, ValueOption<object> AAssignmentPrime) => null;
    
    [Production($"{nameof(NTDeclarationModifiersCombined)}: {nameof(NTDeclarationModifier)}*")]
    public object NTDeclarationModifiersCombined(List<object> Modifiers) => null;
    
    [Production($"{nameof(NTDeclarationModifier)}: [Ref | Readonly | Frozen | Immut]")]
    public object NTDeclarationModifier(Token<Issue574Token> Mod) => null;
    [Production($"{nameof(NTFunctionArgDeclModifier)}: [Ref | Readonly | Frozen | Immut | Copy]")]
    public object NTFunctionArgDeclModifier(Token<Issue574Token> Mod) => null;
    
    [Production($"{nameof(NTFunctionArgDeclModifiersCombined)}: {nameof(NTFunctionArgDeclModifier)}*")]
    public object NTFunctionArgDeclModifiersCombined(List<object> Modifiers) => null;
    
    [Production($"{nameof(NTAssignmentPrime)}: Equals {nameof(NTExpression)}")]
    public object NTAssignmentPrime(Token<Issue574Token> EQ, object Expr) => null;
    
    [Production($"{nameof(NTAssignmentExpr)}: {nameof(NTAssignmentExpr1)}")]
    public object NTAssignmentExpr(object Node) => null;
    
    [Production($"{nameof(NTAssignmentExpr1)}: SmallLangParser_expressions")]
    public object NTAssignmentExpr1(object P1) => null;
    [Operand]
    [Production($"{nameof(NTPrimary)}: {nameof(NTLPrimary)}")]
    public object NTPrimary(object Node) => null;

    [Operand]
    [Production($"{nameof(NTPrimary)}: {nameof(NTLPrimary)} OpenSquare {nameof(NTExpression)} CloseSquare")]
    public object NTPrimary(object Node, Token<Issue574Token> Open, object Expr, Token<Issue574Token> Close) => null;

    [Operand]
    [Production($"{nameof(NTPrimary)}: {nameof(NTLPrimary)}  OpenParen {nameof(NTArgListElement)}* CloseParen")]
    public object NTPrimary(object Node, Token<Issue574Token> Open, List<object> Expression, Token<Issue574Token> Close) => null;


    [Production($"{nameof(NTLPrimary)}: [{nameof(NTNewExpr)} | {nameof(NTLPrimary1)} | {nameof(NTLPrimary2)} | {nameof(NTLPrimary3)}]")]
    public object NTLPrimary(object Node) => null;
    
    [Production($"{nameof(NTLPrimary1)}: OpenParen [d] {nameof(NTExpression)} CloseParen [d]")]
    public object NTLPrimary1(object Expr) => null;
    
    [Production($"{nameof(NTLPrimary2)}: Copy [d] {nameof(NTExpression)}")]
    public object NTLPrimary2(object Expr) => null;

    [Production($"{nameof(NTLPrimary3)}: [Identifier | Number | String | TrueLiteral | FalseLiteral]")]
    public object NTLPrimary3(Token<Issue574Token> Token) => null;
    
    [Production($"{nameof(NTNewExpr)}: New [d] {nameof(NTType)} OpenParen [d] {nameof(NTArgListElement)}* CloseParen [d]")]
    public object NTNewExpr(object AType, List<object> AArgList) =>null;
    
    [Production($"{nameof(NTArgListElement)}: {nameof(NTArgumentLabel)}? {nameof(NTExpression)} Comma?")]
    public object NTArgListElement(ValueOption<object> Label, object Expr, Token<Issue574Token> _) => null;
    
    public object NTArgListPrime(object Element) => Element;
    [Production($"{nameof(NTArgumentLabel)}: Identifier Colon [d]")]
    public object NTArgumentLabel(Token<Issue574Token> Ident) => null;

    [Production($"{nameof(NTTypeCSV)}: {nameof(NTType)} (Comma [d] {nameof(NTType)})*")]
    public object NTTypeCSV(object AType, List<Group<Issue574Token, object>> OtherTypes) => null;
    
    [Production($"{nameof(NTType)}: [{nameof(NTBaseType)} | {nameof(NTGenericType)}]")]
    public object NTType(object Node) => Node;
    [Production($"{nameof(NTGenericType)}: [TypeArray | TypeList | TypeSet | TypeDict | TypeCollection] OpenAngleSquare [d] {nameof(NTTypeCSV)} CloseAngleSquare [d]")]
    public object NTGenericType(Token<Issue574Token> TypeToken, object TypeArgs) => null;
    
    [Production($"{nameof(NTBaseType)}: [TypeBool | TypeByte | TypeShort | TypeInt | TypeLong | TypeLongInt | TypeFloat | TypeDouble | TypeRational | TypeNumber | TypeString | TypeChar | TypeVoid]")]
    public object NTBaseType(Token<Issue574Token> TypeToken) => null;
}