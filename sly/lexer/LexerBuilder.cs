using sly.buildresult;
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
            if (attributes.Count() > 0)
            {
                return (T)attributes?.ToArray()[0];
            }
            else
            {
                return default(T);
            }
        }
    }

    public class LexerBuilder
    {

        public static BuildResult<ILexer<IN>> BuildLexer<IN>(BuildResult<ILexer<IN>> result) where IN:struct
        {
           Type type = typeof(IN);
            TypeInfo typeInfo = type.GetTypeInfo();
            ILexer<IN> lexer = new Lexer<IN>();

            var values = Enum.GetValues(typeof(IN));
            
            var fields = typeof(IN).GetFields();
            foreach(Enum value in values)
            {
                IN tokenID = (IN)(object)value; 

                LexemeAttribute lexem = value.GetAttributeOfType<LexemeAttribute>();
                if (lexem != null)
                {
                    try
                    {
                        lexer.AddDefinition(new TokenDefinition<IN>(tokenID, lexem.Pattern, lexem.IsSkippable, lexem.IsLineEnding));
                    }
                    catch(Exception e)
                    {
                        result.AddError(new LexerInitializationError(ErrorLevel.ERROR, $"error at lexem {tokenID} : {e.Message}"));
                    }
                }
                else
                {
                    if (!tokenID.Equals(default(IN)))
                    {
                        result.AddError(new LexerInitializationError(ErrorLevel.WARN, $"token {tokenID} in lexer definition {typeof(IN).FullName} does not have Lexeme definition"));
                    }
                }
                ;
            }

            result.Result = lexer;

            return result;
        }

    }
}
