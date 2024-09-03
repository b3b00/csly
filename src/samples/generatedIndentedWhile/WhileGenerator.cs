using csly.generatedIndentedWhileLang.parser;
using csly.whileLang.model;
using sly.sourceGenerator;

namespace csly.generatedIndentedWhileLang.compiler;

[ParserGenerator(typeof(GeneratedIndentedWhileTokenGeneric),typeof(GeneratedIndentedWhileParserGeneric),typeof(WhileAST))]
public partial class WhileGenerator : AbstractParserGenerator<GeneratedIndentedWhileTokenGeneric>
{
    
}