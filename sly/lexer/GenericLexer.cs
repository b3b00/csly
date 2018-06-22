using sly.lexer.fsm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        protected FSMLexer<GenericToken> LexerFsm;

        protected BuildExtension<IN> ExtensionBuilder;

        protected Dictionary<GenericToken, Dictionary<string, IN>> derivedTokens;
        protected IN identifierDerivedToken;
        protected IN intDerivedToken;
        protected IN doubleDerivedToken;
        public FSMLexerBuilder<GenericToken> FSMBuilder;


        protected char StringDelimiterChar;
        protected char EscapeStringDelimiterChar;
        protected int StringCounter = 0;

        public string SingleLineComment { get; set; }
        public string MultiLineCommentStart { get; set; }

        public string MultiLineCommentEnd { get; set; }

        public GenericLexer(IdentifierType idType = IdentifierType.Alpha, BuildExtension<IN> extensionBuilder = null, params GenericToken[] staticTokens)
        {
            InitializeStaticLexer(idType, staticTokens);
            derivedTokens = new Dictionary<GenericToken, Dictionary<string, IN>>();
            ExtensionBuilder = extensionBuilder;
        }


        private void InitializeStaticLexer(IdentifierType idType = IdentifierType.Alpha, params GenericToken[] staticTokens)
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

            if (staticTokens.ToList().Contains(GenericToken.Identifier) || staticTokens.ToList().Contains(GenericToken.KeyWord))
            {
                InitializeIdentifier(idType);

            }

            //numeric
            if (staticTokens.ToList().Contains(GenericToken.Int) || staticTokens.ToList().Contains(GenericToken.Double))
            {
                FSMBuilder = FSMBuilder.GoTo(start)
            .RangeTransition('0', '9')
            .Mark(in_int)
            .RangeTransitionTo('0', '9', in_int)
            .End(GenericToken.Int);
                if (staticTokens.ToList().Contains(GenericToken.Double))
                {
                    FSMBuilder.Transition('.')
                    .Mark(start_double)
                    .RangeTransition('0', '9')
                    .Mark(in_double)
                    .RangeTransitionTo('0', '9', in_double)
                    .End(GenericToken.Double);
                }
            }

            LexerFsm = FSMBuilder.Fsm;
        }


        private void InitializeIdentifier(IdentifierType idType = IdentifierType.Alpha)
        {

            // identifier
            FSMBuilder.GoTo(start).
        RangeTransition('a', 'z').
        Mark(in_identifier)
        .End(GenericToken.Identifier);

            FSMBuilder.GoTo(start).
            RangeTransitionTo('A', 'Z', in_identifier).
            RangeTransitionTo('a', 'z', in_identifier).
            RangeTransitionTo('A', 'Z', in_identifier).
            End(GenericToken.Identifier);

            if (idType == IdentifierType.AlphaNumeric || idType == IdentifierType.AlphaNumericDash)
            {
                FSMBuilder.GoTo(in_identifier).
                    RangeTransitionTo('0', '9', in_identifier);
            }
            if (idType == IdentifierType.AlphaNumericDash)
            {
                FSMBuilder.GoTo(in_identifier).
                    TransitionTo('-', in_identifier).
                    TransitionTo('_', in_identifier);
                FSMBuilder.GoTo(start).
                    TransitionTo('_', in_identifier);
            }

        }
        public void AddLexeme(GenericToken generic, IN token)
        {
            if (generic == GenericToken.Identifier)
            {
                identifierDerivedToken = token;
                var derived = new Dictionary<string, IN>();
                if (derivedTokens.ContainsKey(generic))
                {
                    derived = derivedTokens[generic];
                }
                derived[defaultIdentifierKey] = token;
                //return;
            }
            if (generic == GenericToken.Double)
            {
                doubleDerivedToken = token;
            }
            if (generic == GenericToken.Int)
            {
                intDerivedToken = token;
            }

            NodeCallback<GenericToken> callback = (FSMMatch<GenericToken> match) =>
            {
                switch (match.Result.TokenID)
                {
                    case GenericToken.Identifier:
                        {
                            if (derivedTokens.ContainsKey(GenericToken.Identifier)) {
                                var possibleTokens = derivedTokens[GenericToken.Identifier];
                                if (possibleTokens.ContainsKey(match.Result.Value))
                                {
                                    match.Properties[DerivedToken] = possibleTokens[match.Result.Value];
                                }
                                else
                                {
                                    match.Properties[DerivedToken] = identifierDerivedToken;
                                }
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
            Dictionary<string, IN> tokensForGeneric = new Dictionary<string, IN>();
            if (derivedTokens.ContainsKey(genericToken))
            {
                tokensForGeneric = derivedTokens[genericToken];
            }
            tokensForGeneric[specialValue] = token;
            derivedTokens[genericToken] = tokensForGeneric;


        }

        public void AddKeyWord(IN token, string keyword)
        {
            NodeCallback<GenericToken> callback = (FSMMatch<GenericToken> match) =>
            {
                if (derivedTokens.ContainsKey(GenericToken.Identifier))
                {
                    Dictionary<string, IN> derived = derivedTokens[GenericToken.Identifier];
                    if (derived.ContainsKey(match.Result.Value))
                    {
                        match.Properties[DerivedToken] = derived[match.Result.Value];
                    }
                    else if (derived.ContainsKey(defaultIdentifierKey))
                    {
                        match.Properties[DerivedToken] = identifierDerivedToken;
                    }
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
            {
                FSMBuilder.GoTo(in_identifier).
                    CallBack(callback);
            }
        }


        public void AddStringLexem(IN token, string stringDelimiter, string escapeDelimiterChar = "\\")
        {

            if (string.IsNullOrEmpty(stringDelimiter) || stringDelimiter.Length > 1)
            {
                throw new InvalidLexerException($"bad lexem {stringDelimiter} :  StringToken lexeme delimiter char <{token.ToString()}> must be 1 character length.");
            }
            if (char.IsLetterOrDigit(stringDelimiter[0]))
            {
                throw new InvalidLexerException($"bad lexem {stringDelimiter} :  StringToken lexeme delimiter char <{token.ToString()}> can not start with a letter.");
            }

            if (string.IsNullOrEmpty(escapeDelimiterChar) || escapeDelimiterChar.Length > 1)
            {
                throw new InvalidLexerException($"bad lexem {escapeDelimiterChar} :  StringToken lexeme escape char  <{token.ToString()}> must be 1 character length.");
            }
            if (char.IsLetterOrDigit(escapeDelimiterChar[0]))
            {
                throw new InvalidLexerException($"bad lexem {escapeDelimiterChar} :  StringToken lexeme escape char lexeme <{token.ToString()}> can not start with a letter.");
            }

            StringCounter++;

            StringDelimiterChar = stringDelimiter[0];

            EscapeStringDelimiterChar = escapeDelimiterChar[0];



            NodeCallback<GenericToken> callback = (FSMMatch<GenericToken> match) =>
                        {
                            match.Properties[DerivedToken] = token;
                            string value = match.Result.Value;

                            match.Result.Value = value;
                            return match;
                        };

            if (StringDelimiterChar != EscapeStringDelimiterChar)
            {

                FSMBuilder.GoTo(start);
                FSMBuilder.Transition(StringDelimiterChar)
                    .Mark(in_string + StringCounter)
                    .ExceptTransitionTo(new char[] { StringDelimiterChar, EscapeStringDelimiterChar }, in_string + StringCounter)
                    .Transition(EscapeStringDelimiterChar)
                    .Mark(escape_string + StringCounter)
                    .AnyTransitionTo(' ', in_string + StringCounter)
                    .Transition(StringDelimiterChar)
                    .End(GenericToken.String)
                    .Mark(string_end + StringCounter)
                    .CallBack(callback);
                FSMBuilder.Fsm.StringDelimiter = StringDelimiterChar;
            }
            else
            {
                NodeAction collapseDelimiter = (string value) =>
                {
                    if (value.EndsWith("" + StringDelimiterChar + StringDelimiterChar))
                    {
                        return value.Substring(0, value.Length - 2) + StringDelimiterChar;
                    }
                    return value;
                };

                var exceptDelimiter = new char[] { StringDelimiterChar };
                string in_string = "in_string_same";
                string escaped = "escaped_same";
                string delim = "delim_same";

                FSMBuilder.GoTo(start)
                .Transition(StringDelimiterChar)
                .Mark(in_string + StringCounter)
                .ExceptTransitionTo(exceptDelimiter, in_string + StringCounter)
                .Transition(StringDelimiterChar)

                .Mark(escaped + StringCounter)
                .End(GenericToken.String)
                .CallBack(callback)
                .Transition(StringDelimiterChar)

                .Mark(delim + StringCounter)
                .Action(collapseDelimiter)
                .ExceptTransitionTo(exceptDelimiter, in_string + StringCounter);

                FSMBuilder.GoTo(delim + StringCounter)
                .TransitionTo(StringDelimiterChar, escaped + StringCounter)

                .ExceptTransitionTo(exceptDelimiter, in_string + StringCounter);
            }

        }
        public void AddSugarLexem(IN token, string specialValue)
        {
            if (char.IsLetter(specialValue[0]))
            {
                throw new InvalidLexerException($"bad lexem {specialValue} :  SugarToken lexeme <{token.ToString()}>  can not start with a letter.");
            }
            NodeCallback<GenericToken> callback = (FSMMatch<GenericToken> match) =>
            {
                match.Properties[DerivedToken] = token;
                return match;
            };

            FSMBuilder.GoTo(start);
            for (int i = 0; i < specialValue.Length; i++)
            {
                FSMBuilder.SafeTransition(specialValue[i]);
            }
            FSMBuilder.End(GenericToken.SugarToken)
                .CallBack(callback);

        }


        public void AddDefinition(TokenDefinition<IN> tokenDefinition)
        {

        }

        public IEnumerable<Token<IN>> Tokenize(string source)
        {
            List<Token<IN>> tokens = new List<Token<IN>>();
            var r = LexerFsm.Run(source, 0);
            while (r.IsSuccess)
            {
                var transcoded = Transcode(r);   
                tokens.Add(transcoded);             
                r = LexerFsm.Run(source);
                if (r.Result.IsComment)
                {
                    ConsumeComment(r.Result, source);
                }
            }
            return tokens;

        }




        public void ConsumeComment(Token<GenericToken> comment, string source)
        {

            string commentValue = "";

            if (comment.IsSingleLineComment)
            {
                int position = LexerFsm.CurrentPosition;
                commentValue = EOLManager.GetToEndOfLine(source, position);
                position = position + commentValue.Length;
                comment.Value = commentValue.Replace("\n", "").Replace("\r", "");
                LexerFsm.Move(position, LexerFsm.CurrentLine + 1, 0);
            }
            else if (comment.IsMultiLineComment)
            {
                int position = LexerFsm.CurrentPosition;
                int end = source.IndexOf(MultiLineCommentEnd, position);
                if (end < 0)
                {
                    position = source.Length;
                }
                else
                {
                    position = end;
                }
                commentValue = source.Substring(LexerFsm.CurrentPosition, position - LexerFsm.CurrentPosition);
                comment.Value = commentValue;

                int newPosition = LexerFsm.CurrentPosition + commentValue.Length + MultiLineCommentEnd.Length;

                var lines = EOLManager.GetLines(commentValue);
                int newLine = LexerFsm.CurrentLine + lines.Count - 1;
                int newColumn = 0;
                if (lines.Count > 1)
                {
                    newColumn = lines[lines.Count - 1].Length + MultiLineCommentEnd.Length;
                }
                else
                {
                    newColumn = LexerFsm.CurrentColumn + lines[0].Length + MultiLineCommentEnd.Length;
                }


                LexerFsm.Move(newPosition, newLine, newColumn);
            }
        }

        public Token<IN> Transcode(FSMMatch<GenericToken> match)
        {
            
            var tok = new Token<IN>();
            var inTok = match.Result;            
            tok.IsComment = inTok.IsComment;
            tok.IsEmpty = inTok.IsEmpty;
            tok.Value = inTok.Value;
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
