using System;

namespace sly.lexer
{
    [AttributeUsage(AttributeTargets.Enum, AllowMultiple = false)]
    public class CallBacksAttribute : Attribute
    {
        public Type CallBacksClass { get; set; }
        
        public CallBacksAttribute(Type callBacksClass)
        {
            CallBacksClass = callBacksClass;
        } 
            
    }
}