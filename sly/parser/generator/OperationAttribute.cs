using System;

namespace sly.parser.generator
{
    public enum Associativity
    {
        None = 0,
        Left = 1,
        Right = 2
    }


    public enum Affix
    {
        NotOperator = 0,
        PreFix = 1,
        InFix = 2,
        PostFix = 3
    }


    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class OperationAttribute : Attribute //where IN : struct
    {
        /// <summary>
        ///     token as an int as attribute can not be generics.
        /// </summary>
        /// <param name="intToken">token enum as int value</param>
        /// <param name="arity">operator arity</param>
        /// <param name="assoc">operator aosociativity (<see cref="Associativity" />) </param>
        /// <param name="precedence">precedence level: the greater, the higher</param>
        public OperationAttribute(int intToken, Affix affix, Associativity assoc, int precedence)
        {
            IntToken = intToken;
            IsIntToken = true;
            IsStringToken = false;
            Affix = affix;
            Assoc = assoc;
            Precedence = precedence;
        }
        
        public OperationAttribute(string stringToken, Affix affix, Associativity assoc, int precedence)
        {
            StringToken = stringToken;
            IsStringToken = true;
            IsIntToken = false;
            Affix = affix;
            Assoc = assoc;
            Precedence = precedence;
        }

        public bool IsIntToken { get; set; }
        
        public bool IsStringToken { get; set; }
        
        
        public int IntToken { get; set; }
        
        public string StringToken { get; set; }

        public Affix Affix { get; set; }

        public Associativity Assoc { get; set; }

        public int Precedence { get; set; }
    }
}