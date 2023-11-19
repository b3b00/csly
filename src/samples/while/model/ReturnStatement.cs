using System;
using System.Text;
using csly.whileLang.compiler;
using sly.lexer;
using Sigil;

namespace csly.whileLang.model
{
    public class ReturnStatement : Statement
    {
        public ReturnStatement(Expression value)
        {
            Value = value;
        }

        public Expression Value { get; set; }

        public Scope CompilerScope { get; set; }

        public LexerPosition Position { get; set; }

        public string Dump(string tab)
        {
            var dmp = new StringBuilder();
            dmp.AppendLine($"{tab}(RETURN ");
            dmp.AppendLine($"{Value.Dump("\t" + tab)}");
            dmp.AppendLine($"{tab})");
            return dmp.ToString();
        }

        public string Transpile(CompilerContext context)
        {
            return $"return {Value.Transpile(context)};";
        }

        public Emit<Func<int>> EmitByteCode(CompilerContext context, Emit<Func<int>> emiter)
        {
            emiter = Value.EmitByteCode(context, emiter);
            emiter.Return();
            return emiter;
        }
    }
}