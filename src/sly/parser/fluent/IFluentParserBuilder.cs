using System;
using sly.buildresult;
using sly.lexer;
using sly.lexer.fluent;
using sly.parser;
using sly.parser.generator;

namespace sly.parser.fluent;

public interface IFluentParserBuilder<IN,OUT> where IN : struct, Enum
{

    IFluentParserBuilder<IN, OUT> UseMemoization(bool use = true);
    
    IFluentParserBuilder<IN, OUT> UseBroadenTokenWindow(bool use = true);
    
    IFluentParserBuilder<IN, OUT> UseAutoCloseIndentations(bool use = true);
    
    IFluentParserBuilder<IN,OUT> Production(string ruleString, Func<object[], OUT> visitor);
    
    
    public ISyntaxParser<IN, OUT> BuildSyntaxParser(BuildResult<ParserConfiguration<IN, OUT>> result); 
    
    public BuildResult<Parser<IN, OUT>> BuildParser();

    public IFluentParserBuilder<IN, OUT> WithLexerbuilder(IFluentLexerBuilder<IN> lexerBuilder);


}