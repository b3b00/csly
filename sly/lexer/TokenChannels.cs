using System.Collections.Generic;
using System.Linq;

namespace sly.lexer
{

    public class TokenChannel<IN>
    {
        private List<Token<IN>> Tokens;
        public int ChannelId;
        public int Count => Tokens.Count;

        public TokenChannel(List<Token<IN>> tokens)
        {
            Tokens = tokens;
        }

        public TokenChannel() : this(new List<Token<IN>>())
        {
        }

        private Token<IN> GetValue(int i)
        {
            return Tokens[i];
        }
        
        private void SetValue(int i, Token<IN> token)
        {
            Tokens[i] = token;
        }
        
        public Token<IN> this[int key]
        {
            get => GetValue(key);
            set => SetValue(key,value);
        }

        public void Add(Token<IN> token)
        {
            Tokens.Add(token);
        }
    }
    
    public class TokenChannels<IN>
    {
        private Dictionary<int, TokenChannel<IN>> Channels;

        public readonly int ChannelId;

        public TokenChannels(int channelId)
        {
            ChannelId = channelId;
            Channels = new Dictionary<int, TokenChannel<IN>>();
        }
        
        // private TokenChannel<IN> GetValue(int i)
        // {
        //     return Channels[i];
        // }
        //
        // private void SetValue(int i, TokenChannel<IN> token)
        // {
        //     Channels[i] = token;
        // }

        public TokenChannel<IN> this[int key] => Channels[key];
        // {
        //     get => GetValue(key);
        //     set => SetValue(key,value);
        // }

        public void Add(Token<IN> token)
        {
            TokenChannel<IN> channel = null;
            if (!TryGet(token.Channel, out channel))
            {
                channel = new TokenChannel<IN>();
                Channels[token.Channel] = channel;
            }

            int index = Channels.Values.Max(x => x.Count);
            foreach (var VARIABLE in Channels.Values)
            {
                for (int i = channel.Count; i < index; i++)
                {
                    channel.Add(null);
                }

                if (channel.ChannelId == token.Channel)
                {
                    channel[index] = token;
                }
            }
            
            
        }

        public bool ContainsChannel(int channel) => Channels.ContainsKey(channel);

        
        
        public Token<IN> TokenAt(int index)
        {
            foreach (var channel in Channels)
            {
                var token = TokenInChannelAt(channel.Value, index);
                if (token != null)
                {
                    return null;
                }
            }
            return null;
        }

        public Token<IN> TokenInChannelAt(TokenChannel<IN> channel, int index)
        {
            
            if (channel != null)
            {
                if (index >= 0 && index < channel.Count)
                {
                    return channel[index];
                }
            }
            return null;
        }
        
        public Token<IN> TokenInChannelAt(int channelId, int index)
        {
            TokenChannel<IN> channel = null;
            if (TryGet(index, out channel))
            {
                if (index >= 0 && index < channel.Count)
                {
                    return channel[index];
                }
            }
            return null;
        }

        private bool TryGet(int index, out TokenChannel<IN> channel) => Channels.TryGetValue(index, out channel);
        
    }
}