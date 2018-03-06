using sly.buildresult;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using sly.lexer.fsm;

namespace sly.lexer
{



    public static class EnumHelper
    {
        /// <summary>
        /// Gets an attribute on an enum field value
        /// </summary>
        /// <typeparam name="T">The type of the attribute you want to retrieve</typeparam>
        /// <param name="enumVal">The enum value</param>
        /// <returns>The attributes of type T that exist on the enum value</returns>
        /// <example>var attrs = myEnumVariable.GetAttributeOfType<DescriptionAttribute>().Description;</example>     

        public static List<T> GetAttributesOfType<T>(this Enum enumVal) where T : System.Attribute
        {
            var type = enumVal.GetType();
            var memInfo = type.GetMember(enumVal.ToString());
            IEnumerable<T> attributes = (IEnumerable<T>)(memInfo[0].GetCustomAttributes(typeof(T), false));
            if (attributes.Count() > 0)
            {
                return attributes.ToList<T>();
            }
            else
            {
                return new List<T>();
            }
        }
    }

    public class LexerBuilder
    {

        public static Dictionary<IN, List<LexemeAttribute>> GetLexemes<IN>(BuildResult<ILexer<IN>> result)
        {
            var values = Enum.GetValues(typeof(IN));

            var attributes = new Dictionary<IN, List<LexemeAttribute>>();

            var fields = typeof(IN).GetFields();
            foreach (Enum value in values)
            {
                IN tokenID = (IN)(object)value;
                List<LexemeAttribute> enumattributes = value.GetAttributesOfType<LexemeAttribute>();
                if (enumattributes == null || enumattributes.Count == 0)
                {
                    result?.AddError(new LexerInitializationError(ErrorLevel.WARN, $"token {tokenID} in lexer definition {typeof(IN).FullName} does not have Lexeme"));
                }
                else
                {
                    foreach (var lexem in enumattributes)
                    {
                        if (lexem != null)
                        {
                            List<LexemeAttribute> lex = new List<LexemeAttribute>();
                            if (attributes.ContainsKey(tokenID))
                            {
                                lex = attributes[tokenID];
                            }
                            lex.Add(lexem);
                            attributes[tokenID] = lex;
                        }
                        else
                        {
                            if (!tokenID.Equals(default(IN)))
                            {
                                result?.AddError(new LexerInitializationError(ErrorLevel.WARN, $"token {tokenID} in lexer definition {typeof(IN).FullName} does not have Lexeme"));
                            }
                        }
                    }
                }

                ;
            }
            return attributes;
        }



        public static BuildResult<ILexer<IN>> BuildLexer<IN>(BuildResult<ILexer<IN>> result, BuildExtension<IN> extensionBuilder = null) where IN : struct
        {
            Type type = typeof(IN);
            TypeInfo typeInfo = type.GetTypeInfo();
            ILexer<IN> lexer = new Lexer<IN>();



            var attributes = GetLexemes(result);

            result = Build(attributes, result, extensionBuilder);

            return result;
        }


        private static BuildResult<ILexer<IN>> Build<IN>(Dictionary<IN, List<LexemeAttribute>> attributes, BuildResult<ILexer<IN>> result, BuildExtension<IN> extensionBuilder = null) where IN : struct
        {

            bool hasRegexLexem = IsRegexLexer(attributes);
            bool hasGenericLexem = IsGenericLexer(attributes);

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
                    result = BuildGenericLexer<IN>(attributes, extensionBuilder, result);
                }
            }
            return result;
        }

        private static bool IsRegexLexer<IN>(Dictionary<IN, List<LexemeAttribute>> attributes)
        {
            bool isGeneric = false;
            foreach (var ls in attributes)
            {
                foreach (var l in ls.Value)
                {
                    isGeneric = !string.IsNullOrEmpty(l.Pattern);
                    if (isGeneric)
                    {
                        break;
                    }
                }
                if (isGeneric)
                {
                    break;
                }
            }

            return isGeneric;
        }

        private static bool IsGenericLexer<IN>(Dictionary<IN, List<LexemeAttribute>> attributes)
        {
            bool isRegex = false;
            foreach (var ls in attributes)
            {
                foreach (var l in ls.Value)
                {
                    isRegex = l.GenericToken != default(GenericToken);
                    if (isRegex)
                    {
                        break;
                    }
                }
                if (isRegex)
                {
                    break;
                }
            }

            return isRegex;
        }


        private static BuildResult<ILexer<IN>> BuildRegexLexer<IN>(Dictionary<IN, List<LexemeAttribute>> attributes, BuildResult<ILexer<IN>> result) where IN : struct
        {
            ILexer<IN> lexer = new Lexer<IN>();
            foreach (KeyValuePair<IN, List<LexemeAttribute>> pair in attributes)
            {
                IN tokenID = pair.Key;

                List<LexemeAttribute> lexems = pair.Value;

                if (lexems != null)
                {
                    try
                    {
                        foreach (var lexem in lexems)
                        {
                            lexer.AddDefinition(new TokenDefinition<IN>(tokenID, lexem.Pattern, lexem.IsSkippable, lexem.IsLineEnding));
                        }
                    }
                    catch (Exception e)
                    {
                        result.AddError(new LexerInitializationError(ErrorLevel.ERROR, $"error at lexem {tokenID} : {e.Message}"));
                    }
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

        private static (List<GenericToken> tokens, IdentifierType idType) GetGenericTokensAndIdentifierType<IN>(Dictionary<IN, List<LexemeAttribute>> attributes)
        {
            (List<GenericToken> tokens, IdentifierType idType) result = (new List<GenericToken>(), IdentifierType.Alpha);
            List<GenericToken> statics = new List<GenericToken>();
            foreach (var ls in attributes)
            {
                foreach (var l in ls.Value)
                {
                    statics.Add(l.GenericToken);
                    if (l.IsIdentifier)
                    {
                        result.idType = l.IdentifierType;
                    }
                }
            }
            statics.Distinct();
            result.tokens = statics;

            return result;
        }

        private static BuildResult<ILexer<IN>> BuildGenericLexer<IN>(Dictionary<IN, List<LexemeAttribute>> attributes, BuildExtension<IN> extensionBuilder, BuildResult<ILexer<IN>> result) where IN : struct
        {
            (List<GenericToken> tokens, IdentifierType idType) statics = GetGenericTokensAndIdentifierType(attributes);
            Dictionary<IN, LexemeAttribute> Extensions = new Dictionary<IN, LexemeAttribute>();
            GenericLexer<IN> lexer = new GenericLexer<IN>(statics.idType, extensionBuilder, statics.tokens.ToArray());
            foreach (KeyValuePair<IN, List<LexemeAttribute>> pair in attributes)
            {
                IN tokenID = pair.Key;

                List<LexemeAttribute> lexems = pair.Value;
                foreach (var lexem in lexems)
                {
                    if (lexem.IsStaticGeneric)
                    {
                        lexer.AddLexeme(lexem.GenericToken, tokenID);
                    }
                    if (lexem.IsKeyWord)
                    {
                        foreach (string param in lexem.GenericTokenParameters)
                        {
                            lexer.AddKeyWord(tokenID, param);
                        }
                    }
                    if (lexem.IsSugar)
                    {
                        foreach (string param in lexem.GenericTokenParameters)
                        {
                            lexer.AddSugarLexem(tokenID, param);
                        }
                    }
                    if (lexem.IsString)
                    {
                        if (lexem.GenericTokenParameters != null && lexem.GenericTokenParameters.Length > 0)
                        {
                            try {
                            lexer.AddStringLexem(tokenID, lexem.GenericTokenParameters[0]);
                            }
                            catch (Exception e) {
                                result.IsError = true;
                                result.AddError(new InitializationError(ErrorLevel.FATAL,e.Message));
                            }
                        }
                        else
                        {
                            lexer.AddStringLexem(tokenID, "\"");
                        }
                    }
                    if (lexem.IsExtension)
                    {
                        Extensions[tokenID] = lexem;
                    }

                }



                AddExtensions(Extensions, extensionBuilder, lexer);
            }


            var comments = GetCommentsAttribute<IN>(result);

            if (!result.IsError)
            {
                foreach (var comment in comments)
                {
                    NodeCallback<GenericToken> callbackSingle = (FSMMatch<GenericToken> match) =>
                    {
                        match.Properties[GenericLexer<IN>.DerivedToken] = comment.Key;
                        match.Result.IsComment = true;
                        match.Result.CommentType = CommentType.Single;
                        return match;
                    };

                    NodeCallback<GenericToken> callbackMulti = (FSMMatch<GenericToken> match) =>
                    {
                        match.Properties[GenericLexer<IN>.DerivedToken] = comment.Key;
                        match.Result.IsComment = true;
                        match.Result.CommentType = CommentType.Multi;
                        return match;
                    };



                    var commentAttr = comment.Value[0];

                    lexer.SingleLineComment = commentAttr.SingleLineCommentStart;
                    lexer.MultiLineCommentStart = commentAttr.MultiLineCommentStart;
                    lexer.MultiLineCommentEnd = commentAttr.MultiLineCommentEnd;


                    var fsmBuilder = lexer.FSMBuilder;
                    fsmBuilder.GoTo(GenericLexer<IN>.start);
                    fsmBuilder.ConstantTransition(commentAttr.SingleLineCommentStart);
                    fsmBuilder.Mark(GenericLexer<IN>.single_line_comment_start);
                    fsmBuilder.End(GenericToken.Comment);
                    fsmBuilder.CallBack(callbackSingle);
                    fsmBuilder.GoTo(GenericLexer<IN>.start);
                    fsmBuilder.ConstantTransition(commentAttr.MultiLineCommentStart);
                    fsmBuilder.Mark(GenericLexer<IN>.multi_line_comment_start);
                    fsmBuilder.End(GenericToken.Comment);
                    fsmBuilder.CallBack(callbackMulti);
                }
            }


            result.Result = lexer;
            return result;
        }

        private static Dictionary<IN, List<CommentAttribute>> GetCommentsAttribute<IN>(BuildResult<ILexer<IN>> result)
        {
            var values = Enum.GetValues(typeof(IN));

            var attributes = new Dictionary<IN, List<CommentAttribute>>();

            var fields = typeof(IN).GetFields();
            foreach (Enum value in values)
            {
                IN tokenID = (IN)(object)value;
                List<CommentAttribute> enumattributes = value.GetAttributesOfType<CommentAttribute>();
                if (enumattributes != null && enumattributes.Any())
                {
                    attributes[tokenID] = enumattributes;
                }
            }
            int count = attributes.Values.ToList().Select((List<CommentAttribute> l) => l != null ? l.Count : 0).ToList().Sum();
            if (count > 1)
            {
                result.AddError(new LexerInitializationError(ErrorLevel.FATAL, $"too many comment lexem"));
            }

            return attributes;
        }
        private static void AddExtensions<IN>(Dictionary<IN, LexemeAttribute> Extensions, BuildExtension<IN> extensionBuilder, GenericLexer<IN> lexer) where IN : struct
        {

            if (extensionBuilder != null)
            {
                foreach (KeyValuePair<IN, LexemeAttribute> attr in Extensions)
                {
                    extensionBuilder(attr.Key, attr.Value, lexer);
                }
            }
            ;
        }
    }
}