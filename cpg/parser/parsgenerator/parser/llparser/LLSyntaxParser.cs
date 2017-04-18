using parser.parsergenerator.generator;
using parser.parsergenerator.parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cpg.parser.parsgenerator.parser.llparser
{
    public class LLSyntaxParser<T> : ISyntaxParser<T>
    {
        public ParserConfiguration<T> Configuration { get; set; }

        public LLSyntaxParser(ParserConfiguration<T> configuration, string startingNonTerminal)
        {
            Configuration = configuration;
        }

        public SyntaxParseResult<T> Parse(IList<T> tokens)
        {
            throw new NotImplementedException();
        }

    }
}
