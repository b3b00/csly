using cpg.parser.parsgenerator.generator;
using cpg.parser.parsgenerator.syntax;
using lexer;
using parser.parsergenerator.generator;
using parser.parsergenerator.parser;
using parser.parsergenerator.syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cpg.parser.parsgenerator.parser.llparser
{

    class ParserState<T>
    {

        int CurrentTokenPosition { get; set; }

        IList<Token<T>> Tokens { get; set; }

        Rule<T> Rule { get; set; }

        public ParserState(int position, Rule<T> rule, IList<Token<T>> tokens)
        {
            this.CurrentTokenPosition = position;
            this.Rule = rule;
            this.Tokens = tokens;
        }


    }

    public class LLSyntaxParser<T> : ISyntaxParser<T>
    {
        public ParserConfiguration<T> Configuration { get; set; }

        public string StartingNonTerminal { get; set; }

        public LLSyntaxParser(ParserConfiguration<T> configuration, string startingNonTerminal)
        {
            Configuration = configuration;
            StartingNonTerminal = startingNonTerminal;
        }

        public SyntaxParseResult<T> Parse(IList<Token<T>> tokens)
        {
            Dictionary<string, NonTerminal<T>> NonTerminals = Configuration.NonTerminals;

            Stack<ParserState<T>> stack = new Stack<ParserState<T>>();

            NonTerminal<T> nt = NonTerminals[StartingNonTerminal];

            List<Rule<T>> rules = nt.Rules.Where<Rule<T>>(r => r.PossibleLeadingTokens.Contains(tokens[0].TokenID)).ToList<Rule<T>>();

            List<ParserState<T>> branches = rules.Select(r => new ParserState<T>(0, r, tokens)).ToList<ParserState<T>>();
            branches.Reverse();

            foreach (Rule<T> rule in rules)
            {
                SyntaxParseResult<T> r = Parse(tokens, rule, 0);
                if (!r.IsError)
                {
                    return r;
                }
            }

            return null;

        }


        public SyntaxParseResult<T> Parse(IList<Token<T>> tokens, Rule<T> rule, int position)
        {
            int currentPosition = position;
            bool isError = false;
            List<IConcreteSyntaxNode<T>> children = new List<IConcreteSyntaxNode<T>>();
            if (rule.PossibleLeadingTokens.Contains(tokens[position].TokenID))
            {
                if (rule.Clauses != null && rule.Clauses.Count > 0)
                {
                    foreach (Clause<T> clause in rule.Clauses)
                    {
                        if (clause is TerminalClause<T>)
                        {
                            SyntaxParseResult<T> termRes = ParseTerminal(tokens, clause as TerminalClause<T>, currentPosition);
                            if (!termRes.IsError)
                            {
                                children.Add(termRes.Root);
                                currentPosition = termRes.EndingPosition;
                            }
                            isError = isError && termRes.IsError;
                        }
                        else if (clause is NonTerminalClause<T>)
                        {
                            NonTerminalClause<T> nonTermClause = clause as NonTerminalClause<T>;
                            NonTerminal<T> nt = Configuration.NonTerminals[nonTermClause.NonTerminalName];
                            bool found = false;
                            int i = 0;

                            List<Rule<T>> rules = nt.Rules.Where<Rule<T>>(r => r.PossibleLeadingTokens.Contains(tokens[0].TokenID)).ToList<Rule<T>>();

                            while (!found && i < rules.Count)
                            {
                                Rule<T> innerrule = rules[i];
                                SyntaxParseResult<T> innerRuleRes = Parse(tokens, innerrule, currentPosition);                                
                                if(!innerRuleRes.IsError)
                                {
                                    children.Add(innerRuleRes.Root);
                                }
                                isError = isError && innerRuleRes.IsError;
                                currentPosition = innerRuleRes.EndingPosition;
                                i++;
                            }
                        }
                        if (isError)
                        {
                            break;
                        }
                    }
                }
            }

            SyntaxParseResult<T> result = new SyntaxParseResult<T>();
            result.IsError = isError;
            if (!isError) {
                ConcreteSyntaxNode<T> node = new ConcreteSyntaxNode<T>(rule.Key, children);
                result.Root = node;
                result.EndingPosition = currentPosition;                
            }
            

            return result;
        }

        public SyntaxParseResult<T> ParseTerminal(IList<Token<T>> tokens, TerminalClause<T> term, int position)
        {
            SyntaxParseResult<T> result = new SyntaxParseResult<T>();
            result.IsError = !term.Check(tokens[position].TokenID);
            result.EndingPosition = !result.IsError ? position + 1 : position;
            result.Root = new ConcreteSyntaxLeaf<T>(tokens[position]);
            return result;
        }






    }
}
