using System;
using sly.buildresult;
using sly.lexer;
using sly.parser;
using sly.parser.generator;

namespace aot.parser;

public interface IAotParserBuilder<IN,OUT> where IN : struct
{

    IAotParserBuilder<IN, OUT> UseMemoization(bool use = true);
    
    IAotParserBuilder<IN, OUT> UseBroadenTokenWindow(bool use = true);
    
    IAotParserBuilder<IN, OUT> UseAutoCloseIndentations(bool use = true);
    
    IAotParserBuilder<IN,OUT> Production(string ruleString, Func<object[], OUT> visitor);
    
    IAotParserBuilder<IN,OUT> Operand(string rule, Func<object[], OUT> visitor);
    
    IAotParserBuilder<IN,OUT> Right(int precedence, IN operation, Func<object[], OUT> visitor);
    
    IAotParserBuilder<IN,OUT> Right(int precedence, string explicitOperation, Func<object[], OUT> visitor);
    IAotParserBuilder<IN,OUT> Left(int precedence, IN operation, Func<object[], OUT> visitor);
    
    IAotParserBuilder<IN,OUT> Left(int precedence, string explicitOperation, Func<object[], OUT> visitor);
    IAotParserBuilder<IN,OUT> Prefix(int precedence, IN operation, Func<object[], OUT> visitor);
    
    IAotParserBuilder<IN,OUT> Prefix(int precedence, string explicitOperation, Func<object[], OUT> visitor);
    IAotParserBuilder<IN,OUT> Postfix(int precedence, IN operation, Func<object[], OUT> visitor);
    
    IAotParserBuilder<IN,OUT> Postfix(int precedence, string explicitOperation, Func<object[], OUT> visitor);
    
    public ISyntaxParser<IN, OUT> BuildSyntaxParser(BuildResult<ParserConfiguration<IN, OUT>> result); // TODO AOT this build result is strange
    
    public Parser<IN, OUT> BuildParser();

    public IAotParserBuilder<IN, OUT> WithLexerbuilder(IAotLexerBuilder<IN> lexerBuilder);


}