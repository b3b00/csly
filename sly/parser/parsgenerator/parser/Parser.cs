using lexer;
using parser.parsergenerator.generator;
using parser.parsergenerator.parser;
using parser.parsergenerator.syntax;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace cpg.parser.parsgenerator.parser
{
    public class Parser<T>
    {
        public Lexer<T> Lexer { get; set; }
        public ISyntaxParser<T> SyntaxParser { get; set; }
        public ConcreteSyntaxTreeVisitor<T> Visitor { get; set; }
        public Parser(ISyntaxParser<T> syntaxParser, ConcreteSyntaxTreeVisitor<T> visitor)
        {
            SyntaxParser = syntaxParser;
            Visitor = visitor;
        }
        

        public ParseResult<T> Parse(string source)
        {
            ParseResult<T> result = null;
            try
            {
                IList<Token<T>> tokens = Lexer.Tokenize(source).ToList<Token<T>>();
                result = Parse(tokens);
            }
            catch(LexerException<T> e)
            {
                result = new ParseResult<T>();
                result.IsError = true;
                result.Errors = new List<ParseError>();
                result.Errors.Add((e as LexerException<T>).Error);                
            }
            return result;            
        }
        public ParseResult<T> Parse(IList<Token<T>> tokens)
        {
            
            ParseResult<T> result = new ParseResult<T>();
            SyntaxParseResult<T> syntaxResult = SyntaxParser.Parse(tokens);
            if (!syntaxResult.IsError && syntaxResult.Root != null)
            {
                object r  = Visitor.VisitSyntaxTree(syntaxResult.Root);
                result.Result = r;
                result.IsError = false;
            }
            else
            {
                result.Errors = new List<ParseError>();
                result.Errors.AddRange(syntaxResult.Errors);
                result.IsError = true;
            }
            return result;
        }

    }
}

