using System.Collections.Generic;
using System.Text;

namespace sly.lexer
{
    public class TokenChannel<IN>
    {
        public readonly List<Token<IN>> Tokens;
        
        public int ChannelId;
        public int Count => Tokens.Count;

        public TokenChannel(List<Token<IN>> tokens, int channelId)
        {
            Tokens = tokens;
            ChannelId = channelId;
        }

        public TokenChannel(int channelId) : this(new List<Token<IN>>(),channelId)
        {
        }

        private Token<IN> GetToken(int i)
        {
            return Tokens[i];
        }
        
        private void SetToken(int i, Token<IN> token)
        {
            if (i >= Tokens.Count)
            {
                for (int j = Tokens.Count; j <= i; j++)
                {
                    Tokens.Add(null);
                }
            }
            Tokens[i] = token;
        }
        
        public Token<IN> this[int key]
        {
            get => GetToken(key);
            set => SetToken(key,value);
        }

        public void Add(Token<IN> token)
        {
            Tokens.Add(token);
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("#" + ChannelId);
            foreach (var token in Tokens)
            {
                if (token != null)
                {
                    builder.Append(token.ToString()).Append(" > ");
                }
            }
            return builder.ToString();
        }
    }
}