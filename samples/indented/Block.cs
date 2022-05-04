using System.Collections.Generic;
using System.Text;

namespace indented
{
    public class Block : Statement
    {
        public List<Ast> Statements { get; set; } = new List<Ast>();
        
        public string Comment { get; set; }

        public Block(List<Ast> stats)
        {
            Statements = stats;
        }

        public string Dump(string tab)
        {
            StringBuilder builder = new StringBuilder();
            string newTab = tab + "\t";
            foreach (var statement in Statements)
            {
                builder.AppendLine(statement.Dump(newTab));
            }

            return builder.ToString();
        }
    }
}