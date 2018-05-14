using sly.parser.syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace sly.parser.syntax
{

    public class ManySyntaxNode<IN> : SyntaxNode<IN> where IN : struct
    {

        public bool IsManyTokens { get; set; }
        
        public bool IsManyValues { get { return !IsManyTokens; } set { IsManyTokens = !value; }}
        
        public ManySyntaxNode(string name) : base(name, new List<ISyntaxNode<IN>>())
        {
        }
        

        public void Add(ISyntaxNode<IN> child)
        {
            Children.Add(child);
        }



        
    }
}