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
    
    
    public ISyntaxParser<IN, OUT> BuildSyntaxParser(BuildResult<ParserConfiguration<IN, OUT>> result); 
    
    public BuildResult<Parser<IN, OUT>> BuildParser();

    public IAotParserBuilder<IN, OUT> WithLexerbuilder(IAotLexerBuilder<IN> lexerBuilder);


}