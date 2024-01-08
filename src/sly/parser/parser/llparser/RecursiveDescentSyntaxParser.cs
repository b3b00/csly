using System.Collections.Generic;
using sly.lexer;
using sly.parser.generator;
using sly.parser.syntax.grammar;
using sly.parser.syntax.tree;
using System.Linq;

namespace sly.parser.llparser
{
    public partial class RecursiveDescentSyntaxParser<IN, OUT> where IN : struct
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

        public SyntaxParseResult<IN> Parse(IList<Token<IN>> tokens, string startingNonTerminal = null)
        {
            return SafeParse(tokens, new SyntaxParsingContext<IN>(), startingNonTerminal);
        }
        
        public SyntaxParseResult<IN> SafeParse(IList<Token<IN>> tokens, SyntaxParsingContext<IN> parsingContext, string startingNonTerminal = null)
        {
            var start = startingNonTerminal ?? StartingNonTerminal;
            var NonTerminals = Configuration.NonTerminals;
            var errors = new List<UnexpectedTokenSyntaxError<IN>>();
            var nt = NonTerminals[start];


            var rs = new List<SyntaxParseResult<IN>>();

            var matchingRuleCount = 0;

            foreach (var rule in nt.Rules)
            {
                if (!tokens[0].IsEOS && rule.PossibleLeadingTokens.Any(x => x.Match(tokens[0])))
                {
                    matchingRuleCount++;
                    var r = Parse(tokens, rule, 0, start, parsingContext);
                    rs.Add(r);
                }
            }

            if (matchingRuleCount == 0)
            {
                errors.Add(new UnexpectedTokenSyntaxError<IN>(tokens[0], LexemeLabels, I18n,
                    nt.PossibleLeadingTokens.ToArray()));
            }

            SyntaxParseResult<IN> result = null;


            if (rs.Count > 0)
            {
                result = rs.Find(r => r.IsEnded && !r.IsError);

                if (result == null)
                {
                    int lastPosition = -1;
                    List<SyntaxParseResult<IN>> furtherResults = new List<SyntaxParseResult<IN>>();
                    //List<UnexpectedTokenSyntaxError<IN>> furtherErrors = new List<UnexpectedTokenSyntaxError<IN>>();
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
                            errors.AddRange(r.Errors);
                        }
                    }


                    if (errors.Count == 0)
                    {
                        errors.Add(new UnexpectedTokenSyntaxError<IN>(tokens[lastPosition], LexemeLabels, null));
                    }
                }
            }

            if (result == null)
            {
                result = new SyntaxParseResult<IN>();
                errors.Sort();

                if (errors.Count > 0)
                {
                    var lastErrorPosition = errors
                        .Select(e => e.UnexpectedToken.PositionInTokenFlow)
                        .Max();
                    var lastErrors = errors
                        .Where(e =>
                            e.UnexpectedToken.PositionInTokenFlow == lastErrorPosition)
                        .ToList();
                    result.Errors = lastErrors;
                }
                else
                {
                    result.Errors = errors;
                }

                result.IsError = true;
            }

            return result;
        }


        public virtual SyntaxParseResult<IN> Parse(IList<Token<IN>> tokens, Rule<IN> rule, int position,
            string nonTerminalName, SyntaxParsingContext<IN> parsingContext)
        {
            var currentPosition = position;
            var errors = new List<UnexpectedTokenSyntaxError<IN>>();
            var isError = false;
            var children = new List<ISyntaxNode<IN>>();
            if (!tokens[position].IsEOS && rule.PossibleLeadingTokens.Any(x => x.Match(tokens[position])))
                if (rule.Clauses != null && rule.Clauses.Count > 0)
                {
                    children = new List<ISyntaxNode<IN>>();
                    foreach (var clause in rule.Clauses)
                    {
                        switch (clause)
                        {
                            case TerminalClause<IN> terminalClause:
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

                                isError = isError || termRes.IsError;
                                break;
                            }
                            case NonTerminalClause<IN> terminalClause:
                            {
                                var nonTerminalResult =
                                    ParseNonTerminal(tokens, terminalClause, currentPosition, parsingContext);
                                if (!nonTerminalResult.IsError)
                                {
                                    children.Add(nonTerminalResult.Root);
                                    currentPosition = nonTerminalResult.EndingPosition;
                                    if (nonTerminalResult.Errors != null && nonTerminalResult.Errors.Count > 0)
                                        errors.AddRange(nonTerminalResult.Errors);
                                }
                                else
                                {
                                    errors.AddRange(nonTerminalResult.Errors);
                                }

                                isError = isError || nonTerminalResult.IsError;
                                break;
                            }
                        }

                        if (isError) break;
                    }
                }

            var result = new SyntaxParseResult<IN>();
            result.IsError = isError;
            result.Errors = errors;
            result.EndingPosition = currentPosition;
            if (!isError)
            {
                SyntaxNode<IN> node = null;
                if (rule.IsSubRule)
                    node = new GroupSyntaxNode<IN>(nonTerminalName, children);
                else
                    node = new SyntaxNode<IN>(nonTerminalName, children);
                node = ManageExpressionRules(rule, node);
                if (node.IsByPassNode) // inutile de créer un niveau supplémentaire
                    result.Root = children[0];
                result.Root = node;
                result.IsEnded = tokens[result.EndingPosition].IsEOS;
            }

            return result;
        }

        protected SyntaxNode<IN> ManageExpressionRules(Rule<IN> rule, SyntaxNode<IN> node)
        {
            var operatorIndex = -1;
            switch (rule.IsExpressionRule)
            {
                case true when rule.IsByPassRule:
                    node.IsByPassNode = true;
                    node.HasByPassNodes = true;
                    break;
                case true when !rule.IsByPassRule:
                {
                    node.ExpressionAffix = rule.ExpressionAffix;
                    switch (node.Children.Count)
                    {
                        case 3:
                            operatorIndex = 1;
                            break;
                        case 2 when node.ExpressionAffix == Affix.PreFix:
                            operatorIndex = 0;
                            break;
                        case 2:
                        {
                            if (node.ExpressionAffix == Affix.PostFix) operatorIndex = 1;
                            break;
                        }
                    }

                    if (operatorIndex >= 0)
                        if (node.Children[operatorIndex] is SyntaxLeaf<IN> operatorNode)
                            if (operatorNode != null)
                            {
                                var visitor = rule.GetVisitor(operatorNode.Token.TokenID);
                                if (visitor != null)
                                {
                                    node.Visitor = visitor;
                                    node.Operation = rule.GetOperation(operatorNode.Token.TokenID);
                                }
                            }

                    break;
                }
                case false:
                    node.Visitor = rule.GetVisitor();
                    break;
            }

            return node;
        }

        public SyntaxParseResult<IN> ParseTerminal(IList<Token<IN>> tokens, TerminalClause<IN> terminal, int position,
            SyntaxParsingContext<IN> parsingContext)
        {
            if (parsingContext.TryGetParseResult(terminal, position, out var parseResult))
            {
                return parseResult;
            }
            var result = new SyntaxParseResult<IN>();
            result.IsError = !terminal.Check(tokens[position]);
            result.EndingPosition = !result.IsError ? position + 1 : position;
            var token = tokens[position];
            token.Discarded = terminal.Discarded;
            token.IsExplicit = terminal.IsExplicitToken;
            result.Root = new SyntaxLeaf<IN>(token, terminal.Discarded);
            result.HasByPassNodes = false;
            if (result.IsError)
            {
                result.Errors.Add(
                    new UnexpectedTokenSyntaxError<IN>(token, LexemeLabels, I18n, terminal.ExpectedToken));
                result.AddExpecting(terminal.ExpectedToken);
            }
            parsingContext.Memoize(terminal,position,result);
            return result;
        }


        
        public SyntaxParseResult<IN> ParseNonTerminal(IList<Token<IN>> tokens, NonTerminalClause<IN> nonTermClause,
            int currentPosition, SyntaxParsingContext<IN> parsingContext)
        {
            var result = ParseNonTerminal(tokens, nonTermClause.NonTerminalName, currentPosition, parsingContext);
            return result;
        }

        
        
        public SyntaxParseResult<IN> ParseNonTerminal(IList<Token<IN>> tokens, string nonTerminalName,
            int currentPosition, SyntaxParsingContext<IN> parsingContext)
        {
            if (parsingContext.TryGetParseResult(new NonTerminalClause<IN>(nonTerminalName),currentPosition, out var memoizedResult))
            {
                return memoizedResult;
            }
            var startPosition = currentPosition;
            var nt = Configuration.NonTerminals[nonTerminalName];
            var errors = new List<UnexpectedTokenSyntaxError<IN>>();

            var i = 0;
            var rules = nt.Rules;

            var innerRuleErrors = new List<UnexpectedTokenSyntaxError<IN>>();
            var greaterIndex = 0;
            var rulesResults = new List<SyntaxParseResult<IN>>();
            while (i < rules.Count)
            {
                var innerrule = rules[i];
                if (startPosition < tokens.Count && !tokens[startPosition].IsEOS &&
                    innerrule.PossibleLeadingTokens.Any(x => x.Match(tokens[startPosition])) || innerrule.MayBeEmpty)
                {
                    var innerRuleRes = Parse(tokens, innerrule, startPosition, nonTerminalName, parsingContext);
                    rulesResults.Add(innerRuleRes);

                    var other = greaterIndex == 0 && innerRuleRes.EndingPosition == 0;
                    if (innerRuleRes.EndingPosition > greaterIndex && innerRuleRes.Errors != null &&
                        innerRuleRes.Errors.Count == 0 || other)
                    {
                        greaterIndex = innerRuleRes.EndingPosition;
                        //innerRuleErrors.Clear();
                        innerRuleErrors.AddRange(innerRuleRes.Errors);
                    }

                    innerRuleErrors.AddRange(innerRuleRes.Errors);
                }

                i++;
            }

            if (rulesResults.Count == 0)
            {
                var allAcceptableTokens = new List<LeadingToken<IN>>();
                nt.Rules.ForEach(r =>
                {
                    if (r != null && r.PossibleLeadingTokens != null)
                        allAcceptableTokens.AddRange(r.PossibleLeadingTokens);
                });
                // allAcceptableTokens = allAcceptableTokens.ToList();
                
                var noMatching =  NoMatchingRuleError(tokens, currentPosition, allAcceptableTokens);
                parsingContext.Memoize(new NonTerminalClause<IN>(nonTerminalName),currentPosition,noMatching);
                return noMatching;
            }

            errors.AddRange(innerRuleErrors);
            SyntaxParseResult<IN> max = null;
            int okEndingPosition = -1;
            int koEndingPosition = -1;
            bool hasOk = false;
            SyntaxParseResult<IN> maxOk = null;
            SyntaxParseResult<IN> maxKo = null;
            foreach (var rulesResult in rulesResults)
            {
                if (rulesResult.IsOk)
                {
                    hasOk = true;
                    if (rulesResult.EndingPosition > okEndingPosition)
                    {
                        okEndingPosition = rulesResult.EndingPosition;
                        maxOk = rulesResult;
                    }
                }

                if (rulesResult.IsError)
                {
                    if (rulesResult.EndingPosition > koEndingPosition)
                    {
                        koEndingPosition = rulesResult.EndingPosition;
                        maxKo = rulesResult;
                    }
                }
            }

            if (hasOk)
            {
                max = maxOk;
            }
            else
            {
                max = maxKo;
            }


            var result = new SyntaxParseResult<IN>();
            result.Errors = errors;
            result.Root = max.Root;
            result.EndingPosition = max.EndingPosition;
            result.IsError = max.IsError;
            result.IsEnded = max.IsEnded;
            result.HasByPassNodes = max.HasByPassNodes;

            if (rulesResults.Count > 0)
            {
                List<UnexpectedTokenSyntaxError<IN>> terr = new List<UnexpectedTokenSyntaxError<IN>>();
                foreach (var ruleResult in rulesResults)
                {
                    terr.AddRange(ruleResult.Errors);
                    foreach (var err in ruleResult.Errors)
                    {
                        result.AddExpectings(err.ExpectedTokens);
                    }
                }
            }
            parsingContext.Memoize(new NonTerminalClause<IN>(nonTerminalName),currentPosition,result);
            return result;
        }

        private SyntaxParseResult<IN> NoMatchingRuleError(IList<Token<IN>> tokens, int currentPosition,
            List<LeadingToken<IN>> allAcceptableTokens)
        {
            var noRuleErrors = new List<UnexpectedTokenSyntaxError<IN>>();

            if (currentPosition < tokens.Count)
            {
                noRuleErrors.Add(new UnexpectedTokenSyntaxError<IN>(tokens[currentPosition], I18n,
                    allAcceptableTokens));
            }
            else
            {
                noRuleErrors.Add(new UnexpectedTokenSyntaxError<IN>(new Token<IN> { IsEOS = true }, I18n,
                    allAcceptableTokens));
            }

            var error = new SyntaxParseResult<IN>();
            error.IsError = true;
            error.Root = null;
            error.IsEnded = false;
            error.Errors = noRuleErrors;
            error.EndingPosition = currentPosition;
            error.Expecting = allAcceptableTokens;

            return error;
        }


        

        public virtual void Init(ParserConfiguration<IN, OUT> configuration, string root)
        {
            if (root != null) StartingNonTerminal = root;
            InitializeStartingTokens(configuration, StartingNonTerminal);
        }



        #endregion
    }
}