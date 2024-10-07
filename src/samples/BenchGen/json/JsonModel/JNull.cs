namespace benchgen.jsonparser.JsonModel
{
    public class JNull : JSon
    {
        public override bool IsNull => true;
        public override string ToJson() => "null";
    }
}