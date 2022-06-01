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
        
        
        private readonly Dictionary<int, TokenChannel<IN>> _tokenChannels;

        public List<Token<IN>> Tokens => GetChannel(Channels.Main).Tokens.Where(x => x != null).ToList();
         
        
        public readonly int ChannelId;

        public TokenChannels()
        {
            _tokenChannels = new Dictionary<int, TokenChannel<IN>>();
        }

        public TokenChannels(List<Token<IN>> tokens) : this()
        {
            if (nameof(IN) == "DoNotIgnoreCommentsTokenWithChannels")
            {
                ;
            }
            int i = 0;
            foreach (var token in tokens)
            {
                token.PositionInTokenFlow = i;
                Add(token);
                i++;
            }
        }

        public IEnumerable<TokenChannel<IN>> GetChannels()
        {
            return _tokenChannels.Values.OrderBy(x => x.ChannelId);
        }
        
        public TokenChannel<IN> GetChannel(int i)
        {
            return _tokenChannels[i];
        }
        
        public void SetChannel(int i, TokenChannel<IN> token)
        {
            _tokenChannels[i] = token;
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
            token.TokenChannels = this;
            TokenChannel<IN> channel = null;
            if (!TryGet(token.Channel, out channel))
            {
                channel = new TokenChannel<IN>(token.Channel);
                int shift = 0;
                if (_tokenChannels.Values.Any())
                {
                    shift = _tokenChannels.Values.Max(x => x.Count);
                }
                for (int i = 0; i < shift; i++)
                {
                    channel.Add(null);
                }
                _tokenChannels[token.Channel] = channel;
            }

            int index = _tokenChannels.Values.Max(x => x.Count);
            foreach (var VARIABLE in _tokenChannels.Values)
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

        public bool ContainsChannel(int channel) => _tokenChannels.ContainsKey(channel);

        
        
        public Token<IN> TokenAt(int index)
        {
            foreach (var channel in _tokenChannels)
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

        public bool TryGet(int index, out TokenChannel<IN> channel) => _tokenChannels.TryGetValue(index, out channel);

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
            var channels = _tokenChannels.Values.OrderBy(x => x.ChannelId);
            return string.Join("\n", _tokenChannels.Values.Select(x => x.ToString()).ToArray());
        }
    }
}