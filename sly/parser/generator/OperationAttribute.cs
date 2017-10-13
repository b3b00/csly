using System;
using System.Collections.Generic;
using System.Text;

namespace sly.parser.generator
{

    public enum Associativity
    {
        Left = 1,
        Right = 2
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class OperationAttribute : Attribute //where IN : struct
    {

        public int Token { get; set; }

        public int Arity { get; set; }

        public Associativity Assoc { get; set; }

        public int Precedence { get; set; }


        public OperationAttribute(int token, int arity, Associativity assoc, int precedence)        {
            Token = token;
            Arity = arity;
            Assoc = assoc;
            Precedence = precedence;
        }
    }
}
