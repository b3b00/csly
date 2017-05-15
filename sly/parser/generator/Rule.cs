using sly.lexer;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using sly.parser.syntax;

namespace sly.parser.generator
{

    public class Rule<T>
    {

        public string RuleString { get;  }

        public string NonTerminalName { get; set; }

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

        public List<IClause<T>> Clauses { get; set; }
        public List<T> PossibleLeadingTokens { get; set; }
        

        public bool IsEmpty { get
            {
                return Clauses == null
                    || Clauses.Count == 0
                    || (Clauses.Count == 1 && Clauses[0] is EmptyClause<T>);
            } }

    }
}
