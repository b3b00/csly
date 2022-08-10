using System.Collections.Generic;
using System.Linq;
using sly.lexer;
using sly.parser.generator;
using sly.parser.syntax.grammar;

namespace sly.parser.llparser.iterative
{
    public class StackItem<IN> where IN : struct
    {
        public int Position { get; set; }
        
        public IList<Token<IN>> Tokens { get; set; }
        
        public IClause<IN> Clause { get; set; }
        
        public List<Rule<IN>> RulesTrials { get; set; }
        
        public string NonTerminal { get; set; }
        
        public int RuleIndex { get; set; }

        public bool IsRuleTrial => !string.IsNullOrEmpty(NonTerminal);

        public bool IsClause => Clause != null;


        public StackItem(IClause<IN> clause, IList<Token<IN>> tokens, int position)
        {
            Clause = clause;
            Position = position;
            Tokens = tokens;
        }

        public StackItem(NonTerminal<IN> nonTerminal, IList<Token<IN>> tokens, int position)
        {
            NonTerminal = nonTerminal.Name;
            RulesTrials = nonTerminal.Rules.Where(r => r.PossibleLeadingTokens.Any(x => x.Match(tokens[position]))).ToList();
            RuleIndex = 0;
            Clause = null;
        }

        public string Dump()
        {
            if (IsClause)
            {
                return Clause.Dump();
            }
            else
            {
                return $">>>{NonTerminal.ToString()} @ {RuleIndex} / {RulesTrials.Count}<<<";
            }
        }

    }
}