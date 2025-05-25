using System.Collections.Generic;
using System.Linq;

namespace sly.parser.parser.llparser.ebnf.stackist.state;

public enum ExpressionRuleState
{
    NotStarted,
    Left,
    Operator,
    Right,
    Done
}