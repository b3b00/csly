using System;
using System.Collections.Generic;
using postProcessedLexerParser.expressionModel;
using sly.lexer;
using sly.lexer.fsm;
using sly.parser;
using sly.parser.generator;
using sly.sourceGenerator;

namespace postProcessedLexerParser
{
    [ParserGenerator(typeof(FormulaToken),typeof(FormulaParser),typeof(Expression))]
    public partial class PostProcessedLexerGenerator : AbstractParserGenerator<FormulaToken>
    {
        public override LexerPostProcess<FormulaToken> UseTokenPostProcessor() =>  PostProcessedLexerParserBuilder.postProcessFormula;
    }
    
    public class PostProcessedLexerParserBuilder
    {
        
        public static List<Token<FormulaToken>> postProcessFormula(List<Token<FormulaToken>> tokens)
        {
            var mayLeft = new List<FormulaToken>()
            {
                FormulaToken.INT, FormulaToken.DOUBLE,  FormulaToken.IDENTIFIER
            };
            
            var mayRight = new List<FormulaToken>()
            {
                FormulaToken.INT, FormulaToken.DOUBLE, FormulaToken.LPAREN, FormulaToken.IDENTIFIER
            };
            
            Func<FormulaToken,bool> mayOmmitLeft = (FormulaToken tokenid) =>  mayLeft.Contains(tokenid);
            
            Func<FormulaToken,bool> mayOmmitRight = (FormulaToken tokenid) =>  mayRight.Contains(tokenid);
                
            
            List<Token<FormulaToken>> newTokens = new List<Token<FormulaToken>>();
            for (int i = 0; i < tokens.Count; i++)
            {
                if ( i >= 1 &&
                     mayOmmitRight(tokens[i].TokenID) && mayOmmitLeft(tokens[i-1].TokenID))
                {
                    newTokens.Add(new Token<FormulaToken>()
                    {
                        TokenID = FormulaToken.TIMES
                    });
                }
                newTokens.Add(tokens[i]);
            }

            return newTokens;
        }

        public static Parser<FormulaToken, Expression> buildPostProcessedLexerParser()
        {
            var parserInstance = new FormulaParser();
            var builder = new ParserBuilder<FormulaToken, Expression>();
            var build = builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, $"{nameof(FormulaParser)}_expressions",
                lexerPostProcess: postProcessFormula);
            if (build.IsError)
            {
                foreach (var error in build.Errors)
                {
                    Console.WriteLine(error.Message);
                }

                return null;
            }

            var Parser = build.Result;
            return Parser;
        }
    }
}