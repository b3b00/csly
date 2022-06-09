using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using sly.buildresult;
using sly.i18n;
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

    public static class LexerBuilder
    {
        public static Dictionary<IN, List<LexemeAttribute>> GetLexemes<IN>(BuildResult<ILexer<IN>> result, string lang) where IN: struct
        {
            var attributes = new Dictionary<IN, List<LexemeAttribute>>();

            var values = Enum.GetValues(typeof(IN));
            var grouped = values.Cast<IN>().GroupBy<IN, IN>(x => x).ToList<IGrouping<IN, IN>>();
            foreach (var group in grouped)
            {
                
                var v = group.Key;
                if (group.Count<IN>() > 1)
                {
                 
                    Enum enumValue = Enum.Parse(typeof(IN), v.ToString()) as Enum;
                    int intValue = Convert.ToInt32(enumValue); // x is the integer value of enum
                    
                    result.AddError(new LexerInitializationError(ErrorLevel.FATAL,
                        I18N.Instance.GetText(lang,I18NMessage.SameValueUsedManyTime,intValue.ToString(),group.Count<IN>().ToString(),typeof(IN).FullName),
                        ErrorCodes.LEXER_SAME_VALUE_USED_MANY_TIME));
                    
                }
            }

            if (!result.IsError)
            {

                foreach (Enum value in values)
                {
                    var tokenID = (IN) (object) value;
                    var enumAttributes = value.GetAttributesOfType<LexemeAttribute>();
                    var singleCommentAttributes = value.GetAttributesOfType<SingleLineCommentAttribute>();
                    var multiCommentAttributes = value.GetAttributesOfType<MultiLineCommentAttribute>();
                    var commentAttributes = value.GetAttributesOfType<CommentAttribute>();
                    if (enumAttributes.Length == 0 && singleCommentAttributes.Length == 0 &&
                        multiCommentAttributes.Length == 0 && commentAttributes.Length == 0)
                    {
                        result?.AddError(new LexerInitializationError(ErrorLevel.WARN,
                            $"token {tokenID} in lexer definition {typeof(IN).FullName} does not have Lexeme",ErrorCodes.NOT_AN_ERROR));
                    }
                    else
                    {
                        attributes[tokenID] = enumAttributes.ToList<LexemeAttribute>();
                    }
                }
            }

            return attributes;
        }

        public static BuildResult<ILexer<IN>> BuildLexer<IN>(BuildExtension<IN> extensionBuilder = null) where IN : struct
        {
            return BuildLexer<IN>(new BuildResult < ILexer < IN >>() , extensionBuilder);
        }

        public static BuildResult<ILexer<IN>> BuildLexer<IN>(BuildResult<ILexer<IN>> result,
            BuildExtension<IN> extensionBuilder = null,
            string lang = null, LexerPostProcess<IN> lexerPostProcess = null, IList<string> implicitTokens = null) where IN : struct
        {
            var attributes = GetLexemes<IN>(result,lang);
            if (!result.IsError)
            {
                result = Build<IN>(attributes, result, extensionBuilder,lang, implicitTokens);
                if (!result.IsError)
                {
                    result.Result.LexerPostProcess = lexerPostProcess;
                }
            }
            
            return result;
        }


        private static BuildResult<ILexer<IN>> Build<IN>(Dictionary<IN, List<LexemeAttribute>> attributes,
            BuildResult<ILexer<IN>> result, BuildExtension<IN> extensionBuilder = null, string lang = null,
            IList<string> implicitTokens = null) where IN : struct
        {
            var hasRegexLexemes = IsRegexLexer<IN>(attributes);
            var hasGenericLexemes = IsGenericLexer<IN>(attributes);

            if (hasGenericLexemes && hasRegexLexemes)
            {
                result.AddError(new LexerInitializationError(ErrorLevel.ERROR,
                    I18N.Instance.GetText(lang,I18NMessage.CannotMixGenericAndRegex),
                    ErrorCodes.LEXER_CANNOT_MIX_GENERIC_AND_REGEX));
            }
            else
            {
                if (hasRegexLexemes)
                {
                    if (implicitTokens != null && implicitTokens.Any())
                    {
                        result.AddError(new LexerInitializationError(ErrorLevel.ERROR,
                            I18N.Instance.GetText(lang,I18NMessage.CannotUseImplicitTokensWithRegexLexer),
                            ErrorCodes.LEXER_CANNOT_USE_IMPLICIT_TOKENS_WITH_REGEX_LEXER));
                    }
                    else
                    {
                        result = BuildRegexLexer<IN>(attributes, result, lang);
                    }
                }
                else if (hasGenericLexemes)
                {
                    result = BuildGenericLexer<IN>(attributes, extensionBuilder, result, lang, implicitTokens);
                }
            }

            return result;
        }

        private static bool IsRegexLexer<IN>(Dictionary<IN, List<LexemeAttribute>> attributes)
        {
            return attributes.Values.SelectMany<List<LexemeAttribute>, LexemeAttribute>(list => list)
                .Any<LexemeAttribute>(lexeme => !string.IsNullOrEmpty(lexeme.Pattern));
        }

        private static bool IsGenericLexer<IN>(Dictionary<IN, List<LexemeAttribute>> attributes)
        {
            return attributes.Values.SelectMany<List<LexemeAttribute>, LexemeAttribute>(list => list)
                .Any<LexemeAttribute>(lexeme => lexeme.GenericToken != default);
        }


        private static BuildResult<ILexer<IN>> BuildRegexLexer<IN>(Dictionary<IN, List<LexemeAttribute>> attributes,
            BuildResult<ILexer<IN>> result, string lang = null) where IN : struct
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
                            var channel = lexeme.Channel.HasValue ? lexeme.Channel.Value : 0;
                            lexer.AddDefinition(new TokenDefinition<IN>(tokenID, lexeme.Pattern, channel,lexeme.IsSkippable,
                                lexeme.IsLineEnding));
                        }
                    }
                    catch (Exception e)
                    {
                        result.AddError(new LexerInitializationError(ErrorLevel.ERROR,
                            $"error at lexem {tokenID} : {e.Message}", ErrorCodes.LEXER_UNKNOWN_ERROR));
                    }
                }
                else if (!tokenID.Equals(default(IN)))
                {
                    result.AddError(new LexerInitializationError(ErrorLevel.WARN,
                        $"token {tokenID} in lexer definition {typeof(IN).FullName} does not have Lexeme",ErrorCodes.NOT_AN_ERROR));
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
                config.Indentation = lexerAttribute.Indentation;
                config.IndentationAware = lexerAttribute.IndentationAWare;
            }

            var statics = new List<GenericToken>();
            foreach (var lexeme in attributes.Values.SelectMany<List<LexemeAttribute>, LexemeAttribute>(list => list))
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

            return (config, statics.Distinct<GenericToken>().ToArray<GenericToken>());
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

        private static NodeCallback<GenericToken> GetCallbackSingle<IN>(IN token, bool doNotIgnore, int channel) where IN : struct
        {
            NodeCallback<GenericToken> callback = match =>
            {
                match.Properties[GenericLexer<IN>.DerivedToken] = token;
                match.Result.IsComment = true;
                match.Result.CommentType = CommentType.Single;
                match.Result.Notignored = doNotIgnore;
                match.Result.Channel = channel;
                return match;
            };
            return callback;
        }

        private static NodeCallback<GenericToken> GetCallbackMulti<IN>(IN token, bool doNotIgnore, int channel) where IN : struct
        {
            NodeCallback<GenericToken> callbackMulti = match =>
            {
                match.Properties[GenericLexer<IN>.DerivedToken] = token;
                match.Result.IsComment = true;
                match.Result.Notignored = doNotIgnore;
                match.Result.CommentType = CommentType.Multi;
                match.Result.Channel = channel;
                return match;
            };
            return callbackMulti;
        }
        
        private static BuildResult<ILexer<IN>> BuildGenericLexer<IN>(Dictionary<IN, List<LexemeAttribute>> attributes,
            BuildExtension<IN> extensionBuilder, BuildResult<ILexer<IN>> result, string lang,
            IList<string> implicitTokens = null) where IN : struct
        {
            result = CheckStringAndCharTokens<IN>(attributes, result, lang);
            var (config, tokens) = GetConfigAndGenericTokens<IN>(attributes);
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
                                lexer.AddKeyWord(tokenID, param, result);
                            }
                        }

                        if (lexeme.IsSugar)
                        {
                            foreach (var param in lexeme.GenericTokenParameters)
                            {
                                lexer.AddSugarLexem(tokenID,result, param, lexeme.IsLineEnding);
                            }
                        }

                        if (lexeme.IsString)
                        {
                            var (delimiter, escape) = GetDelimiters(lexeme, "\"", "\\");
                            lexer.AddStringLexem(tokenID, result, delimiter, escape);
                        }

                        if (lexeme.IsChar)
                        {
                            var (delimiter, escape) = GetDelimiters(lexeme, "'", "\\");
                            lexer.AddCharLexem(tokenID, result, delimiter, escape);
                        }

                        if (lexeme.IsExtension)
                        {
                            Extensions[tokenID] = lexeme;
                        }
                    }
                    catch (Exception e)
                    {
                        result.AddError(new InitializationError(ErrorLevel.FATAL, e.Message,ErrorCodes.LEXER_UNKNOWN_ERROR));
                    }
                }
            }

            AddExtensions<IN>(Extensions, extensionBuilder, lexer);

            var comments = GetCommentsAttribute<IN>(result,lang);
            
            if (!result.IsError)
            {
                foreach (var comment in comments)
                {
                    
                    ;

                   

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
                            fsmBuilder.CallBack(GetCallbackSingle<IN>(comment.Key,commentAttr.DoNotIgnore, commentAttr.Channel));
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
                            fsmBuilder.CallBack(GetCallbackMulti<IN>(comment.Key,commentAttr.DoNotIgnore, commentAttr.Channel));
                        }
                    }
                }

                if (implicitTokens != null)
                {
                    foreach (var implicitToken in implicitTokens)
                    {
                        var fsmBuilder = lexer.FSMBuilder;

                        if (!fsmBuilder.Marks.Any(mark => mark.Key == GenericLexer<IN>.in_identifier))
                        {
                            // no identifier pattern has been defined. Creating a default one to allow implicit keyword tokens
                            (lexer as GenericLexer<IN>).InitializeIdentifier(new GenericLexer<IN>.Config()  { IdType = IdentifierType.Alpha});
                        }
                        
                        var x = fsmBuilder.Fsm.Run(implicitToken, new LexerPosition());
                        if (x.IsSuccess)
                        {
                            
                            
                            
                            var t = fsmBuilder.Marks;
                            var y = fsmBuilder.Marks.FirstOrDefault(k => k.Value == x.NodeId);
                            if (y.Key == GenericLexer<IN>.in_identifier) // implicit keyword
                            {
                                var resultx = new BuildResult<ILexer<IN>>();
                                result.Errors.AddRange(resultx.Errors);
                                lexer.AddKeyWord(default(IN), implicitToken, resultx);
                                ;
                            }
                        }
                        else
                        {
                            var resulty = new BuildResult<ILexer<IN>>();
                            result.Errors.AddRange(resulty.Errors);
                            lexer.AddSugarLexem(default(IN), resulty, implicitToken);
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
            Dictionary<IN, List<LexemeAttribute>> attributes, BuildResult<ILexer<IN>> result, string lang) where IN : struct
        {
            var allLexemes = attributes.Values.SelectMany<List<LexemeAttribute>, LexemeAttribute>(a => a);

            var allDelimiters = allLexemes
                                .Where<LexemeAttribute>(a => a.IsString || a.IsChar)
                                .Where<LexemeAttribute>(a => a.HasGenericTokenParameters)
                                .Select<LexemeAttribute, string>(a => a.GenericTokenParameters[0]);

            var duplicates = allDelimiters.GroupBy<string, string>(x => x)
                                        .Where<IGrouping<string, string>>(g => g.Count<string>() > 1)
                                        .Select(y => new { Element = y.Key, Counter = y.Count<string>() });

            foreach (var duplicate in duplicates)
            {
                result.AddError(new LexerInitializationError(ErrorLevel.FATAL,
                    I18N.Instance.GetText(lang,I18NMessage.DuplicateStringCharDelimiters,duplicate.Element,duplicate.Counter.ToString()),
                    ErrorCodes.LEXER_DUPLICATE_STRING_CHAR_DELIMITERS));
            }

            return result;
        }


        private static Dictionary<IN, List<CommentAttribute>> GetCommentsAttribute<IN>(BuildResult<ILexer<IN>> result, string lang) where IN : struct
        {
            var attributes = new Dictionary<IN, List<CommentAttribute>>();

            var values = Enum.GetValues(typeof(IN));
            foreach (Enum value in values)
            {
                var tokenID = (IN) (object) value;
                var enumAttributes = value.GetAttributesOfType<CommentAttribute>();
                if (enumAttributes != null && enumAttributes.Any<CommentAttribute>()) attributes[tokenID] = enumAttributes.ToList<CommentAttribute>();
            }

            var commentCount = attributes.Values.Select<List<CommentAttribute>, int>(l => l?.Count<CommentAttribute>(attr => attr.GetType() == typeof(CommentAttribute)) ?? 0).Sum();
            var multiLineCommentCount = attributes.Values.Select<List<CommentAttribute>, int>(l => l?.Count<CommentAttribute>(attr => attr.GetType() == typeof(MultiLineCommentAttribute)) ?? 0).Sum();
            var singleLineCommentCount = attributes.Values.Select<List<CommentAttribute>, int>(l => l?.Count<CommentAttribute>(attr => attr.GetType() == typeof(SingleLineCommentAttribute)) ?? 0).Sum();

            if (commentCount > 1)
            {
                result.AddError(new LexerInitializationError(ErrorLevel.FATAL,
                    I18N.Instance.GetText(lang,I18NMessage.TooManyComment),
                    ErrorCodes.LEXER_TOO_MANY_COMMNENT));
            }

            if (multiLineCommentCount > 1)
            {
                result.AddError(new LexerInitializationError(ErrorLevel.FATAL,
                    I18N.Instance.GetText(lang,I18NMessage.TooManyMultilineComment),
                    ErrorCodes.LEXER_TOO_MANY_MULTILINE_COMMNENT));
            }

            if (singleLineCommentCount > 1)
            {
                result.AddError(new LexerInitializationError(ErrorLevel.FATAL, 
                    I18N.Instance.GetText(lang,I18NMessage.TooManySingleLineComment),ErrorCodes.LEXER_TOO_MANY_SINGLELINE_COMMNENT));
            }

            if (commentCount > 0 && (multiLineCommentCount > 0 || singleLineCommentCount > 0))
            {
                result.AddError(new LexerInitializationError(ErrorLevel.FATAL, 
                    I18N.Instance.GetText(lang,I18NMessage.CannotMixCommentAndSingleOrMulti),
                    ErrorCodes.LEXER_CANNOT_MIX_COMMENT_AND_SINGLE_OR_MULTI));
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