using csly.whileLang.compiler;
using sly.lexer;
using System;
using System.Collections.Generic;
using System.Text;
using Sigil;

namespace csly.whileLang.model
{
    public class Neg : Expression
    {
        public Expression Value { get; set; }

        public Scope CompilerScope { get; set; }

        public TokenPosition Position { get; set; }

        public Neg(Expression value)
        {
            Value = value;
        }

        public WhileType Whiletype { get { return WhileType.INT; } set { } }

        public string Dump(string tab)
        {
            StringBuilder dmp = new StringBuilder();
            dmp.AppendLine($"{tab}(NEG");
            dmp.AppendLine(Value.Dump(tab + "\t"));
            dmp.AppendLine($"{tab})");
            return dmp.ToString();
        }

        public string Transpile(CompilerContext context)
        {
            return $"- {Value.Transpile(context)}";
        }

        public Emit<Func<int>> EmitByteCode(CompilerContext context, Emit<Func<int>> emiter)
        {
            emiter = Value.EmitByteCode(context, emiter);
            emiter.Negate();
            return emiter;
        }
    }
}
