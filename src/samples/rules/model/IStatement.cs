using System.Collections.Generic;

namespace rules.model
{
    public interface IStatement : IRule
    {
        List<RuleComment> PrecedingComments { get; set; }
    }
}
