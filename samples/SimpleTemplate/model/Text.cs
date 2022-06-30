using System.Collections.Generic;

namespace SimpleTemplate.model
{
    public class Text : ITemplate
    {
        public string Content { get; set; }

        public Text(string content)
        {
            Content = content;
        }
        
        public string GetValue(Dictionary<string, object> context)
        {
            return Content;
        }
    }
}