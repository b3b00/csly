using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace sly.lexer
{



    public static class Channels
    {
        public const int Main = 0;
        public const int WhiteSpaces = 1;
        public const int Comments = 2;
        public const int Islands = 3;
    }
    
    public class TokenChannels<IN> : IEnumerable<Token<IN>>
    {
        
        
        private readonly Dictionary<int, TokenChannel<IN>> Channels;

        public List<Token<IN>> Tokens => GetChannel(0).Tokens.Where(x => x != null).ToList();

        public int Width => Channels.Select(x => x.Value.Tokens.Count).Max();
        
        public readonly int ChannelId;

        public TokenChannels()
        {
            Channels = new Dictionary<int, TokenChannel<IN>>();
        }

        public TokenChannels(List<Token<IN>> tokens) : this()
        {
            int i = 0;
            var channels = tokens.Select(x => x.Channel).Distinct();
            foreach (var channelId in channels)
            {
                var channel = new TokenChannel<IN>(channelId);
                Channels[channelId] = channel;
                for (int j = 0; j < tokens.Count; j++)
                {
                    channel.Tokens.Add(null);
                }
            }
            
            
            foreach (var token in tokens)
            {
                token.PositionInTokenFlow = i;
                var channel = Channels[token.Channel];
                token.TokenChannels = this;
                channel.Tokens[token.PositionInTokenFlow] = token;
                i++;
            }

            var csv = ToCsv();
            
            ;
        }

        public IEnumerable<TokenChannel<IN>> GetChannels()
        {
            return Channels.Values.OrderBy(x => x.ChannelId);
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
            if (token.TokenID.Equals(default(IN)))
            {
                ;
            }
            token.TokenChannels = this;
            TokenChannel<IN> channel = null;
            if (!TryGet(token.Channel, out channel))
            {
                channel = new TokenChannel<IN>(token.Channel);
                int shift = 0;
                if (Channels.Values.Any())
                {
                    shift = Channels.Values.Max(x => x.Count);
                    for (int i = 0; i < shift; i++)
                    {
                        channel.Add(null);
                    }
                }
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
                    return token;
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

        public bool TryGet(int index, out TokenChannel<IN> channel) => Channels.TryGetValue(index, out channel);

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
            var channels = Channels.Values.OrderBy(x => x.ChannelId);
            return string.Join("\n", Channels.Values.Select(x => x.ToString()).ToArray());
        }

        public string ToCsv()
        {
            StringBuilder builder = new StringBuilder();
            var channels = Channels.Values.OrderBy(x => x.ChannelId);
            foreach (var channel in channels)
            {
                builder.Append($"#{channel.ChannelId} ; ");
                foreach (var token in channel.Tokens)
                {
                    builder.Append(token?.ToString() ?? "null").Append(" ; ");
                }

                builder.AppendLine();
            }

            return builder.ToString();
        }
    }
}