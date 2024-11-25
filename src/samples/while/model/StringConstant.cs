using System;
using System.Collections.Generic;
using csly.whileLang.compiler;
using sly.lexer;
using Sigil;

namespace csly.whileLang.model
{
    public class StringConstant : Expression
    {
        public StringConstant(string value, LexerPosition position)
        {
            Value = value;
            Position = position;
        }

        public string Value { get; set; }

        public Scope CompilerScope { get; set; }

        public LexerPosition Position { get; set; }

        public WhileType Whiletype
        {
            get => WhileType.STRING;
            set { }
        }

        public void AppendTernaries(List<TernaryExpression> ternaries)
        {
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