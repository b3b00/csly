using System.Collections.Generic;
using SimpleTemplate.model;

namespace SimpleTemplate.model.expressions
{
    public interface Expression : ITemplate
    {

        object Evaluate(Dictionary<string, object> context);

        

    }
}