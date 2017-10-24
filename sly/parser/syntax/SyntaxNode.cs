using sly.parser.syntax;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;


namespace sly.parser.syntax
{

    public class SyntaxNode<T> : ISyntaxNode<T>
    {
        
        

        public string Name {get; set;} 

        public List<ISyntaxNode<T>> Children { get; }

        public MethodInfo visitor;

        public SyntaxNode(string name, List<ISyntaxNode<T>> children = null, MethodInfo visitor = null)
        {
            this.Name = name;            
            this.Children = children;
            this.visitor = visitor;
        }
        
        public override string ToString()
        {
            string r = Name+"(\n";
            Children.ForEach(c => r += c.ToString() + ",\n");
            return r+"\n)";
        }

        public void AddChildren(List<ISyntaxNode<T>> children)
        {
            this.Children.AddRange(children);
        }

        public void AddChild(ISyntaxNode<T> child)
        {
            this.Children.Add(child);
        }

        public bool IsTerminal() {
            return false;
        }



        public string Dump(string tab)
        {
            StringBuilder dump = new StringBuilder();
            dump.AppendLine($"{tab}({Name} [");
            Children.ForEach(c => dump.AppendLine($"{c.Dump(tab + "\t")},"));
            dump.AppendLine($"{tab}]");
            return dump.ToString();
        }

    }
}