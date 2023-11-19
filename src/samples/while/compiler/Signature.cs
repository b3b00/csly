namespace csly.whileLang.compiler
{
    public class Signature
    {
        private readonly WhileType Left;
        public WhileType Result;
        private readonly WhileType Right;

        public Signature(WhileType left, WhileType right, WhileType result)
        {
            Left = left;
            Right = right;
            Result = result;
        }

        public bool Match(WhileType l, WhileType r)
        {
            return (Left == WhileType.ANY || l == Left) &&
                   (Right == WhileType.ANY || r == Right);
        }
    }
}