using System;
using System.Collections.Generic;
using System.Text;
using csly.whileLang.compiler;
using sly.lexer;
using Sigil;

namespace csly.whileLang.model
{
    public class Printer
    {
        public static List<string> lines = new List<string>();

        public static void WriteLine(string line)
        {
            lines.Add(line);
        }

        public static void Clear()
        {
            lines.Clear();
        }
    }
    
    public class PrintStatement : Statement
    {
        public PrintStatement(Expression value)
        {
            Value = value;
        }

        public Expression Value { get; set; }

        public Scope CompilerScope { get; set; }

        public LexerPosition Position { get; set; }

        public string Dump(string tab)
        {
            var dmp = new StringBuilder();
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
            if (!context.IsQuiet)
            {
                var mi = typeof(Console).GetMethod("WriteLine", new[] { typeof(string) });
                emiter = Value.EmitByteCode(context, emiter);
                emiter.Call(mi);
            }
            else
            {
                var mi = typeof(Printer).GetMethod("WriteLine", new[] { typeof(string) });
                emiter = Value.EmitByteCode(context, emiter);
                emiter.Call(mi);
            }

            return emiter;
        }

        public void AppendTernaries(List<TernaryExpression> ternaries)
        {
            Value.AppendTernaries(ternaries);
        }
    }
}