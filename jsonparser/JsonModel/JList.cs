using System.Collections.Generic;

namespace jsonparser.JsonModel
{
    public class JList : JSon

    {
        public override bool IsList => true;

        private List<JSon> list;

        public  List<JSon> Items => list;
        public int Count => list.Count;

        public JSon this[int index]
        {
            get
            {
                return list[index];
            }
            set
            {
                list[index] = value;
            }
        }

        public JList()
        {
            list = new List<JSon>();
        }
        
        public JList(List<JSon> lst)
        {
            list = lst;
        }
        
        
        public JList(JSon item)
        {
            list = new List<JSon>();
            list.Add(item);
        }

        public void Add(JSon item)
        {
            list.Add(item);
        }
        
        public void AddRange(JList items)
        {
            list.AddRange(items.Items);
        }
        
        
    }
}