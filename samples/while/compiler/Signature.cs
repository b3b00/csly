using System.Collections.Generic;
using csly.whileLang.model;

namespace csly.whileLang.compiler
{


    public class Signature
    {
        WhileType Left;
        WhileType Right;
        public WhileType Result;

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
 
