using System;
using System.Collections.Generic;
using System.Linq;
using sly.buildresult;
using sly.lexer.fsm;

namespace sly.lexer
{
    public enum GenericToken
    {
        Default,
        Identifier,
        Int,
        Double,
        KeyWord,
        String,
        SugarToken,

        Extension,

        Comment
    }

    public enum IdentifierType
    {
        Alpha,
        AlphaNumeric,
        AlphaNumericDash
    }

    public enum EOLType
    {
        Windows,
        Nix,

        Mac,
        Environment,

        No
    }

    public class GenericLexer<IN> : ILexer<IN> where IN : struct
    {
        public static string in_string = "in_string";
        public static string string_end = "string_end";
        public static string start = "start";
        public static string in_int = "in_int";
        public static string start_double = "start_double";
        public static string in_double = "in_double";
        public static string in_identifier = "in_identifier";
        public static string token_property = "token";
        public static string DerivedToken = "derivedToken";
        public static string defaultIdentifierKey = "identifier";
        public static string escape_string = "escape_string";

        public static string single_line_comment_start = "single_line_comment_start";

        public static string multi_line_comment_start = "multi_line_comment_start";

        protected Dictionary<GenericToken, Dictionary<string, IN>> derivedTokens;
        protected IN doubleDerivedToken;
        protected char EscapeStringDelimiterChar;

        protected BuildExtension<IN> ExtensionBuilder;
        public FSMLexerBuilder<GenericToken> FSMBuilder;
        protected IN identifierDerivedToken;
        protected IN intDerivedToken;

        protected FSMLexer<GenericToken> LexerFsm;
        protected int StringCounter;


        protected Dictionary<IN, Func<Token<IN>, Token<IN>>> CallBacks = new Dictionary<IN, Func<Token<IN>, Token<IN>>>();

        protected char StringDelimiterChar;

        public GenericLexer(IdentifierType idType = IdentifierType.Alpha, BuildExtension<IN> extensionBuilder = null,
            params GenericToken[] staticTokens)
        {
            InitializeStaticLexer(idType, staticTokens);
            derivedTokens = new Dictionary<GenericToken, Dictionary<string, IN>>();
            ExtensionBuilder = extensionBuilder;
        }

        public string SingleLineComment { get; set; }
        public string MultiLineCommentStart { get; set; }

        public string MultiLineCommentEnd { get; set; }


        public void AddCallBack(IN token, Func<Token<IN>, Token<IN>> callback)
        {
            CallBacks[token] = callback;
        }

        public void AddDefinition(TokenDefinition<IN> tokenDefinition) { }

        public LexerResult<IN> Tokenize(string source)
        {
            var memorySource = source.AsMemory();

            var tokens = new List<Token<IN>>();
            FSMMatch<GenericToken> r = null;

            r = LexerFsm.Run(source, 0);
            if (!r.IsSuccess && !r.IsEOS)
            {
                var resultPosition = r.Result.Position;
                LexicalError error =
                    new LexicalError(resultPosition.Line, resultPosition.Column, r.Result.CharValue);
                return new LexerResult<IN>(error);
            }

            while (r.IsSuccess)
            {
                var transcoded = Transcode(r);
                Func<Token<IN>, Token<IN>> callback = null;
                if (CallBacks.TryGetValue(transcoded.TokenID, out callback))
                {
                    transcoded = callback(transcoded);
                }

                tokens.Add(transcoded);

                r = LexerFsm.Run(source);
                if (!r.IsSuccess && !r.IsEOS)
                {
                    var resultPosition = r.Result.Position;
                    LexicalError error =
                        new LexicalError(resultPosition.Line, resultPosition.Column, r.Result.CharValue);
                    return new LexerResult<IN>(error);
                }


                if (r.IsSuccess && r.Result.IsComment) ConsumeComment(r.Result, memorySource);
            }

            var eos = new Token<IN>();
            var prev = tokens.Last();
            eos.Position = new TokenPosition(prev.Position.Index + 1, prev.Position.Line,
                prev.Position.Column + prev.Value.Length);
            tokens.Add(eos);
            return new LexerResult<IN>(tokens);
        }


        private void InitializeStaticLexer(IdentifierType idType = IdentifierType.Alpha,
            params GenericToken[] staticTokens)
        {
            FSMBuilder = new FSMLexerBuilder<GenericToken>();
            StringCounter = 0;

            // conf
            FSMBuilder.IgnoreWS()
                .WhiteSpace(' ')
                .WhiteSpace('\t')
                .IgnoreEOL();

            // start machine definition
            FSMBuilder.Mark(start);

            if (staticTokens.ToList().Contains(GenericToken.Identifier) ||
                staticTokens.ToList().Contains(GenericToken.KeyWord)) InitializeIdentifier(idType);

            //numeric
            if (staticTokens.ToList().Contains(GenericToken.Int) || staticTokens.ToList().Contains(GenericToken.Double))
            {
                FSMBuilder = FSMBuilder.GoTo(start)
                    .RangeTransition('0', '9')
                    .Mark(in_int)
                    .RangeTransitionTo('0', '9', in_int)
                    .End(GenericToken.Int);
                if (staticTokens.ToList().Contains(GenericToken.Double))
                    FSMBuilder.Transition('.')
                        .Mark(start_double)
                        .RangeTransition('0', '9')
                        .Mark(in_double)
                        .RangeTransitionTo('0', '9', in_double)
                        .End(GenericToken.Double);
            }

            LexerFsm = FSMBuilder.Fsm;
        }


        private void InitializeIdentifier(IdentifierType idType = IdentifierType.Alpha)
        {
            // identifier
            FSMBuilder.GoTo(start).RangeTransition('a', 'z').Mark(in_identifier)
                .End(GenericToken.Identifier);

            FSMBuilder.GoTo(start).RangeTransitionTo('A', 'Z', in_identifier).RangeTransitionTo('a', 'z', in_identifier)
                .RangeTransitionTo('A', 'Z', in_identifier).End(GenericToken.Identifier);

            if (idType == IdentifierType.AlphaNumeric || idType == IdentifierType.AlphaNumericDash)
                FSMBuilder.GoTo(in_identifier).RangeTransitionTo('0', '9', in_identifier);
            if (idType == IdentifierType.AlphaNumericDash)
            {
                FSMBuilder.GoTo(in_identifier).TransitionTo('-', in_identifier).TransitionTo('_', in_identifier);
                FSMBuilder.GoTo(start).TransitionTo('_', in_identifier);
            }
        }

        public void AddLexeme(GenericToken generic, IN token)
        {
            if (generic == GenericToken.Identifier)
            {
                identifierDerivedToken = token;
                var derived = new Dictionary<string, IN>();
                if (derivedTokens.ContainsKey(generic)) derived = derivedTokens[generic];
                derived[defaultIdentifierKey] = token;
                //return;
            }

            if (generic == GenericToken.Double) doubleDerivedToken = token;
            if (generic == GenericToken.Int) intDerivedToken = token;

            NodeCallback<GenericToken> callback = match =>
            {
                switch (match.Result.TokenID)
                {
                    case GenericToken.Identifier:
                        {
                            if (derivedTokens.ContainsKey(GenericToken.Identifier))
                            {
                                var possibleTokens = derivedTokens[GenericToken.Identifier];
                                if (possibleTokens.ContainsKey(match.Result.Value))
                                    match.Properties[DerivedToken] = possibleTokens[match.Result.Value];
                                else
                                    match.Properties[DerivedToken] = identifierDerivedToken;
                            }
                            else
                            {
                                match.Properties[DerivedToken] = identifierDerivedToken;
                            }

                        ;
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
                case GenericToken.Double:
                    {
                        FSMBuilder.GoTo(in_double);
                        FSMBuilder.CallBack(callback);
                        break;
                    }
                case GenericToken.Int:
                    {
                        FSMBuilder.GoTo(in_int);
                        FSMBuilder.CallBack(callback);
                        break;
                    }
                case GenericToken.Identifier:
                    {
                        FSMBuilder.GoTo(in_identifier);
                        FSMBuilder.CallBack(callback);
                        break;
                    }
            }
        }

        public void AddLexeme(GenericToken genericToken, IN token, string specialValue)
        {
            switch (genericToken)
            {
                case GenericToken.SugarToken:
                    {
                        AddSugarLexem(token, specialValue);
                        break;
                    }
            }

            var tokensForGeneric = new Dictionary<string, IN>();
            if (derivedTokens.ContainsKey(genericToken)) tokensForGeneric = derivedTokens[genericToken];
            tokensForGeneric[specialValue] = token;
            derivedTokens[genericToken] = tokensForGeneric;
        }

        public void AddKeyWord(IN token, string keyword)
        {
            NodeCallback<GenericToken> callback = match =>
            {
                if (derivedTokens.ContainsKey(GenericToken.Identifier))
                {
                    var derived = derivedTokens[GenericToken.Identifier];
                    if (derived.ContainsKey(match.Result.Value))
                        match.Properties[DerivedToken] = derived[match.Result.Value];
                    else if (derived.ContainsKey(defaultIdentifierKey))
                        match.Properties[DerivedToken] = identifierDerivedToken;
                }
                else
                {
                    match.Properties[DerivedToken] = identifierDerivedToken;
                }

                return match;
            };

            AddLexeme(GenericToken.Identifier, token, keyword);
            FSMBuilder.GoTo(in_identifier);
            var node = FSMBuilder.GetNode(in_identifier);
            if (!FSMBuilder.Fsm.HasCallback(node.Id))
                FSMBuilder.GoTo(in_identifier).CallBack(callback);
        }


        public void AddStringLexem(IN token, string stringDelimiter, string escapeDelimiterChar = "\\")
        {
            if (string.IsNullOrEmpty(stringDelimiter) || stringDelimiter.Length > 1)
                throw new InvalidLexerException(
                    $"bad lexem {stringDelimiter} :  StringToken lexeme delimiter char <{token.ToString()}> must be 1 character length.");
            if (char.IsLetterOrDigit(stringDelimiter[0]))
                throw new InvalidLexerException(
                    $"bad lexem {stringDelimiter} :  StringToken lexeme delimiter char <{token.ToString()}> can not start with a letter.");

            if (string.IsNullOrEmpty(escapeDelimiterChar) || escapeDelimiterChar.Length > 1)
                throw new InvalidLexerException(
                    $"bad lexem {escapeDelimiterChar} :  StringToken lexeme escape char  <{token.ToString()}> must be 1 character length.");
            if (char.IsLetterOrDigit(escapeDelimiterChar[0]))
                throw new InvalidLexerException(
                    $"bad lexem {escapeDelimiterChar} :  StringToken lexeme escape char lexeme <{token.ToString()}> can not start with a letter.");

            StringCounter++;

            StringDelimiterChar = stringDelimiter[0];
            char stringDelimiterChar = stringDelimiter[0];

            EscapeStringDelimiterChar = escapeDelimiterChar[0];
            char escapeStringDelimiterChar = escapeDelimiterChar[0];


            NodeCallback<GenericToken> callback = match =>
            {
                match.Properties[DerivedToken] = token;
                var value = match.Result.SpanValue;

                match.Result.SpanValue = value;


                if (stringDelimiterChar != escapeStringDelimiterChar)                
                {
                    int i = 1;
                    bool substitutionHappened = false;
                    // TODO : iterate on chars and operate substitution if needed
                    // if no subst then value stay the same 
                    bool escaping = false;
                    string r = string.Empty;                    
                    while(i < value.Length-1) {
                        char current = value.At(i);
                        if (current == escapeStringDelimiterChar && i < value.Length - 2) {
                            escaping = true;
                            if (!substitutionHappened) {
                                r = value.Slice(0,i).ToString();
                                substitutionHappened = true;
                            }                            
                        }
                        else {
                            if (escaping) {
                                if (current != stringDelimiterChar) {
                                    r += escapeStringDelimiterChar;
                                }
                                escaping = false;
                            }
                            if (substitutionHappened) {
                                r += current;
                            }
                        }
                        i++;
                    }
                     if (substitutionHappened) {
                        r += value.At(value.Length-1);
                        value = r.AsMemory();
                        match.Result.SpanValue = value;
                    }
                    else {
                        ;
                    }
                }
                else
                {
                    int i = 1;
                    bool substitutionHappened = false;
                    // TODO : iterate on chars and operate substitution if needed
                    // if no subst then value stay the same 
                    bool escaping = false;
                    string r = string.Empty;                    
                    while(i < value.Length-1) {
                        char current = value.At(i);
                        if (current == escapeStringDelimiterChar && !escaping && i < value.Length - 2) {
                            escaping = true;
                            if (!substitutionHappened) {
                                r = value.Slice(0,i).ToString();
                                substitutionHappened = true;
                            }                            
                        }
                        else {
                            if (escaping) {
                                // if (current != stringDelimiterChar) {
                                    r += escapeStringDelimiterChar;
                                // }
                                escaping = false;
                            }
                            else if (substitutionHappened) {
                                r += current;
                            }
                        }
                        i++;
                    }
                    if (substitutionHappened) {
                        r += value.At(value.Length-1);
                        value = r.AsMemory();
                        match.Result.SpanValue = value;
                    }
                    else {
                        ;
                    }
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
                in_string = "in_string_same";
                var escaped = "escaped_same";
                var delim = "delim_same";

                FSMBuilder.GoTo(start)
                    .Transition(stringDelimiterChar)
                    .Mark(in_string + StringCounter)
                    .ExceptTransitionTo(exceptDelimiter, in_string + StringCounter)
                    .Transition(stringDelimiterChar)
                    .Mark(escaped + StringCounter)
                    .End(GenericToken.String)
                    .CallBack(callback)
                    .Transition(stringDelimiterChar)
                    .Mark(delim + StringCounter)
                    .ExceptTransitionTo(exceptDelimiter, in_string + StringCounter);

                FSMBuilder.GoTo(delim + StringCounter)
                    .TransitionTo(stringDelimiterChar, escaped + StringCounter)
                    .ExceptTransitionTo(exceptDelimiter, in_string + StringCounter);
            }
        }

        public void AddSugarLexem(IN token, string specialValue)
        {
            if (char.IsLetter(specialValue[0]))
                throw new InvalidLexerException(
                    $"bad lexem {specialValue} :  SugarToken lexeme <{token.ToString()}>  can not start with a letter.");
            NodeCallback<GenericToken> callback = match =>
            {
                match.Properties[DerivedToken] = token;
                return match;
            };

            FSMBuilder.GoTo(start);
            for (var i = 0; i < specialValue.Length; i++) FSMBuilder.SafeTransition(specialValue[i]);
            FSMBuilder.End(GenericToken.SugarToken)
                .CallBack(callback);
        }

        public void ConsumeComment(Token<GenericToken> comment, ReadOnlyMemory<char> source)
        {

            ReadOnlyMemory<char> commentValue;

            if (comment.IsSingleLineComment)
            {
                var position = LexerFsm.CurrentPosition;
                commentValue = EOLManager.GetToEndOfLine(source, position);
                position = position + commentValue.Length;
                comment.SpanValue = commentValue;
                LexerFsm.Move(position, LexerFsm.CurrentLine + 1, 0);
            }
            else if (comment.IsMultiLineComment)
            {
                var position = LexerFsm.CurrentPosition;

                var end = source.Span.Slice(position).IndexOf(MultiLineCommentEnd.AsSpan());
                if (end < 0)
                    position = source.Length;
                else
                    position = end + position;
                commentValue = source.Slice(LexerFsm.CurrentPosition, position - LexerFsm.CurrentPosition);
                comment.SpanValue = commentValue;

                var newPosition = LexerFsm.CurrentPosition + commentValue.Length + MultiLineCommentEnd.Length;
                var lines = EOLManager.GetLinesLength(commentValue);
                var newLine = LexerFsm.CurrentLine + lines.Count - 1;
                int newColumn;
                if (lines.Count > 1)
                    newColumn = lines.Last() + MultiLineCommentEnd.Length;
                else
                    newColumn = LexerFsm.CurrentColumn + lines[0] + MultiLineCommentEnd.Length;


                LexerFsm.Move(newPosition, newLine, newColumn);
            }
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
            tok.StringDelimiter = StringDelimiterChar;
            tok.TokenID = (IN)match.Properties[DerivedToken];
            return tok;
        }

        public override string ToString()
        {
            return LexerFsm.ToString();
        }
    }
}