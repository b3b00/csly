using System;
using System.Collections.Generic;
using System.Linq;
using sly.buildresult;
using sly.lexer;
using sly.parser.generator;
using sly.parser.generator.visitor;
using sly.parser.llparser.bnf;
using sly.parser.llparser.ebnf;
using sly.parser.parser;
using sly.parser.syntax.grammar;
using sly.parser.syntax.tree;

namespace sly.parser
{
    public class Parser<IN, OUT> where IN : struct, Enum
    {   

        public Dictionary<IN, Dictionary<string, string>> LexemeLabels => Lexer.LexemeLabels;

        public Parser(string i18n, ISyntaxParser<IN, OUT> syntaxParser, SyntaxTreeVisitor<IN, OUT> visitor)
        {
            I18n = i18n;
            SyntaxParser = syntaxParser;
            Visitor = visitor;
        }

        public string I18n { get; set; }
        
        public ILexer<IN> Lexer { get; set; }
        public object Instance { get; set; }
        public ISyntaxParser<IN, OUT> SyntaxParser { get; set; }
        public SyntaxTreeVisitor<IN, OUT> Visitor { get; set; }
        public ParserConfiguration<IN, OUT> Configuration { get; set; }


        #region expression generator

        public virtual BuildResult<ParserConfiguration<IN, OUT>> BuildExpressionParser(
            BuildResult<Parser<IN, OUT>> result, string startingRule = null)
        {
            var exprResult = new BuildResult<ParserConfiguration<IN, OUT>>(Configuration);
            var expressionGenerator = new ExpressionRulesGenerator<IN, OUT>(I18n);
            exprResult = expressionGenerator.BuildExpressionRules(Configuration, Instance.GetType(), exprResult);
            Configuration = exprResult.Result;
            if (exprResult.IsOk)
            {
                // #540 : recompute starting tokens taking account of expression rules 
                SyntaxParser.Init(exprResult.Result, startingRule);
            }

            if (startingRule != null)
            {
                Configuration.StartingRule = startingRule;
                SyntaxParser.StartingNonTerminal = startingRule;
            }
           
            return exprResult;
        }

        #endregion



        public ParseResult<IN, OUT> Parse(string source, string startingNonTerminal = null)
        {
            return ParseWithContext(source,new NoContext(),startingNonTerminal);
        }


        public ParseResult<IN, OUT> ParseWithContext(string source, object context, string startingNonTerminal = null)
        {
            ParseResult<IN, OUT> result = null;
            var lexingResult = Lexer.Tokenize(source);
            if (lexingResult.IsError)
            {
                result = new ParseResult<IN, OUT>();
                result.IsError = true;
                result.Errors = new List<ParseError>();
                lexingResult.Error.SetContextualErrorMessage(source);
                result.Errors.Add(lexingResult.Error);
                return result;
            }
            

            var tokens = lexingResult.Tokens.MainTokens();

            tokens = AutoCloseIndentation(tokens);



            if (Lexer.LexerPostProcess != null)
            {
                tokens = Lexer.LexerPostProcess(tokens);
            }
            
            result = ParseWithContext(tokens, context, startingNonTerminal);
            if (result != null && result.Errors != null && result.Errors.Any())
            {
                result.Errors.ForEach(error => error.SetContextualErrorMessage(source));
            }

            return result;
        }

        


        public ParseResult<IN, OUT> ParseWithContext(IList<Token<IN>> tokens, object parsingContext = null, string startingNonTerminal = null)
        {
            var result = new ParseResult<IN, OUT>();

            var cleaner = new SyntaxTreeCleaner<IN, OUT>();
            if (SyntaxParser is sly.parser.llparser.bnf.RecursiveDescentSyntaxParser<IN, OUT> rdParser)
            {
                rdParser.Init(Configuration, startingNonTerminal ?? Configuration?.StartingRule);
                if (Configuration != null)
                {
                    rdParser.Configuration.CaptureAmbiguities = Configuration.CaptureAmbiguities;
                    rdParser.Configuration.AmbiguityStrategy = Configuration.AmbiguityStrategy;
                }
            }
            var syntaxResult = SyntaxParser.Parse(tokens.ToArray(), startingNonTerminal);
            if (Configuration?.CaptureAmbiguities == true && !syntaxResult.HasAmbiguity
                && SyntaxParser is sly.parser.llparser.bnf.RecursiveDescentSyntaxParser<IN, OUT> ambiguityParser)
            {
                var start = startingNonTerminal ?? ambiguityParser.StartingNonTerminal;
                if (start != null && Configuration.NonTerminals.TryGetValue(start, out var nonTerminal))
                {
                    var ambiguityContext = new SyntaxParsingContext<IN, OUT>(Configuration.UseMemoization, Configuration.UsePool);
                    var alternativeResults = new List<SyntaxParseResult<IN, OUT>>();
                    foreach (var rule in nonTerminal.Rules)
                    {
                        var canStart = tokens.Count == 0
                            ? rule.MayBeEmpty
                            : (!tokens[0].IsEOS && rule.Match(tokens, 0, Configuration))
                              || (tokens[0].IsEOS && rule.MayBeEmpty);
                        if (!canStart)
                        {
                            continue;
                        }

                        var ruleResult = ambiguityParser.Parse(tokens.ToArray(), rule, 0, start, ambiguityContext);
                        if (ruleResult.IsEnded && !ruleResult.IsError)
                        {
                            alternativeResults.Add(ruleResult);
                        }
                    }

                    if (alternativeResults.Count > 1)
                    {
                        syntaxResult = new SyntaxParseResult<IN, OUT>
                        {
                            AlternativeRoots = alternativeResults.Select(r => r.Root).ToList(),
                            EndingPosition = alternativeResults[0].EndingPosition,
                            IsError = false,
                            IsEnded = true,
                            HasByPassNodes = alternativeResults.Any(r => r.HasByPassNodes),
                            Ambiguities = new List<AmbiguityInfo<IN, OUT>>
                            {
                                new AmbiguityInfo<IN, OUT>
                                {
                                    NonTerminalName = start,
                                    Position = 0,
                                    AlternativeCount = alternativeResults.Count
                                }
                            }
                        };
                    }
                }
            }
            syntaxResult.UsesOperations = Configuration.UsesOperations;
            syntaxResult = cleaner.CleanSyntaxTree(syntaxResult);
            if (!syntaxResult.IsError && (syntaxResult.Root != null || syntaxResult.HasAmbiguity))
            {
                // if there is ambiguity, we will have multiple roots in AlternativeRoots, and we will decide which one to visit according to the AmbiguityStrategy
                if (syntaxResult.HasAmbiguity)
                {
                    result.Forest = new syntax.tree.ParseForest<IN, OUT>
                    {
                        Trees = syntaxResult.AlternativeRoots,
                        Ambiguities = syntaxResult.Ambiguities
                    };
                    
                    // Visit trees according to ambiguity strategy
                    switch (Configuration.AmbiguityStrategy)
                    {
                        case generator.AmbiguityResolutionStrategy.First:
                            result.Result = Visitor.VisitSyntaxTree(result.Forest.MainTree, parsingContext ?? new NoContext());
                            break;
                            
                        case generator.AmbiguityResolutionStrategy.ThrowException:
                            throw new exceptions.AmbiguousGrammarException<IN, OUT>(result.Forest);
                            
                        case generator.AmbiguityResolutionStrategy.All:
                            // Do not set result.Result, let the user handle all alternatives in the forest
                            result.Result = default(OUT);
                            break;
                            
                        case generator.AmbiguityResolutionStrategy.Longest:
                            // Visit longest derivation (assuming MainTree is the longest, otherwise we would need to determine it)
                            result.Result = Visitor.VisitSyntaxTree(result.Forest.MainTree, parsingContext ?? new NoContext());
                            break;
                    }
                }
                else
                {
                    // no ambiguity, visit the single tree
                    var r = Visitor.VisitSyntaxTree(syntaxResult.Root, parsingContext ?? new NoContext());
                    result.Result = r;
                    result.SyntaxTree = syntaxResult.Root;
                }
                
                result.IsError = false;
            }
            else
            {
                result.Errors = new List<ParseError>();
                var unexpectedTokens = syntaxResult.GetErrors() ?? new List<UnexpectedTokenSyntaxError<IN>>();
                var errors = new List<ParseError>();
                if (unexpectedTokens.Any())
                {
                    var targetPosition = unexpectedTokens.Max(x => x.UnexpectedToken.PositionInTokenFlow);

                    var eosPositions = unexpectedTokens
                        .Where(x => x.UnexpectedToken.IsEOS)
                        .Select(x => x.UnexpectedToken.PositionInTokenFlow)
                        .ToList();
                    if (eosPositions.Any())
                    {
                        var maxEosPosition = eosPositions.Max();
                        if (maxEosPosition >= targetPosition)
                        {
                            targetPosition = maxEosPosition;
                        }
                    }

                    var targetErrors = unexpectedTokens
                        .Where(x => x.UnexpectedToken.PositionInTokenFlow == targetPosition)
                        .ToList();

                    var expectingTokens = targetErrors
                        .SelectMany(x => x.ExpectedTokens ?? new List<LeadingToken<IN>>())
                        .Distinct()
                        .ToArray();
                    var expected = new UnexpectedTokenSyntaxError<IN>(targetErrors.First().UnexpectedToken, LexemeLabels, I18n,
                        expectingTokens);
                    errors.Add(expected);
                }
                else
                {
                    var fallbackToken = tokens != null && tokens.Count > 0
                        ? tokens[Math.Min(Math.Max(syntaxResult.EndingPosition, 0), tokens.Count - 1)]
                        : new Token<IN> { IsEOS = true };
                    errors.Add(new UnexpectedTokenSyntaxError<IN>(fallbackToken, LexemeLabels, I18n,
                        Array.Empty<LeadingToken<IN>>()));
                }

                result.Errors.AddRange(errors);
                result.IsError = true;
            }

            return result;
        }
        
        private List<Token<IN>> AutoCloseIndentation(List<Token<IN>> tokens)
        {
            if (SyntaxParser is EBNFRecursiveDescentSyntaxParser<IN,OUT> ebnf && ebnf.Configuration.AutoCloseIndentations)
            {
                var indents = tokens
                    .Where(x => x.IsIndentation);
                if (indents.Any())
                {
                    var finalIndentation = indents
                        .Select(x => x.IsIndent ? 1 : -1)
                        .Aggregate((int x, int y) => x + y);
                    if (finalIndentation > 0)
                    {
                        tokens = tokens.Take(tokens.Count - 1).ToList();
                        for (int i = 0; i < finalIndentation; i++)
                        {
                            tokens.Add(new Token<IN>()
                            {
                                IsUnIndent = true,
                                IsEOS = false,
                                IsEOL = false,
                                IndentationLevel = finalIndentation - i - 1
                            });
                        }

                        tokens.Add(new Token<IN>()
                        {
                            IsEOS = true
                        });
                    }
                }
            }

            return tokens;
        }
    }
}