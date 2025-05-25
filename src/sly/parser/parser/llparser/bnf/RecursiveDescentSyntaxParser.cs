using System.Collections.Generic;
using sly.lexer;
using sly.parser.generator;
using sly.parser.syntax.grammar;
using sly.parser.syntax.tree;
using System.Linq;
using System;

namespace sly.parser.llparser.bnf
{
    public partial class RecursiveDescentSyntaxParser<IN, OUT> where IN : struct, Enum
    {
        public RecursiveDescentSyntaxParser(ParserConfiguration<IN, OUT> configuration, string startingNonTerminal,
            string i18n)
        {
            
            I18n = i18n;
            Configuration = configuration;
            StartingNonTerminal = startingNonTerminal;
            ComputeSubRules(configuration);
            InitializeStartingTokens(Configuration, startingNonTerminal);
        }

        public Dictionary<IN, Dictionary<string, string>> LexemeLabels { get; set; }

        #region parsing

        public SyntaxParseResult<IN, OUT> Parse(Token<IN>[] tokens, string startingNonTerminal = null)
        {
            return SafeParse(tokens, new SyntaxParsingContext<IN, OUT>(Configuration.UseMemoization), startingNonTerminal);
        }
        
        public SyntaxParseResult<IN, OUT> SafeParse(Token<IN>[] tokens, SyntaxParsingContext<IN, OUT> parsingContext, string startingNonTerminal = null)
        {
            var start = startingNonTerminal ?? StartingNonTerminal;
            var NonTerminals = Configuration.NonTerminals;
            var errors = new List<UnexpectedTokenSyntaxError<IN>>();
            var nt = NonTerminals[start];

            var rs = new List<SyntaxParseResult<IN, OUT>>();

            var matchingRuleCount = 0;

            foreach (var rule in nt.Rules)
            {
                if (!tokens[0].IsEOS && rule.Match(tokens,0,Configuration) 
                    || tokens[0].IsEOS && rule.MayBeEmpty)
                {
                    matchingRuleCount++;
                    var r = Parse(tokens, rule, 0, start, parsingContext);
                    rs.Add(r);
                }
            }

            if (matchingRuleCount == 0)
            {
                errors.Add(new UnexpectedTokenSyntaxError<IN>(tokens[0], LexemeLabels, I18n,
                    nt.GetPossibleLeadingTokens().ToArray()));
            }

            SyntaxParseResult<IN, OUT> result = null;


            if (rs.Count > 0)
            {
                result = rs.Find(r => r.IsEnded && !r.IsError);
                
                
                if (result == null)
                {
                    int lastPosition = -1;
                    List<SyntaxParseResult<IN, OUT>> furtherResults = new List<SyntaxParseResult<IN, OUT>>();
                    foreach (var r in rs)
                    {
                        if (r.EndingPosition > lastPosition)
                        {
                            lastPosition = r.EndingPosition;
                            furtherResults.Clear();
                            errors.Clear();
                        }

                        if (r.EndingPosition == lastPosition)
                        {
                            furtherResults.Add(r);
                            errors.AddRange(r.GetErrors());
                        }
                    }


                    if (errors.Count == 0)
                    {
                        errors.Add(new UnexpectedTokenSyntaxError<IN>(tokens[lastPosition], LexemeLabels, null));
                    }
                }
                else
                {
                    if (result.EndingPosition < tokens.Length-1)
                    {
                        SyntaxParseResult<IN, OUT> r = new SyntaxParseResult<IN, OUT>()
                        {
                            IsError = true,
                            IsEnded = false
                        };
                        r.AddErrors(result.GetErrors().ToArray());
                        return r;
                    }
                }
            }

            if (result == null)
            {
                result = new SyntaxParseResult<IN, OUT>();

                if (errors.Count > 0)
                {
                    var lastErrorPosition = errors
                        .Select(e => e.UnexpectedToken.PositionInTokenFlow)
                        .Max();
                    var lastErrors = errors
                        .Where(e =>
                            e.UnexpectedToken.PositionInTokenFlow == lastErrorPosition)
                        .ToList();
                    result.AddErrors(lastErrors);
                }
                else
                {
                    result.AddErrors(errors);
                }

                result.IsError = true;
            }

            return result;
        }


        public virtual SyntaxParseResult<IN, OUT> Parse(Token<IN>[] tokens, Rule<IN, OUT> rule, int position,
            string nonTerminalName, SyntaxParsingContext<IN, OUT> parsingContext)
        {
            var currentPosition = position;
            var errors = new List<UnexpectedTokenSyntaxError<IN>>();
            var isError = false;
            var children = new List<ISyntaxNode<IN, OUT>>();
            if (!tokens[position].IsEOS && rule.Match(tokens, position, Configuration) && rule.Clauses is { Count: > 0 })
            {
                children = new List<ISyntaxNode<IN, OUT>>();
                foreach (var clause in rule.Clauses)
                {
                    switch (clause)
                    {
                        case TerminalClause<IN, OUT> terminalClause:
                        {
                            var termRes = ParseTerminal(tokens, terminalClause, currentPosition, parsingContext);
                            if (!termRes.IsError)
                            {
                                children.Add(termRes.Root);
                                currentPosition = termRes.EndingPosition;
                            }
                            else
                            {
                                var tok = tokens[currentPosition];
                                errors.Add(new UnexpectedTokenSyntaxError<IN>(tok, LexemeLabels, I18n,
                                    terminalClause.ExpectedToken));
                            }

                            isError = termRes.IsError;
                            break;
                        }
                        case NonTerminalClause<IN, OUT> terminalClause:
                        {
                            var nonTerminalResult =
                                ParseNonTerminal(tokens, terminalClause, currentPosition, parsingContext);
                            var ntErrors = nonTerminalResult.GetErrors();
                            if (!nonTerminalResult.IsError)
                            {
                                children.Add(nonTerminalResult.Root);
                                currentPosition = nonTerminalResult.EndingPosition;
                                if (ntErrors != null && ntErrors.Count > 0)
                                    errors.AddRange(ntErrors);
                            }
                            else
                            {
                                errors.AddRange(ntErrors);
                            }

                            isError = nonTerminalResult.IsError;
                            break;
                        }
                    }

                    if (isError) break;
                }
            }

            var result = new SyntaxParseResult<IN, OUT>();
            result.IsError = isError;
            result.AddErrors(errors);
            result.EndingPosition = currentPosition;
            if (!isError)
            {
                SyntaxNode<IN, OUT> node = null;
                if (rule.IsSubRule)
                    node = new GroupSyntaxNode<IN, OUT>(nonTerminalName, children);
                else
                    node = new SyntaxNode<IN, OUT>(rule.NodeName ?? nonTerminalName, children);
                node = ExpressionRuleManager<IN, OUT>.ManageExpressionRules(rule, node);
                if (node.IsByPassNode) // inutile de créer un niveau supplémentaire
                    result.Root = children[0];
                result.Root = node;
                result.IsEnded = tokens[result.EndingPosition].IsEOS;
            }

            return result;
        }




        private SyntaxParseResult<IN, OUT> NoMatchingRuleError(Token<IN>[] tokens, int currentPosition,
            List<LeadingToken<IN>> allAcceptableTokens)
        {
            var noRuleErrors = new List<UnexpectedTokenSyntaxError<IN>>();

            if (currentPosition < tokens.Length)
            {
                noRuleErrors.Add(new UnexpectedTokenSyntaxError<IN>(tokens[currentPosition], I18n,
                    allAcceptableTokens));
            }
            else
            {
                noRuleErrors.Add(new UnexpectedTokenSyntaxError<IN>(new Token<IN> { IsEOS = true }, I18n,
                    allAcceptableTokens));
            }

            var error = new SyntaxParseResult<IN, OUT>();
            error.IsError = true;
            error.Root = null;
            error.IsEnded = false;
            error.AddErrors(noRuleErrors);
            error.EndingPosition = currentPosition;
            error.Expecting = allAcceptableTokens;

            return error;
        }


        

        public virtual void Init(ParserConfiguration<IN, OUT> configuration, string root)
        {
            if (root != null) StartingNonTerminal = root;
            // #540 : reset all leading tokens and recompute with the new configuration (expression rules)
            foreach (var nonTerminal in configuration.NonTerminals.Values)
            {
                nonTerminal?.Rules?.ForEach(x => x?.PossibleLeadingTokens?.Clear());
            }
            InitializeStartingTokens(configuration, StartingNonTerminal);
        }



        #endregion
    }
}