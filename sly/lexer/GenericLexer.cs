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

        Extension
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
        protected const string in_string = "in_string";
        protected const string string_end = "string_end";
        protected const string start = "start";
        protected const string in_int = "in_int";
        protected const string start_double = "start_double";
        protected const string in_double = "in_double";
        protected const string in_identifier = "in_identifier";
        protected const string token_property = "token";
        protected const string DerivedToken = "derivedToken";
        protected const string defaultIdentifierKey = "identifier";
        protected const string escape_string = "escape";
        protected FSMLexer<GenericToken, GenericToken> LexerFsm;

        protected Dictionary<GenericToken, Dictionary<string, IN>> derivedTokens;
        protected IN identifierDerivedToken;
        protected IN intDerivedToken;
        protected IN doubleDerivedToken;
        protected FSMLexerBuilder<GenericToken, GenericToken> FSMBuilder;


        protected char StringDelimiter;


        public GenericLexer()
        {

        }

        public GenericLexer(EOLType eolType, IdentifierType idType = IdentifierType.Alpha, params GenericToken[] staticTokens)
        {
            InitializeStaticLexer(eolType, idType, staticTokens);
            derivedTokens = new Dictionary<GenericToken, Dictionary<string, IN>>();
        }

        public void CopyTo(GenericLexer<IN> otherLexer)
        {
            otherLexer.LexerFsm = LexerFsm;
            
            otherLexer.derivedTokens = derivedTokens;
            otherLexer.identifierDerivedToken = identifierDerivedToken;
            otherLexer.intDerivedToken = intDerivedToken;
            otherLexer.doubleDerivedToken = doubleDerivedToken;
            otherLexer.FSMBuilder = FSMBuilder;
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
                case GenericToken.String:
                    {
                        FSMBuilder.GoTo(string_end);
                        FSMBuilder.CallBack(callback);
                        FSMBuilder.GoTo(in_string);
                        FSMBuilder.CallBack(callback);
                        break;
                    }
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

            }
            return tokens;

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

        public virtual void AddExtension(LexemeAttribute lexem, IN token) {            

        }

        public void AddExtensions() {
            var attributes = LexerBuilder.GetLexemes<IN>(null);
            Console.WriteLine(attributes.Count);
            foreach(KeyValuePair<IN,List<LexemeAttribute>> attrs in attributes) {
                foreach(LexemeAttribute lexem in attrs.Value) {
                    AddExtension(lexem,attrs.Key);
                }
            }
            ;
        }
    }
}
