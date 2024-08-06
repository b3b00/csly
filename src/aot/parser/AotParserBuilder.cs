using aot.lexer;
using sly.lexer;
using sly.parser;

namespace aot.parser;

public class AotParserBuilder
{
    public ISyntaxParser<AotLexer, double> FluentInitializeCenericLexer()
    {
        var builder = ParserBuilder<AotLexer,double>.NewBuilder<AotLexer,double > ();
        var p = builder.Production("root : SimpleExpressionParser_expressions", (args) =>
            {
                var parser = (AotParser)args[0];
                var result = parser.Root((double)args[1]);
                return result;
            })
            .Right(AotLexer.PLUS, (args =>
            {
                var parser = (AotParser)args[0];
                double result = parser.BinaryTermExpression((double)args[1], (Token<AotLexer>)args[2], (double)args[3]);
                return result;
            }))
            .Right(AotLexer.MINUS, (args =>
            {
                var parser = (AotParser)args[0];
                double result = parser.BinaryTermExpression((double)args[1], (Token<AotLexer>)args[2], (double)args[3]);
                return result;
            })).Build();
        return p;
    }
}