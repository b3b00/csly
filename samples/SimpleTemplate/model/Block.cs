using System.Collections.Generic;
using System.Text;

namespace SimpleTemplate.model
{
    public class Block : ITemplate
    {
        public List<ITemplate> Items { get; set; }


        public Block()
        {
            Items = new List<ITemplate>();
        }
        
        public Block(List<ITemplate> items)
        {
            Items = items;
        }
        
        public string GetValue(Dictionary<string, object> context)
        {
            StringBuilder builder = new StringBuilder();
            foreach (var item in Items)
            {
                builder.Append(item.GetValue(context));
            }

            return builder.ToString();
        }
    }
}