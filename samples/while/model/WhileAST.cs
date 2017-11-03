using csly.whileLang.compiler;
using sly.lexer;
using System;
using System.Collections.Generic;
using System.Text;
using Sigil;

namespace csly.whileLang.model
{
    public interface WhileAST
    {
        string Dump(string tab);

        TokenPosition Position { get; set; }

        Scope CompilerScope { get; set; }

        string Transpile(CompilerContext context);
        Emit<Func<int>> EmitByteCode(CompilerContext context, Emit<Func<int>> emiter);
    }
}
