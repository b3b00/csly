using csly.whileLang.compiler;
using System;
using System.Collections.Generic;
using System.Text;

namespace csly.whileLang.model
{
    public interface Expression : WhileAST
    {
            WhileType Whiletype { get; set; }
    }
}
