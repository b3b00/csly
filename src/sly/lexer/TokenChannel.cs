using System.Collections.Generic;
using System.Text;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace sly.lexer
{
    public class TokenChannel<IN>  where IN : struct
    {
        public readonly List<Token<IN>> Tokens;

        private List<Token<IN>> _notNullTokens;

        public List<Token<IN>> NotNullTokens => _notNullTokens;
        
        private List<Token<IN>> _notNullOrEosTokens;
        public List<Token<IN>> NotNullOrEosTokens => _notNullOrEosTokens; 
        
        public int ChannelId;
        public int Count => Tokens.Count;

        public TokenChannel(List<Token<IN>> tokens, int channelId)
        {
            Tokens = tokens;
            _notNullTokens = tokens.Where(x => x != null).ToList();
            _notNullOrEosTokens = tokens.Where(x => x != null && !x.IsEOS).ToList();
            ChannelId = channelId;
        }

        public TokenChannel(int channelId) : this(new List<Token<IN>>(), channelId)
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
            _notNullTokens = Tokens.Where(x => x != null).ToList();
            _notNullOrEosTokens = Tokens.Where(x => x != null && !x.IsEOS).ToList();
        }
        
        public Token<IN> this[int key]
        {
            get => GetToken(key);
            set => SetToken(key,value);
        }

        public void Add(Token<IN> token)
        {
            Tokens.Add(token);
            if (token != null) 
                token.PositionInTokenFlow = Tokens.Count;
            _notNullTokens = Tokens.Where(x => x != null).ToList();
            _notNullOrEosTokens = Tokens.Where(x => x != null && !x.IsEOS).ToList();
        }

        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append("#").Append(ChannelId).Append(" ");
            foreach (var token in Tokens.Where(token => token != null))
            {
                builder.Append(token).Append(" > ");
            }
            return builder.ToString();
        }
    }
}
