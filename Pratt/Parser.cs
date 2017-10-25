using System;
using System.Collections.Generic;
using sly.pratt.parselets;
using sly.lexer;



namespace sly.pratt
{


    public static class DictionaryExtensions
    {

        public static V Get<K, V>(this Dictionary<K, V> dict, K key)
        {
            if (dict.ContainsKey(key))
            {
                return dict[key];
            }
            return default(V);
        }

    }

    public class Parser<IN, OUT> where IN : struct
    {

        private int Position;

        private List<Token<IN>> Tokens;
        //private List<Token<IN>> ReadTokens = new List<Token<IN>>();
        private Dictionary<IN, PrefixParselet<IN, OUT>> PrefixParselets =
            new Dictionary<IN, PrefixParselet<IN, OUT>>();
        private Dictionary<IN, InfixParselet<IN, OUT>> InfixParselets =
            new Dictionary<IN, InfixParselet<IN, OUT>>();

        public Parser(List<Token<IN>> tokens, int startPosition)
        {
            Position = startPosition;
            Tokens = tokens;
        }

        


        public void infix(IN token, int precedence, BinaryExpressionBuilder<IN,OUT> builder, Associativity assoc = Associativity.Right) 
        {
            InfixParselet<IN, OUT> infixparse = new InfixParselet<IN, OUT>(token, precedence, assoc, builder);
            InfixParselets[token] = infixparse;
        }

        public void prefix(IN token, int precedence, UnaryExpressionBuilder<IN, OUT> builder)
        {
            PrefixParselet<IN, OUT> prefixparse = new PrefixParselet<IN, OUT>(token, precedence, builder);
            PrefixParselets[token] = prefixparse;
        }

        public void register(IN token, PrefixParselet<IN, OUT> parselet)
        {
            PrefixParselets[token] = parselet;
        }

        public void register(IN token, InfixParselet<IN, OUT> parselet)
        {
            InfixParselets[token] = parselet;
        }

        public OUT parseExpression(int precedence)
        {
            Token<IN> token = lookAhead(0);
            PrefixParselet<IN, OUT> prefix = PrefixParselets.Get(token.TokenID);

            OUT left = default(OUT);

            if (prefix != null)
            {
                left = prefix.Parse(this, token);
            }
            
            //OUT left = prefix.Parse(this, token);

            while (precedence < getPrecedence())
            {
                token = consume();

                InfixParselet<IN, OUT> infix = InfixParselets.Get(token.TokenID);
                left = infix.Parse(this, left, token);
            }

            return left;
        }

        public OUT parseExpression()
        {
            return parseExpression(0);
        }

        public bool match(IN expected)
        {
            Token<IN> token = lookAhead(0);
            if (!token.TokenID.Equals(expected))
            {
                return false;
            }
            
            return true;
        }

        public Token<IN> consume(IN expected)
        {

            Token<IN> token = Tokens[Position];
            if (!token.TokenID.Equals(expected))
            {
                throw new Exception("Expected token " + expected +
                    " and found " + token.TokenID);
            }
            Position++;

            return token;
        }

        public Token<IN> consume()
        {
            // Make sure we've read the token.
            lookAhead(0);
            Token<IN> t = Tokens[Position];
            Position++;
            return t;
        }

        private Token<IN> lookAhead(int distance)
        {
            // Read in as many as needed.
            //while (distance >= ReadTokens.Count)
            //{
            //    ReadTokens.Add(Tokens[distance]);
            //}

            // Get the queued token.
            return Tokens[Position+distance];

        }

        private int getPrecedence()
        {
            Token<IN> next = lookAhead(0);
            IN toktyp = next.TokenID;
            InfixParselet<IN,OUT> parser = InfixParselets.ContainsKey(toktyp) ? InfixParselets[toktyp] : null;
            if (parser != null) return parser.Precedence;

            return 0;
        }


    }

}