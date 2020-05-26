using System;
using System.Collections.Generic;
using System.Text;
using csly.whileLang.compiler;
using sly.lexer;
using Sigil;

namespace csly.whileLang.model
{
    public class SequenceStatement : Statement
    {
        public SequenceStatement()
        {
            Statements = new List<Statement>();
        }

        public SequenceStatement(Statement statement)
        {
            Statements = new List<Statement> {statement};
        }

        public SequenceStatement(List<Statement> seq)
        {
            Statements = seq;
        }

        private List<Statement> Statements { get; }
        public int Count => Statements.Count;

        public Scope CompilerScope { get; set; }

        public LexerPosition Position { get; set; }

        public string Dump(string tab)
        {
            var dump = new StringBuilder();
            dump.AppendLine($"{tab}(SEQUENCE [");
            Statements.ForEach(c => dump.AppendLine($"{c.Dump(tab + "\t")},"));
            dump.AppendLine($"{tab}] )");
            return dump.ToString();
        }

        public string Transpile(CompilerContext context)
        {
            var block = new StringBuilder("{\n");
            foreach (var stmt in Statements)
            {
                block.Append(stmt.Transpile(context));
                block.AppendLine(";");
            }

            block.AppendLine("}");
            return block.ToString();
        }

        public Emit<Func<int>> EmitByteCode(CompilerContext context, Emit<Func<int>> emiter)
        {
            foreach (var stmt in Statements) emiter = stmt.EmitByteCode(context, emiter);
            return emiter;
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