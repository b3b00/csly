using System.Text;

namespace indented
{
    public class IfThenElse : Statement
    {
        public Cond Cond { get; set; }

        public Block Then { get; set; }

        public Block Else { get; set; }

        public IfThenElse(Cond cond, Block thenBlock, Block elseBlock)
        {
            Cond = cond;
            Then = thenBlock;
            Else = elseBlock;
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
                builder.AppendLine(Else.Dump(tab));
            }
            
            return builder.ToString();
        }
    }
}