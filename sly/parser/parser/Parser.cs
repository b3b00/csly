using sly.lexer;
using sly.parser;
using sly.parser.generator;

using System.Linq;
using System.Collections.Generic;
using sly.buildresult;
using sly.parser.parser;

namespace sly.parser
{
    public class Parser<IN,OUT> where IN : struct
    {
        public ILexer<IN> Lexer { get; set; }
        public object Instance { get; set; }
        public ISyntaxParser<IN,OUT> SyntaxParser { get; set; }
        public SyntaxTreeVisitor<IN,OUT> Visitor { get; set; }
        public ParserConfiguration<IN,OUT> Configuration { get; set; }
        
        public Parser(ISyntaxParser<IN,OUT> syntaxParser, SyntaxTreeVisitor<IN,OUT> visitor)
        {
            SyntaxParser = syntaxParser;
            Visitor = visitor;
        }


        #region expression generator

        public virtual BuildResult<ParserConfiguration<IN, OUT>> BuildExpressionParser(BuildResult<Parser<IN, OUT>> result, string startingRule = null)
        {
            var exprResult = new BuildResult<ParserConfiguration<IN, OUT>>(Configuration);
            exprResult = ExpressionRulesGenerator.BuildExpressionRules<IN, OUT>(Configuration, Instance.GetType(), exprResult);
            Configuration = exprResult.Result;
            SyntaxParser.Init(exprResult.Result,startingRule);
            if (startingRule != null)
            {
                Configuration.StartingRule = startingRule;
                SyntaxParser.StartingNonTerminal = startingRule;
            }
            if (exprResult.IsError)
            {
                result.AddErrors(exprResult.Errors);
            }           
            else
            {
                result.Result.Configuration = Configuration;
            }
            return exprResult;
        }


        #endregion


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
            
            var result = new ParseResult<IN,OUT>();

            var cleaner = new SyntaxTreeCleaner<IN>();            
            SyntaxParseResult<IN> syntaxResult = SyntaxParser.Parse(tokens, startingNonTerminal);
            syntaxResult = cleaner.CleanSyntaxTree(syntaxResult);
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

