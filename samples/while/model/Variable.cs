using System;
using System.Collections.Generic;
using System.Text;
using sly.lexer;
using csly.whileLang.compiler;
using Sigil;

namespace csly.whileLang.model
{
    public class Variable : Expression
    {
        public string Name { get; }
        public Scope CompilerScope { get; set; }

        public TokenPosition Position { get; set; }

        public Variable(string name)
        {
            Name = name;
        }

        public string Dump(string tab)
        {
            return $"{tab}(VARIABLE {Name})";
        }

        public string Transpile(CompilerContext context)
        {
            return Name;
        }

        public Emit<Func<int>> EmitByteCode(CompilerContext context, Emit<Func<int>> emiter)
        {
            emiter.LoadLocal(emiter.Locals[Name]);
            return emiter;
        }
    }

}
