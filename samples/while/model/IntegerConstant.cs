using csly.whileLang.compiler;
using sly.lexer;
using System;
using System.Collections.Generic;
using System.Text;
using Sigil;

namespace csly.whileLang.model
{
    public class IntegerConstant : Expression
    {
        public int Value { get; set; }

        public TokenPosition Position { get; set; }

        public Scope CompilerScope { get; set; }


        public WhileType Whiletype { get { return WhileType.INT; } set { } }

        public IntegerConstant(int value)
        {
            Value = value;
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
