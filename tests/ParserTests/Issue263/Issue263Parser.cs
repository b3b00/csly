using System.Collections.Generic;
using sly.lexer;
using sly.parser.generator;

namespace ParserTests.Issue263
{
    public class Issue263Parser
    {
        [Production("operation : chunklist")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Not used in test code")]
        public object Operation(object chunklist)
        {
            return null;
        }

        [Production("chunklist : chunk+")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Not used in test code")]
        public object ChunkList(List<object> chunklist)
        {
            return null;
        }

        [Production("chunk : LPARA RPARA")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Not used in test code")]
        public object Chunk(Token<Issue263Token> lParaToken, Token<Issue263Token> rParaToken)
        {
            return null;
        }
    }
}
