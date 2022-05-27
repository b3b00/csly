using System.Text;

namespace indented
{
    public class IfThenElse : Statement
    {
        public Cond Cond { get; set; }

        public Block Then { get; set; }

        public Block Else { get; set; }
        
        public string Comment { get; set; }
        public bool IsCommented => !string.IsNullOrWhiteSpace(Comment);

        public IfThenElse(Cond cond, Block thenBlock, Block elseBlock, string comment = null)
        {
            Cond = cond;
            Then = thenBlock;
            Else = elseBlock;
            Comment = comment;
        }

        public string Dump(string tab)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append($"{tab}IF")
                .AppendLine(Cond.Dump(tab))
                .Append(Then.Dump(tab));
            if (Else != null)
            {
                builder.AppendLine($"{tab}ELSE");
                builder.Append(Else.Dump(tab));
            }
            
            return builder.ToString();
        }
    }
}