using lexer;
using parser.parsergenerator.syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace cpg.parser.parsgenerator.generator
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

    }
}
