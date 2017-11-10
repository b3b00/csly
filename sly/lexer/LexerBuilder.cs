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

        public static BuildResult<ILexer<IN>> BuildLexer<IN>(BuildResult<ILexer<IN>> result) where IN : struct
        {
            Type type = typeof(IN);
            TypeInfo typeInfo = type.GetTypeInfo();
            ILexer<IN> lexer = new Lexer<IN>();

            var values = Enum.GetValues(typeof(IN));

            var attributes = new Dictionary<IN, LexemeAttribute>();

            var fields = typeof(IN).GetFields();
            foreach (Enum value in values)
            {
                IN tokenID = (IN)(object)value;

                LexemeAttribute lexem = value.GetAttributeOfType<LexemeAttribute>();
                if (lexem != null)
                {
                    attributes[tokenID] = lexem;
                }
                else
                {
                    if (!tokenID.Equals(default(IN)))
                    {
                        result.AddError(new LexerInitializationError(ErrorLevel.WARN, $"token {tokenID} in lexer definition {typeof(IN).FullName} does not have"));
                    }
                }
                ;
            }

            result = Build(attributes,result);

            return result;
        }


        private static BuildResult<ILexer<IN>> Build<IN>(Dictionary<IN, LexemeAttribute> attributes, BuildResult<ILexer<IN>> result) where IN : struct
        {
            bool hasRegexLexem = attributes.Values.FirstOrDefault(a => !string.IsNullOrEmpty(a.Pattern)) != null;
            bool hasGenericLexem = attributes.Values.FirstOrDefault(a => a.GenericToken != default(GenericToken)) != null;

            if (hasGenericLexem && hasRegexLexem)
            {
                result.AddError(new LexerInitializationError(ErrorLevel.WARN, $"cannot mix Regex lexemes and Generic lexemes in same lexer"));
                result.IsError = true;
            }
            else
            {
                if (hasRegexLexem)
                {
                    result = BuildRegexLexer<IN>(attributes, result);
                }
                else if (hasGenericLexem)
                {
                    result = BuildGenericLexer<IN>(attributes, result);
                }
            }
            return result;
        }


        private static BuildResult<ILexer<IN>> BuildRegexLexer<IN>(Dictionary<IN, LexemeAttribute> attributes, BuildResult<ILexer<IN>> result) where IN : struct
        {
            ILexer<IN> lexer = new Lexer<IN>();
            foreach (KeyValuePair<IN, LexemeAttribute> pair in attributes)
            {
                IN tokenID = pair.Key;

                LexemeAttribute lexem = pair.Value;

                if (lexem != null)
                {
                    lexer.AddDefinition(new TokenDefinition<IN>(tokenID, lexem.Pattern, lexem.IsSkippable, lexem.IsLineEnding));
                }
                else
                {
                    if (!tokenID.Equals(default(IN)))
                    {
                        result.AddError(new LexerInitializationError(ErrorLevel.WARN, $"token {tokenID} in lexer definition {typeof(IN).FullName} does not have"));
                    }
                }
                ;
            }

            result.Result = lexer;
            return result;
        }

        private static BuildResult<ILexer<IN>> BuildGenericLexer<IN>(Dictionary<IN, LexemeAttribute> attributes, BuildResult<ILexer<IN>> result) where IN : struct
        {
            List<GenericToken> statics = attributes.Values.Select(a => a.GenericToken).ToList();

            GenericLexer<IN> lexer = new GenericLexer<IN>(EOLType.Environment, statics.ToArray());
            foreach (KeyValuePair<IN, LexemeAttribute> pair in attributes)
            {
                IN tokenID = pair.Key;

                LexemeAttribute lexem = pair.Value;
                if (lexem.IsStaticGeneric)
                {
                    lexer.AddLexeme(lexem.GenericToken, tokenID);
                }
                if (lexem.IsKeyWord)
                {
                    lexer.AddKeyWord(tokenID, lexem.GenericTokenParameter);
                }
                if (lexem.IsSugar)
                {
                    lexer.AddSugarLexem(tokenID, lexem.GenericTokenParameter);
                }
            }
            result.Result = lexer;
            return result;
        }
    }
}
