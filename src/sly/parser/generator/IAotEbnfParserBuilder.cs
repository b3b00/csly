using System;
using sly.buildresult;
using sly.lexer;

namespace sly.parser.generator;

public interface IAotEbnfRuleBuilder<IN, OUT> : IAotEbnfParserBuilder<IN, OUT> where IN : struct
{
    public IAotEbnfParserBuilder<IN, OUT> Named(string name);
}

public interface IAotEbnfParserBuilder<IN,OUT> where IN : struct
{

    IAotEbnfParserBuilder<IN, OUT> UseMemoization(bool use = true);
    
    IAotEbnfParserBuilder<IN, OUT> UseBroadenTokenWindow(bool use = true);
    
    IAotEbnfParserBuilder<IN, OUT> UseAutoCloseIndentations(bool use = true);
    
    IAotEbnfRuleBuilder<IN,OUT> Production(string ruleString, Func<object[], OUT> visitor);
    
    IAotEbnfRuleBuilder<IN,OUT> Operand(string rule, Func<object[], OUT> visitor);
    
    IAotEbnfRuleBuilder<IN,OUT> Operation(string operation, Affix affix, Associativity associativity, int precedence, Func<object[], OUT> visitor);
    
    IAotEbnfRuleBuilder<IN,OUT> Operation(IN operation, Affix affix, Associativity associativity, int precedence, Func<object[], OUT> visitor);
    
    IAotEbnfRuleBuilder<IN,OUT> Right(IN operation, int precedence, Func<object[], OUT> visitor);
    
    IAotEbnfRuleBuilder<IN,OUT> Right(string operation, int precedence, Func<object[], OUT> visitor);
    IAotEbnfRuleBuilder<IN,OUT> Left(IN operation, int precedence, Func<object[], OUT> visitor);
    
    IAotEbnfRuleBuilder<IN,OUT> Left(string operation, int precedence, Func<object[], OUT> visitor);
    
    IAotEbnfRuleBuilder<IN,OUT> Infix(string operation, Associativity associativity, int precedence, Func<object[], OUT> visitor);
    IAotEbnfRuleBuilder<IN,OUT> Infix(IN operation, Associativity associativity, int precedence, Func<object[], OUT> visitor);
    IAotEbnfRuleBuilder<IN,OUT> Prefix(IN operation, int precedence, Func<object[], OUT> visitor);
    
    IAotEbnfRuleBuilder<IN,OUT> Prefix(string operation, int precedence, Func<object[], OUT> visitor);
    IAotEbnfRuleBuilder<IN,OUT> Postfix(IN operation, int precedence, Func<object[], OUT> visitor);
    
    IAotEbnfRuleBuilder<IN,OUT> Postfix(string operation, int precedence, Func<object[], OUT> visitor);
    
    public ISyntaxParser<IN, OUT> BuildSyntaxParser(BuildResult<ParserConfiguration<IN, OUT>> result); 
    
    public BuildResult<Parser<IN, OUT>> BuildParser();

    public IAotEbnfParserBuilder<IN, OUT> WithLexerbuilder(IAotLexerBuilder<IN> lexerBuilder);


}