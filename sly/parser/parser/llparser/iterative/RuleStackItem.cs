using System;
using System.Collections.Generic;
using System.Linq;
using sly.lexer;
using sly.parser.generator;
using sly.parser.syntax.grammar;
using sly.parser.syntax.tree;

namespace sly.parser.llparser.iterative
{
    public class RuleStackItem<IN> : IStackItem<IN> where IN : struct
    {
        
        /// <summary>
        /// position
        /// </summary>
        public int Position { get; set; }

        
        /// <summary>
        /// tokens
        /// </summary>
        public IList<Token<IN>> Tokens { get; set; }

        
        /// <summary>
        /// les différentes options pour une rule 
        /// </summary>
        public List<Rule<IN>> RulesTrials { get; set; }

        /// <summary>
        /// le non terminal de la rule
        /// </summary>
        public string NonTerminal { get; set; }

        /// <summary>
        /// le numéro de l'alternative courante (dans RuleTrials)
        /// </summary>
        public int RuleIndex { get; set; }

        /// <summary>
        /// ????
        /// </summary>
        public bool IsRuleTrial => true;

        /// <summary>
        /// 
        /// </summary>
        public bool IsClause => false;

        /// <summary>
        /// is this the root rule ?
        /// </summary>
        public bool IsRoot => !HasParent;

        public bool IsExhaustedRuleTrial => IsRuleTrial && RuleIndex == RulesTrials.Count - 1;

        
        public List<SyntaxParseResult<IN>> CurrentChildren => Children[RuleIndex];

        public Rule<IN> CurrentRule => RulesTrials[RuleIndex]; 

        public RuleStackItem<IN> Parent { get; set; }

        public bool HasParent => Parent != null;
        
        public bool HasBackTracked { get; set; }

        /// <summary>
        /// children ? 
        /// </summary>
        private List<List<SyntaxParseResult<IN>>> Children { get; set; }


        public bool IsDone => Children.Any() && Children[RuleIndex].Count == RulesTrials.Count;

        public bool IsSuccess => IsDone && Children[RuleIndex].All(x => x.IsOk);
            

        public RuleStackItem(RuleStackItem<IN> parent, NonTerminal<IN> nonTerminal, IList<Token<IN>> tokens, int position)
        {
            
            if (tokens == null)
            {
                Console.WriteLine($"RuleStackItem() :: tokens are null");
            }

            Tokens = tokens;
            Parent = parent;
            Children = new List<List<SyntaxParseResult<IN>>>();
            if (nonTerminal != null)
            {
                NonTerminal = nonTerminal.Name;
                RulesTrials = nonTerminal.Rules.Where(r => r.PossibleLeadingTokens.Any(x => x.Match(tokens[position])))
                    .ToList();
            }
            else
            {
                NonTerminal = "root";
            }

            RuleIndex = 0;
        }


        public void Shift(Stack<IStackItem<IN>> stack)
        {
            if (Tokens == null)
            {
                Console.WriteLine($"RuleStackItem.Shift() :: tokens are null");
            }

            RuleIndex++;
            if (RuleIndex >= RulesTrials.Count)
            {
                return;
            }
            var rule = RulesTrials[RuleIndex];
            stack.Push(this);
            for (int i = rule.Clauses.Count - 1; i >= 0; i--)
            {
                stack.Push(new ClauseStackItem<IN>(this, rule.Clauses[i],Tokens,Position));
            }
            
        }

        public void AddChild(SyntaxParseResult<IN> result)
        {
            if (RuleIndex >= Children.Count)
            {
                Children.Add(new List<SyntaxParseResult<IN>>());
            }
            Children[RuleIndex].Add(result);
        }
        
        
        public string Dump()
        {
            return $">>>{NonTerminal.ToString()} @ {RuleIndex} / {RulesTrials.Count}<<<";
        }

    }
}