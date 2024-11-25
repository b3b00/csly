using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using sly.lexer;
using sly.parser.generator;

namespace sly.parser.syntax.grammar
{
    public class Rule<IN> : GrammarNode<IN> where IN : struct
    {
        public Rule()
        {
            Clauses = new List<IClause<IN>>();
            VisitorMethodsForOperation = new Dictionary<IN, OperationMetaData<IN>>();
            Visitor = null;
            IsSubRule = false;
            NodeName = "";
        }

        public string NodeName { get; set; } = null;
        
        public bool IsByPassRule { get; set; } = false;

        // visitors for operation rules
        private Dictionary<IN, OperationMetaData<IN>> VisitorMethodsForOperation { get; }

        // visitor for classical rules
        private MethodInfo Visitor { get; set; }

        public bool IsExpressionRule { get; set; }
        
        public bool IsInfixExpressionRule { get; set; }

        public Affix ExpressionAffix { get; set; }

        public string RuleString { get; set;  }

        public List<IClause<IN>> Clauses { get; set; }
        public List<LeadingToken<IN>> PossibleLeadingTokens { get; set; }

        public string NonTerminalName { get; set; }

        public bool ContainsSubRule
        {
            get
            {
                if (Clauses != null && Clauses.Any())
                {
                    bool contains = false;
                    foreach (var clause in Clauses)
                    {
                        switch (clause)
                        {
                            case GroupClause<IN> _:
                                contains = true;
                                break;
                            case ManyClause<IN> many:
                                contains  |=  many.Clause is GroupClause<IN>;
                                break;
                            case OptionClause<IN> option:
                                contains  |=  option.Clause is GroupClause<IN>;
                                break;
                        }

                        if (contains)
                        {
                            return true;
                        }
                        }
                    }

                return false;
            }
        }

        public bool IsSubRule { get; set; }

        public bool MayBeEmpty => Clauses == null
                                  || Clauses.Count == 0
                                  || Clauses.Count == 1 && Clauses[0].MayBeEmpty();


        public OperationMetaData<IN> GetOperation(IN token = default)
        {
            if (IsExpressionRule)
            {
                var operation = VisitorMethodsForOperation.TryGetValue(token, out var value)
                    ? value
                    : null;
                return operation;
            }

            return null;
        }
        
        public List<OperationMetaData<IN>> GetOperations()
        {
            if (IsExpressionRule)
            {
                return VisitorMethodsForOperation.Values.ToList();
            }

            return null;
        }

        public MethodInfo GetVisitor(IN token = default)
        {
            MethodInfo visitor = null;
            if (IsExpressionRule)
            {
                var operation = VisitorMethodsForOperation.TryGetValue(token, out var value)
                    ? value
                    : null;
                visitor = operation?.VisitorMethod;
            }
            else
            {
                visitor = Visitor;
            }

            return visitor;
        }

        public void SetVisitor(MethodInfo visitor)
        {
            Visitor = visitor;
        }

        public void SetVisitor(OperationMetaData<IN> operation)
        {
            VisitorMethodsForOperation[operation.OperatorToken] = operation;
        }

        
        
        public bool Match<OUT>(IList<Token<IN>> tokens, int position, ParserConfiguration<IN,OUT> configuration)
        {
            bool activateBroadWindow = configuration.BroadenTokenWindow;
            if (activateBroadWindow && Clauses.Count >= 2 && Clauses[0] is TerminalClause<IN> startingTerminalClause && Clauses[1] is NonTerminalClause<IN> nTerm)
            {
                if (startingTerminalClause.MayBeEmpty())
                {
                    return true;
                }
                if (!startingTerminalClause.MayBeEmpty() && startingTerminalClause.Check(tokens[position]))
                {
                    var secondPossibleLeadings =
                        configuration.NonTerminals[nTerm.NonTerminalName].GetPossibleLeadingTokens();
                    if (secondPossibleLeadings.Exists(x => x.Match(tokens[position+1])) || nTerm.MayBeEmpty())
                    {
                        return true;
                    }
                }

                

                return false;
            }

            int i = 0;
            bool match = false;
            var token = tokens[position];
            if (MayBeEmpty)
            {
                return true;
            }
            while (i < PossibleLeadingTokens.Count && !match)
            {
                var leader = PossibleLeadingTokens[i];
                match = leader.Match(token);
                i++;
            }
            return match;
        }

        [ExcludeFromCodeCoverage]
        public string Dump()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(NonTerminalName).Append(" : ");
            foreach (var clause in Clauses)
            {
                builder.Append(clause.Dump()).Append(" ");
            }

            return builder.ToString();
        }
    }
}