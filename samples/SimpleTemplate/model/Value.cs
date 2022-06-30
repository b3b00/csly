using System.Collections.Generic;

namespace SimpleTemplate.model
{
    public class Value : ITemplate
    {
        public string VariableName { get; set; }

        public Value(string variableName)
        {
            VariableName = variableName;
        }
        
        public string GetValue(Dictionary<string, object> context)
        {
            object value = "";
            if (context.TryGetValue(VariableName, out value))
            {
                return value.ToString();
            }
            return "";
        }
    }
}