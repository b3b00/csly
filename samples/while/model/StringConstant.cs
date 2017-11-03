using csly.whileLang.compiler;
using sly.lexer;
using System;
using System.Collections.Generic;
using System.Text;
using Sigil;

namespace csly.whileLang.model
{
    public class StringConstant : Expression
    {

        public string Value { get; set; }

        public Scope CompilerScope { get; set; }

        public TokenPosition Position { get; set; }

        public StringConstant(string value)
        {
            Value = value;
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
