using sly.lexer;
using sly.parser;
using sly.parser.generator;

using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace sly.parser
{
    public class Parser<IN,OUT>
    {
        public ILexer<IN> Lexer { get; set; }
        public object Instance { get; set; }
        public ISyntaxParser<IN> SyntaxParser { get; set; }
        public SyntaxTreeVisitor<IN,OUT> Visitor { get; set; }
        public ParserConfiguration<IN,OUT> Configuration { get; set; }
        
        public Parser(ISyntaxParser<IN> syntaxParser, SyntaxTreeVisitor<IN,OUT> visitor)
        {
            SyntaxParser = syntaxParser;
            Visitor = visitor;
        }

       

        public ParseResult<IN,OUT> Parse(string source, string startingNonTerminal = null)
        {
            ParseResult<IN,OUT> result = null;
            try
            {

                IList<Token<IN>> tokens = Lexer.Tokenize(source).ToList<Token<IN>>();
                result = Parse(tokens, startingNonTerminal);
            }
            catch(LexerException<IN> e)
            {
                result = new ParseResult<IN,OUT>();
                result.IsError = true;
                result.Errors = new List<ParseError>();
                result.Errors.Add((e as LexerException<IN>).Error);                
            }
            return result;            
        }
        public ParseResult<IN,OUT> Parse(IList<Token<IN>> tokens, string startingNonTerminal = null)
        {
            
            ParseResult<IN,OUT> result = new ParseResult<IN,OUT>();
            SyntaxParseResult<IN> syntaxResult = SyntaxParser.Parse(tokens, startingNonTerminal);
            if (!syntaxResult.IsError && syntaxResult.Root != null)
            {
                OUT r  = Visitor.VisitSyntaxTree(syntaxResult.Root);
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

