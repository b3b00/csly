using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using sly.buildresult;
using sly.lexer.fsm;

namespace sly.lexer
{
    public static class EnumHelper
    {
        /// <summary>
        ///     Gets an attribute on an enum field value
        /// </summary>
        /// <typeparam name="T">The type of the attribute you want to retrieve</typeparam>
        /// <param name="enumVal">The enum value</param>
        /// <returns>The attributes of type T that exist on the enum value</returns>
        /// <example>var attrs = myEnumVariable.GetAttributeOfType<DescriptionAttribute>().Description;</example>
        public static List<T> GetAttributesOfType<T>(this Enum enumVal) where T : Attribute
        {
            var type = enumVal.GetType();
            var memInfo = type.GetMember(enumVal.ToString());
            var attributes = (IEnumerable<T>) memInfo[0].GetCustomAttributes(typeof(T), false);

            if (attributes.Any())
            {
                return attributes.ToList();
            }

            return new List<T>();
        }
    }

    public class LexerBuilder
    {
        public static Dictionary<IN, List<LexemeAttribute>> GetLexemes<IN>(BuildResult<ILexer<IN>> result) where IN: struct
        {
            var values = Enum.GetValues(typeof(IN));

            var attributes = new Dictionary<IN, List<LexemeAttribute>>();

            var fields = typeof(IN).GetFields();
            foreach (Enum value in values)
            {
                var tokenID = (IN) (object) value;
                var enumattributes = value.GetAttributesOfType<LexemeAttribute>();
                if (enumattributes == null || enumattributes.Count == 0)
                    result?.AddError(new LexerInitializationError(ErrorLevel.WARN,
                        $"token {tokenID} in lexer definition {typeof(IN).FullName} does not have Lexeme"));
                else
                    foreach (var lexem in enumattributes)
                        if (lexem != null)
                        {
                            var lex = new List<LexemeAttribute>();
                            if (attributes.ContainsKey(tokenID)) lex = attributes[tokenID];
                            lex.Add(lexem);
                            attributes[tokenID] = lex;
                        }
                        else
                        {
                            if (!tokenID.Equals(default(IN)))
                                result?.AddError(new LexerInitializationError(ErrorLevel.WARN,
                                    $"token {tokenID} in lexer definition {typeof(IN).FullName} does not have Lexeme"));
                        }

                ;
            }

            return attributes;
        }


        public static BuildResult<ILexer<IN>> BuildLexer<IN>(BuildResult<ILexer<IN>> result,
            BuildExtension<IN> extensionBuilder = null) where IN : struct
        {
            var type = typeof(IN);
            var typeInfo = type.GetTypeInfo();
            ILexer<IN> lexer = new Lexer<IN>();


            var attributes = GetLexemes(result);

            result = Build(attributes, result, extensionBuilder);

            return result;
        }


        private static BuildResult<ILexer<IN>> Build<IN>(Dictionary<IN, List<LexemeAttribute>> attributes,
            BuildResult<ILexer<IN>> result, BuildExtension<IN> extensionBuilder = null) where IN : struct
        {
            var hasRegexLexem = IsRegexLexer(attributes);
            var hasGenericLexem = IsGenericLexer(attributes);

            if (hasGenericLexem && hasRegexLexem)
            {
                result.AddError(new LexerInitializationError(ErrorLevel.WARN,
                    "cannot mix Regex lexemes and Generic lexemes in same lexer"));
                result.IsError = true;
            }
            else
            {
                if (hasRegexLexem)
                    result = BuildRegexLexer(attributes, result);
                else if (hasGenericLexem) result = BuildGenericLexer(attributes, extensionBuilder, result);
            }

            return result;
        }

        private static bool IsRegexLexer<IN>(Dictionary<IN, List<LexemeAttribute>> attributes)
        {
            var isGeneric = false;
            foreach (var ls in attributes)
            {
                foreach (var l in ls.Value)
                {
                    isGeneric = !string.IsNullOrEmpty(l.Pattern);
                    if (isGeneric) break;
                }

                if (isGeneric) break;
            }

            return isGeneric;
        }

        private static bool IsGenericLexer<IN>(Dictionary<IN, List<LexemeAttribute>> attributes)
        {
            var isRegex = false;
            foreach (var ls in attributes)
            {
                foreach (var l in ls.Value)
                {
                    isRegex = l.GenericToken != default(GenericToken);
                    if (isRegex) break;
                }

                if (isRegex) break;
            }

            return isRegex;
        }


        private static BuildResult<ILexer<IN>> BuildRegexLexer<IN>(Dictionary<IN, List<LexemeAttribute>> attributes,
            BuildResult<ILexer<IN>> result) where IN : struct
        {
            ILexer<IN> lexer = new Lexer<IN>();
            foreach (var pair in attributes)
            {
                var tokenID = pair.Key;

                var lexems = pair.Value;

                if (lexems != null)
                {
                    try
                    {
                        foreach (var lexem in lexems)
                            lexer.AddDefinition(new TokenDefinition<IN>(tokenID, lexem.Pattern, lexem.IsSkippable,
                                lexem.IsLineEnding));
                    }
                    catch (Exception e)
                    {
                        result.AddError(new LexerInitializationError(ErrorLevel.ERROR,
                            $"error at lexem {tokenID} : {e.Message}"));
                    }
                }
                else
                {
                    if (!tokenID.Equals(default(IN)))
                        result.AddError(new LexerInitializationError(ErrorLevel.WARN,
                            $"token {tokenID} in lexer definition {typeof(IN).FullName} does not have"));
                }

                ;
            }

            result.Result = lexer;
            return result;
        }

        private static (List<GenericToken> tokens, IdentifierType idType) GetGenericTokensAndIdentifierType<IN>(
            Dictionary<IN, List<LexemeAttribute>> attributes)
        {
            (List<GenericToken> tokens, IdentifierType idType)
                result = (new List<GenericToken>(), IdentifierType.Alpha);
            var statics = new List<GenericToken>();
            foreach (var ls in attributes)
            foreach (var l in ls.Value)
            {
                statics.Add(l.GenericToken);
                if (l.IsIdentifier) result.idType = l.IdentifierType;
            }

            statics.Distinct();
            result.tokens = statics;

            return result;
        }

        private static BuildResult<ILexer<IN>> BuildGenericLexer<IN>(Dictionary<IN, List<LexemeAttribute>> attributes,
            BuildExtension<IN> extensionBuilder, BuildResult<ILexer<IN>> result) where IN : struct
        {
            var statics = GetGenericTokensAndIdentifierType(attributes);
            var Extensions = new Dictionary<IN, LexemeAttribute>();
            var lexer = new GenericLexer<IN>(statics.idType, extensionBuilder, statics.tokens.ToArray());
            foreach (var pair in attributes)
            {
                var tokenID = pair.Key;

                var lexems = pair.Value;
                foreach (var lexem in lexems)
                {
                    if (lexem.IsStaticGeneric) lexer.AddLexeme(lexem.GenericToken, tokenID);
                    if (lexem.IsKeyWord)
                        foreach (var param in lexem.GenericTokenParameters)
                            lexer.AddKeyWord(tokenID, param);
                    if (lexem.IsSugar)
                        foreach (var param in lexem.GenericTokenParameters)
                            lexer.AddSugarLexem(tokenID, param);
                    if (lexem.IsString)
                    {
                        if (lexem.GenericTokenParameters != null && lexem.GenericTokenParameters.Length > 0)
                            try
                            {
                                var delimiter = lexem.GenericTokenParameters[0];
                                if (lexem.GenericTokenParameters.Length > 1)
                                {
                                    var escape = lexem.GenericTokenParameters[1];
                                    lexer.AddStringLexem(tokenID, delimiter, escape);
                                }
                                else
                                {
                                    lexer.AddStringLexem(tokenID, delimiter);
                                }
                            }
                            catch (Exception e)
                            {
                                result.IsError = true;
                                result.AddError(new InitializationError(ErrorLevel.FATAL, e.Message));
                            }
                        else
                            lexer.AddStringLexem(tokenID, "\"");
                    }

                    if (lexem.IsExtension) Extensions[tokenID] = lexem;
                }


                AddExtensions(Extensions, extensionBuilder, lexer);
            }


            var comments = GetCommentsAttribute(result);

            if (!result.IsError)
                foreach (var comment in comments)
                {
                    NodeCallback<GenericToken> callbackSingle = match =>
                    {
                        match.Properties[GenericLexer<IN>.DerivedToken] = comment.Key;
                        match.Result.IsComment = true;
                        match.Result.CommentType = CommentType.Single;
                        return match;
                    };

                    NodeCallback<GenericToken> callbackMulti = match =>
                    {
                        match.Properties[GenericLexer<IN>.DerivedToken] = comment.Key;
                        match.Result.IsComment = true;
                        match.Result.CommentType = CommentType.Multi;
                        return match;
                    };

                    foreach (var commentAttr in comment.Value)
                    {
                        bool hasSingleLine = !string.IsNullOrWhiteSpace(commentAttr.SingleLineCommentStart);
                        bool hasMultiLine = !string.IsNullOrWhiteSpace(commentAttr.MultiLineCommentStart);

                        if (hasSingleLine)
                        {
                            lexer.SingleLineComment = commentAttr.SingleLineCommentStart;
                        }

                        if (hasMultiLine)
                        {
                            lexer.MultiLineCommentStart = commentAttr.MultiLineCommentStart;
                            lexer.MultiLineCommentEnd = commentAttr.MultiLineCommentEnd;
                        }

                        var fsmBuilder = lexer.FSMBuilder;

                        if (hasSingleLine)
                        {
                            fsmBuilder.GoTo(GenericLexer<IN>.start);
                            fsmBuilder.ConstantTransition(commentAttr.SingleLineCommentStart);
                            fsmBuilder.Mark(GenericLexer<IN>.single_line_comment_start);
                            fsmBuilder.End(GenericToken.Comment);
                            fsmBuilder.CallBack(callbackSingle);
                        }

                        if (hasMultiLine)
                        {
                            fsmBuilder.GoTo(GenericLexer<IN>.start);
                            fsmBuilder.ConstantTransition(commentAttr.MultiLineCommentStart);
                            fsmBuilder.Mark(GenericLexer<IN>.multi_line_comment_start);
                            fsmBuilder.End(GenericToken.Comment);
                            fsmBuilder.CallBack(callbackMulti);
                        }
                    }
                }


            result.Result = lexer;
            return result;
        }


        private static Dictionary<IN, List<CommentAttribute>> GetCommentsAttribute<IN>(BuildResult<ILexer<IN>> result) where IN : struct
        {
            var values = Enum.GetValues(typeof(IN));

            var attributes = new Dictionary<IN, List<CommentAttribute>>();
    
            var fields = typeof(IN).GetFields();
            foreach (Enum value in values)
            {
                var tokenID = (IN) (object) value;
                var enumAttributes = value.GetAttributesOfType<CommentAttribute>();
                if (enumAttributes != null && enumAttributes.Any()) attributes[tokenID] = enumAttributes;
            }

            var commentCount = attributes.Values.ToList().Select(l => l?.Count(attr => attr.GetType() == typeof(CommentAttribute)) ?? 0).ToList().Sum();
            var multiLineCommentCount = attributes.Values.ToList().Select(l => l?.Count(attr => attr.GetType() == typeof(MultiLineCommentAttribute)) ?? 0).ToList().Sum();
            var singleLineCommentCount = attributes.Values.ToList().Select(l => l?.Count(attr => attr.GetType() == typeof(SingleLineCommentAttribute)) ?? 0).ToList().Sum();

            if (commentCount > 1) result.AddError(new LexerInitializationError(ErrorLevel.FATAL, "too many comment lexem"));

            if (multiLineCommentCount > 1) result.AddError(new LexerInitializationError(ErrorLevel.FATAL, "too many multi-line comment lexem"));
            if (singleLineCommentCount > 1) result.AddError(new LexerInitializationError(ErrorLevel.FATAL, "too many single-line comment lexem"));

            if (commentCount > 0 && (multiLineCommentCount > 0 || singleLineCommentCount > 0)) result.AddError(new LexerInitializationError(ErrorLevel.FATAL, "comment lexem can't be used together with single-line or multi-line comment lexems"));

            return attributes;
        }

        private static void AddExtensions<IN>(Dictionary<IN, LexemeAttribute> Extensions,
            BuildExtension<IN> extensionBuilder, GenericLexer<IN> lexer) where IN : struct
        {
            if (extensionBuilder != null)
                foreach (var attr in Extensions)
                    extensionBuilder(attr.Key, attr.Value, lexer);
            ;
        }
    }
}