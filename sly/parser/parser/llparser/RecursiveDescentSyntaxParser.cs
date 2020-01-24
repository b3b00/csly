using System.Collections.Generic;
using System.Linq;
using sly.lexer;
using sly.parser.generator;
using sly.parser.syntax.grammar;
using sly.parser.syntax.tree;

namespace sly.parser.llparser
{
    public class RecursiveDescentSyntaxParser<IN, OUT> : ISyntaxParser<IN, OUT> where IN : struct
    {
        public RecursiveDescentSyntaxParser(ParserConfiguration<IN, OUT> configuration, string startingNonTerminal)
        {
            Configuration = configuration;
            StartingNonTerminal = startingNonTerminal;
            ComputeSubRules(configuration);
            InitializeStartingTokens(Configuration, startingNonTerminal);
        }

        public ParserConfiguration<IN, OUT> Configuration { get; set; }
        public string StartingNonTerminal { get; set; }

        public ParserConfiguration<IN, OUT> ComputeSubRules(ParserConfiguration<IN, OUT> configuration)
        {
            var newNonTerms = new List<NonTerminal<IN>>();
            foreach (var nonTerm in configuration.NonTerminals)
            foreach (var rule in nonTerm.Value.Rules)
            {
                var newclauses = new List<IClause<IN>>();
                if (rule.ContainsSubRule)
                {
                    foreach (var clause in rule.Clauses)
                        if (clause is GroupClause<IN> group)
                        {
                            var newNonTerm = CreateSubRule(group);
                            newNonTerms.Add(newNonTerm);
                            var newClause = new NonTerminalClause<IN>(newNonTerm.Name);
                            newClause.IsGroup = true;
                            newclauses.Add(newClause);
                        }
                        else if (clause is ManyClause<IN> many)
                        {
                            if (many.Clause is GroupClause<IN> manyGroup)
                            {
                                var newNonTerm = CreateSubRule(manyGroup);
                                newNonTerms.Add(newNonTerm);
                                var newInnerNonTermClause = new NonTerminalClause<IN>(newNonTerm.Name);
                                newInnerNonTermClause.IsGroup = true;
                                many.Clause = newInnerNonTermClause;
                                newclauses.Add(many);
                            }
                        }
                        else if (clause is OptionClause<IN> option)
                        {
                            if (option.Clause is GroupClause<IN> optionGroup)
                            {
                                var newNonTerm = CreateSubRule(optionGroup);
                                newNonTerms.Add(newNonTerm);
                                var newInnerNonTermClause = new NonTerminalClause<IN>(newNonTerm.Name);
                                newInnerNonTermClause.IsGroup = true;
                                option.Clause = newInnerNonTermClause;
                                newclauses.Add(option);
                            }
                        }
                        else
                        {
                            newclauses.Add(clause);
                        }

                    rule.Clauses.Clear();
                    rule.Clauses.AddRange(newclauses);
                }
            }

            newNonTerms.ForEach(nonTerminal => configuration.AddNonTerminalIfNotExists(nonTerminal));
            return configuration;
        }

        public NonTerminal<IN> CreateSubRule(GroupClause<IN> group)
        {
            var subRuleNonTerminalName = "GROUP-" + group.Clauses.Select(c => c.ToString())
                                             .Aggregate((c1, c2) => $"{c1.ToString()}-{c2.ToString()}");
            var nonTerminal = new NonTerminal<IN>(subRuleNonTerminalName);
            var subRule = new Rule<IN>();
            subRule.Clauses = group.Clauses;
            subRule.IsSubRule = true;
            nonTerminal.Rules.Add(subRule);
            nonTerminal.IsSubRule = true;

            return nonTerminal;
        }

        #region STARTING_TOKENS

        protected virtual void InitializeStartingTokens(ParserConfiguration<IN, OUT> configuration, string root)
        {
            var nts = configuration.NonTerminals;


            InitStartingTokensForNonTerminal(nts, root);
            foreach (var nt in nts.Values)
            {
                foreach (var rule in nt.Rules)
                {
                    if (rule.PossibleLeadingTokens == null || rule.PossibleLeadingTokens.Count == 0)
                        InitStartingTokensForRule(nts, rule);
                }
            }
        }

        protected virtual void InitStartingTokensForNonTerminal(Dictionary<string, NonTerminal<IN>> nonTerminals,
            string name)
        {
            if (nonTerminals.ContainsKey(name))
            {
                var nt = nonTerminals[name];
                nt.Rules.ForEach(r => InitStartingTokensForRule(nonTerminals, r));
            }
        }

        protected void InitStartingTokensForRule(Dictionary<string, NonTerminal<IN>> nonTerminals,
            Rule<IN> rule)
        {
            if (rule.PossibleLeadingTokens == null || rule.PossibleLeadingTokens.Count == 0)
            {
                rule.PossibleLeadingTokens = new List<IN>();
                if (rule.Clauses.Count > 0)
                {
                    var first = rule.Clauses[0];
                    if (first is TerminalClause<IN> term)
                    {
                        rule.PossibleLeadingTokens.Add(term.ExpectedToken);
                        rule.PossibleLeadingTokens = rule.PossibleLeadingTokens.Distinct().ToList();
                    }
                    else if (first  is NonTerminalClause<IN> nonterm)
                    {
                        InitStartingTokensForNonTerminal(nonTerminals, nonterm.NonTerminalName);
                        if (nonTerminals.ContainsKey(nonterm.NonTerminalName))
                        {
                            var firstNonTerminal = nonTerminals[nonterm.NonTerminalName];
                            firstNonTerminal.Rules.ForEach(r =>
                            {
                                rule.PossibleLeadingTokens.AddRange(r.PossibleLeadingTokens);
                            });
                            rule.PossibleLeadingTokens = rule.PossibleLeadingTokens.Distinct().ToList();
                        }
                    }
                    else
                    {
                        InitStartingTokensForRuleExtensions(first,rule,nonTerminals);
                    }
                }
            }
        }

        protected virtual void InitStartingTokensForRuleExtensions(IClause<IN> first, Rule<IN> rule,
            Dictionary<string, NonTerminal<IN>> nonTerminals)
        {
        }

        #endregion

        #region parsing

        public SyntaxParseResult<IN> Parse(IList<Token<IN>> tokens, string startingNonTerminal = null)
        {
            var start = startingNonTerminal ?? StartingNonTerminal;
            var NonTerminals = Configuration.NonTerminals;
            var errors = new List<UnexpectedTokenSyntaxError<IN>>();
            var nt = NonTerminals[start];

            var rules = nt.Rules.Where(r => r.PossibleLeadingTokens.Contains(tokens[0].TokenID)).ToList();

            if (!rules.Any())
            {
                errors.Add(new UnexpectedTokenSyntaxError<IN>(tokens[0], nt.PossibleLeadingTokens.ToArray()));
            }
            
            var rs = new List<SyntaxParseResult<IN>>();
            foreach (var rule in rules)
            {
                var r = Parse(tokens, rule, 0, start);
                rs.Add(r);
            }

            SyntaxParseResult<IN> result = null;


            if (rs.Count > 0)
            {
                result = rs.Find(r => r.IsEnded && !r.IsError);

                if (result == null)
                {
                    var endingPositions = rs.Select(r => r.EndingPosition).ToList();
                    var lastposition = endingPositions.Max();
                    var furtherResults = rs.Where(r => r.EndingPosition == lastposition).ToList();

                    errors.Add(new UnexpectedTokenSyntaxError<IN>(tokens[lastposition], null));
                    furtherResults.ForEach(r =>
                    {
                        if (r.Errors != null) errors.AddRange(r.Errors);
                    });
                }
            }

            if (result == null)
            {
                result = new SyntaxParseResult<IN>();
                errors.Sort();

                if (errors.Count > 0)
                {
                    var lastErrorPosition = errors.Select(e => e.UnexpectedToken.PositionInTokenFlow).ToList().Max();
                    var lastErrors = errors.Where(e => e.UnexpectedToken.PositionInTokenFlow == lastErrorPosition)
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
            string nonTerminalName)
        {
            var currentPosition = position;
            var errors = new List<UnexpectedTokenSyntaxError<IN>>();
            var isError = false;
            var children = new List<ISyntaxNode<IN>>();
            if (rule.PossibleLeadingTokens.Contains(tokens[position].TokenID))
                if (rule.Clauses != null && rule.Clauses.Count > 0)
                {
                    children = new List<ISyntaxNode<IN>>();
                    foreach (var clause in rule.Clauses)
                    {
                        if (clause is TerminalClause<IN>)
                        {
                            var termRes = ParseTerminal(tokens, clause as TerminalClause<IN>, currentPosition);
                            if (!termRes.IsError)
                            {
                                children.Add(termRes.Root);
                                currentPosition = termRes.EndingPosition;
                            }
                            else
                            {
                                var tok = tokens[currentPosition];
                                errors.Add(new UnexpectedTokenSyntaxError<IN>(tok,
                                    ((TerminalClause<IN>) clause).ExpectedToken));
                            }

                            isError = isError || termRes.IsError;
                        }
                        else if (clause is NonTerminalClause<IN>)
                        {
                            var nonTerminalResult =
                                ParseNonTerminal(tokens, clause as NonTerminalClause<IN>, currentPosition);
                            if (!nonTerminalResult.IsError)
                            {
                                children.Add(nonTerminalResult.Root);
                                currentPosition = nonTerminalResult.EndingPosition;
                                if (nonTerminalResult.Errors != null && nonTerminalResult.Errors.Any())
                                    errors.AddRange(nonTerminalResult.Errors);
                            }
                            else
                            {
                                errors.AddRange(nonTerminalResult.Errors);
                            }

                            isError = isError || nonTerminalResult.IsError;
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
                    node = new SyntaxNode<IN>( nonTerminalName, children);
                node = ManageExpressionRules(rule, node);
                if (node.IsByPassNode) // inutile de créer un niveau supplémentaire
                    result.Root = children[0];
                result.Root = node;
                result.IsEnded = result.EndingPosition >= tokens.Count - 1
                                 || result.EndingPosition == tokens.Count - 2 &&
                                 tokens[tokens.Count - 1].TokenID.Equals(default(IN));
                
            }


            return result;
        }

        protected SyntaxNode<IN> ManageExpressionRules(Rule<IN> rule, SyntaxNode<IN> node)
        {
            var operatorIndex = -1;
            if (rule.IsExpressionRule && rule.IsByPassRule)
            {
                node.IsByPassNode = true;
            }
            else if (rule.IsExpressionRule && !rule.IsByPassRule)
            {
                node.ExpressionAffix = rule.ExpressionAffix;
                if (node.Children.Count == 3)
                {
                    operatorIndex = 1;
                }
                else if (node.Children.Count == 2)
                {
                    if (node.ExpressionAffix == Affix.PreFix)
                        operatorIndex = 0;
                    else if (node.ExpressionAffix == Affix.PostFix) operatorIndex = 1;
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
            }
            else if (!rule.IsExpressionRule)
            {
                node.Visitor = rule.GetVisitor();
            }

            return node;
        }

        public SyntaxParseResult<IN> ParseTerminal(IList<Token<IN>> tokens, TerminalClause<IN> terminal, int position)
        {
            var result = new SyntaxParseResult<IN>();
            result.IsError = !terminal.Check(tokens[position].TokenID);
            result.EndingPosition = !result.IsError ? position + 1 : position;
            var token = tokens[position];
            token.Discarded = terminal.Discarded;
            result.Root = new SyntaxLeaf<IN>(token,terminal.Discarded);
            return result;
        }


        public SyntaxParseResult<IN> ParseNonTerminal(IList<Token<IN>> tokens, NonTerminalClause<IN> nonTermClause,
            int currentPosition)
        {
            var startPosition = currentPosition;
            var endingPosition = 0;
            var nt = Configuration.NonTerminals[nonTermClause.NonTerminalName];
            var errors = new List<UnexpectedTokenSyntaxError<IN>>();

            var i = 0;

            var allAcceptableTokens = new List<IN>();
            nt.Rules.ForEach(r =>
            {
                if (r != null && r.PossibleLeadingTokens != null) allAcceptableTokens.AddRange(r.PossibleLeadingTokens);
            });
            allAcceptableTokens = allAcceptableTokens.Distinct().ToList();

            var rules = nt.Rules
                .Where(r => r.PossibleLeadingTokens.Contains(tokens[startPosition].TokenID) || r.MayBeEmpty)
                .ToList();

            if (rules.Count == 0)
                errors.Add(new UnexpectedTokenSyntaxError<IN>(tokens[startPosition],
                    allAcceptableTokens.ToArray<IN>()));

            var innerRuleErrors = new List<UnexpectedTokenSyntaxError<IN>>();
            SyntaxParseResult<IN> okResult = null;
            var greaterIndex = 0;
            var allRulesInError = true;
            while (i < rules.Count)
            {
                var innerrule = rules[i];
                var innerRuleRes = Parse(tokens, innerrule, startPosition, nonTermClause.NonTerminalName);
                if (!innerRuleRes.IsError && okResult == null ||
                    okResult != null && innerRuleRes.EndingPosition > okResult.EndingPosition)
                {
                    okResult = innerRuleRes;
                    okResult.Errors = innerRuleRes.Errors;
                    endingPosition = innerRuleRes.EndingPosition;
                }

                var other = greaterIndex == 0 && innerRuleRes.EndingPosition == 0;
                if (innerRuleRes.EndingPosition > greaterIndex && innerRuleRes.Errors != null &&
                    !innerRuleRes.Errors.Any() || other)
                {
                    greaterIndex = innerRuleRes.EndingPosition;
                    innerRuleErrors.Clear();
                    innerRuleErrors.AddRange(innerRuleRes.Errors);
                }

                innerRuleErrors.AddRange(innerRuleRes.Errors);
                allRulesInError = allRulesInError && innerRuleRes.IsError;
                i++;
            }

            errors.AddRange(innerRuleErrors);

            var result = new SyntaxParseResult<IN>();
            result.Errors = errors;
            if (okResult != null)
            {
                result.Root = okResult.Root;
                result.IsError = false;
                result.EndingPosition = okResult.EndingPosition;
                result.IsEnded = okResult.IsEnded;

                result.Errors = errors;
            }
            else
            {
                result.IsError = true;
                result.Errors = errors;
                greaterIndex = errors.Count > 0 ? errors.Select(e => e.UnexpectedToken.PositionInTokenFlow).Max() : 0;
                result.EndingPosition = greaterIndex;
            }

            return result;
        }

        public virtual void Init(ParserConfiguration<IN, OUT> configuration, string root)
        {
            if (root != null) StartingNonTerminal = root;
            InitializeStartingTokens(configuration, StartingNonTerminal);
        }

        #endregion
    }
}