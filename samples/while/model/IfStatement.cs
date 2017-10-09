using System;
using System.Collections.Generic;
using System.Text;

namespace csly.whileLang.model
{
    public class IfStatement : Statement
    {

        public Expression Condition { get; set; }

        public Statement ThenStmt { get; set; }

        public Statement ElseStmt { get; set; }

        public IfStatement(Expression condition, Statement thenStmt, Statement elseStmt)
        {
            Condition = condition;
            ThenStmt = thenStmt;
            ElseStmt = elseStmt;
        }

    }
}
