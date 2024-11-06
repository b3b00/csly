using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using sly.buildresult;
using sly.i18n;
using sly.lexer.fsm;

namespace sly.lexer
{
    public static class DicExt
    {
        public static void AddToKey<K, K2, V>(this IDictionary<K, IDictionary<K2, V>> dic, K key, K2 k2, V value)
        {
            IDictionary<K2, V> values;
            if (!dic.TryGetValue(key, out values))
            {
                values = new Dictionary<K2, V>();
            }

            values[k2] = value;
            dic[key] = values;
        }
    }

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
            var attributes = (T[])memInfo[0].GetCustomAttributes(typeof(T), false);
            return attributes;
        }
    }

    public static class LexerBuilder
    {
        
        public static Dictionary<IN, (List<LexemeAttribute>,List<LexemeLabelAttribute>)> GetLexemes<IN>(BuildResult<ILexer<IN>> result, string lang)
            where IN : struct
        {
            var attributes = new Dictionary<IN, (List<LexemeAttribute>,List<LexemeLabelAttribute>)>();

            var values = Enum.GetValues(typeof(IN));
            var grouped = values.Cast<IN>().GroupBy(x => x).ToList();
            foreach (var group in grouped)
            {
                var v = group.Key;
                if (group.Count<IN>() > 1)
                {
                    Enum enumValue = Enum.Parse(typeof(IN), v.ToString()) as Enum;
                    int intValue = Convert.ToInt32(enumValue); // x is the integer value of enum

                    result.AddError(new LexerInitializationError(ErrorLevel.FATAL,
                        I18N.Instance.GetText(lang, I18NMessage.SameValueUsedManyTime, intValue.ToString(),
                            group.Count<IN>().ToString(), typeof(IN).FullName),
                        ErrorCodes.LEXER_SAME_VALUE_USED_MANY_TIME));
                }
            }

            if (!result.IsError)
            {
                foreach (Enum value in values)
                {
                    var tokenID = (IN)(object)value;
                    var labelAttributes = value.GetAttributesOfType<LexemeLabelAttribute>().ToList();
                    var enumAttributes = value.GetAttributesOfType<LexemeAttribute>();
                    var singleCommentAttributes = value.GetAttributesOfType<SingleLineCommentAttribute>();
                    var multiCommentAttributes = value.GetAttributesOfType<MultiLineCommentAttribute>();
                    var commentAttributes = value.GetAttributesOfType<CommentAttribute>();
                    if (enumAttributes.Length == 0 && singleCommentAttributes.Length == 0 &&
                        multiCommentAttributes.Length == 0 && commentAttributes.Length == 0)
                    {
                        result.AddError(new LexerInitializationError(ErrorLevel.WARN,
                            $"token {tokenID} in lexer definition {typeof(IN).FullName} does not have Lexeme",
                            ErrorCodes.NOT_AN_ERROR));
                    }
                    else
                    {
                        attributes[tokenID] = (enumAttributes.ToList<LexemeAttribute>(),labelAttributes);
                    }
                }
            }

            return attributes;
        }

        public static BuildResult<ILexer<IN>> BuildLexer<IN>(
            Action<IN, LexemeAttribute, GenericLexer<IN>> extensionBuilder = null,
            LexerPostProcess<IN> lexerPostProcess = null) where IN : struct
        {
            return BuildLexer<IN>(new BuildResult<ILexer<IN>>(), extensionBuilder, lexerPostProcess: lexerPostProcess);
        }

        public static BuildResult<ILexer<IN>> BuildLexer<IN>(BuildResult<ILexer<IN>> result,
            Action<IN, LexemeAttribute, GenericLexer<IN>> extensionBuilder = null,
            string lang = null, LexerPostProcess<IN> lexerPostProcess = null, IList<string> explicitTokens = null)
            where IN : struct
        {
            var attributes = GetLexemes<IN>(result, lang);
            if (!result.IsError)
            {
                result = Build<IN>(attributes, result, extensionBuilder, lang, explicitTokens);
                if (!result.IsError)
                {
                    var labels = result.Result.LexemeLabels;
                    LexerPostProcess<IN> post = tokens =>
                    {
                        var labeledTokens = tokens;
                        var count = labels.Select(x => x.Value.Count).Sum(); 
                        if (count > 0)
                        {
                            labeledTokens = tokens.Select(token =>
                            {
                                token.Label = token.TokenID.ToString();
                                if (labels.TryGetValue(token.TokenID, out var tokenLabels)
                                    && tokenLabels.TryGetValue(lang, out string label))
                                {
                                    token.Label = label;
                                }
                                else if (token.IsUnIndent)
                                {
                                    token.Label = "<<UINDENT>>";
                                }
                                else if (token.IsIndent)
                                {
                                    token.Label = "<<INDENT>>";
                                }

                                return token;
                            }).ToList();
                        }

                        if (lexerPostProcess != null)
                        {
                            return lexerPostProcess(labeledTokens);
                        }

                        return labeledTokens;
                    }; 
                    result.Result.LexerPostProcess = post;
                }
            }

            return result;
        }


        private static BuildResult<ILexer<IN>> Build<IN>(Dictionary<IN, (List<LexemeAttribute>,List<LexemeLabelAttribute>)> attributes,
            BuildResult<ILexer<IN>> result, Action<IN, LexemeAttribute, GenericLexer<IN>> extensionBuilder = null,
            string lang = null,
            IList<string> explicitTokens = null) where IN : struct
        {
            var hasRegexLexemes = IsRegexLexer<IN>(attributes);
            var hasGenericLexemes = IsGenericLexer<IN>(attributes);

            if (hasGenericLexemes && hasRegexLexemes)
            {
                result.AddError(new LexerInitializationError(ErrorLevel.ERROR,
                    I18N.Instance.GetText(lang, I18NMessage.CannotMixGenericAndRegex),
                    ErrorCodes.LEXER_CANNOT_MIX_GENERIC_AND_REGEX));
            }
            else
            {
                if (hasRegexLexemes)
                {
                    if (explicitTokens != null && explicitTokens.Any())
                    {
                        result.AddError(new LexerInitializationError(ErrorLevel.ERROR,
                            I18N.Instance.GetText(lang, I18NMessage.CannotUseExplicitTokensWithRegexLexer),
                            ErrorCodes.LEXER_CANNOT_USE_IMPLICIT_TOKENS_WITH_REGEX_LEXER));
                    }
                    else
                    {
                        result = BuildRegexLexer<IN>(attributes, lang, result);
                    }
                }
                else if (hasGenericLexemes)
                {
                    result = BuildGenericSubLexers<IN>(attributes, extensionBuilder, result, lang, explicitTokens);
                }

                result = SetLabels(attributes, result);
            }

            return result;
        }

        private static BuildResult<ILexer<IN>> SetLabels<IN>(
            Dictionary<IN, (List<LexemeAttribute> lexemes, List<LexemeLabelAttribute> labels)> attributes,
            BuildResult<ILexer<IN>> result) where IN : struct
        {
            if (result.IsOk && result.Result != null)
            {
                result.Result.LexemeLabels = new Dictionary<IN, Dictionary<string, string>>();
                foreach (var kvp in attributes)
                {
                    var labels = kvp.Value.labels.ToDictionary(x => x.Language, x => x.Label);
                    result.Result.LexemeLabels[kvp.Key] = labels;
                }


                for (int i = 0; i < result.Result.LexemeLabels.Values.Count; i++)
                {
                    var l = result.Result.LexemeLabels.Values.ToList()[i];
                    for (int j = i + 1; j < result.Result.LexemeLabels.Values.Count; j++)
                    {
                        var l2 = result.Result.LexemeLabels.Values.ToList()[j];

                        var duplicate = l.Where(x => l2.Values.Contains(x.Value)).ToList();
                        if (duplicate.Any())
                        {
                            var message = I18N.Instance.GetText(CultureInfo.CurrentCulture.TwoLetterISOLanguageName,
                                I18NMessage.ManyLexemWithSamelabel,
                                string.Join(", ", duplicate.Select(x => $@"""{x.Value}""")));
                            result.AddInitializationError(ErrorLevel.WARN, message,
                                ErrorCodes.LEXER_MANY_LEXEM_WITH_SAME_LABEL);
                        }

                    }
                }
            }

            return result;
        }

        private static bool IsRegexLexer<IN>(Dictionary<IN, (List<LexemeAttribute> lexemes,List<LexemeLabelAttribute> labels)> attributes)
        {
            return attributes.Values.Select(x => x.lexemes).SelectMany(x => x)
                .Any<LexemeAttribute>(lexeme => !string.IsNullOrEmpty(lexeme.Pattern));
        }

        private static bool IsGenericLexer<IN>(Dictionary<IN, (List<LexemeAttribute> lexemes,List<LexemeLabelAttribute> labels)> attributes)
        {
            return attributes.Values.Select(x => x.lexemes).SelectMany(x => x)
                .Any<LexemeAttribute>(lexeme => lexeme.GenericToken != default);
        }


        private static BuildResult<ILexer<IN>> BuildRegexLexer<IN>(
            Dictionary<IN, (List<LexemeAttribute> lexemes, List<LexemeLabelAttribute> labels)> attributes,
            string lang,
            BuildResult<ILexer<IN>> result) where IN : struct
        {
            var lexer = new Lexer<IN>()
            {
                I18n = lang
            };
            foreach (var pair in attributes)
            {
                var tokenID = pair.Key;

                var lexemes = pair.Value;

                if (lexemes.lexemes != null)
                {
                    try
                    {
                        foreach (var lexeme in lexemes.lexemes)
                        {
                            lexer.AddDefinition(new TokenDefinition<IN>(tokenID, lexeme.Pattern, lexeme.Channel,
                                lexeme.IsSkippable,
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
                        $"token {tokenID} in lexer definition {typeof(IN).FullName} does not have Lexeme",
                        ErrorCodes.NOT_AN_ERROR));
                }
            }

            result.Result = lexer;
            return result;
        }

        private static Dictionary<string, IDictionary<IN, List<LexemeAttribute>>> GetSubLexers<IN>(
            IDictionary<IN, (List<LexemeAttribute> lexemes,List<LexemeLabelAttribute> labels)> attributes) where IN : struct
        {
            Dictionary<string, IDictionary<IN, List<LexemeAttribute>>> subLexers =
                new Dictionary<string, IDictionary<IN, List<LexemeAttribute>>>();
            foreach (var attribute in attributes)
            {
                if (attribute.Key is Enum enumValue)
                {
                    var modeAtributes = enumValue.GetAttributesOfType<ModeAttribute>();

                    if (modeAtributes != null && modeAtributes.Any())
                    {
                        foreach (var modes in modeAtributes.Select(x => x.Modes))
                        {
                            if (modes != null && modes.Any())
                            {
                                foreach (var mode in modes)
                                {
                                    subLexers.AddToKey(mode, attribute.Key, attribute.Value.lexemes);
                                }
                            }
                        }
                    }
                    else
                    {
                        subLexers.AddToKey(ModeAttribute.DefaultLexerMode, attribute.Key, attribute.Value.lexemes);
                    }

                    var push = enumValue.GetAttributesOfType<PushAttribute>();
                    if (push != null && push.Length >= 1)
                    {
                        attribute.Value.lexemes.ForEach(x =>
                        {
                            x.IsPush = true;
                            x.Pushtarget = push.First().TargetMode;
                        });
                    }

                    var pop = enumValue.GetAttributesOfType<PopAttribute>();
                    if (pop != null && pop.Length >= 1)
                    {
                        attribute.Value.lexemes.ForEach(x => { x.IsPop = true; });
                    }
                }
            }

            if (!subLexers.Any())
            {
                subLexers = new Dictionary<string, IDictionary<IN, List<LexemeAttribute>>>();
                subLexers[ModeAttribute.DefaultLexerMode] = attributes.ToDictionary(x => x.Key,x => x.Value.lexemes);
            }

            return subLexers;
        }

        private static (GenericLexer<IN>.Config, GenericToken[]) GetConfigAndGenericTokens<IN>(
            IDictionary<IN, List<LexemeAttribute>> attributes)
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

        private static NodeCallback<GenericToken> GetCallbackSingle<IN>(IN token, bool doNotIgnore, int channel)
            where IN : struct
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

        private static NodeCallback<GenericToken> GetCallbackMulti<IN>(IN token, bool doNotIgnore, int channel)
            where IN : struct
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


        private static BuildResult<ILexer<IN>> BuildGenericSubLexers<IN>(
            Dictionary<IN, (List<LexemeAttribute>,List<LexemeLabelAttribute>)> attributes,
            Action<IN, LexemeAttribute, GenericLexer<IN>> extensionBuilder, BuildResult<ILexer<IN>> result, string lang,
            IList<string> explicitTokens = null) where IN : struct
        {
            GenericLexer<IN> genLexer = null;
            var subLexers = GetSubLexers(attributes);
            foreach (var subLexer in subLexers)
            {
                BuildResult<ILexer<IN>> b = null;
                if (subLexer.Key == ModeAttribute.DefaultLexerMode)
                {
                    b = BuildGenericLexer(subLexer.Value, extensionBuilder, result, lang, explicitTokens);
                }
                else
                {
                    b = BuildGenericLexer(subLexer.Value, extensionBuilder, result, lang, null);
                }

                var currentGenericLexer = b.Result as GenericLexer<IN>;
                if (genLexer == null)
                {
                    genLexer = currentGenericLexer;
                }

                currentGenericLexer.FSMBuilder.Fsm.Mode = subLexer.Key;

                genLexer.SubLexersFsm[subLexer.Key] = currentGenericLexer.FSMBuilder.Fsm;
            }

            result.Result = genLexer;

            return result;
        }


        private static BuildResult<ILexer<IN>> BuildGenericLexer<IN>(IDictionary<IN, List<LexemeAttribute>> attributes,
            Action<IN, LexemeAttribute, GenericLexer<IN>> extensionBuilder, BuildResult<ILexer<IN>> result, string lang,
            IList<string> explicitTokens = null) where IN : struct
        {
            result = CheckStringAndCharTokens<IN>(attributes, result, lang);
            var (config, tokens) = GetConfigAndGenericTokens<IN>(attributes);


            config.ExtensionBuilder = extensionBuilder;
            var lexer = new GenericLexer<IN>(config, tokens);
            var Extensions = new Dictionary<IN, LexemeAttribute>();
            
            var doubleLexeme = attributes.Values.SelectMany(x => x).FirstOrDefault(x => x.IsDouble);
            
            foreach (var pair in attributes)
            {
                var tokenID = pair.Key;

                var lexemes = pair.Value;
                // first configure identifiers as they can be used for keywords
                foreach (var lexeme in lexemes.Where(x => x.GenericToken == GenericToken.Identifier))
                {
                    lexer.AddLexeme(lexeme.GenericToken, tokenID);
                }

               
                
                foreach (var lexeme in lexemes.Where(x => x.GenericToken != GenericToken.Identifier))
                {
                    try
                    {
                        if (lexeme.IsStaticGeneric)
                        {
                            lexer.AddLexeme(lexeme.GenericToken, tokenID);
                        }

                        if (lexeme.IsDouble)
                        {
                            string separator = ".";
                            if (lexeme.GenericTokenParameters != null && lexeme.GenericTokenParameters.Any())
                            {
                                separator = lexeme.GenericTokenParameters[0];
                            }

                            lexer.AddDouble(tokenID, separator, result);
                            lexer.AddLexeme(lexeme.GenericToken, tokenID);
                        }

                        if (lexeme.IsHexa)
                        {
                            string prefix = "0x";
                            if (lexeme.GenericTokenParameters != null && lexeme.GenericTokenParameters.Any())
                            {
                               prefix = lexeme.GenericTokenParameters[0];
                            }

                            lexer.AddHexa(tokenID, prefix, result);
                        }

                        if (lexeme.IsKeyWord)
                        {
                            foreach (var param in lexeme.GenericTokenParameters)
                            {
                                lexer.AddKeyWord(tokenID, 
                                    keyword:param, 
                                    isPop:lexeme.IsPop, 
                                    isPush:lexeme.IsPush, 
                                    isExplicit:false,
                                    mode:lexeme.Pushtarget,
                                    result);
                            }
                        }

                        if (lexeme.IsSugar)
                        {
                            foreach (var param in lexeme.GenericTokenParameters)
                            {
                                lexer.AddSugarLexem(tokenID, result, param, lexeme.IsLineEnding, lexeme.Channel);
                            }
                        }

                        if (lexeme.IsUpTo)
                        {
                            lexer.AddUpTo(tokenID, result, lexeme.GenericTokenParameters);
                        }

                        if (lexeme.IsString)
                        {
                            var (delimiter, escape, doEscape) = GetDelimiters(lexeme, "\"", "\\", true);
                            lexer.AddStringLexem(tokenID, result, delimiter, escape, doEscape);
                        }

                        if (lexeme.IsChar)
                        {
                            var (delimiter, escape, doEscape) = GetDelimiters(lexeme, "'", "\\", true);
                            lexer.AddCharLexem(tokenID, result, delimiter, escape);
                        }

                        if (lexeme.IsExtension)
                        {
                            Extensions[tokenID] = lexeme;
                        }

                        if (lexeme.IsPush)
                        {
                            lexer.FSMBuilder.Push(lexeme.Pushtarget);
                        }

                        if (lexeme.IsPop)
                        {
                            lexer.FSMBuilder.Pop();
                        }

                        if (lexeme.IsDate)
                        {
                            DateFormat format = DateFormat.DDMMYYYY;
                            if (lexeme.GenericTokenParameters != null && lexeme.GenericTokenParameters.Any() && !Enum.TryParse<DateFormat>(lexeme.GenericTokenParameters[0], out format))
                            {
                                format = DateFormat.DDMMYYYY;
                            }

                            lexer.AddDate(tokenID, format, lexeme.GenericTokenParameters[1].First(), doubleLexeme, result);
                            lexer.AddLexeme(lexeme.GenericToken, tokenID);
                        }
                    }
                    catch (Exception e)
                    {
                        result.AddInitializationError(ErrorLevel.FATAL, e.Message,
                            ErrorCodes.LEXER_UNKNOWN_ERROR);
                    }
                }
            }


            AddExtensions<IN>(Extensions, extensionBuilder, lexer);


            var allComments = GetCommentsAttribute<IN>(result, lang);
            var CommentsForSubLexer = allComments.Where(x => attributes.Keys.ToList().Contains(x.Key))
                .ToDictionary(x => x.Key, x => x.Value);
            if (!result.IsError)
            {
                foreach (var comment in CommentsForSubLexer)
                {
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
                            fsmBuilder.CallBack(GetCallbackSingle<IN>(comment.Key, commentAttr.DoNotIgnore,
                                commentAttr.Channel));
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
                            fsmBuilder.CallBack(GetCallbackMulti(comment.Key, commentAttr.DoNotIgnore,
                                commentAttr.Channel));
                        }
                    }
                }

                if (explicitTokens != null)
                {
                    foreach (var explicitToken in explicitTokens)
                    {
                        var fsmBuilder = lexer.FSMBuilder;

                        if (!fsmBuilder.Marks.Any(mark => mark.Key == GenericLexer<IN>.in_identifier))
                        {
                            // no identifier pattern has been defined. Creating a default one to allow explicit keyword tokens
                            (lexer as GenericLexer<IN>).InitializeIdentifier(new GenericLexer<IN>.Config()
                                { IdType = IdentifierType.Alpha, HasImplicitIdentifier = true});
                            config.HasImplicitIdentifier = true;
                            lexer.LexerConfig.HasImplicitIdentifier = true;
                        }

                        var x = fsmBuilder.Fsm.Run(explicitToken, new LexerPosition());
                        if (x.IsSuccess)
                        {
                            var t = fsmBuilder.Marks;
                            var y = fsmBuilder.Marks.FirstOrDefault(k => k.Value == x.NodeId);
                            if (y.Key == GenericLexer<IN>.in_identifier) // explicit keyword
                            {
                                var resultx = new BuildResult<ILexer<IN>>();
                                result.Errors.AddRange(resultx.Errors);
                                lexer.AddKeyWord(default(IN), 
                                    keyword:explicitToken, 
                                    isPop:false, 
                                    isPush:false,
                                    isExplicit:true, 
                                    mode:ModeAttribute.DefaultLexerMode,
                                    resultx);
                            }
                            else
                            {
                                var resulty = new BuildResult<ILexer<IN>>();
                                result.Errors.AddRange(resulty.Errors);
                                lexer.AddSugarLexem(default(IN), resulty, explicitToken);
                            }
                        }
                        else
                        {
                            var resulty = new BuildResult<ILexer<IN>>();
                            result.Errors.AddRange(resulty.Errors);
                            lexer.AddSugarLexem(default(IN), resulty, explicitToken);
                        }
                    }
                }
            }

            result.Result = lexer;
            return result;
        }


        private static (string delimiter, string escape, bool doEscape) GetDelimiters(LexemeAttribute lexeme, string delimiter,
            string escape, bool doEscape)
        {
            if (lexeme.HasGenericTokenParameters)
            {
                delimiter = lexeme.GenericTokenParameters[0];
                if (lexeme.GenericTokenParameters.Length > 1)
                {
                    escape = lexeme.GenericTokenParameters[1];
                }
                if (lexeme.GenericTokenParameters.Length > 2)
                {
                    Boolean.TryParse(lexeme.GenericTokenParameters[2], out doEscape);
                }
            }

            return (delimiter, escape, doEscape);
        }

        private static BuildResult<ILexer<IN>> CheckStringAndCharTokens<IN>(
            IDictionary<IN, List<LexemeAttribute>> attributes, BuildResult<ILexer<IN>> result, string lang)
            where IN : struct
        {
            var allLexemes = attributes.Values.SelectMany<List<LexemeAttribute>, LexemeAttribute>(a => a);

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
                    I18N.Instance.GetText(lang, I18NMessage.DuplicateStringCharDelimiters, duplicate.Element,
                        duplicate.Counter.ToString()),
                    ErrorCodes.LEXER_DUPLICATE_STRING_CHAR_DELIMITERS));
            }

            return result;
        }


        private static Dictionary<IN, List<CommentAttribute>> GetCommentsAttribute<IN>(BuildResult<ILexer<IN>> result,
            string lang) where IN : struct
        {
            var attributes = new Dictionary<IN, List<CommentAttribute>>();

            var values = Enum.GetValues(typeof(IN));
            foreach (Enum value in values)
            {
                var tokenID = (IN)(object)value;
                var enumAttributes = value.GetAttributesOfType<CommentAttribute>();
                if (enumAttributes != null && enumAttributes.Any())
                    attributes[tokenID] = enumAttributes.ToList();
            }

            var commentCount = attributes.Values.Select(l =>
                l?.Count(attr => attr.GetType() == typeof(CommentAttribute)) ?? 0).Sum();
            var multiLineCommentCount = attributes.Values.Select(l =>
                l?.Count(attr => attr.GetType() == typeof(MultiLineCommentAttribute)) ?? 0).Sum();
            var singleLineCommentCount = attributes.Values.Select(l =>
                l?.Count(attr => attr.GetType() == typeof(SingleLineCommentAttribute)) ?? 0).Sum();

            if (commentCount > 1)
            {
                result.AddError(new LexerInitializationError(ErrorLevel.FATAL,
                    I18N.Instance.GetText(lang, I18NMessage.TooManyComment),
                    ErrorCodes.LEXER_TOO_MANY_COMMNENT));
            }

            if (multiLineCommentCount > 1)
            {
                result.AddError(new LexerInitializationError(ErrorLevel.FATAL,
                    I18N.Instance.GetText(lang, I18NMessage.TooManyMultilineComment),
                    ErrorCodes.LEXER_TOO_MANY_MULTILINE_COMMNENT));
            }

            if (singleLineCommentCount > 1)
            {
                result.AddError(new LexerInitializationError(ErrorLevel.FATAL,
                    I18N.Instance.GetText(lang, I18NMessage.TooManySingleLineComment),
                    ErrorCodes.LEXER_TOO_MANY_SINGLELINE_COMMNENT));
            }

            if (commentCount > 0 && (multiLineCommentCount > 0 || singleLineCommentCount > 0))
            {
                result.AddError(new LexerInitializationError(ErrorLevel.FATAL,
                    I18N.Instance.GetText(lang, I18NMessage.CannotMixCommentAndSingleOrMulti),
                    ErrorCodes.LEXER_CANNOT_MIX_COMMENT_AND_SINGLE_OR_MULTI));
            }

            return attributes;
        }

        private static void AddExtensions<IN>(Dictionary<IN, LexemeAttribute> extensions,
            Action<IN, LexemeAttribute, GenericLexer<IN>> extensionBuilder, GenericLexer<IN> lexer) where IN : struct
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