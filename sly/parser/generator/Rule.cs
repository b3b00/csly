using sly.lexer;
using System;
using System.Collections.Generic;
using System.Text;
using sly.parser.syntax;

namespace sly.parser.generator
{

    public class Functions
    {
        public delegate object ReductionFunction(List<object> reductedClauses);
    }

    public class Rule<T>
    {

        public string RuleString { get; set; }
        public string Key { get; set; }

        public List<Clause<T>> Clauses { get; set; }
        public List<T> PossibleLeadingTokens { get; set; }

        public Functions.ReductionFunction Function { get; set; } 

        public bool IsEmpty { get
            {
                return Clauses == null
                    || Clauses.Count == 0
                    || (Clauses.Count == 1 && Clauses[0] is EmptyClause<T>);
            } }

    }
}
