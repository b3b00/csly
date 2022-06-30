using System.Collections.Generic;

namespace SimpleTemplate.model
{
    public interface ITemplate
    {
        string GetValue(Dictionary<string,object> context);
    }
}