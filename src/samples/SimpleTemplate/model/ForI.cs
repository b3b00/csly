using System.Collections.Generic;
using System.Text;

namespace SimpleTemplate.model
{
    public class ForI : ITemplate
    {
        
        public int Start { get; set; }
        
        public int End { get; set; }
        
        public string IteratorName { get; set; }
        
        public List<ITemplate> Items { get; set; }

        public ForI(int start, int end, string iteratorName, List<ITemplate> items)
        {
            Start = start;
            End = end;
            IteratorName = iteratorName;
            Items = items;
        }
        public string GetValue(Dictionary<string, object> context)
        {
            StringBuilder builder = new StringBuilder();
            if (Start < End)
            {
                for (int i = Start; i <= End; i++)
                {
                    foreach (var item in Items)
                    {
                        context[IteratorName] = i;
                        builder.Append(item.GetValue(context));
                        context.Remove(IteratorName);
                    }
                }
            }
            else if (Start > End)
            {
                for (int i = Start; i >= End; i--)
                {
                    foreach (var item in Items)
                    {
                        context[IteratorName] = i;
                        builder.Append(item.GetValue(context));
                        context.Remove(IteratorName);
                    }
                }
            }

            return builder.ToString();
        }
    }
}