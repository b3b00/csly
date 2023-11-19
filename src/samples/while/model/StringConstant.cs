using System;
using csly.whileLang.compiler;
using sly.lexer;
using Sigil;

namespace csly.whileLang.model
{
    public class StringConstant : Expression
    {
        public StringConstant(string value)
        {
            Value = value;
        }

        public string Value { get; set; }

        public Scope CompilerScope { get; set; }

        public LexerPosition Position { get; set; }

        public WhileType Whiletype
        {
            get => WhileType.STRING;
            set { }
        }

        public string Dump(string tab)
        {
            return $"{tab}(STRING {Value})";
        }

        public string Transpile(CompilerContext context)
        {
            return $"\"{Value}\"";
        }

        public Emit<Func<int>> EmitByteCode(CompilerContext context, Emit<Func<int>> emiter)
        {
            emiter.LoadConstant(Value);
            return emiter;
        }
    }
}