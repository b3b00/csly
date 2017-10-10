using System;
using System.Collections.Generic;
using System.Text;

namespace csly.whileLang.model
{
    public class SequenceStatement : Statement
    {

        List<Statement> Statements { get; set; }
        public int Count => Statements.Count; 

        public SequenceStatement()
        {
            this.Statements = new List<Statement>();
        }

        public SequenceStatement(Statement statement)
        {
            this.Statements = new List<Statement>() { statement };
        }

        public SequenceStatement(List<Statement> seq)
        {
            Statements = seq;
        }

        public Statement Get(int i)
        {
            return Statements[i];
        }

        public void Add(Statement statement)
        {
            Statements.Add(statement);
        }

        public void AddRange(List<Statement> stmts)
        {
            Statements.AddRange(stmts);
        }

        public string Dump(string tab)
        {
            StringBuilder dump = new StringBuilder();
            dump.AppendLine($"{tab}(SEQUENCE [");
            Statements.ForEach(c => dump.AppendLine($"{c.Dump(tab + "\t")},"));
            dump.AppendLine($"{tab}] )");
            return dump.ToString();
        }

    }
}
