using sly.parser.generator;
using sly.parser.syntax;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;


namespace sly.parser.syntax
{

    public class SyntaxNode<IN> : ISyntaxNode<IN> where IN : struct
    {
        
        

        public string Name {get; set;} 

        public List<ISyntaxNode<IN>> Children { get; }

        public MethodInfo Visitor { get; set; }

        public bool IsByPassNode { get; set; } = false;

        public OperationMetaData<IN> Operation { get; set; } = null;

        public bool IsExpressionNode => Operation != null; 

        public SyntaxNode(string name, List<ISyntaxNode<IN>> children = null, MethodInfo visitor = null)
        {
            this.Name = name;            
            this.Children = children;
            this.Visitor = visitor;
        }
        
        public override string ToString()
        {
            string r = Name+"(\n";
            Children.ForEach(c => r += c.ToString() + ",\n");
            return r+"\n)";
        }

        public void AddChildren(List<ISyntaxNode<IN>> children)
        {
            this.Children.AddRange(children);
        }

        public void AddChild(ISyntaxNode<IN> child)
        {
            this.Children.Add(child);
        }

        public bool IsTerminal() {
            return false;
        }



        public string Dump(string tab)
        {
            StringBuilder dump = new StringBuilder();
            string bypass = IsByPassNode ? "#BYPASS#" : "";
            string precedence = Operation != null ? $"@{Operation.Precedence}@" : "";
            dump.AppendLine($"{tab}({Name} {bypass} {precedence} [");
            Children.ForEach(c => dump.AppendLine($"{c.Dump(tab + "\t")},"));
            dump.AppendLine($"{tab}]");
            return dump.ToString();
        }

    }
}