using System;
using System.Collections.Generic;
using System.Text;

using sly.lexer;
using sly.parser.generator;

namespace ParserTests.Issue263
{
    public class Issue263Parser
    {
        [Production("operation : chunklist")]
        public object Operation(object chunklist)
        {
            return chunklist;
        }

        [Production("chunklist : chunk+")]
        public object ChunkList(List<object> chunklist)
        {
            return null;
        }

        [Production("chunk : LPARA RPARA")] // | bracketchunk | genericchunk | curlychunk
        public object Chunk(Token<Issue263Token> lParaToken, Token<Issue263Token> rParaToken)
        {
            return null;
        }
    }
}
