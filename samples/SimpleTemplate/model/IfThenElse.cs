using System.Collections.Generic;
using SimpleTemplate.model.expressions;
using sly.lexer;

namespace SimpleTemplate.model
{
    public class IfThenElse : ITemplate
    {
        public Expression Condition { get; set; }

        public Block ThenBlock
        {
            get;
            set;
        }
        
        public Block ElseBlock
        {
            get;
            set;
        }

        public IfThenElse(Expression condition, Block thenBlock, Block elseBlock)
        {
            Condition = condition;
            ThenBlock = thenBlock;
            ElseBlock = elseBlock;
        }

        public string GetValue(Dictionary<string, object> context)
        {
            object testObj = Condition.Evaluate(context);
            if (testObj is bool test && test)
            {
                return ThenBlock.GetValue(context);
            }
            else
            {
                return ElseBlock.GetValue(context);
            }
        }
    }
}