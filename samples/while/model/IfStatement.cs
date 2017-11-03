using csly.whileLang.compiler;
using sly.lexer;
using System;
using System.Collections.Generic;
using System.Text;
using Sigil;

namespace csly.whileLang.model
{
    public class IfStatement : Statement
    {

        public Expression Condition { get; set; }

        public Statement ThenStmt { get; set; }

        public Statement ElseStmt { get; set; }


        public Scope CompilerScope { get; set; }

        public TokenPosition Position { get; set; }

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

            dmp.AppendLine($"{tab + "\t"}(ELSE");
            dmp.AppendLine(ElseStmt.Dump("\t\t" + tab));
            dmp.AppendLine($"{tab + "\t"})");

            dmp.AppendLine($"{tab})");
            return dmp.ToString();
        }

        public string Transpile(CompilerContext context)
        {
            StringBuilder code = new StringBuilder();
            code.AppendLine($"if({Condition.Transpile(context)}) {{ ");
            code.AppendLine(ThenStmt.Transpile(context));
            code.AppendLine("}");
            if (ElseStmt != null)
            {
                code.AppendLine("else {");
                code.AppendLine(ElseStmt.Transpile(context));
                code.AppendLine("}");
            }
            return code.ToString();
        }

        public Emit<Func<int>> EmitByteCode(CompilerContext context, Emit<Func<int>> emiter)
        {
            var thenLabel = emiter.DefineLabel();
            var elseLabel = emiter.DefineLabel();
            var endLabel = emiter.DefineLabel();
            Condition.EmitByteCode(context, emiter);
            emiter.BranchIfTrue(thenLabel);
            emiter.Branch(elseLabel);
            emiter.MarkLabel(thenLabel);
            ThenStmt.EmitByteCode(context, emiter);
            emiter.Branch(endLabel);
            emiter.MarkLabel(elseLabel);
            ElseStmt.EmitByteCode(context, emiter);
            emiter.Branch(endLabel);
            emiter.MarkLabel(endLabel);
            return emiter;
        }
    }
}
