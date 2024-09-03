using aot.lexer;
using aot.parser;
using  cslyGenerator;

namespace aot;

[ParserGenerator(typeof(AotLexer), typeof(AotParser), typeof(double))]
public partial class TestGenerator
{
    
}