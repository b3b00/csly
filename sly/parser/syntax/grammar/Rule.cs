using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using sly.parser.generator;
using sly.parser.parser;

namespace sly.parser.syntax.grammar
{

    public delegate OUT CallVisitor<OUT>(object instance, object[] parameters);
    
    public class Rule<IN,OUT> : GrammarNode<IN,OUT> where IN : struct
    {
        public Rule()
        {
            Clauses = new List<IClause<IN,OUT>>();
            VisitorMethodsForOperation = new Dictionary<IN, OperationMetaData<IN,OUT>>();
            Visitor = null;
            IsSubRule = false;
        }

        public bool IsByPassRule { get; set; } = false;

        // visitors for operation rules
        private Dictionary<IN, OperationMetaData<IN,OUT>> VisitorMethodsForOperation { get; }

        // visitor for classical rules
        private MethodInfo Visitor { get; set; }
        
        private CallVisitor<OUT> VisitorCaller { get; set; }
        
         

        public bool IsExpressionRule { get; set; }
        
        public bool IsInfixExpressionRule { get; set; }

        public Affix ExpressionAffix { get; set; }

        public string RuleString { get; set;  }

        public string Key
        {
            get
            {
                var key = string.Join("_", Clauses.Select(c => c.ToString()));
                
                if (Clauses.Count == 1) 
                    key += "_";

                return IsExpressionRule ? key.Replace(" | ", "_") : key;
            }
        }

        public List<IClause<IN,OUT>> Clauses { get; set; }
        public List<IN> PossibleLeadingTokens { get; set; }

        public string NonTerminalName { get; set; }

        public bool ContainsSubRule
        {
            get
            {
                if (Clauses != null && Clauses.Any())
                    foreach (var clause in Clauses)
                    {
                        if (clause is GroupClause<IN,OUT>) return true;
                        if (clause is ManyClause<IN,OUT> many) return many.Clause is GroupClause<IN,OUT>;
                        if (clause is OptionClause<IN,OUT> option) return option.Clause is GroupClause<IN,OUT>;
                    }

                return false;
            }
        }

        public bool IsSubRule { get; set; }

        public bool MayBeEmpty => Clauses == null
                                  || Clauses.Count == 0
                                  || Clauses.Count == 1 && Clauses[0].MayBeEmpty();


        public OperationMetaData<IN,OUT> GetOperation(IN token = default(IN))
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
        
        public List<OperationMetaData<IN,OUT>> GetOperations()
        {
            if (IsExpressionRule)
            {
                return VisitorMethodsForOperation.Values.ToList();
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
        
        public CallVisitor<OUT> GetVisitorCaller(IN token = default(IN))
        {
            CallVisitor<OUT> caller = null;
            if (IsExpressionRule)
            {
                var operation = VisitorMethodsForOperation.ContainsKey(token)
                    ? VisitorMethodsForOperation[token]
                    : null;
                caller = operation?.VisitorCaller;
            }
            else
            {
                caller = VisitorCaller;
            }

            return caller;
        }

        public void SetVisitor(MethodInfo visitor, object parserInstance)
        {
            Visitor = visitor;
            VisitorCaller = VisitorCallerBuilder.BuildLambda<OUT>(parserInstance, visitor); // TODO : set instance !
            ;
        }

        public void SetVisitor(OperationMetaData<IN,OUT> operation)
        {
            VisitorMethodsForOperation[operation.OperatorToken] = operation;
        }
    }
}