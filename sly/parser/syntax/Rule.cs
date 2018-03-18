using sly.lexer;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using sly.parser.syntax;
using System.Reflection;
using sly.parser.generator;

namespace sly.parser.syntax
{

    public class Rule<IN> : GrammarNode<IN> where IN : struct
    {

        public bool IsByPassRule { get; set; } = false;

        // visitors for operation rules
        private Dictionary<IN, OperationMetaData<IN>> VisitorMethodsForOperation { get; set; }

        // visitor for classical rules
        private MethodInfo Visitor { get; set; } 
        
        public bool IsExpressionRule { get; set; }

        public string RuleString { get; }

        public string NonTerminalName { get; set; }


        public OperationMetaData<IN> GetOperation(IN token = default(IN))
        {
            if (IsExpressionRule)
            {
                OperationMetaData<IN> operation = VisitorMethodsForOperation.ContainsKey(token) ? VisitorMethodsForOperation[token] : null;
                return operation;
            }
            return null;
        }

        public MethodInfo GetVisitor(IN token = default(IN))
        {
            MethodInfo visitor = null;
            if (IsExpressionRule)
            {
                OperationMetaData < IN > operation = VisitorMethodsForOperation.ContainsKey(token) ? VisitorMethodsForOperation[token] : null;
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

        public string Key
        {

            get
            {
                string k = Clauses
                    .Select(c => c.ToString())
                    .Aggregate<string>((c1, c2) => c1.ToString() + "_" + c2.ToString());
                if (Clauses.Count == 1)
                {
                    k += "_";
                }
                return k;
            }
        }

        public List<IClause<IN>> Clauses { get; set; }
        public List<IN> PossibleLeadingTokens { get; set; }


        public Rule()
        {
            Clauses = new List<IClause<IN>>();
            VisitorMethodsForOperation = new Dictionary<IN, OperationMetaData<IN>>();
            Visitor = null;
        }

        public bool MayBeEmpty { get
            {
                return Clauses == null
                    || Clauses.Count == 0
                    || (Clauses.Count == 1 && Clauses[0].MayBeEmpty());
            } }

        

    }
}
