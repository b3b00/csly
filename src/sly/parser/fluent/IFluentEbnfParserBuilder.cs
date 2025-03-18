using System;
using sly.buildresult;
using sly.lexer.fluent;

namespace sly.parser.generator;

public interface IFluentEbnfRuleBuilder<IN, OUT> : IFluentEbnfParserBuilder<IN, OUT> where IN : struct
{
    public IFluentEbnfParserBuilder<IN, OUT> Named(string name);
    
    public IFluentEbnfParserBuilder<IN, OUT> WithLang(string i18nLang);
    
    public IFluentEbnfParserBuilder<IN, OUT> WithSubNodeNamed(params string[] subNodeNames);
}

public interface IFluentEbnfParserBuilder<IN,OUT> where IN : struct
{

    IFluentEbnfParserBuilder<IN, OUT> UseMemoization(bool use = true);
    
    IFluentEbnfParserBuilder<IN, OUT> UseBroadenTokenWindow(bool use = true);
    
    IFluentEbnfParserBuilder<IN, OUT> UseAutoCloseIndentations(bool use = true);
    
    IFluentEbnfRuleBuilder<IN,OUT> Production(string ruleString, Func<object[], OUT> visitor);
    
    IFluentEbnfRuleBuilder<IN,OUT> Operand(string rule, Func<object[], OUT> visitor);
    
    IFluentEbnfRuleBuilder<IN,OUT> Operation(string operation, Affix affix, Associativity associativity, int precedence, Func<object[], OUT> visitor);
    
    IFluentEbnfRuleBuilder<IN,OUT> Operation(IN operation, Affix affix, Associativity associativity, int precedence, Func<object[], OUT> visitor);
    
    IFluentEbnfRuleBuilder<IN,OUT> Right(IN operation, int precedence, Func<object[], OUT> visitor);
    
    IFluentEbnfRuleBuilder<IN,OUT> Right(string operation, int precedence, Func<object[], OUT> visitor);
    IFluentEbnfRuleBuilder<IN,OUT> Left(IN operation, int precedence, Func<object[], OUT> visitor);
    
    IFluentEbnfRuleBuilder<IN,OUT> Left(string operation, int precedence, Func<object[], OUT> visitor);
    
    IFluentEbnfRuleBuilder<IN,OUT> Infix(string operation, Associativity associativity, int precedence, Func<object[], OUT> visitor);
    IFluentEbnfRuleBuilder<IN,OUT> Infix(IN operation, Associativity associativity, int precedence, Func<object[], OUT> visitor);
    IFluentEbnfRuleBuilder<IN,OUT> Prefix(IN operation, int precedence, Func<object[], OUT> visitor);
    
    IFluentEbnfRuleBuilder<IN,OUT> Prefix(string operation, int precedence, Func<object[], OUT> visitor);
    IFluentEbnfRuleBuilder<IN,OUT> Postfix(IN operation, int precedence, Func<object[], OUT> visitor);
    
    IFluentEbnfRuleBuilder<IN,OUT> Postfix(string operation, int precedence, Func<object[], OUT> visitor);
    
    public BuildResult<ISyntaxParser<IN, OUT>> BuildSyntaxParser(BuildResult<ParserConfiguration<IN, OUT>> result); 
    
    public BuildResult<Parser<IN, OUT>> BuildParser();

    public IFluentEbnfParserBuilder<IN, OUT> WithLexerbuilder(IFluentLexerBuilder<IN> lexerBuilder);


}