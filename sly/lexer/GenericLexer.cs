using sly.lexer.fsm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sly.lexer
{


    public static class StringExtension
    {

        public static List<string> SplitString(this string value, string delimiter)
        {
            List<string> elements = new List<string>();
            int index = value.IndexOf(delimiter);
            int lastPosition = 0;
            if (index > 0)
            {
                while (index > 0)
                {
                    string element = value.Substring(lastPosition, index - lastPosition);
                    lastPosition = index + delimiter.Length;
                    elements.Add(element);
                    index = value.IndexOf(delimiter, lastPosition);
                }
                elements.Add(value.Substring(lastPosition, value.Length - lastPosition));

            }
            else
            {
                elements.Add(value);
            }
            return elements;
        }
    }

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
        Environment
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
        protected FSMLexer<GenericToken, GenericToken> LexerFsm;

        protected BuildExtension<IN> ExtensionBuilder;

        protected Dictionary<GenericToken, Dictionary<string, IN>> derivedTokens;
        protected IN identifierDerivedToken;
        protected IN intDerivedToken;
        protected IN doubleDerivedToken;
        public FSMLexerBuilder<GenericToken, GenericToken> FSMBuilder;


        protected char StringDelimiter;

        public string SingleLineComment { get; set; }
        public string MultiLineCommentStart { get; set; }

        public string MultiLineCommentEnd { get; set; }

        public GenericLexer(EOLType eolType, IdentifierType idType = IdentifierType.Alpha, BuildExtension<IN> extensionBuilder = null, params GenericToken[] staticTokens)
        {
            InitializeStaticLexer(eolType, idType, staticTokens);
            derivedTokens = new Dictionary<GenericToken, Dictionary<string, IN>>();
            this.ExtensionBuilder = extensionBuilder;
        }


        private void InitializeStaticLexer(EOLType eolType, IdentifierType idType = IdentifierType.Alpha, params GenericToken[] staticTokens)
        {
            FSMBuilder = new FSMLexerBuilder<GenericToken, GenericToken>();


            // conf
            FSMBuilder.IgnoreWS()
                .WhiteSpace(' ')
                .WhiteSpace('\t')
                .IgnoreEOL();
            switch (eolType)
            {
                case EOLType.Windows:
                    {
                        FSMBuilder.UseWindowsEOL();
                        break;
                    }
                case EOLType.Nix:
                    {
                        FSMBuilder.UseNixEOL();
                        break;
                    }
                case EOLType.Environment:
                    {
                        FSMBuilder.UseEnvironmentEOL();
                        break;
                    }
            }


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
            .RangeTransition('0', '9', GenericToken.Int, GenericToken.Double)
            .Mark(in_int)
            .RangeTransitionTo('0', '9', in_int, GenericToken.Int, GenericToken.Double)
            .End(GenericToken.Int);
                if (staticTokens.ToList().Contains(GenericToken.Double))
                {
                    FSMBuilder.Transition('.', GenericToken.Double)
                    .Mark(start_double)
                    .RangeTransition('0', '9', GenericToken.Int, GenericToken.Int, GenericToken.Double)
                    .Mark(in_double)
                    .RangeTransitionTo('0', '9', in_double, GenericToken.Int, GenericToken.Double)
                    .End(GenericToken.Double);
                }
            }

            LexerFsm = FSMBuilder.Fsm;
        }


        private void InitializeIdentifier(IdentifierType idType = IdentifierType.Alpha)
        {

            // identifier
            FSMBuilder.GoTo(start).
        RangeTransition('a', 'z', GenericToken.Identifier).
        Mark(in_identifier)
        .End(GenericToken.Identifier);

            FSMBuilder.GoTo(start).
            RangeTransitionTo('A', 'Z', in_identifier, GenericToken.Identifier, GenericToken.Identifier).
            RangeTransitionTo('a', 'z', in_identifier, GenericToken.Identifier).
            RangeTransitionTo('A', 'Z', in_identifier, GenericToken.Identifier).
            End(GenericToken.Identifier);

            if (idType == IdentifierType.AlphaNumeric || idType == IdentifierType.AlphaNumericDash)
            {
                FSMBuilder.GoTo(in_identifier).
                    RangeTransitionTo('0', '9', in_identifier, GenericToken.Identifier, GenericToken.Identifier);
            }
            if (idType == IdentifierType.AlphaNumericDash)
            {
                FSMBuilder.GoTo(in_identifier).
                    TransitionTo('-', in_identifier, GenericToken.Identifier, GenericToken.Identifier).
                    TransitionTo('_', in_identifier, GenericToken.Identifier, GenericToken.Identifier);
                FSMBuilder.GoTo(start).
                    TransitionTo('_', in_identifier, GenericToken.Identifier, GenericToken.Identifier);
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
                            match.Properties[DerivedToken] = identifierDerivedToken;
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


        public void AddStringLexem(IN token, string stringDelimiter)
        {
            if (string.IsNullOrEmpty(stringDelimiter) || stringDelimiter.Length > 1)
            {
                throw new ArgumentException($"bad lexem {stringDelimiter} :  StringToken lexeme <{token.ToString()}> must be  1 character length.");
            }
            if (char.IsLetterOrDigit(stringDelimiter[0]))
            {
                throw new ArgumentException($"bad lexem {stringDelimiter} :  SugarToken lexeme <{token.ToString()}> can not start with a letter.");
            }

            StringDelimiter = stringDelimiter[0];

            NodeCallback<GenericToken> callback = (FSMMatch<GenericToken> match) =>
                        {
                            match.Properties[DerivedToken] = token;
                            string value = match.Result.Value;

                            match.Result.Value = value;
                            return match;
                        };

            FSMBuilder.GoTo(start);
            FSMBuilder.Transition(StringDelimiter, GenericToken.String)
                .Mark(in_string)
                .ExceptTransitionTo(new char[] { StringDelimiter, '\\' }, in_string, GenericToken.String)
                .Transition('\\', GenericToken.String)
                .Mark(escape_string)
                .AnyTransitionTo(' ', in_string, GenericToken.String)
                .Transition(StringDelimiter, GenericToken.String)
                .End(GenericToken.String)
                .Mark(string_end)
                .CallBack(callback);
            FSMBuilder.Fsm.StringDelimiter = StringDelimiter;

        }
        public void AddSugarLexem(IN token, string specialValue)
        {
            if (char.IsLetter(specialValue[0]))
            {
                throw new ArgumentException($"bad lexem {specialValue} :  SugarToken lexeme <{token.ToString()}>  can not start with a letter.");
            }
            NodeCallback<GenericToken> callback = (FSMMatch<GenericToken> match) =>
            {
                match.Properties[DerivedToken] = token;
                return match;
            };

            FSMBuilder.GoTo(start);
            for (int i = 0; i < specialValue.Length; i++)
            {
                FSMBuilder.SafeTransition(specialValue[i], GenericToken.SugarToken);
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
                tokens.Add(Transcode(r));
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
                int position = this.LexerFsm.CurrentPosition;
                int end = source.IndexOf(LexerFsm.EOL, position);
                if (end < 0)
                {
                    position = source.Length;
                }
                else
                {
                    position = end;
                }
                commentValue = source.Substring(this.LexerFsm.CurrentPosition, position - this.LexerFsm.CurrentPosition);
                comment.Value = commentValue;
                LexerFsm.Move(position, LexerFsm.CurrentLine + 1, 0);
            }
            else if (comment.IsMultiLineComment)
            {
                int position = this.LexerFsm.CurrentPosition;
                int end = source.IndexOf(this.MultiLineCommentEnd, position);
                if (end < 0)
                {
                    position = source.Length+this.MultiLineCommentEnd.Length;
                }
                else
                {
                    position = end;
                }
                commentValue = source.Substring(this.LexerFsm.CurrentPosition, position - this.LexerFsm.CurrentPosition);
                comment.Value = commentValue;

                // TODO : compute new line and column
                int newPosition = LexerFsm.CurrentPosition + commentValue.Length + this.MultiLineCommentEnd.Length;
                var remaining = source.Substring(newPosition);
                
                var lines = commentValue.SplitString(LexerFsm.EOL);
                int newLine = LexerFsm.CurrentLine + lines.Count - 1;
                int newColumn = lines[lines.Count - 1].Length;



                LexerFsm.Move(newPosition, newLine, newColumn);
            }
        }

        public Token<IN> Transcode(FSMMatch<GenericToken> match)
        {
            var tok = new Token<IN>();
            tok.Value = match.Result.Value;
            tok.Position = match.Result.Position;
            tok.StringDelimiter = StringDelimiter;
            tok.TokenID = (IN)match.Properties[DerivedToken];
            return tok;
        }

        public override string ToString()
        {
            return LexerFsm.ToString();
        }


    }
}
