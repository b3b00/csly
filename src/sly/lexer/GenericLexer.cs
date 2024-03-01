using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using sly.buildresult;
using sly.i18n;
using sly.lexer.fsm;
using sly.lexer.fsm.transitioncheck;

namespace sly.lexer
{
    public class GenericLexer<IN> : ILexer<IN> where IN : struct
    {
        public class Config
        {
            public Config()
            {
                IdType = IdentifierType.Alpha;
                IgnoreEOL = true;
                IgnoreWS = true;
                WhiteSpace = new[] { ' ', '\t' };
            }

            public IdentifierType IdType { get; set; }

            public bool IgnoreEOL { get; set; }

            public bool IgnoreWS { get; set; }

            public char[] WhiteSpace { get; set; }

            public bool KeyWordIgnoreCase { get; set; }

            public bool IndentationAware { get; set; }

            public string Indentation { get; set; }

            public IEnumerable<char[]> IdentifierStartPattern { get; set; }

            public IEnumerable<char[]> IdentifierRestPattern { get; set; }

            public Action<IN, LexemeAttribute, GenericLexer<IN>> ExtensionBuilder { get; set; }

            public IEqualityComparer<string> KeyWordComparer =>
                KeyWordIgnoreCase ? StringComparer.OrdinalIgnoreCase : null;
        }

        public LexerPostProcess<IN> LexerPostProcess { get; set; }

        public Dictionary<IN, Dictionary<string, string>> LexemeLabels { get; set; }
        
        public string I18n { get; set; }

        public const string in_string = "in_string";
        public const string string_end = "string_end";
        public const string start_char = "start_char";
        public const string escapeChar_char = "escapeChar_char";
        public const string unicode_char = "unicode_char";
        public const string in_char = "in_char";
        public const string end_char = "char_end";
        public const string start = "start";
        public const string in_int = "in_int";
        public const string start_double = "start_double";
        public const string in_double = "in_double";
        public const string in_identifier = "in_identifier";
        public const string token_property = "token";
        public const string DerivedToken = "derivedToken";
        public const string defaultIdentifierKey = "identifier";
        public const string escape_string = "escape_string";
        public const string escape_char = "escape_char";
        public const string in_up_to = "in_upto";
        public const string end_date = "end_date";


        public const string single_line_comment_start = "single_line_comment_start";

        public const string multi_line_comment_start = "multi_line_comment_start";

        protected readonly
            Dictionary<GenericToken, Dictionary<string, (IN tokenId, bool isPop, bool isPush, string mode)>>
            derivedTokens;

        protected IN doubleDerivedToken;
        protected char EscapeStringDelimiterChar;

        protected readonly Action<IN, LexemeAttribute, GenericLexer<IN>> ExtensionBuilder;
        public FSMLexerBuilder<GenericToken> FSMBuilder { get; private set; }
        protected IN identifierDerivedToken;
        protected IN intDerivedToken;


        protected FSMLexer<GenericToken> TempLexerFsm;

        internal IDictionary<string, FSMLexer<GenericToken>> SubLexersFsm { get; set; }

        protected int StringCounter;
        protected int CharCounter;
        protected int upToCounter;


        protected Dictionary<IN, Func<Token<IN>, Token<IN>>> CallBacks =
            new Dictionary<IN, Func<Token<IN>, Token<IN>>>();

        protected char StringDelimiterChar;

        private readonly IEqualityComparer<string> KeyWordComparer;

        public GenericLexer(IdentifierType idType = IdentifierType.Alpha,
            Action<IN, LexemeAttribute, GenericLexer<IN>> extensionBuilder = null,
            params GenericToken[] staticTokens)
            : this(new Config { IdType = idType, ExtensionBuilder = extensionBuilder }, staticTokens)
        {
        }

        public GenericLexer(Config config, GenericToken[] staticTokens)
        {
            derivedTokens =
                new Dictionary<GenericToken, Dictionary<string, (IN tokenId, bool isPop, bool isPush, string mode)>>();
            ExtensionBuilder = config.ExtensionBuilder;
            KeyWordComparer = config.KeyWordComparer;
            SubLexersFsm = new Dictionary<string, FSMLexer<GenericToken>>();
            InitializeStaticLexer(config, staticTokens);
        }

        public string SingleLineComment { get; set; }

        public string MultiLineCommentStart { get; set; }

        public string MultiLineCommentEnd { get; set; }

        public void AddCallBack(IN token, Func<Token<IN>, Token<IN>> callback)
        {
            CallBacks[token] = callback;
        }

        public void AddDefinition(TokenDefinition<IN> tokenDefinition)
        {
        }


        public LexerResult<IN> Tokenize(string source)
        {
            var memorySource = new ReadOnlyMemory<char>(source.ToCharArray());
            return Tokenize(memorySource);
        }

        public LexerResult<IN> Tokenize(ReadOnlyMemory<char> source)
        {
            Stack<FSMLexer<GenericToken>> lexersStack = new Stack<FSMLexer<GenericToken>>();

            FSMLexer<GenericToken> LexerFsm = SubLexersFsm[ModeAttribute.DefaultLexerMode];
            lexersStack.Push(LexerFsm);
            LexerPosition position = new LexerPosition();

            var tokens = new List<Token<IN>>();
            string src = source.ToString();

            var r = LexerFsm.Run(source, new LexerPosition());
            LexerFsm = SetLexerMode(r, lexersStack);

            var ignored = r.IgnoredTokens.Select(x =>
                new Token<IN>(default(IN), x.SpanValue, x.Position, x.CommentType, x.Channel)).ToList();
            tokens.AddRange(ignored);


            switch (r.IsSuccess)
            {
                case false when !r.IsEOS:
                {
                    var result = r.Result;
                    var error = new LexicalError(result.Position.Line, result.Position.Column, result.CharValue, I18n);
                    return new LexerResult<IN>(error, tokens);
                }
                case true when r.Result.IsComment:
                    position = r.NewPosition;
                    position = ConsumeComment(r.Result, source, position);
                    break;
                case true when !r.Result.IsComment:
                    position = r.NewPosition;
                    break;
            }

            while (r.IsSuccess)
            {
                ComputePositionWhenIgnoringEOL(r, tokens, LexerFsm);
                var transcoded = Transcode(r);
                if (CallBacks.TryGetValue(transcoded.TokenID, out var callback))
                {
                    transcoded = callback(transcoded);
                }

                if (transcoded.IsLineEnding)
                {
                    ComputePositionWhenIgnoringEOL(r, tokens, LexerFsm);
                }

                if (r.IsUnIndent && r.UnIndentCount > 1)
                {
                    for (int i = 1; i < r.UnIndentCount; i++)
                    {
                        tokens.Add(transcoded);
                    }
                }

                tokens.Add(transcoded);
                r = LexerFsm.Run(source, position);
                LexerFsm = SetLexerMode(r, lexersStack);

                ignored = r.IgnoredTokens.Select(x =>
                    new Token<IN>(default(IN), x.SpanValue, x.Position, 
                        x.CommentType, x.Channel, x.IsWhiteSpace, x.DecimalSeparator)).ToList();
                tokens.AddRange(ignored);

                switch (r.IsSuccess)
                {
                    case false when !r.IsEOS:
                    {
                        if (r.IsIndentationError)
                        {
                            var result = r.Result;
                            var error = new IndentationError(result.Position.Line, result.Position.Column, I18n);
                            return new LexerResult<IN>(error, tokens);
                        }
                        else
                        {
                            var result = r.Result;
                            var error = new LexicalError(result.Position.Line, result.Position.Column, result.CharValue,
                                I18n);
                            return new LexerResult<IN>(error, tokens);
                        }
                    }
                    case true when r.Result.IsComment:
                        position = r.NewPosition;
                        position = ConsumeComment(r.Result, source, position);
                        break;
                    case true when !r.Result.IsComment:
                        position = r.NewPosition;
                        break;
                }
            }

            var eos = new Token<IN>();
            var prev = tokens.LastOrDefault<Token<IN>>();
            if (prev == null)
            {
                eos.Position = new LexerPosition(1, 0, 0);
            }
            else
            {
                eos.Position = new LexerPosition(prev.Position.Index + 1, prev.Position.Line,
                    prev.Position.Column + prev.Value.Length);
            }

            tokens.Add(eos);
            return new LexerResult<IN>(tokens);
        }

        private FSMLexer<GenericToken> SetLexerMode(FSMMatch<GenericToken> r, Stack<FSMLexer<GenericToken>> lexersStack)
        {
            FSMLexer<GenericToken> LexerFsm = lexersStack.Peek();

            if (!r.IsEOS)
            {
                if (r.IsPop)
                {
                    lexersStack.Pop();
                    LexerFsm = lexersStack.Peek();
                    r.NewPosition.Mode = LexerFsm.Mode;
                    return LexerFsm;
                }

                if (r.IsPush)
                {
                    LexerFsm = SubLexersFsm[r.NewPosition.Mode];
                    lexersStack.Push(LexerFsm);
                }
                else
                {
                    LexerFsm = SubLexersFsm[r.NewPosition.Mode];
                }
            }

            return LexerFsm;
        }

        private void ComputePositionWhenIgnoringEOL(FSMMatch<GenericToken> r, List<Token<IN>> tokens,
            FSMLexer<GenericToken> LexerFsm)
        {
            if (!LexerFsm.IgnoreEOL)
            {
                var newPosition = r.Result.Position.Clone();

                if (r.IsLineEnding) // only compute if token is eol
                {
                    var eols = tokens.Where(t => t.IsLineEnding).ToList();
                    int line = eols.Any() ? eols.Count : 0;
                    int column = 0;
                    int index = newPosition.Index;
                    r.NewPosition.Line = line + 1;
                    r.NewPosition.Column = column;
                }
            }
        }


        private void InitializeStaticLexer(Config config, GenericToken[] staticTokens)
        {
            FSMBuilder = new FSMLexerBuilder<GenericToken>();
            StringCounter = 0;

            // conf
            FSMBuilder
                .IgnoreWS(config.IgnoreWS)
                .WhiteSpace(config.WhiteSpace)
                .IgnoreEOL(config.IgnoreEOL)
                .Indentation(config.IndentationAware, config.Indentation);


            // start machine definition
            FSMBuilder.Mark(start);

            if (staticTokens.Contains(GenericToken.Identifier) ||
                staticTokens.Contains(GenericToken.KeyWord))
            {
                InitializeIdentifier(config);
            }

            // numeric
            if (staticTokens.Contains(GenericToken.Int) ||
                staticTokens.Contains(GenericToken.Double) || 
                staticTokens.Contains(GenericToken.Date)) 
            {
                FSMBuilder = FSMBuilder.GoTo(start)
                    .RangeTransition('0', '9')
                    .Mark(in_int)
                    .RangeTransitionTo('0', '9', in_int)
                    .End(GenericToken.Int);
            }

            TempLexerFsm = FSMBuilder.Fsm;
        }


        public void InitializeIdentifier(Config config)
        {
            // identifier
            if (config.IdType == IdentifierType.Custom)
            {
                var marked = false;
                foreach (var pattern in config.IdentifierStartPattern)
                {
                    FSMBuilder.GoTo(start);
                    if (pattern.Length == 1)
                    {
                        if (marked)
                        {
                            FSMBuilder.TransitionTo(pattern[0], in_identifier);
                        }
                        else
                        {
                            FSMBuilder.Transition(pattern[0]).Mark(in_identifier).End(GenericToken.Identifier);
                            marked = true;
                        }
                    }
                    else
                    {
                        if (marked)
                        {
                            FSMBuilder.RangeTransitionTo(pattern[0], pattern[1], in_identifier);
                        }
                        else
                        {
                            FSMBuilder.RangeTransition(pattern[0], pattern[1]).Mark(in_identifier)
                                .End(GenericToken.Identifier);
                            marked = true;
                        }
                    }
                }

                foreach (var pattern in config.IdentifierRestPattern)
                {
                    if (pattern.Length == 1)
                    {
                        FSMBuilder.TransitionTo(pattern[0], in_identifier);
                    }
                    else
                    {
                        FSMBuilder.RangeTransitionTo(pattern[0], pattern[1], in_identifier);
                    }
                }
            }
            else
            {
                FSMBuilder
                    .GoTo(start)
                    .RangeTransition('a', 'z')
                    .Mark(in_identifier)
                    .GoTo(start)
                    .RangeTransitionTo('A', 'Z', in_identifier)
                    .RangeTransitionTo('a', 'z', in_identifier)
                    .RangeTransitionTo('A', 'Z', in_identifier)
                    .End(GenericToken.Identifier);

                if (config.IdType == IdentifierType.AlphaNumeric || config.IdType == IdentifierType.AlphaNumericDash)
                {
                    FSMBuilder
                        .GoTo(in_identifier)
                        .RangeTransitionTo('0', '9', in_identifier);
                }

                if (config.IdType == IdentifierType.AlphaNumericDash)
                {
                    FSMBuilder
                        .GoTo(start)
                        .TransitionTo('_', in_identifier)
                        .TransitionTo('_', in_identifier)
                        .TransitionTo('-', in_identifier);
                }
            }
        }

        public void AddLexeme(GenericToken generic, IN token)
        {
            NodeCallback<GenericToken> callback = match =>
            {
                switch (match.Result.TokenID)
                {
                    case GenericToken.Identifier:
                    {
                        if (derivedTokens.TryGetValue(GenericToken.Identifier, out var possibleTokens))
                        {
                            if (possibleTokens.TryGetValue(match.Result.Value, out var possibleToken))
                                match.Properties[DerivedToken] = possibleToken.tokenId;
                            else
                                match.Properties[DerivedToken] = identifierDerivedToken;
                        }
                        else
                        {
                            match.Properties[DerivedToken] = identifierDerivedToken;
                        }

                        break;
                    }
                    case GenericToken.Int:
                    {
                        match.Properties[DerivedToken] = intDerivedToken;
                        break;
                    }
                    case GenericToken.Double:
                    {
                        match.Properties[DerivedToken] = doubleDerivedToken;
                        break;
                    }
                    default:
                    {
                        match.Properties[DerivedToken] = token;
                        break;
                    }
                }

                return match;
            };

            switch (generic)
            {
                case GenericToken.Int:
                {
                    intDerivedToken = token;
                    FSMBuilder.GoTo(in_int);
                    FSMBuilder.CallBack(callback);
                    break;
                }
                case GenericToken.Identifier:
                {
                    identifierDerivedToken = token;
                    FSMBuilder.GoTo(in_identifier);
                    FSMBuilder.CallBack(callback);
                    break;
                }
            }
        }

        public void AddLexeme(GenericToken genericToken, BuildResult<ILexer<IN>> result, IN token, bool isPop,
            bool isPush, string mode, string specialValue)
        {
            if (genericToken == GenericToken.SugarToken)
            {
                AddSugarLexem(token, result, specialValue);
            }

            if (!derivedTokens.TryGetValue(genericToken, out var tokensForGeneric))
            {
                if (genericToken == GenericToken.Identifier)
                {
                    tokensForGeneric =
                        new Dictionary<string, (IN tokenId, bool isPop, bool isPush, string mode)>(KeyWordComparer);
                }
                else
                {
                    tokensForGeneric = new Dictionary<string, (IN tokenId, bool isPop, bool isPush, string mode)>();
                }

                derivedTokens[genericToken] = tokensForGeneric;
            }

            tokensForGeneric[specialValue] = (token, isPop, isPush, mode);
        }

        public void AddDouble(IN token, string separator, BuildResult<ILexer<IN>> result)
        {
            NodeCallback<GenericToken> callback = match =>
            {
                IN derivedToken = token;


                match.Properties[DerivedToken] = derivedToken;

                return match;
            };

            var separatorChar = separator[0];

            FSMBuilder.GoTo(in_int)
                .Transition(separatorChar)
                .RangeTransition('0', '9')
                .Mark(in_double)
                .RangeTransitionTo('0', '9', in_double)
                .End(GenericToken.Double)
                .CallBack(callback);

            FSMBuilder.Fsm.DecimalSeparator = separatorChar;
        }

        public void AddDate(IN token, DateFormat format, char separator, LexemeAttribute doubleLexeme,
            BuildResult<ILexer<IN>> result)
        {
            if (doubleLexeme != null)
            {
                char decimalSeparator = (doubleLexeme.HasGenericTokenParameters)
                    ? doubleLexeme.GenericTokenParameters[0][0]
                    : '.';
                if (decimalSeparator == separator)
                {
                    AddDoubleWhenDoubleTokenPresent(token, format, separator);
                    return;
                }
            }
            AddDoubleWhenNoDoubleToken(token, format, separator);
        }

        private void AddDoubleWhenNoDoubleToken(IN token, DateFormat format, char separator)
        {
            TransitionPrecondition checkDate = delegate(ReadOnlyMemory<char> value)
            {
                if (format == DateFormat.DDMMYYYY)
                {
                    return value.Length == 3 && value.At(2) == separator;
                }
                else if (format == DateFormat.YYYYMMDD)
                {
                    return value.Length == 5 && value.At(4) == separator;
                }

                return false;
            };
            NodeCallback<GenericToken> callback = delegate(FSMMatch<GenericToken> match)
            {
                match.Properties[DerivedToken] = token;
                var elements = match.Result.Value.Split(separator);
                DateTime date;
                if (format == DateFormat.DDMMYYYY)
                {
                    date = new DateTime(int.Parse(elements[2]), int.Parse(elements[1]), int.Parse(elements[0]));
                }
                else
                {
                    date = new DateTime(int.Parse(elements[0]), int.Parse(elements[1]), int.Parse(elements[2]));
                }

                match.DateTimeValue = date;

                return match;
            };
            FSMBuilder.GoTo(in_int);
            FSMBuilder.Transition(separator, checkDate)
                .RepetitionTransition(2, "[0-9]")
                .Transition(separator);
            var repetitions = 2;
            switch (format)
            {
                case DateFormat.DDMMYYYY:
                {
                    repetitions = 4;
                    break;
                }
                case DateFormat.YYYYMMDD:
                {
                    repetitions = 2;
                    break;
                }
            }


            FSMBuilder.RepetitionTransition(repetitions, "[0-9]")
                .Mark(end_date)
                .End(GenericToken.Date)
                .CallBack(callback);
        }
        
        private void AddDoubleWhenDoubleTokenPresent(IN token, DateFormat format, char separator)
        {
            TransitionPrecondition checkDate = delegate(ReadOnlyMemory<char> value)
            {
                if (format == DateFormat.DDMMYYYY)
                {
                    bool match = value.Length == 6 && value.At(2) == separator && value.At(5) == separator; 
                    return match;
                }
                else if (format == DateFormat.YYYYMMDD)
                {
                    bool match = value.Length == 8 && value.At(4)  == separator && value.At(7)  == separator;
                    return match;
                }

                return false;
            };
            NodeCallback<GenericToken> callback = delegate(FSMMatch<GenericToken> match)
            {
                match.Properties[DerivedToken] = token;
                var elements = match.Result.Value.Split(separator);
                DateTime date;
                if (format == DateFormat.DDMMYYYY)
                {
                    date = new DateTime(int.Parse(elements[2]), int.Parse(elements[1]), int.Parse(elements[0]));
                }
                else
                {
                    date = new DateTime(int.Parse(elements[0]), int.Parse(elements[1]), int.Parse(elements[2]));
                }

                match.DateTimeValue = date;

                return match;
            };
            FSMBuilder.GoTo(in_double);
            FSMBuilder.Transition(separator, checkDate);
            var repetitions = 2;
            switch (format)
            {
                case DateFormat.DDMMYYYY:
                {
                    repetitions = 4;
                    break;
                }
                case DateFormat.YYYYMMDD:
                {
                    repetitions = 2;
                    break;
                }
            }
            
            FSMBuilder.RepetitionTransition(repetitions, "[0-9]")
                .Mark(end_date)
                .End(GenericToken.Date)
                .CallBack(callback);
        }

        public void AddKeyWord(IN token, string keyword, bool isPop, bool isPush, string mode,
            BuildResult<ILexer<IN>> result)
        {
            NodeCallback<GenericToken> callback = match =>
            {
                IN derivedToken = default;
                if (derivedTokens.TryGetValue(GenericToken.Identifier, out var derived))
                {
                    if (derived.TryGetValue(match.Result.Value,
                            out (IN tokenId, bool isPop, bool isPush, string mode) derived2))
                    {
                        derivedToken = derived2.tokenId;
                        match.IsPush = derived2.isPush;
                        match.IsPop = derived2.isPop;
                        match.NewPosition.Mode = derived2.mode ?? ModeAttribute.DefaultLexerMode;
                    }
                    else
                    {
                        derivedToken = identifierDerivedToken;
                    }
                }
                else
                {
                    derivedToken = identifierDerivedToken;
                }

                match.Properties[DerivedToken] = derivedToken;

                return match;
            };

            AddLexeme(GenericToken.Identifier, result, token, isPop, isPush, mode, keyword);
            var node = FSMBuilder.GetNode(in_identifier);
            if (!FSMBuilder.Fsm.HasCallback(node.Id))
            {
                FSMBuilder.GoTo(in_identifier).CallBack(callback);
            }
        }


        public ReadOnlyMemory<char> diffCharEscaper(char escapeStringDelimiterChar, char stringDelimiterChar,
            ReadOnlyMemory<char> stringValue)
        {
            var value = stringValue;
            string newValue = "";
            int i = 0;
            while (i < value.Length)
            {
                char current = value.At(i);
                if (current == escapeStringDelimiterChar)
                {
                    i++;
                }

                newValue += value.At(i);
                i++;

            }

            return newValue.AsMemory();
        }

        public ReadOnlyMemory<char> sameCharEscaper(char escapeStringDelimiterChar, char stringDelimiterChar,
            ReadOnlyMemory<char> stringValue)
        {
            var value = stringValue;
            int i = 1;
            bool substitutionHappened = false;
            bool escaping = false;
            string r = string.Empty;
            while (i < value.Length - 1)
            {
                char current = value.At<char>(i);
                if (current == escapeStringDelimiterChar && !escaping && i < value.Length - 2)
                {
                    escaping = true;
                    if (!substitutionHappened)
                    {
                        r = value.Slice(0, i).ToString();
                        substitutionHappened = true;
                    }
                }
                else
                {
                    if (escaping)
                    {
                        r += escapeStringDelimiterChar;
                        escaping = false;
                    }
                    else if (substitutionHappened)
                    {
                        r += current;
                    }
                }

                i++;
            }

            if (substitutionHappened)
            {
                r += value.At(value.Length - 1);
                value = r.AsMemory();
            }

            return value;
        }

        public void AddStringLexem(IN token, BuildResult<ILexer<IN>> result, string stringDelimiter,
            string escapeDelimiterChar = "\\")
        {
            if (string.IsNullOrEmpty(stringDelimiter) || stringDelimiter.Length > 1) {
                result.AddError(new LexerInitializationError(ErrorLevel.FATAL,
                    I18N.Instance.GetText(I18n, I18NMessage.StringDelimiterMustBe1Char, stringDelimiter,
                        token.ToString()),
                    ErrorCodes.LEXER_STRING_DELIMITER_MUST_BE_1_CHAR));
                return;
            }

            if (stringDelimiter.Length == 1 && char.IsLetterOrDigit(stringDelimiter[0]))
            {
                result.AddInitializationError(ErrorLevel.FATAL,
                    I18N.Instance.GetText(I18n, I18NMessage.StringDelimiterCannotBeLetterOrDigit, stringDelimiter,
                        token.ToString()),
                    ErrorCodes.LEXER_STRING_DELIMITER_CANNOT_BE_LETTER_OR_DIGIT);
                return;
            }

            if (string.IsNullOrEmpty(escapeDelimiterChar) || escapeDelimiterChar.Length > 1)
                result.AddInitializationError(ErrorLevel.FATAL,
                    I18N.Instance.GetText(I18n, I18NMessage.StringEscapeCharMustBe1Char, escapeDelimiterChar,
                        token.ToString()),
                    ErrorCodes.LEXER_STRING_ESCAPE_CHAR_MUST_BE_1_CHAR);
            if (escapeDelimiterChar.Length == 1 && char.IsLetterOrDigit(escapeDelimiterChar[0]))
                result.AddInitializationError(ErrorLevel.FATAL,
                    I18N.Instance.GetText(I18n, I18NMessage.StringEscapeCharCannotBeLetterOrDigit, escapeDelimiterChar,
                        token.ToString()),
                    ErrorCodes.LEXER_STRING_ESCAPE_CHAR_CANNOT_BE_LETTER_OR_DIGIT);

            StringDelimiterChar = (char)0;
            var stringDelimiterChar = (char)0;

            EscapeStringDelimiterChar = (char)0;
            var escapeStringDelimiterChar = (char)0;

            if (stringDelimiter.Length == 1)
            {
                StringCounter++;

                StringDelimiterChar = stringDelimiter[0];
                stringDelimiterChar = stringDelimiter[0];

                EscapeStringDelimiterChar = escapeDelimiterChar[0];
                escapeStringDelimiterChar = escapeDelimiterChar[0];
            }


            NodeCallback<GenericToken> callback = match =>
            {
                match.Properties[DerivedToken] = token;
                var value = match.Result.SpanValue;

                match.Result.SpanValue = value;

                match.StringDelimiterChar = stringDelimiterChar;
                if (stringDelimiterChar != escapeStringDelimiterChar)
                {
                    match.Result.SpanValue = diffCharEscaper(escapeStringDelimiterChar, stringDelimiterChar,
                        match.Result.SpanValue);
                }
                else
                {
                    match.Result.SpanValue = sameCharEscaper(escapeStringDelimiterChar, stringDelimiterChar,
                        match.Result.SpanValue);
                }

                return match;
            };

            if (stringDelimiterChar != escapeStringDelimiterChar)
            {
                FSMBuilder.GoTo(start);
                FSMBuilder.Transition(stringDelimiterChar)
                    .Mark(in_string + StringCounter)
                    .ExceptTransitionTo(new[] { stringDelimiterChar, escapeStringDelimiterChar },
                        in_string + StringCounter)
                    .Transition(escapeStringDelimiterChar)
                    .Mark(escape_string + StringCounter)
                    .ExceptTransitionTo(new[] { stringDelimiterChar }, in_string + StringCounter)
                    .GoTo(escape_string + StringCounter)
                    .TransitionTo(stringDelimiterChar, in_string + StringCounter)
                    .Transition(stringDelimiterChar)
                    .End(GenericToken.String)
                    .Mark(string_end + StringCounter)
                    .CallBack(callback);
                FSMBuilder.Fsm.StringDelimiter = stringDelimiterChar;
            }
            else
            {
                var exceptDelimiter = new[] { stringDelimiterChar };
                var escaped = "escaped_same";

                FSMBuilder.GoTo(start)
                    .Transition(stringDelimiterChar)
                    .Mark(in_string + StringCounter)
                    .ExceptTransitionTo(exceptDelimiter, in_string + StringCounter)
                    .Transition(stringDelimiterChar)
                    .Mark(escaped + StringCounter)
                    .End(GenericToken.String)
                    .CallBack(callback)
                    .TransitionTo(stringDelimiterChar, in_string + StringCounter)
                    .Transition(stringDelimiterChar);
            }
        }

        public void AddCharLexem(IN token, BuildResult<ILexer<IN>> result, string charDelimiter,
            string escapeDelimiterChar = "\\")
        {
            if (string.IsNullOrEmpty(charDelimiter) || charDelimiter.Length > 1)
                result.AddInitializationError(ErrorLevel.FATAL,
                    I18N.Instance.GetText(I18n, I18NMessage.CharDelimiterMustBe1Char, charDelimiter, token.ToString()),
                    ErrorCodes.LEXER_CHAR_DELIMITER_MUST_BE_1_CHAR);
            if (charDelimiter.Length == 1 && char.IsLetterOrDigit(charDelimiter[0]))
                result.AddInitializationError(ErrorLevel.FATAL,
                    I18N.Instance.GetText(I18n, I18NMessage.CharDelimiterCannotBeLetter, charDelimiter,
                        token.ToString()),
                    ErrorCodes.LEXER_CHAR_DELIMITER_CANNOT_BE_LETTER);

            if (string.IsNullOrEmpty(escapeDelimiterChar) || escapeDelimiterChar.Length > 1)
                result.AddInitializationError(ErrorLevel.FATAL,
                    I18N.Instance.GetText(I18n, I18NMessage.CharEscapeCharMustBe1Char, escapeDelimiterChar,
                        token.ToString()),
                    ErrorCodes.LEXER_CHAR_ESCAPE_CHAR_MUST_BE_1_CHAR);
            if (escapeDelimiterChar.Length == 1 && char.IsLetterOrDigit(escapeDelimiterChar[0]))
                result.AddInitializationError(ErrorLevel.FATAL,
                    I18N.Instance.GetText(I18n, I18NMessage.CharEscapeCharCannotBeLetterOrDigit, escapeDelimiterChar,
                        token.ToString()),
                    ErrorCodes.LEXER_CHAR_ESCAPE_CHAR_CANNOT_BE_LETTER_OR_DIGIT);

            CharCounter++;

            var charDelimiterChar = charDelimiter[0];

            var escapeChar = escapeDelimiterChar[0];


            NodeCallback<GenericToken> callback = match =>
            {
                match.Properties[DerivedToken] = token;
                var value = match.Result.SpanValue;

                match.Result.SpanValue = value;
                return match;
            };

            FSMBuilder.GoTo(start);
            FSMBuilder.Transition(charDelimiterChar)
                .Mark(start_char + "_" + CharCounter)
                .ExceptTransition(new[] { charDelimiterChar, escapeChar })
                .Mark(in_char + "_" + CharCounter)
                .Transition(charDelimiterChar)
                .Mark(end_char + "_" + CharCounter)
                .End(GenericToken.Char)
                .CallBack(callback)
                .GoTo(start_char + "_" + CharCounter)
                .Transition(escapeChar)
                .Mark(escapeChar_char + "_" + CharCounter)
                .ExceptTransitionTo(new[] { 'u' }, in_char + "_" + CharCounter)
                .CallBack(callback);
            FSMBuilder.Fsm.StringDelimiter = charDelimiterChar;

            // unicode transitions ?
            FSMBuilder = FSMBuilder.GoTo(escapeChar_char + "_" + CharCounter)
                .Transition('u')
                .Mark(unicode_char + "_" + CharCounter)
                .RepetitionTransitionTo(in_char + "_" + CharCounter, 4, "[0-9,a-z,A-Z]");
        }

        public void AddSugarLexem(IN token, BuildResult<ILexer<IN>> buildResult, string specialValue,
            bool isLineEnding = false, int? channel = null)
        {
            if (char.IsLetter(specialValue[0]))
            {
                buildResult.AddInitializationError(ErrorLevel.FATAL,
                    I18N.Instance.GetText(I18n, I18NMessage.SugarTokenCannotStartWithLetter, specialValue,
                        token.ToString()),
                    ErrorCodes.LEXER_SUGAR_TOKEN_CANNOT_START_WITH_LETTER);
                return;
            }

            NodeCallback<GenericToken> callback = match =>
            {
                match.Properties[DerivedToken] = token;
                match.Result.Channel = channel ?? Channels.Main;
                return match;
            };

            Func<int, TransitionPrecondition> precond = (i) =>
            {
                return (value) => { return value.Length == i + 1; };
            };

            FSMBuilder.GoTo(start);
            for (var i = 0; i < specialValue.Length; i++) FSMBuilder.SafeTransition(specialValue[i], precond(i));
            FSMBuilder.End(GenericToken.SugarToken, isLineEnding)
                .CallBack(callback);
        }

        public void AddUpTo(IN token, BuildResult<ILexer<IN>> buildResult, string[] exceptions,
            bool isLineEnding = false)
        {
            NodeCallback<GenericToken> callback = match =>
            {
                match.Properties[DerivedToken] = token;
                return match;
            };

            FSMBuilder.GoTo(start);

            var upToChars0 = exceptions.Select(x => x.First()).Distinct().ToArray();

            Func<int, int, string> GetEndLabel = (int exception, int exceptionIndex) =>
            {
                if (exception < 0 || exceptionIndex < 0)
                {
                    return $"{in_up_to}_text_{upToCounter}";
                }

                return $"{in_up_to}_{exception}_{exceptionIndex}_{upToCounter}";
            };

            FSMBuilder.ExceptTransition(upToChars0)
                .Mark(GetEndLabel(-1, -1))
                .End(GenericToken.UpTo)
                .CallBack(callback);
            FSMBuilder.ExceptTransitionTo(upToChars0, GetEndLabel(-1, -1));
            for (int i = 0; i < exceptions.Length; i++)
            {
                string exception = exceptions[i];
                for (int j = 0; j < exception.Length - 1; j++)
                {
                    char exceptionChar = exception[j];
                    var end = GetEndLabel(i, j);
                    var startNode = GetEndLabel(i, j - 1);
                    if (j == 0)
                    {
                        FSMBuilder.GoTo(startNode);
                        FSMBuilder.SafeTransition(exceptionChar);
                        FSMBuilder.Mark(end);

                        startNode = start;
                    }


                    FSMBuilder.GoTo(startNode);

                    if (j < exception.Length - 1)
                    {
                        FSMBuilder.TransitionToAndMark(new[] { exceptionChar }, end);


                        var transition = FSMBuilder.GetTransition(exception[j + 1]);
                        if (transition != null)
                        {
                            if (transition.Check is TransitionAnyExcept except)
                            {
                                except.AddException(exception[j + 1]);
                            }
                        }
                        else
                        {
                            FSMBuilder.ExceptTransitionTo(new[] { exception[j + 1] }, GetEndLabel(-1, -1));
                        }
                    }
                }
            }

            upToCounter++;
        }

        public LexerPosition ConsumeComment(Token<GenericToken> comment, ReadOnlyMemory<char> source,
            LexerPosition lexerPosition)
        {
            ReadOnlyMemory<char> commentValue;

            if (comment.IsSingleLineComment)
            {
                var position = lexerPosition.Index;
                if (position < source.Length -1)
                {
                    commentValue = source.GetToEndOfLine(position);
                }
                else
                {
                    commentValue = "".ToCharArray();
                }
                position = position + commentValue.Length;
                if (commentValue.Length > comment.SpanValue.Length)
                {
                    comment.SpanValue = commentValue.RemoveEndOfLineChars();    
                }
                else
                {
                    comment.SpanValue = "".ToCharArray();
                }
                return new LexerPosition(position, lexerPosition.Line + 1, 0);
            }
            else if (comment.IsMultiLineComment)
            {
                var position = lexerPosition.Index;

                var end = source.Span.Slice(position).IndexOf<char>(MultiLineCommentEnd.AsSpan());
                if (end < 0)
                    position = source.Length;
                else
                    position = end + position;
                commentValue = source.Slice(lexerPosition.Index, position - lexerPosition.Index);
                comment.SpanValue = commentValue;

                var newPosition = lexerPosition.Index + commentValue.Length + MultiLineCommentEnd.Length;
                var lines = EOLManager.GetLinesLength(commentValue);
                var newLine = lexerPosition.Line + lines.Count - 1;
                int newColumn;
                if (lines.Count > 1)
                    newColumn = lines[lines.Count-1] + MultiLineCommentEnd.Length;
                else
                    newColumn = lexerPosition.Column + lines[0] + MultiLineCommentEnd.Length;

                return new LexerPosition(newPosition, newLine, newColumn);
            }

            return lexerPosition;
        }

        public Token<IN> Transcode(FSMMatch<GenericToken> match)
        {
            var tok = new Token<IN>();
            var inTok = match.Result;
            tok.IsComment = inTok.IsComment;
            tok.IsEmpty = inTok.IsEmpty;
            tok.SpanValue = inTok.SpanValue;
            tok.CommentType = inTok.CommentType;
            tok.Position = inTok.Position;
            tok.Discarded = inTok.Discarded;
            tok.StringDelimiter = match.StringDelimiterChar;
            tok.TokenID = match.Properties.TryGetValue(DerivedToken, out var property) ? (IN)property : default;
            tok.IsLineEnding = match.IsLineEnding;
            tok.IsEOS = match.IsEOS;
            tok.IsIndent = match.IsIndent;
            tok.IsUnIndent = match.IsUnIndent;
            tok.IndentationLevel = match.IndentationLevel;
            tok.Notignored = match.Result.Notignored;
            tok.Channel = match.Result.Channel;
            tok.DecimalSeparator = match.DecimalSeparator;
            tok.DateTimeValue = match.DateTimeValue;
            return tok;
        }

        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return TempLexerFsm.ToString();
        }

        public string ToGraphViz()
        {
            return TempLexerFsm.ToGraphViz();
        }
    }
}