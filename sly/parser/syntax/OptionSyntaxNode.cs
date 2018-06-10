using sly.parser.generator;
using sly.parser.syntax;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Text;


namespace sly.parser.syntax
{

    public class OptionSyntaxNode<IN> : SyntaxNode<IN> where IN : struct
    {

        public OptionSyntaxNode(string name, List<ISyntaxNode<IN>> children = null, MethodInfo visitor = null) : base(name,children,visitor)
        {           
        }

    }
}