using System;
using csly.whileLang.compiler;
using sly.lexer;
using Sigil;

namespace csly.whileLang.model
{
    public class BoolConstant : Expression
    {
        public BoolConstant(bool value)
        {
            Value = value;
        }

        public bool Value { get; set; }


        public Scope CompilerScope { get; set; }

        public LexerPosition Position { get; set; }

        public WhileType Whiletype
        {
            get => WhileType.BOOL;
            set { }
        }

        public string Dump(string tab)
        {
            return $"{tab}(BOOL {Value})";
        }

        public string Transpile(CompilerContext context)
        {
            return Value.ToString();
        }

        public Emit<Func<int>> EmitByteCode(CompilerContext context, Emit<Func<int>> emiter)
        {
            emiter.LoadConstant(Value);
            return emiter;
        }
    }
}