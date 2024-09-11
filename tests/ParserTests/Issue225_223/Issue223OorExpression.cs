using System.Collections.Generic;

namespace ParserTests.Issue225_IndexOutOfRangeException
{
    public abstract class Issue223OorExpression
    {
        public virtual bool IsRange => false;
        public abstract IEnumerable<Issue223OorExpression> Children { get; }
        
        public abstract string ToQuery();
    }
}