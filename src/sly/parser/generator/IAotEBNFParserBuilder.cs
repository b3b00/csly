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
    
    IAotEBNFParserBuilder<IN,OUT> Operation(string operation, Affix affix, Associativity associativity, int precedence, Func<object[], OUT> visitor);
    
    IAotEBNFParserBuilder<IN,OUT> Operation(IN operation, Affix affix, Associativity associativity, int precedence, Func<object[], OUT> visitor);
    
    IAotEBNFParserBuilder<IN,OUT> Right(IN operation, int precedence, Func<object[], OUT> visitor);
    
    IAotEBNFParserBuilder<IN,OUT> Right(string operation, int precedence, Func<object[], OUT> visitor);
    IAotEBNFParserBuilder<IN,OUT> Left(IN operation, int precedence, Func<object[], OUT> visitor);
    
    IAotEBNFParserBuilder<IN,OUT> Left(string operation, int precedence, Func<object[], OUT> visitor);
    IAotEBNFParserBuilder<IN,OUT> Prefix(IN operation, int precedence, Func<object[], OUT> visitor);
    
    IAotEBNFParserBuilder<IN,OUT> Prefix(string operation, int precedence, Func<object[], OUT> visitor);
    IAotEBNFParserBuilder<IN,OUT> Postfix(IN operation, int precedence, Func<object[], OUT> visitor);
    
    IAotEBNFParserBuilder<IN,OUT> Postfix(string operation, int precedence, Func<object[], OUT> visitor);
    
    public ISyntaxParser<IN, OUT> BuildSyntaxParser(BuildResult<ParserConfiguration<IN, OUT>> result); // TODO AOT this build result is strange
    
    public BuildResult<Parser<IN, OUT>> BuildParser();

    public IAotEBNFParserBuilder<IN, OUT> WithLexerbuilder(IAotLexerBuilder<IN> lexerBuilder);


}