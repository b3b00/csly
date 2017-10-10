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

        public string Dump(string tab)
        {
            StringBuilder dmp = new StringBuilder();
            dmp.AppendLine($"{tab}(IF");

            dmp.AppendLine($"{tab+"\t"}(COND");
            dmp.AppendLine(Condition.Dump("\t\t" + tab));
            dmp.AppendLine($"{tab + "\t"})");

            dmp.AppendLine($"{tab + "\t"}(THEN");
            dmp.AppendLine(ThenStmt.Dump("\t\t" + tab));
            dmp.AppendLine($"{tab})");

            dmp.AppendLine($"{tab + "\t"}(THEN");
            dmp.AppendLine(ThenStmt.Dump("\t\t" + tab));
            dmp.AppendLine($"{tab + "\t"})");

            dmp.AppendLine($"{tab})");
            return dmp.ToString();
        }

    }
}
