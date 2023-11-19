using System;
using csly.whileLang.compiler;
using sly.lexer;
using Sigil;

namespace csly.whileLang.model
{
    public class Variable : Expression
    {
        public Variable(string name)
        {
            Name = name;
        }

        public string Name { get; }
        public Scope CompilerScope { get; set; }

        public LexerPosition Position { get; set; }

        public WhileType Whiletype { get; set; }

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