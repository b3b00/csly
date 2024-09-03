using aot.lexer;
using aot.parser;
using Generators;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace aot;

[ParserGenerator(typeof(AotLexer), typeof(AotParser), typeof(double))]
public partial class TestGenerator
{
    
}