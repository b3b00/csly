using sly.parser.syntax;
using sly.lexer;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using sly.parser.generator;
using System.Reflection;
using Newtonsoft.Json;
using System.Diagnostics;

namespace sly.parser.llparser
{


    public class RecursiveDescentSyntaxParser<IN,OUT> : ISyntaxParser<IN,OUT> where IN : struct
    {
        public ParserConfiguration<IN,OUT> Configuration { get; set; }
        public string StartingNonTerminal { get; set; }

        public RecursiveDescentSyntaxParser(ParserConfiguration<IN,OUT> configuration, string startingNonTerminal)
        {
            Configuration = configuration;
            StartingNonTerminal = startingNonTerminal;
            ComputeSubRules(configuration);
            InitializeStartingTokens(Configuration, startingNonTerminal);
        }

        public ParserConfiguration<IN, OUT> ComputeSubRules(ParserConfiguration<IN, OUT> configuration)
        {
            List<NonTerminal<IN>> newNonTerms = new List<NonTerminal<IN>>();
            foreach(var nonTerm in configuration.NonTerminals)
            {
                foreach (var rule in nonTerm.Value.Rules)
                {
                    var newclauses = new List<IClause<IN>>();
                    if (rule.ContainsSubRule)
                    {
                        foreach(IClause<IN> clause in rule.Clauses)
                        {
                            if (clause is GroupClause<IN> group)
                            {
                                NonTerminal<IN> newNonTerm = CreateSubRule(group);
                                newNonTerms.Add(newNonTerm);
                                IClause<IN> newClause = new NonTerminalClause<IN>(newNonTerm.Name);
                                newclauses.Add(newClause);
                            }                           
                            if (clause is ManyClause<IN> many)
                            {
                                if (many.Clause is GroupClause<IN> manyGroup)
                                {
                                    NonTerminal<IN> newNonTerm = CreateSubRule(manyGroup);
                                    newNonTerms.Add(newNonTerm);
                                    many.Clause = new NonTerminalClause<IN>(newNonTerm.Name);
                                    newclauses.Add(many);
                                }

                            }
                            else
                            {
                                newclauses.Add(clause);
                            }
                        }
                    }
                }
            }
            newNonTerms.ForEach(nonTerm => configuration.AddNonTerminal(nonTerm));
            return configuration;
        }

        public NonTerminal<IN> CreateSubRule(GroupClause<IN> group)
        {
            // TODO create new dynamic non term
            string subRuleNonTerminalName = group.Clauses.Select(c => c.ToString()).Aggregate<string>((string c1, string c2) => $"{c1.ToString()}-{c2.ToString()}");
            NonTerminal<IN> nonTerminal = new NonTerminal<IN>(subRuleNonTerminalName);
            // TODO create new dynamic rule for new non term, mark it as subrul
            Rule<IN> subRule = new Rule<IN>();
            subRule.Clauses = group.Clauses;
            subRule.IsSubRule = true;
            nonTerminal.Rules.Add(subRule);
            nonTerminal.IsSubRule = true;

            return nonTerminal;
        }

        #region STARTING_TOKENS


        protected virtual void InitializeStartingTokens(ParserConfiguration<IN,OUT> configuration, string root)
        {


            Dictionary<string, NonTerminal<IN>> nts = configuration.NonTerminals;


            InitStartingTokensForNonTerminal(nts, root);
            foreach (NonTerminal<IN> nt in nts.Values)
            {
                foreach (Rule<IN> rule in nt.Rules)
                {
                    if (rule.PossibleLeadingTokens == null || rule.PossibleLeadingTokens.Count == 0)
                    {
                        InitStartingTokensForRule(nts, rule);
                    }
                }
            }
        }

        protected virtual void InitStartingTokensForNonTerminal(Dictionary<string, NonTerminal<IN>> nonTerminals, string name)
        {
            if (nonTerminals.ContainsKey(name))
            {
                NonTerminal<IN> nt = nonTerminals[name];
                nt.Rules.ForEach(r => InitStartingTokensForRule(nonTerminals, r));
            }
        }

        protected virtual void InitStartingTokensForRule(Dictionary<string, NonTerminal<IN>> nonTerminals, Rule<IN> rule)
        {
            if (rule.PossibleLeadingTokens == null || rule.PossibleLeadingTokens.Count == 0)
            {
                rule.PossibleLeadingTokens = new List<IN>();
                if (rule.Clauses.Count > 0)
                {
                    IClause<IN> first = rule.Clauses[0];
                    if (first is TerminalClause<IN>)
                    {
                        TerminalClause<IN> term = first as TerminalClause<IN>;
                        rule.PossibleLeadingTokens.Add(term.ExpectedToken);
                        rule.PossibleLeadingTokens = rule.PossibleLeadingTokens.Distinct<IN>().ToList<IN>();
                    }
                    else
                    {
                        NonTerminalClause<IN> nonterm = first as NonTerminalClause<IN>;
                        InitStartingTokensForNonTerminal(nonTerminals, nonterm.NonTerminalName);
                        if (nonTerminals.ContainsKey(nonterm.NonTerminalName))
                        {
                            NonTerminal<IN> firstNonTerminal = nonTerminals[nonterm.NonTerminalName];
                            firstNonTerminal.Rules.ForEach(r =>
                            {
                                rule.PossibleLeadingTokens.AddRange(r.PossibleLeadingTokens);
                            });
                            rule.PossibleLeadingTokens = rule.PossibleLeadingTokens.Distinct<IN>().ToList<IN>();
                        }
                    }
                }

            }
        }

        #endregion

        #region parsing

    
        public SyntaxParseResult<IN> Parse(IList<Token<IN>> tokens, string startingNonTerminal = null)
        {
            string start = startingNonTerminal ?? StartingNonTerminal;
            Dictionary<string, NonTerminal<IN>> NonTerminals = Configuration.NonTerminals;
            List<UnexpectedTokenSyntaxError<IN>> errors = new List<UnexpectedTokenSyntaxError<IN>>();
            NonTerminal<IN> nt = NonTerminals[start];

            List<Rule<IN>> rules = nt.Rules.Where<Rule<IN>>(r => r.PossibleLeadingTokens.Contains(tokens[0].TokenID)).ToList<Rule<IN>>();

            List<SyntaxParseResult<IN>> rs = new List<SyntaxParseResult<IN>>();
            foreach (Rule<IN> rule in rules)
            {
                SyntaxParseResult<IN> r = Parse(tokens, rule, 0, start);                
                rs.Add(r);                
            }
            SyntaxParseResult<IN> result = null;


            if (rs.Count > 0)
            {
                result = rs.Find(r => r.IsEnded && !r.IsError);

                if (result == null)
                {
                    List<int> endingPositions = rs.Select(r => r.EndingPosition).ToList<int>();
                    int lastposition = endingPositions.Max();
                    List<SyntaxParseResult<IN>> furtherResults = rs.Where<SyntaxParseResult<IN>>(r => r.EndingPosition == lastposition).ToList<SyntaxParseResult<IN>>();
                    
                    errors.Add(new UnexpectedTokenSyntaxError<IN>(tokens[lastposition], null));
                    furtherResults.ForEach(r =>
                    {
                        if (r.Errors != null)
                        {
                            errors.AddRange(r.Errors);
                        }
                    });
                }

            }
            if (result == null)
            {
                result = new SyntaxParseResult<IN>();
                errors.Sort();
                
                if (errors.Count > 0)
                {
                    int lastErrorPosition = errors.Select(e => e.UnexpectedToken.PositionInTokenFlow).ToList().Max();
                    var lastErrors = errors.Where(e => e.UnexpectedToken.PositionInTokenFlow == lastErrorPosition).ToList();                    
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


        public virtual SyntaxParseResult<IN> Parse(IList<Token<IN>> tokens, Rule<IN> rule, int position, string nonTerminalName)
        {
            int currentPosition = position;
            List<UnexpectedTokenSyntaxError<IN>> errors = new List<UnexpectedTokenSyntaxError<IN>>();
            bool isError = false;
            List<ISyntaxNode<IN>> children = new List<ISyntaxNode<IN>>();
            if (rule.PossibleLeadingTokens.Contains(tokens[position].TokenID))
            {
                if (rule.Clauses != null && rule.Clauses.Count > 0)
                {
                    children = new List<ISyntaxNode<IN>>();
                    foreach (IClause<IN> clause in rule.Clauses)
                    {
                        if (clause is TerminalClause<IN>)
                        {
                            SyntaxParseResult<IN> termRes = ParseTerminal(tokens, clause as TerminalClause<IN>, currentPosition);
                            if (!termRes.IsError)
                            {
                                children.Add(termRes.Root);
                                currentPosition = termRes.EndingPosition;
                            }
                            else
                            {
                                Token<IN> tok = tokens[currentPosition];
                                errors.Add(new UnexpectedTokenSyntaxError<IN>(tok, ((TerminalClause<IN>)clause).ExpectedToken));
                            }
                            isError = isError || termRes.IsError;
                        }
                        else if (clause is NonTerminalClause<IN>)
                        {
                            SyntaxParseResult<IN> nonTerminalResult =
                                ParseNonTerminal(tokens, clause as NonTerminalClause<IN>, currentPosition);
                            if (!nonTerminalResult.IsError)
                            {
                                children.Add(nonTerminalResult.Root);
                                currentPosition = nonTerminalResult.EndingPosition;
                                if (nonTerminalResult.Errors != null && nonTerminalResult.Errors.Any())
                                {
                                    errors.AddRange(nonTerminalResult.Errors);
                                }
                            }
                            else
                            {
                                errors.AddRange(nonTerminalResult.Errors);
                            }
                            isError = isError || nonTerminalResult.IsError;
                        }
                        if (isError)
                        {
                            break;
                        }
                    }
                }
            }

            SyntaxParseResult<IN> result = new SyntaxParseResult<IN>();
            result.IsError = isError;
            result.Errors = errors;
            result.EndingPosition = currentPosition;
            if (!isError)
            {                
                SyntaxNode<IN> node = new SyntaxNode<IN>(nonTerminalName + "__" + rule.Key, children);                
                node = ManageExpressionRules(rule, node);
                if (node.IsByPassNode) // inutile de créer un niveau supplémentaire
                {
                    result.Root = children[0];
                }
                result.Root =  node;
                result.IsEnded = result.EndingPosition >= tokens.Count - 1
                                || result.EndingPosition == tokens.Count - 2 && tokens[tokens.Count - 1].TokenID.Equals(default(IN));
                if (result.IsEnded)
                {
                    ;
                }
                if (rule.IsExpressionRule)
                {

                }
            }


            return result;
        }

        protected SyntaxNode<IN> ManageExpressionRules(Rule<IN> rule, SyntaxNode<IN> node)
        {
            int operatorIndex = -1;
            if (rule.IsExpressionRule && rule.IsByPassRule)
            {
                node.IsByPassNode = true;
            }
            else if (rule.IsExpressionRule && !rule.IsByPassRule)
            {
                if (node.Children.Count == 3)
                {
                    operatorIndex = 1;
                }
                else if (node.Children.Count == 2)
                {
                    operatorIndex = 0;
                }
                if (operatorIndex >= 0)
                {
                    if (node.Children[operatorIndex] is SyntaxLeaf<IN> operatorNode)
                    {
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
                }
            }  
            else if( !rule.IsExpressionRule)
            {
                node.Visitor = rule.GetVisitor();
            }
            return node;
                
        }

        public SyntaxParseResult<IN> ParseTerminal(IList<Token<IN>> tokens, TerminalClause<IN> term, int position)
        {
            SyntaxParseResult<IN> result = new SyntaxParseResult<IN>();
            result.IsError = !term.Check(tokens[position].TokenID);
            result.EndingPosition = !result.IsError ? position + 1 : position;
            Token<IN> token = tokens[position];
            token.Discarded = term.Discarded;
            result.Root = new SyntaxLeaf<IN>(token);
            return result;
        }


        public SyntaxParseResult<IN> ParseNonTerminal(IList<Token<IN>> tokens, NonTerminalClause<IN> nonTermClause, int currentPosition)
        {
            int startPosition = currentPosition;
            int endingPosition = 0;
            NonTerminal<IN> nt = Configuration.NonTerminals[nonTermClause.NonTerminalName];
            bool found = false;
            List<UnexpectedTokenSyntaxError<IN>> errors = new List<UnexpectedTokenSyntaxError<IN>>();

            int i = 0;

            List<IN> allAcceptableTokens = new List<IN>();
            nt.Rules.ForEach(r =>
            {
                if (r != null && r.PossibleLeadingTokens != null)
                {
                    allAcceptableTokens.AddRange(r.PossibleLeadingTokens);
                }
            });
            allAcceptableTokens = allAcceptableTokens.Distinct<IN>().ToList<IN>();

            List<Rule<IN>> rules = nt.Rules
                .Where<Rule<IN>>(r => r.PossibleLeadingTokens.Contains(tokens[startPosition].TokenID) || r.MayBeEmpty)
                .ToList<Rule<IN>>();

            if (rules.Count == 0)
            {
                errors.Add(new UnexpectedTokenSyntaxError<IN>(tokens[startPosition],
                    allAcceptableTokens.ToArray<IN>()));
            }

            List<UnexpectedTokenSyntaxError<IN>> innerRuleErrors = new List<UnexpectedTokenSyntaxError<IN>>();
            SyntaxParseResult<IN> okResult = null;
            int greaterIndex = 0;
            bool allRulesInError = true;
            while (i < rules.Count)
            {
                Rule<IN> innerrule = rules[i];
                SyntaxParseResult<IN> innerRuleRes = Parse(tokens, innerrule, startPosition, nonTermClause.NonTerminalName);
                if (!innerRuleRes.IsError && okResult == null || (okResult != null && innerRuleRes.EndingPosition > okResult.EndingPosition))
                {
                    okResult = innerRuleRes;
                    okResult.Errors = innerRuleRes.Errors;
                    found = true;
                    endingPosition = innerRuleRes.EndingPosition;
                }
                bool other = greaterIndex == 0 && innerRuleRes.EndingPosition == 0;
                if ((innerRuleRes.EndingPosition > greaterIndex && innerRuleRes.Errors != null &&
                     !innerRuleRes.Errors.Any()) || other)
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

            SyntaxParseResult<IN> result = new SyntaxParseResult<IN>();
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
                greaterIndex = errors.Select(e => e.UnexpectedToken.PositionInTokenFlow).Max();
                result.EndingPosition = greaterIndex;
            }
            return result;
        }

        public virtual void  Init(ParserConfiguration<IN, OUT> configuration, string root)
        {
            if (root != null)
            {
                StartingNonTerminal = root;
            }
            this.InitializeStartingTokens(configuration, StartingNonTerminal);
        }

        #endregion

    }
}
