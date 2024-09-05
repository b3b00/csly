using System;
using sly.buildresult;
using sly.lexer;

namespace sly.parser.generator;

public interface IAotEbnfParserBuilder<IN,OUT> where IN : struct
{

    IAotEbnfParserBuilder<IN, OUT> UseMemoization(bool use = true);
    
    IAotEbnfParserBuilder<IN, OUT> UseBroadenTokenWindow(bool use = true);
    
    IAotEbnfParserBuilder<IN, OUT> UseAutoCloseIndentations(bool use = true);
    
    IAotEbnfParserBuilder<IN,OUT> Production(string ruleString, Func<object[], OUT> visitor);
    
    IAotEbnfParserBuilder<IN,OUT> Operand(string rule, Func<object[], OUT> visitor);
    
    IAotEbnfParserBuilder<IN,OUT> Operation(string operation, Affix affix, Associativity associativity, int precedence, Func<object[], OUT> visitor);
    
    IAotEbnfParserBuilder<IN,OUT> Operation(IN operation, Affix affix, Associativity associativity, int precedence, Func<object[], OUT> visitor);
    
    IAotEbnfParserBuilder<IN,OUT> Right(IN operation, int precedence, Func<object[], OUT> visitor);
    
    IAotEbnfParserBuilder<IN,OUT> Right(string operation, int precedence, Func<object[], OUT> visitor);
    IAotEbnfParserBuilder<IN,OUT> Left(IN operation, int precedence, Func<object[], OUT> visitor);
    
    IAotEbnfParserBuilder<IN,OUT> Left(string operation, int precedence, Func<object[], OUT> visitor);
    
    IAotEbnfParserBuilder<IN,OUT> Infix(string operation, Associativity associativity, int precedence, Func<object[], OUT> visitor);
    IAotEbnfParserBuilder<IN,OUT> Infix(IN operation, Associativity associativity, int precedence, Func<object[], OUT> visitor);
    IAotEbnfParserBuilder<IN,OUT> Prefix(IN operation, int precedence, Func<object[], OUT> visitor);
    
    IAotEbnfParserBuilder<IN,OUT> Prefix(string operation, int precedence, Func<object[], OUT> visitor);
    IAotEbnfParserBuilder<IN,OUT> Postfix(IN operation, int precedence, Func<object[], OUT> visitor);
    
    IAotEbnfParserBuilder<IN,OUT> Postfix(string operation, int precedence, Func<object[], OUT> visitor);
    
    public ISyntaxParser<IN, OUT> BuildSyntaxParser(BuildResult<ParserConfiguration<IN, OUT>> result); // TODO AOT this build result is strange
    
    public BuildResult<Parser<IN, OUT>> BuildParser();

    public IAotEbnfParserBuilder<IN, OUT> WithLexerbuilder(IAotLexerBuilder<IN> lexerBuilder);


}