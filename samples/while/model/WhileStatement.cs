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

        public string Dump(string tab)
        {
            StringBuilder dmp = new StringBuilder();
            dmp.AppendLine($"{tab}(WHILE");

            dmp.AppendLine($"{tab + "\t"}(COND");
            dmp.AppendLine(Condition.Dump("\t\t" + tab));
            dmp.AppendLine($"{tab + "\t"})");

            dmp.AppendLine($"{tab + "\t"}(BLOCK");
            dmp.AppendLine(BlockStmt.Dump("\t\t" + tab));
            dmp.AppendLine($"{tab})");

            dmp.AppendLine($"{tab})");
            return dmp.ToString();
        }

    }
}
