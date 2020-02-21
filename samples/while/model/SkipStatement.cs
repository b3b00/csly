using System;
using csly.whileLang.compiler;
using sly.lexer;
using Sigil;

namespace csly.whileLang.model
{
    public sealed class SkipStatement : Statement
    {
        public Scope CompilerScope { get; set; }

        public LexerPosition Position { get; set; }

        public string Dump(string tab)
        {
            return $"{tab}(SKIP)";
        }

        public Emit<Func<int>> EmitByteCode(CompilerContext context, Emit<Func<int>> emiter)
        {
            emiter.Nop();
            return emiter;
        }

        public string Transpile(CompilerContext context)
        {
            return ";";
        }
    }
}