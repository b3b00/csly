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
        /// <returns>The attributes of type T that exist on the enum value, or an empty array if no such attributes are found.</returns>
        /// <example>var attrs = myEnumVariable.GetAttributesOfType&lt;DescriptionAttribute&gt;();</example>
        public static T[] GetAttributesOfType<T>(this Enum enumVal) where T : Attribute
        {
            var type = enumVal.GetType();
            var memInfo = type.GetMember(enumVal.ToString());
            var attributes = (T[]) memInfo[0].GetCustomAttributes(typeof(T), false);

            return attributes;
        }
    }

    public class LexerBuilder
    {
        public static Dictionary<IN, List<LexemeAttribute>> GetLexemes<IN>(BuildResult<ILexer<IN>> result) where IN: struct
        {
            var attributes = new Dictionary<IN, List<LexemeAttribute>>();

            var values = Enum.GetValues(typeof(IN));
            foreach (Enum value in values)
            {
                var tokenID = (IN) (object) value;
                var enumAttributes = value.GetAttributesOfType<LexemeAttribute>();
                if (enumAttributes.Length == 0)
                {
                    result?.AddError(new LexerInitializationError(ErrorLevel.WARN,
                        $"token {tokenID} in lexer definition {typeof(IN).FullName} does not have Lexeme"));
                }
                else
                {
                    attributes[tokenID] = enumAttributes.ToList();
                }
            }

            return attributes;
        }


        public static BuildResult<ILexer<IN>> BuildLexer<IN>(BuildResult<ILexer<IN>> result,
            BuildExtension<IN> extensionBuilder = null) where IN : struct
        {
            var attributes = GetLexemes(result);

            result = Build(attributes, result, extensionBuilder);

            return result;
        }


        private static BuildResult<ILexer<IN>> Build<IN>(Dictionary<IN, List<LexemeAttribute>> attributes,
            BuildResult<ILexer<IN>> result, BuildExtension<IN> extensionBuilder = null) where IN : struct
        {
            var hasRegexLexemes = IsRegexLexer(attributes);
            var hasGenericLexemes = IsGenericLexer(attributes);

            if (hasGenericLexemes && hasRegexLexemes)
            {
                result.AddError(new LexerInitializationError(ErrorLevel.WARN,
                    "cannot mix Regex lexemes and Generic lexemes in same lexer"));
            }
            else
            {
                if (hasRegexLexemes)
                {
                    result = BuildRegexLexer(attributes, result);
                }
                else if (hasGenericLexemes)
                {
                    result = BuildGenericLexer(attributes, extensionBuilder, result);
                }
            }

            return result;
        }

        private static bool IsRegexLexer<IN>(Dictionary<IN, List<LexemeAttribute>> attributes)
        {
            return attributes.Values.SelectMany(list => list)
                             .Any(lexeme => !string.IsNullOrEmpty(lexeme.Pattern));
        }

        private static bool IsGenericLexer<IN>(Dictionary<IN, List<LexemeAttribute>> attributes)
        {
            return attributes.Values.SelectMany(list => list)
                             .Any(lexeme => lexeme.GenericToken != default(GenericToken));
        }


        private static BuildResult<ILexer<IN>> BuildRegexLexer<IN>(Dictionary<IN, List<LexemeAttribute>> attributes,
            BuildResult<ILexer<IN>> result) where IN : struct
        {
            var lexer = new Lexer<IN>();
            foreach (var pair in attributes)
            {
                var tokenID = pair.Key;

                var lexemes = pair.Value;

                if (lexemes != null)
                {
                    try
                    {
                        foreach (var lexeme in lexemes)
                        {
                            lexer.AddDefinition(new TokenDefinition<IN>(tokenID, lexeme.Pattern, lexeme.IsSkippable,
                                lexeme.IsLineEnding));
                        }
                    }
                    catch (Exception e)
                    {
                        result.AddError(new LexerInitializationError(ErrorLevel.ERROR,
                            $"error at lexem {tokenID} : {e.Message}"));
                    }
                }
                else if (!tokenID.Equals(default(IN)))
                {
                    result.AddError(new LexerInitializationError(ErrorLevel.WARN,
                        $"token {tokenID} in lexer definition {typeof(IN).FullName} does not have Lexeme"));
                }
            }

            result.Result = lexer;
            return result;
        }

        private static (GenericLexer<IN>.Config, GenericToken[]) GetConfigAndGenericTokens<IN>(IDictionary<IN, List<LexemeAttribute>> attributes)
            where IN : struct
        {
            var config = new GenericLexer<IN>.Config();
            var lexerAttribute = typeof(IN).GetCustomAttribute<LexerAttribute>();
            if (lexerAttribute != null)
            {
                config.IgnoreWS = lexerAttribute.IgnoreWS;
                config.IgnoreEOL = lexerAttribute.IgnoreEOL;
                config.WhiteSpace = lexerAttribute.WhiteSpace;
                config.KeyWordIgnoreCase = lexerAttribute.KeyWordIgnoreCase;
            }

            var statics = new List<GenericToken>();
            foreach (var lexeme in attributes.Values.SelectMany(list => list))
            {
                statics.Add(lexeme.GenericToken);
                if (lexeme.IsIdentifier)
                {
                    config.IdType = lexeme.IdentifierType;
                    if (lexeme.IdentifierType == IdentifierType.Custom)
                    {
                        config.IdentifierStartPattern = ParseIdentifierPattern(lexeme.IdentifierStartPattern);
                        config.IdentifierRestPattern = ParseIdentifierPattern(lexeme.IdentifierRestPattern);
                    }
                }
            }

            return (config, statics.Distinct().ToArray());
        }
        
        private static IEnumerable<char[]> ParseIdentifierPattern(string pattern)
        {
            var index = 0;
            while (index < pattern.Length)
            {
                if (index <= pattern.Length - 3 && pattern[index + 1] == '-')
                {
                    if (pattern[index] < pattern[index + 2])
                    {
                        yield return new[] { pattern[index], pattern[index + 2] };
                    }
                    else
                    {
                        yield return new[] { pattern[index + 2], pattern[index] };
                    }
                    index += 3;
                }
                else
                {
                    yield return new[] { pattern[index++] };
                }
            }
        }

        private static BuildResult<ILexer<IN>> BuildGenericLexer<IN>(Dictionary<IN, List<LexemeAttribute>> attributes,
                                                                     BuildExtension<IN> extensionBuilder, BuildResult<ILexer<IN>> result) where IN : struct
        {
            result = CheckStringAndCharTokens(attributes, result);
            var (config, tokens) = GetConfigAndGenericTokens(attributes);
            config.ExtensionBuilder = extensionBuilder;
            var lexer = new GenericLexer<IN>(config, tokens);
            var Extensions = new Dictionary<IN, LexemeAttribute>();
            foreach (var pair in attributes)
            {
                var tokenID = pair.Key;

                var lexemes = pair.Value;
                foreach (var lexeme in lexemes)
                {
                    try
                    {
                        if (lexeme.IsStaticGeneric)
                        {
                            lexer.AddLexeme(lexeme.GenericToken, tokenID);
                        }

                        if (lexeme.IsKeyWord)
                        {
                            foreach (var param in lexeme.GenericTokenParameters)
                            {
                                lexer.AddKeyWord(tokenID, param);
                            }
                        }

                        if (lexeme.IsSugar)
                        {
                            foreach (var param in lexeme.GenericTokenParameters)
                            {
                                lexer.AddSugarLexem(tokenID, param);
                            }
                        }

                        if (lexeme.IsString)
                        {
                            var (delimiter, escape) = GetDelimiters(lexeme, "\"", "\\");
                            lexer.AddStringLexem(tokenID, delimiter, escape);
                        }

                        if (lexeme.IsChar)
                        {
                            var (delimiter, escape) = GetDelimiters(lexeme, "'", "\\");
                            lexer.AddCharLexem(tokenID, delimiter, escape);
                        }

                        if (lexeme.IsExtension)
                        {
                            Extensions[tokenID] = lexeme;
                        }
                    }
                    catch (Exception e)
                    {
                        result.AddError(new InitializationError(ErrorLevel.FATAL, e.Message));
                    }
                }
            }

            AddExtensions(Extensions, extensionBuilder, lexer);

            var comments = GetCommentsAttribute(result);
            if (!result.IsError)
            {
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
                        var fsmBuilder = lexer.FSMBuilder;

                        var hasSingleLine = !string.IsNullOrWhiteSpace(commentAttr.SingleLineCommentStart);
                        if (hasSingleLine)
                        {
                            lexer.SingleLineComment = commentAttr.SingleLineCommentStart;

                            fsmBuilder.GoTo(GenericLexer<IN>.start);
                            fsmBuilder.ConstantTransition(commentAttr.SingleLineCommentStart);
                            fsmBuilder.Mark(GenericLexer<IN>.single_line_comment_start);
                            fsmBuilder.End(GenericToken.Comment);
                            fsmBuilder.CallBack(callbackSingle);
                        }

                        var hasMultiLine = !string.IsNullOrWhiteSpace(commentAttr.MultiLineCommentStart);
                        if (hasMultiLine)
                        {
                            lexer.MultiLineCommentStart = commentAttr.MultiLineCommentStart;
                            lexer.MultiLineCommentEnd = commentAttr.MultiLineCommentEnd;
                            
                            fsmBuilder.GoTo(GenericLexer<IN>.start);
                            fsmBuilder.ConstantTransition(commentAttr.MultiLineCommentStart);
                            fsmBuilder.Mark(GenericLexer<IN>.multi_line_comment_start);
                            fsmBuilder.End(GenericToken.Comment);
                            fsmBuilder.CallBack(callbackMulti);
                        }
                    }
                }
            }
            
            result.Result = lexer;
            return result;
        }

        private static (string delimiter, string escape) GetDelimiters(LexemeAttribute lexeme, string delimiter, string escape)
        {
            if (lexeme.HasGenericTokenParameters)
            {
                delimiter = lexeme.GenericTokenParameters[0];
                if (lexeme.GenericTokenParameters.Length > 1)
                {
                    escape = lexeme.GenericTokenParameters[1];
                }
            }

            return (delimiter, escape);
        }

        private static BuildResult<ILexer<IN>> CheckStringAndCharTokens<IN>(
            Dictionary<IN, List<LexemeAttribute>> attributes, BuildResult<ILexer<IN>> result) where IN : struct
        {
            var allLexemes = attributes.Values.SelectMany(a => a);

            var allDelimiters = allLexemes
                                .Where(a => a.IsString || a.IsChar)
                                .Where(a => a.HasGenericTokenParameters)
                                .Select(a => a.GenericTokenParameters[0]);

            var duplicates = allDelimiters.GroupBy(x => x)
                                        .Where(g => g.Count() > 1)
                                        .Select(y => new { Element = y.Key, Counter = y.Count() });

            foreach (var duplicate in duplicates)
            {
                result.AddError(new LexerInitializationError(ErrorLevel.FATAL,
                    $"char or string lexeme dilimiter {duplicate.Element} is used {duplicate.Counter} times. This will results in lexing conflicts"));
            }

            return result;
        }


        private static Dictionary<IN, List<CommentAttribute>> GetCommentsAttribute<IN>(BuildResult<ILexer<IN>> result) where IN : struct
        {
            var attributes = new Dictionary<IN, List<CommentAttribute>>();

            var values = Enum.GetValues(typeof(IN));
            foreach (Enum value in values)
            {
                var tokenID = (IN) (object) value;
                var enumAttributes = value.GetAttributesOfType<CommentAttribute>();
                if (enumAttributes != null && enumAttributes.Any()) attributes[tokenID] = enumAttributes.ToList();
            }

            var commentCount = attributes.Values.Select(l => l?.Count(attr => attr.GetType() == typeof(CommentAttribute)) ?? 0).Sum();
            var multiLineCommentCount = attributes.Values.Select(l => l?.Count(attr => attr.GetType() == typeof(MultiLineCommentAttribute)) ?? 0).Sum();
            var singleLineCommentCount = attributes.Values.Select(l => l?.Count(attr => attr.GetType() == typeof(SingleLineCommentAttribute)) ?? 0).Sum();

            if (commentCount > 1)
            {
                result.AddError(new LexerInitializationError(ErrorLevel.FATAL, "too many comment lexem"));
            }

            if (multiLineCommentCount > 1)
            {
                result.AddError(new LexerInitializationError(ErrorLevel.FATAL, "too many multi-line comment lexem"));
            }

            if (singleLineCommentCount > 1)
            {
                result.AddError(new LexerInitializationError(ErrorLevel.FATAL, "too many single-line comment lexem"));
            }

            if (commentCount > 0 && (multiLineCommentCount > 0 || singleLineCommentCount > 0))
            {
                result.AddError(new LexerInitializationError(ErrorLevel.FATAL, "comment lexem can't be used together with single-line or multi-line comment lexems"));
            }

            return attributes;
        }

        private static void AddExtensions<IN>(Dictionary<IN, LexemeAttribute> extensions,
            BuildExtension<IN> extensionBuilder, GenericLexer<IN> lexer) where IN : struct
        {
            if (extensionBuilder != null)
            {
                foreach (var attr in extensions)
                {
                    extensionBuilder(attr.Key, attr.Value, lexer);
                }
            }
        }
    }
}