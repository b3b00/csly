using System;
using sly.lexer;
using sly.lexer.fsm;

namespace sly.sourceGenerator
{

    
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class ParserGeneratorAttribute : System.Attribute 
    {
        public ParserGeneratorAttribute() {
        }
    }
}