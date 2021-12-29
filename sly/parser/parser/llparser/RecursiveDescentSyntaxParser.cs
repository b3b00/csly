using System;
using System.Collections.Generic;
using sly.lexer;
using sly.parser.generator;
using sly.parser.syntax.grammar;
using sly.parser.syntax.tree;
using System.Linq;
using sly.parser.generator.visitor;

namespace sly.parser.llparser
{
    public partial class RecursiveDescentSyntaxParser<IN, OUT> : ISyntaxParser<IN, OUT> where IN : struct
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

        public SyntaxParseResult<IN, OUT> Parse(IList<Token<IN>> tokens, string startingNonTerminal = null)
        {
            var start = startingNonTerminal ?? StartingNonTerminal;
            var NonTerminals = Configuration.NonTerminals;
            var errors = new List<UnexpectedTokenSyntaxError<IN>>();
            var nt = NonTerminals[start];


            var rs = new List<SyntaxParseResult<IN, OUT>>();

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

            SyntaxParseResult<IN, OUT> result = null;


            if (rs.Count > 0)
            {
                result = rs.Find(r => r.IsEnded && !r.IsError);

                if (result == null)
                {
                    int lastPosition = -1;
                    List<SyntaxParseResult<IN, OUT>> furtherResults = new List<SyntaxParseResult<IN, OUT>>();
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
                result = new SyntaxParseResult<IN, OUT>();
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


        public virtual SyntaxParseResult<IN, OUT> Parse(IList<Token<IN>> tokens, Rule<IN,OUT> rule, int position,
            string nonTerminalName)
        {
            var currentPosition = position;
            var errors = new List<UnexpectedTokenSyntaxError<IN>>();
            var isError = false;
            var children = new List<ISyntaxNode<IN, OUT>>();
            if (!tokens[position].IsEOS && rule.PossibleLeadingTokens.Contains(tokens[position].TokenID))
                if (rule.Clauses != null && rule.Clauses.Count > 0)
                {
                    children = new List<ISyntaxNode<IN, OUT>>();
                    foreach (var clause in rule.Clauses)
                    {
                        if (clause is TerminalClause<IN,OUT>)
                        {
                            var termRes = ParseTerminal(tokens, clause as TerminalClause<IN,OUT>, currentPosition);
                            if (!termRes.IsError)
                            {
                                children.Add(termRes.Root);
                                currentPosition = termRes.EndingPosition;
                            }
                            else
                            {
                                var tok = tokens[currentPosition];
                                errors.Add(new UnexpectedTokenSyntaxError<IN>(tok, I18n,
                                    ((TerminalClause<IN,OUT>)clause).ExpectedToken));
                            }

                            isError = isError || termRes.IsError;
                        }
                        else if (clause is NonTerminalClause<IN,OUT>)
                        {
                            var nonTerminalResult =
                                ParseNonTerminal(tokens, clause as NonTerminalClause<IN,OUT>, currentPosition);
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
                        }

                        if (isError) break;
                    }
                }

            var result = new SyntaxParseResult<IN, OUT>();
            result.IsError = isError;
            result.Errors = errors;
            result.EndingPosition = currentPosition;
            if (!isError)
            {
                SyntaxNode<IN, OUT> node = null;
                if (rule.IsSubRule)
                    node = new GroupSyntaxNode<IN, OUT>(nonTerminalName, children);
                else
                    node = new SyntaxNode<IN, OUT>(nonTerminalName, children,rule.GetVisitor(),rule.GetVisitorCaller());
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

        protected SyntaxNode<IN,OUT> ManageExpressionRules(Rule<IN, OUT> rule, SyntaxNode<IN,OUT> node)
        {
            var operatorIndex = -1;
            if (rule.IsExpressionRule && rule.IsByPassRule)
            {
                node.IsByPassNode = true;
                node.HasByPassNodes = true;
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
                    if (node.Children[operatorIndex] is SyntaxLeaf<IN,OUT> operatorNode)
                        if (operatorNode != null)
                        {
                            var visitor = rule.GetVisitor(operatorNode.Token.TokenID);
                            var caller = rule.GetVisitorCaller(operatorNode.Token.TokenID);
                            if (visitor != null)
                            {
                                node.Visitor = visitor;
                                node.Operation = rule.GetOperation(operatorNode.Token.TokenID);
                                node.VisitorCaller = caller;
                            }

                            
                            
                            
                        }
            }
            else if (!rule.IsExpressionRule)
            {
                node.Visitor = rule.GetVisitor();
            }

            return node;
        }

        public SyntaxParseResult<IN,OUT> ParseTerminal(IList<Token<IN>> tokens, TerminalClause<IN,OUT> terminal, int position)
        {
            var result = new SyntaxParseResult<IN,OUT>();
            result.IsError = !terminal.Check(tokens[position]);
            result.EndingPosition = !result.IsError ? position + 1 : position;
            var token = tokens[position];
            token.Discarded = terminal.Discarded;
            result.Root = new SyntaxLeaf<IN,OUT>(token, terminal.Discarded);
            result.HasByPassNodes = false;
            result.Errors.Add(new UnexpectedTokenSyntaxError<IN>(token,I18n,terminal.ExpectedToken));
            result.AddExpecting(terminal.ExpectedToken);
            return result;
        }


        public SyntaxParseResult<IN,OUT> ParseNonTerminal(IList<Token<IN>> tokens, NonTerminalClause<IN,OUT> nonTermClause,
            int currentPosition)
        {
            return ParseNonTerminal(tokens, nonTermClause.NonTerminalName, currentPosition);
        }

        public SyntaxParseResult<IN,OUT> ParseNonTerminal(IList<Token<IN>> tokens, string nonTerminalName,
            int currentPosition)
        {
            var startPosition = currentPosition;
            var nt = Configuration.NonTerminals[nonTerminalName];
            var errors = new List<UnexpectedTokenSyntaxError<IN>>();

            var i = 0;
            var rules = nt.Rules;

            var innerRuleErrors = new List<UnexpectedTokenSyntaxError<IN>>();
            var greaterIndex = 0;
            var rulesResults = new List<SyntaxParseResult<IN,OUT>>();
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
            SyntaxParseResult<IN,OUT> max = null;
            int okEndingPosition = -1;
            int koEndingPosition = -1;
            bool hasOk = false;
            bool hasKo = false;
            SyntaxParseResult<IN,OUT> maxOk = null;
            SyntaxParseResult<IN,OUT> maxKo = null;
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
                        hasKo = true;
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
                max = new SyntaxParseResult<IN,OUT>();
                max.IsError = true;
                max.Root = null;
                max.IsEnded = false;
            }

            var result = new SyntaxParseResult<IN,OUT>();
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

        private SyntaxParseResult<IN,OUT> NoMatchingRuleError(IList<Token<IN>> tokens, int currentPosition,
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
                noRuleErrors.Add(new UnexpectedTokenSyntaxError<IN>(new Token<IN>() { IsEOS = true }, I18n,
                    allAcceptableTokens));
            }

            var error = new SyntaxParseResult<IN,OUT>();
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