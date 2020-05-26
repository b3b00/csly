using System;
using System.Text;
using csly.whileLang.compiler;
using sly.lexer;
using Sigil;

namespace csly.whileLang.model
{
    public class Not : Expression
    {
        public Not(Expression value)
        {
            Value = value;
        }

        public Expression Value { get; set; }

        public Scope CompilerScope { get; set; }

        public LexerPosition Position { get; set; }

        public WhileType Whiletype
        {
            get => WhileType.BOOL;
            set { }
        }


        public string Dump(string tab)
        {
            var dmp = new StringBuilder();
            dmp.AppendLine($"{tab}(NOT");
            dmp.AppendLine(Value.Dump(tab + "\t"));
            dmp.AppendLine($"{tab})");
            return dmp.ToString();
        }

        public string Transpile(CompilerContext context)
        {
            return $"! {Value.Transpile(context)}";
        }

        public Emit<Func<int>> EmitByteCode(CompilerContext context, Emit<Func<int>> emiter)
        {
            emiter = Value.EmitByteCode(context, emiter);
            emiter.Negate();
            return emiter;
        }
    }
}