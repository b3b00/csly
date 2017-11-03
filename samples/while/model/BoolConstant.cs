using csly.whileLang.compiler;
using sly.lexer;
using System;
using System.Collections.Generic;
using System.Text;
using Sigil;

namespace csly.whileLang.model
{
    public class BoolConstant : Expression
    {

        public bool Value { get; set; }


        public Scope CompilerScope { get; set; }

        public TokenPosition Position { get; set; }
       

        public BoolConstant(bool value)
        {
            Value = value;
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
