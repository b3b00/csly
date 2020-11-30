using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace sly.lexer
{



    public static class Channels
    {
        public const int Main = 0;
        public const int WhiteSpaces = 1;
        public const int Comments = 2;
    }
    
    public class TokenChannels<IN> : IEnumerable<Token<IN>>
    {
        
        
        private Dictionary<int, TokenChannel<IN>> Channels;

        public List<Token<IN>> Tokens => GetChannel(0).Tokens.Where(x => x != null).ToList();
         
        
        public readonly int ChannelId;

        public TokenChannels()
        {
            Channels = new Dictionary<int, TokenChannel<IN>>();
        }

        public TokenChannels(List<Token<IN>> tokens) : this()
        {
            foreach (var token in tokens)
            {
                Add(token);
            }
        }
        
        public TokenChannel<IN> GetChannel(int i)
        {
            return Channels[i];
        }
        
        public void SetChannel(int i, TokenChannel<IN> token)
        {
            Channels[i] = token;
        }

        public Token<IN> this[int index]
        {
            get => GetToken(index);
            set
            {
            }
        }

        public int Count => Tokens.Count;

        private Token<IN> GetToken(int index)
        {
            var list = Tokens;
            return list[index];
        }

        public void Add(Token<IN> token)
        {
            TokenChannel<IN> channel = null;
            if (!TryGet(token.Channel, out channel))
            {
                channel = new TokenChannel<IN>(token.Channel);
                
                Channels[token.Channel] = channel;
            }

            int index = Channels.Values.Max(x => x.Count);
            foreach (var VARIABLE in Channels.Values)
            {
                for (int i = channel.Count; i <= index; i++)
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

        public IEnumerator<Token<IN>> GetEnumerator()
        {
            return Tokens.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Tokens.GetEnumerator();
        }

        public override string ToString()
        {
            return string.Join("\n", Channels.Values.Select(x => x.ToString()).ToArray());
        }
    }
}