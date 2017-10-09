using System;
using System.Collections.Generic;
using System.Text;

namespace csly.whileLang.model
{
    public class WhileStatement : Statement
    {

        public Expression Condition { get; set; }

        public Statement BlockStmt { get; set; }

        
        public WhileStatement(Expression condition, Statement blockStmt)
        {
            Condition = condition;
            BlockStmt = blockStmt;            
        }

    }
}
