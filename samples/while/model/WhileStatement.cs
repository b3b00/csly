using csly.whileLang.compiler;
using sly.lexer;
using System;
using System.Collections.Generic;
using System.Text;
using Sigil;

namespace csly.whileLang.model
{
    public class WhileStatement : Statement
    {

        public Expression Condition { get; set; }

        public Statement BlockStmt { get; set; }

        public Scope CompilerScope { get; set; }

        public TokenPosition Position { get; set; }

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

        public string Transpile(CompilerContext context)
        {
            StringBuilder code = new StringBuilder();
            code.AppendLine($"while({Condition.Transpile(context)}) {{ ");
            code.AppendLine(BlockStmt.Transpile(context));
            code.AppendLine("}");            
            return code.ToString();
        }

        public Emit<Func<int>> EmitByteCode(CompilerContext context, Emit<Func<int>> emiter)
        {
            var loopLabel = emiter.DefineLabel();
            var outLabel = emiter.DefineLabel();
            
            emiter.MarkLabel(loopLabel);
            Condition.EmitByteCode(context, emiter);
            emiter.BranchIfFalse(outLabel);            
            BlockStmt.EmitByteCode(context, emiter);            
            emiter.Branch(loopLabel);
            emiter.MarkLabel(outLabel);
            return emiter;
        }
    }
}
