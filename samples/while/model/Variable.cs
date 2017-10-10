using System;
using System.Collections.Generic;
using System.Text;

namespace csly.whileLang.model
{
    public class Variable : Expression
    {
        public string Name { get; }

        public Variable(string name)
        {
            Name = name;
        }

        public string Dump(string tab)
        {
            return $"{tab}(VARIABLE {Name})";
        }

    }

}
