using sly.sourceGenerator;

namespace GenericLexerWithCallbacks;

[ParserGenerator(typeof(CallbackTokens),typeof(ParserCallbacks), typeof(object))]
public partial class ParserCallbacksGenerator
{
        
}