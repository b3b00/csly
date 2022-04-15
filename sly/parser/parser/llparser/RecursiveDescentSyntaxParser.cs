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


        #region parsing

        public SyntaxParseResult<IN> Parse(IList<Token<IN>> tokens, string startingNonTerminal = null)
        {
            var start = startingNonTerminal ?? StartingNonTerminal;
            var NonTerminals = Configuration.NonTerminals;
            var errors = new List<UnexpectedTokenSyntaxError<IN>>();
            var nt = NonTerminals[start];


            var rs = new List<SyntaxParseResult<IN>>();

            var matchingRuleCount = 0;

            foreach (var rule in nt.Rules)
            {
                if (!tokens[0].IsEOS && rule.PossibleLeadingTokens.Contains(tokens[0].TokenID))
                {
                    matchingRuleCount++;
                    var r = Parse(tokens, rule, 0, start);
                    rs.Add(r);
                }
            }

            if (matchingRuleCount == 0)
            {
                errors.Add(new UnexpectedTokenSyntaxError<IN>(tokens[0], I18n, nt.PossibleLeadingTokens.ToArray()));
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
                        errors.Add(new UnexpectedTokenSyntaxError<IN>(tokens[lastPosition], null));
                    }
                }
            }

            if (result == null)
            {
                result = new SyntaxParseResult<IN>();
                errors.Sort();

                if (errors.Count > 0)
                {
                    var lastErrorPosition = errors.Select<UnexpectedTokenSyntaxError<IN>, int>(e => e.UnexpectedToken.PositionInTokenFlow).ToList<int>().Max();
                    var lastErrors = errors.Where<UnexpectedTokenSyntaxError<IN>>(e => e.UnexpectedToken.PositionInTokenFlow == lastErrorPosition)
                        .ToList<UnexpectedTokenSyntaxError<IN>>();
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
            if (!tokens[position].IsEOS && rule.PossibleLeadingTokens.Contains(tokens[position].TokenID))
                if (rule.Clauses != null && rule.Clauses.Count > 0)
                {
                    children = new List<ISyntaxNode<IN>>();
                    foreach (var clause in rule.Clauses)
                    {
                        switch (clause)
                        {
                            case TerminalClause<IN> terminalClause:
                            {
                                var termRes = ParseTerminal(tokens, terminalClause, currentPosition);
                                if (!termRes.IsError)
                                {
                                    children.Add(termRes.Root);
                                    currentPosition = termRes.EndingPosition;
                                }
                                else
                                {
                                    var tok = tokens[currentPosition];
                                    errors.Add(new UnexpectedTokenSyntaxError<IN>(tok, I18n,
                                        terminalClause.ExpectedToken));
                                }

                                isError = isError || termRes.IsError;
                                break;
                            }
                            case NonTerminalClause<IN> terminalClause:
                            {
                                var nonTerminalResult =
                                    ParseNonTerminal(tokens, terminalClause, currentPosition);
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
                result.IsEnded = result.EndingPosition >= tokens.Count - 1
                                 || result.EndingPosition == tokens.Count - 2 &&
                                 tokens[tokens.Count - 1].IsEOS;
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

        public SyntaxParseResult<IN> ParseTerminal(IList<Token<IN>> tokens, TerminalClause<IN> terminal, int position)
        {
            var result = new SyntaxParseResult<IN>();
            result.IsError = !terminal.Check(tokens[position]);
            result.EndingPosition = !result.IsError ? position + 1 : position;
            var token = tokens[position];
            token.Discarded = terminal.Discarded;
            result.Root = new SyntaxLeaf<IN>(token, terminal.Discarded);
            result.HasByPassNodes = false;
            result.Errors.Add(new UnexpectedTokenSyntaxError<IN>(token,I18n,terminal.ExpectedToken));
            result.AddExpecting(terminal.ExpectedToken);
            return result;
        }


        public SyntaxParseResult<IN> ParseNonTerminal(IList<Token<IN>> tokens, NonTerminalClause<IN> nonTermClause,
            int currentPosition)
        {
            return ParseNonTerminal(tokens, nonTermClause.NonTerminalName, currentPosition);
        }

        public SyntaxParseResult<IN> ParseNonTerminal(IList<Token<IN>> tokens, string nonTerminalName,
            int currentPosition)
        {
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
                    innerrule.PossibleLeadingTokens.Contains(tokens[startPosition].TokenID) || innerrule.MayBeEmpty)
                {
                    var innerRuleRes = Parse(tokens, innerrule, startPosition, nonTerminalName);
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
                var allAcceptableTokens = new List<IN>();
                nt.Rules.ForEach(r =>
                {
                    if (r != null && r.PossibleLeadingTokens != null)
                        allAcceptableTokens.AddRange(r.PossibleLeadingTokens);
                });
                // allAcceptableTokens = allAcceptableTokens.ToList();
                return NoMatchingRuleError(tokens, currentPosition, allAcceptableTokens);
            }

            errors.AddRange(innerRuleErrors);
            SyntaxParseResult<IN> max = null;
            int okEndingPosition = -1;
            int koEndingPosition = -1;
            bool hasOk = false;
            SyntaxParseResult<IN> maxOk = null;
            SyntaxParseResult<IN> maxKo = null;
            if (rulesResults.Count > 0)
            {
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
            }
            else
            {
                max = new SyntaxParseResult<IN>();
                max.IsError = true;
                max.Root = null;
                max.IsEnded = false;
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

            return result;
        }

        private SyntaxParseResult<IN> NoMatchingRuleError(IList<Token<IN>> tokens, int currentPosition,
            List<IN> allAcceptableTokens)
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