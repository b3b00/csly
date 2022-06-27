using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        }

        public bool IsByPassRule { get; set; } = false;

        // visitors for operation rules
        private Dictionary<IN, OperationMetaData<IN>> VisitorMethodsForOperation { get; }

        // visitor for classical rules
        private MethodInfo Visitor { get; set; }

        public bool IsExpressionRule { get; set; }
        
        public bool IsInfixExpressionRule { get; set; }

        public Affix ExpressionAffix { get; set; }

        public string RuleString { get; set;  }

        public string Key
        {
            get
            {
                var key = string.Join("_", Clauses.Select<IClause<IN>, string>(c => c.ToString()));
                
                if (Clauses.Count == 1) 
                    key += "_";

                return IsExpressionRule ? key.Replace(" | ", "_") : key;
            }
        }

        public List<IClause<IN>> Clauses { get; set; }
        public List<LeadingToken<IN>> PossibleLeadingTokens { get; set; }

        public string NonTerminalName { get; set; }

        public bool ContainsSubRule
        {
            get
            {
                if (Clauses != null && Clauses.Any<IClause<IN>>())
                    foreach (var clause in Clauses)
                    {
                        switch (clause)
                        {
                            case GroupClause<IN> _:
                                return true;
                            case ManyClause<IN> many:
                                return many.Clause is GroupClause<IN>;
                            case OptionClause<IN> option:
                                return option.Clause is GroupClause<IN>;
                        }
                    }

                return false;
            }
        }

        public bool IsSubRule { get; set; }

        public bool MayBeEmpty => Clauses == null
                                  || Clauses.Count == 0
                                  || Clauses.Count == 1 && Clauses[0].MayBeEmpty();


        public OperationMetaData<IN> GetOperation(IN token = default(IN))
        {
            if (IsExpressionRule)
            {
                var operation = VisitorMethodsForOperation.ContainsKey(token)
                    ? VisitorMethodsForOperation[token]
                    : null;
                return operation;
            }

            return null;
        }
        
        public List<OperationMetaData<IN>> GetOperations()
        {
            if (IsExpressionRule)
            {
                return VisitorMethodsForOperation.Values.ToList<OperationMetaData<IN>>();
            }

            return null;
        }

        public MethodInfo GetVisitor(IN token = default(IN))
        {
            MethodInfo visitor = null;
            if (IsExpressionRule)
            {
                var operation = VisitorMethodsForOperation.ContainsKey(token)
                    ? VisitorMethodsForOperation[token]
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
    }
}