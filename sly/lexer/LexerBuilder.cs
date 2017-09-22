using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace sly.lexer
{

    public static class EnumHelper
    {
        /// <summary>
        /// Gets an attribute on an enum field value
        /// </summary>
        /// <typeparam name="T">The type of the attribute you want to retrieve</typeparam>
        /// <param name="enumVal">The enum value</param>
        /// <returns>The attribute of type T that exists on the enum value</returns>
        /// <example>string desc = myEnumVariable.GetAttributeOfType<DescriptionAttribute>().Description;</example>
        public static T GetAttributeOfType<T>(this Enum enumVal) where T : System.Attribute
        {
            var type = enumVal.GetType();
            var memInfo = type.GetMember(enumVal.ToString());
            IEnumerable<Attribute> attributes = (IEnumerable<Attribute>)(memInfo[0].GetCustomAttributes(typeof(T), false));

            return (T)attributes?.ToArray()[0];
        }
    }

    public class LexerBuilder
    {

        public static ILexer<T> BuildLexer<T>() where T:struct
        {
            Type type = typeof(T);
            TypeInfo typeInfo = type.GetTypeInfo();
            ILexer<T> lexer = new Lexer<T>();

            var values = Enum.GetValues(typeof(T));
            
            var fields = typeof(T).GetFields();
            foreach(Enum value in values)
            {
                T tokenID = (T)(object)value; 

                LexemeAttribute lexem = value.GetAttributeOfType<LexemeAttribute>();
                if (lexem != null)
                {
                    lexer.AddDefinition(new TokenDefinition<T>(tokenID, lexem.Pattern, lexem.IsSkippable, lexem.IsLineEnding));
                }
                ;
            }
            
            return lexer;
        }

    }
}
