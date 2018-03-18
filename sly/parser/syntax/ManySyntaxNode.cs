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

        public ManySyntaxNode(string name, List<ISyntaxNode<IN>> children) : base(name,children)
        {
            this.Name = name;
            this.AddChildren(children);
        }
        
        

        public void Add(ISyntaxNode<IN> child)
        {
            Children.Add(child);
        }

        public void AddRange(List<ISyntaxNode<IN>> children)
        {
            Children.AddRange(children);
        }



        public override string ToString()
        {
            string r = Name+"(\n";
            Children.ForEach(c => r += c.ToString() + ",\n");
            return r+"\n)";
        }


        
    }
}