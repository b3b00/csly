using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace SimpleTemplate.model
{
    public class ForEach : ITemplate
    {
        
        public string ListName { get; set; }
        
        public string IteratorName { get; set; }
        
        public List<ITemplate> Items { get; set; }

        public ForEach(string listName, string iteratorName, List<ITemplate> items)
        {
            ListName = listName;
            IteratorName = iteratorName;
            Items = items;
        }
        public string GetValue(Dictionary<string, object> context)
        {
            StringBuilder builder = new StringBuilder();
            object list = null;
            if (context.TryGetValue(ListName, out list) && list is IEnumerable enumerable)
            {
                foreach (var item in enumerable)
                {
                    context[IteratorName] = item;
                    foreach (var subItem in Items)
                    {
                        builder.Append(subItem.GetValue(context));
                    }

                    context.Remove(IteratorName);
                }

                return builder.ToString();
            }
            throw new TemplateEvaluationException(
                $"{ListName}[{(list != null ? list.GetType().FullName : "")}] does not exist or is not enumerable.");
        }
    }
}