using System;
using System.Collections.Generic;
using System.Text;

namespace csly.whileLang.model
{
    class SequenceStatement : Statement
    {

        List<Statement> Statements { get; set; }

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
