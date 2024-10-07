using System.Collections.Generic;

namespace benchgen.jsonparser.JsonModel
{
    public class JList : JSon

    {
        public JList()
        {
            Items = new List<JSon>();
        }

        public JList(List<JSon> lst)
        {
            Items = lst;
        }


        public JList(JSon item)
        {
            Items = new List<JSon>();
            Items.Add(item);
        }

        public override bool IsList => true;
        public override string ToJson()
        {
            return $"[{string.Join(",", Items.Select(i => i.ToJson()))}]";
        }

        public List<JSon> Items { get; }

        public int Count => Items.Count;

        public JSon this[int index]
        {
            get => Items[index];
            set => Items[index] = value;
        }

        public void Add(JSon item)
        {
            Items.Add(item);
        }

        public void AddRange(JList items)
        {
            Items.AddRange(items.Items);
        }

        public void AddRange(List<JSon> items)
        {
            Items.AddRange(items);
        }
    }
}