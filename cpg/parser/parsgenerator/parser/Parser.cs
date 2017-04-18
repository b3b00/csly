using lexer;
using parser.parsergenerator.generator;
using parser.parsergenerator.parser;
using System;
using System.Collections.Generic;
using System.Text;

namespace cpg.parser.parsgenerator.parser
{
    public class Parser<T>
    {
        public ISyntaxParser<T> SyntaxParser { get; set; }
        public ConcreteSyntaxTreeVisitor<T> Visitor { get; set; }
        public Parser(ISyntaxParser<T> syntaxParser, ConcreteSyntaxTreeVisitor<T> visitor)
        {
            SyntaxParser = syntaxParser;
            Visitor = visitor;
        }


       
        public object Parse(IList<Token<T>> tokens)
        {
            object result = null;
            SyntaxParseResult<T> syntaxResult =  SyntaxParser.Parse(tokens);
            if (!syntaxResult.IsError && syntaxResult.Root != null)
            {
                result = Visitor.VisitSyntaxTree(syntaxResult.Root);
            }
            return result;
        }
    }
}
