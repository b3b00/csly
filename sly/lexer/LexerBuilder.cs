using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using sly.buildresult;
using sly.lexer.fsm;
using sly.parser;
using sly.parser.generator;

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
            var grouped = values.Cast<IN>().GroupBy(x => x).ToList();
            foreach (var group in grouped)
            {
                
                var v = group.Key;
                if (group.Count() > 1)
                {
                 
                    Enum enumValue = Enum.Parse(typeof(IN), v.ToString()) as Enum;
                    int intValue = Convert.ToInt32(enumValue); // x is the integer value of enum
                    
                    result.AddError(new LexerInitializationError(ErrorLevel.FATAL,
                        $"int value {intValue} is used {group.Count()} times in lexer enum   {typeof(IN)}",ErrorCodes.LEXER_SAME_VALUE_USED_MANY_TIME));
                    
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
                    var islandAttributes = value.GetAttributesOfType<IslandAttribute>();
                    if (enumAttributes.Length == 0 && singleCommentAttributes.Length == 0 &&
                        multiCommentAttributes.Length == 0 && commentAttributes.Length == 0 && 
                        islandAttributes.Length == 0)
                    {
                        result?.AddError(new LexerInitializationError(ErrorLevel.WARN,
                            $"token {tokenID} in lexer definition {typeof(IN).FullName} does not have Lexeme",ErrorCodes.NOT_AN_ERROR));
                    }
                    else
                    {
                        attributes[tokenID] = enumAttributes.ToList();
                    }
                }
            }

            return attributes;
        }

        public static BuildResult<ILexer<IN>> BuildLexer<IN>(BuildExtension<IN> extensionBuilder = null) where IN : struct
        {
            return BuildLexer(new BuildResult < ILexer < IN >>() , extensionBuilder);
        }

        public static BuildResult<ILexer<IN>> BuildLexer<IN>(BuildResult<ILexer<IN>> result,
            BuildExtension<IN> extensionBuilder = null) where IN : struct
        {
            var attributes = GetLexemes(result);
            if (!result.IsError)
            {
                result = Build(attributes, result, extensionBuilder);
            }

            return result;
        }


        private static BuildResult<ILexer<IN>> Build<IN>(Dictionary<IN, List<LexemeAttribute>> attributes,
            BuildResult<ILexer<IN>> result, BuildExtension<IN> extensionBuilder = null) where IN : struct
        {
            var hasRegexLexemes = IsRegexLexer(attributes);
            var hasGenericLexemes = IsGenericLexer(attributes);

            if (hasGenericLexemes && hasRegexLexemes)
            {
                result.AddError(new LexerInitializationError(ErrorLevel.ERROR,
                    "cannot mix Regex lexemes and Generic lexemes in same lexer",ErrorCodes.LEXER_CANNOT_MIX_GENERIC_AND_REGEX));
            }
            else
            {
                if (hasRegexLexemes)
                    result = BuildRegexLexer(attributes, result);
                else if (hasGenericLexemes) result = BuildGenericLexer(attributes, extensionBuilder, result);
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
                .Any(lexeme => lexeme.GenericToken != default);
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
                            lexer.AddDefinition(new TokenDefinition<IN>(tokenID, lexeme.Pattern, channel:lexeme.Channel ?? Channels.Main, isIgnored:lexeme.IsSkippable,
                                isEndOfLine:lexeme.IsLineEnding));
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

        private static NodeCallback<GenericToken> GetCommentCallbackSingle<IN>(IN token, bool doNotIgnore, int channel) where IN : struct
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

        private static NodeCallback<GenericToken> GetCommentCallbackMulti<IN>(IN token, bool doNotIgnore, int channel) where IN : struct
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
        
        
        private static NodeCallback<GenericToken> GetIslandCallback<IN>(IN token, IslandAttribute island) where IN : struct
        {
            NodeCallback<GenericToken> callback = match =>
            {
                match.Properties[GenericLexer<IN>.DerivedToken] = token;
                match.Result.IsComment = false;
                match.Result.CommentType = CommentType.No;
                match.Result.IsIsland = true;
                match.Result.IslandType = island.IslandType;
                match.Result.MultiLineIslandEnd = island.MultiLineIslandEnd;
                match.Result.Channel = island.Channel;
                return match;
            };
            return callback;
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
                            lexer.AddLexeme(lexeme.GenericToken, tokenID,lexeme.Channel);
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
                            lexer.AddStringLexem(tokenID, result, delimiter, escape, lexeme.Channel ?? 0);
                        }

                        if (lexeme.IsChar)
                        {
                            var (delimiter, escape) = GetDelimiters(lexeme, "'", "\\");
                            lexer.AddCharLexem(tokenID, result, delimiter, escape, lexeme.Channel ?? 0);
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

            AddExtensions(Extensions, extensionBuilder, lexer);

            var comments = GetCommentsAttribute(result);
            if (!result.IsError)
            {
                foreach (var comment in comments)
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
                            fsmBuilder.CallBack(GetCommentCallbackSingle(comment.Key,commentAttr.DoNotIgnore,commentAttr.Channel));
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
                            fsmBuilder.CallBack(GetCommentCallbackMulti(comment.Key,commentAttr.DoNotIgnore,commentAttr.Channel));
                        }
                    }
                }
            }

            var islands = GetIslandsAttribute(result);
            if (!result.IsError)
            {
                foreach (var island in islands)
                {
                    foreach (var islandAttr in island.Value)
                    {
                        
                        Func<Token<IN>,Token<IN>> callback = token =>
                        {
                            // TODO parse Value and store result in ParsedValue
                            token.ParsedValue = $"this is a parsed token [{token.Value}]";
                            return token;
                        };
                        
                        var fsmBuilder = lexer.FSMBuilder;

                        var hasSingleLine = !string.IsNullOrWhiteSpace(islandAttr.SingleLineIslandStart);
                        if (hasSingleLine)
                        {
                            lexer.SingleLineComment = islandAttr.SingleLineIslandStart;

                            fsmBuilder.GoTo(GenericLexer<IN>.start);
                            fsmBuilder.ConstantTransition(islandAttr.SingleLineIslandStart);
                            fsmBuilder.Mark(GenericLexer<IN>.single_line_comment_start);
                            fsmBuilder.End(GenericToken.Island);
                            fsmBuilder.CallBack(GetIslandCallback(island.Key, islandAttr));
                        }

                        var hasMultiLine = !string.IsNullOrWhiteSpace(islandAttr.MultiLineIslandStart);
                        if (hasMultiLine)
                        {
                            lexer.MultiLineCommentStart = islandAttr.MultiLineIslandStart;
                            lexer.MultiLineCommentEnd = islandAttr.MultiLineIslandEnd;

                            fsmBuilder.GoTo(GenericLexer<IN>.start);
                            fsmBuilder.ConstantTransition(islandAttr.MultiLineIslandStart);
                            fsmBuilder.Mark(GenericLexer<IN>.multi_line_comment_start);
                            fsmBuilder.End(GenericToken.Island);
                            fsmBuilder.CallBack(GetIslandCallback(island.Key, islandAttr));
                        }
                    }
                }
            }

            var subParsers = GetSubParserAttributes(result);
            if (!result.IsError)
            {
                foreach (var subParser in subParsers)
                {
                    IN token = subParser.Key;
                    SubParserAttribute subParserAttribute = subParser.Value;
                    
                    
                        var parserResult = CreateParserMethod(subParserAttribute.VisitorType,
                            subParserAttribute.LexerType,
                            subParserAttribute.OutputType, subParserAttribute.StartingRule) ;
                    

                    if (parserResult.IsOk)
                    {
                        var parser = parserResult.Result;
                        lexer.AddSubParserCallBack(token, (Token<IN> tokenToParse) =>
                        {
                            string source = tokenToParse.Value;
                            var parsed = parser.parseMethod.Invoke(parser.parser,
                                new object[] {source, subParserAttribute.StartingRule});

                            var resultType = parsed.GetType();
                            var propOk = resultType.GetProperty("IsOk");
                            var subParseOk = propOk.GetValue(parsed) as bool?;
                            if (subParseOk.HasValue && subParseOk.Value)
                            {
                                
                            
                            var propResult = resultType.GetProperty("Result");
                            var subParseResult = propResult.GetValue(parsed);                            
                            
                            tokenToParse.ParsedValue = subParseResult;
                            return new SubParserResult<IN>() {token = tokenToParse};

                            }
                            else
                            {
                                var propErrors = resultType.GetProperty("Errors");
                                var errors = propErrors.GetValue(parsed) as List<ParseError>;
                                ;
                                return new SubParserResult<IN>() {Errors = errors};
                            }
                        });
                    }
                    else
                    {
                        parserResult.Errors.ForEach((x => x.Message = $"SubParser ({token} : {subParserAttribute.LexerType}/{subParserAttribute.VisitorType})definition error Error on subParser for token {token}: {x.Message}"));
                        result.AddErrors(parserResult.Errors);
                    }

                    if (result.IsError)
                    {
                        return result;
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
                    $"char or string lexeme dilimiter {duplicate.Element} is used {duplicate.Counter} times. This will results in lexing conflicts",
                    ErrorCodes.LEXER_DUPLICATE_STRING_CHAR_DELIMITERS));
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
                result.AddError(new LexerInitializationError(ErrorLevel.FATAL, "too many comment lexem",ErrorCodes.LEXER_TOO_MANY_COMMNENT));
            }

            if (multiLineCommentCount > 1)
            {
                result.AddError(new LexerInitializationError(ErrorLevel.FATAL, "too many multi-line comment lexem",ErrorCodes.LEXER_TOO_MANY_MULTILINE_COMMNENT));
            }

            if (singleLineCommentCount > 1)
            {
                result.AddError(new LexerInitializationError(ErrorLevel.FATAL, "too many single-line comment lexem",ErrorCodes.LEXER_TOO_MANY_SINGLELINE_COMMNENT));
            }

            if (commentCount > 0 && (multiLineCommentCount > 0 || singleLineCommentCount > 0))
            {
                result.AddError(new LexerInitializationError(ErrorLevel.FATAL, "comment lexem can't be used together with single-line or multi-line comment lexems",ErrorCodes.LEXER_CANNOT_MIX_COMMENT_AND_SINGLE_OR_MULTI));
            }

            return attributes;
        }

        
        private static Dictionary<IN, List<IslandAttribute>> GetIslandsAttribute<IN>(BuildResult<ILexer<IN>> result) where IN : struct
        {
            var attributes = new Dictionary<IN, List<IslandAttribute>>();

            var values = Enum.GetValues(typeof(IN));
            foreach (Enum value in values)
            {
                var tokenID = (IN) (object) value;
                var enumAttributes = value.GetAttributesOfType<IslandAttribute>();
                if (enumAttributes != null && enumAttributes.Any()) attributes[tokenID] = enumAttributes.ToList();
            }

            var commentCount = attributes.Values.Select(l => l?.Count(attr => attr.GetType() == typeof(IslandAttribute)) ?? 0).Sum();
            var multiLineCommentCount = attributes.Values.Select(l => l?.Count(attr => attr.GetType() == typeof(MultiLineIslandAttribute)) ?? 0).Sum();
            var singleLineCommentCount = attributes.Values.Select(l => l?.Count(attr => attr.GetType() == typeof(SingleLineIslandAttribute)) ?? 0).Sum();


            return attributes;
        }

        private static Dictionary<IN, SubParserAttribute> GetSubParserAttributes<IN>(BuildResult<ILexer<IN>> result) where IN : struct
        {
            var attributes = new Dictionary<IN, SubParserAttribute>();

            var values = Enum.GetValues(typeof(IN));
            foreach (Enum value in values)
            {
                var tokenID = (IN) (object) value;
                var enumAttributes = value.GetAttributesOfType<SubParserAttribute>();
                
                if (enumAttributes != null  && enumAttributes.Length > 1)
                {
                    result.AddError(new LexerInitializationError(ErrorLevel.FATAL, $"a Lexem {value} can not have many ({enumAttributes.Length}) SubParserAttributes lexems",ErrorCodes.LEXER_TOO_MANY_SUBPARSER));
                }
                
                if (enumAttributes != null && enumAttributes.Length == 1) attributes[tokenID] = enumAttributes.First();
                
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
        
        
        private static BuildResult<(object parser, MethodInfo parseMethod)> CreateParserMethod(Type visitorType, Type lexerType, Type astType, string startingRule)
        {
            BuildResult<(object parser, MethodInfo parseMethod)> result =
                new BuildResult<(object parser, MethodInfo parseMethod)>();
            try
            {
                

                var parserInstance = Activator.CreateInstance(visitorType);


                var buildertype = typeof(ParserBuilder<,>);
                var builderT = buildertype.MakeGenericType(lexerType, astType);
                var builder = Activator.CreateInstance(builderT);

                var method = builderT.GetMethod("BuildParser");
                var parserResult = method.Invoke(builder,
                    new Object[] {parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, startingRule, null});
                ;
                var realParserResultType = parserResult.GetType();

                var propErrors = realParserResultType.GetProperty("Errors");
                List<InitializationError> errors = propErrors.GetValue(parserResult) as List<InitializationError>;

                if (errors.Any())
                {
                    result.Result = (null, null);
                    result.AddErrors(errors);
                    return result;
                }
                
                var propResult = realParserResultType.GetProperty("Result");
                var parser = propResult.GetValue(parserResult);
                var realParserType = parser.GetType();

                


                MethodInfo parseMethod = realParserType.GetMethod("Parse", new Type[] {typeof(string), typeof(string)});
                ;
                result.Result = (parser, parseMethod);
                result.AddErrors(errors);
                return result;
            }
            catch (Exception e)
            {
                if (e.InnerException != null && e.InnerException is ParserConfigurationException confException)
                {
                    result.Result = (null,null);
                    var error = new InitializationError(ErrorLevel.FATAL, confException.Message,
                        ErrorCodes.LEXER_SUB_PARSER_INITIALIZATION_ERROR);
                    result.AddError(error);
                    return result;
                }
                return null;
            }
        }
    }
}