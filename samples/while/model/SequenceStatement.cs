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

    }
}
