using System;
using csly.whileLang.compiler;
using sly.lexer;
using Sigil;

namespace csly.whileLang.model
{
    public class IntegerConstant : Expression
    {
        public IntegerConstant(int value)
        {
            Value = value;
        }

        public int Value { get; set; }

        public LexerPosition Position { get; set; }

        public Scope CompilerScope { get; set; }


        public WhileType Whiletype
        {
            get => WhileType.INT;
            set { }
        }


        public string Dump(string tab)
        {
            return $"{tab}(INTEGER {Value})";
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