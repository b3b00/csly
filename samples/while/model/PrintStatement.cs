using csly.whileLang.compiler;
using sly.lexer;
using System;
using System.Collections.Generic;
using System.Text;
using Sigil;

namespace csly.whileLang.model
{
    public class PrintStatement : Statement
    {
        public Expression Value { get; set; }

        public PrintStatement(Expression value)
        {
            Value = value;
        }

        public Scope CompilerScope { get; set; }

        public TokenPosition Position { get; set; }

        public string Dump(string tab)
        {
            StringBuilder dmp = new StringBuilder();
            dmp.AppendLine($"{tab}(PRINT ");
            dmp.AppendLine($"{Value.Dump("\t" + tab)}");            
            dmp.AppendLine($"{tab})");
            return dmp.ToString();
        }

        public string Transpile(CompilerContext context)
        {
            
            return $"System.Console.WriteLine({Value.Transpile(context)});";
        }

        public Emit<Func<int>> EmitByteCode(CompilerContext context, Emit<Func<int>> emiter)
        {
            var mi = typeof(System.Console).GetMethod("WriteLine", new[] { typeof(string) });            

            emiter = Value.EmitByteCode(context, emiter);
            emiter.Call(mi);
            
            return emiter;
        }
    }
}
