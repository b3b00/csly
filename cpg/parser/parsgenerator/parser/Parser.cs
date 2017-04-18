using lexer;
using parser.parsergenerator.generator;
using parser.parsergenerator.parser;
using parser.parsergenerator.syntax;
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
            List<object> result = new List<object>();
            SyntaxParseResult<T> syntaxResult = SyntaxParser.Parse(tokens);
            if (!syntaxResult.IsError && syntaxResult.Root != null)
            {

                var r = Visitor.VisitSyntaxTree(syntaxResult.Root);
                result.Add(r);
            }
            return result;
        }

    }
}

