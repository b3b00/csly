using System;
using sly.buildresult;
using sly.lexer;
using sly.parser;
using sly.parser.generator;

namespace aot.parser;

public interface IAotEBNFParserBuilder<IN,OUT> where IN : struct
{

    IAotEBNFParserBuilder<IN, OUT> UseMemoization(bool use = true);
    
    IAotEBNFParserBuilder<IN, OUT> UseBroadenTokenWindow(bool use = true);
    
    IAotEBNFParserBuilder<IN, OUT> UseAutoCloseIndentations(bool use = true);
    
    IAotEBNFParserBuilder<IN,OUT> Production(string ruleString, Func<object[], OUT> visitor);
    
    IAotEBNFParserBuilder<IN,OUT> Operand(string rule, Func<object[], OUT> visitor);
    
    IAotEBNFParserBuilder<IN,OUT> Right(int precedence, IN operation, Func<object[], OUT> visitor);
    
    IAotEBNFParserBuilder<IN,OUT> Right(int precedence, string explicitOperation, Func<object[], OUT> visitor);
    IAotEBNFParserBuilder<IN,OUT> Left(int precedence, IN operation, Func<object[], OUT> visitor);
    
    IAotEBNFParserBuilder<IN,OUT> Left(int precedence, string explicitOperation, Func<object[], OUT> visitor);
    IAotEBNFParserBuilder<IN,OUT> Prefix(int precedence, IN operation, Func<object[], OUT> visitor);
    
    IAotEBNFParserBuilder<IN,OUT> Prefix(int precedence, string explicitOperation, Func<object[], OUT> visitor);
    IAotEBNFParserBuilder<IN,OUT> Postfix(int precedence, IN operation, Func<object[], OUT> visitor);
    
    IAotEBNFParserBuilder<IN,OUT> Postfix(int precedence, string explicitOperation, Func<object[], OUT> visitor);
    
    public ISyntaxParser<IN, OUT> BuildSyntaxParser(BuildResult<ParserConfiguration<IN, OUT>> result); // TODO AOT this build result is strange
    
    public BuildResult<Parser<IN, OUT>> BuildParser();

    public IAotEBNFParserBuilder<IN, OUT> WithLexerbuilder(IAotLexerBuilder<IN> lexerBuilder);


}