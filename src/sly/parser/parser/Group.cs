using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using sly.lexer;

namespace sly.parser.parser
{
    public class Group<IN, OUT> where IN : struct
    {
        public readonly List<GroupItem<IN, OUT>> Items;

        private Dictionary<string, GroupItem<IN, OUT>> ItemsByName;

        public Group()
        {
            Items = new List<GroupItem<IN, OUT>>();
            ItemsByName = new Dictionary<string, GroupItem<IN, OUT>>();
        }

        public int Count => Items.Count;


        public OUT Value(int i)
        {
            return Items[i].Value;
        }

        public Token<IN> Token(int i)
        {
            return Items[i].Token;
        }


        public OUT Value(string name)
        {
            return ItemsByName.TryGetValue(name, out var value) ? value.Value : default(OUT);
        }

        public Token<IN> Token(string name)
        {
            return ItemsByName.TryGetValue(name, out var value) ? value.Token : null;
        }

        public void Add(string name, Token<IN> token)
        {
            var groupItem = new GroupItem<IN, OUT>(name, token);
            Items.Add(groupItem);
            ItemsByName[name] = groupItem;
        }

        public void Add(string name, OUT value)
        {
            var groupItem = new GroupItem<IN, OUT>(name, value);
            Items.Add(groupItem);
            ItemsByName[name] = groupItem;
        }


        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.Append("GROUP(");
            foreach (var item in Items)
            {
                builder.Append(item);
                builder.Append(",");
            }

            builder.Append(")");
            return builder.ToString();
        }
    }
}