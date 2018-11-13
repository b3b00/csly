using System.Collections.Generic;
using csly.whileLang.model;

namespace csly.whileLang.compiler
{
    public class Signatures
    {
        private readonly Dictionary<BinaryOperator, List<Signature>> binaryOperationSignatures;

        public Signatures()
        {
            binaryOperationSignatures = new Dictionary<BinaryOperator, List<Signature>>
            {
                {
                    BinaryOperator.ADD, new List<Signature>
                    {
                        new Signature(WhileType.INT, WhileType.INT, WhileType.INT)
                    }
                },
                {
                    BinaryOperator.SUB, new List<Signature>
                    {
                        new Signature(WhileType.INT, WhileType.INT, WhileType.INT)
                    }
                },
                {
                    BinaryOperator.DIVIDE, new List<Signature>
                    {
                        new Signature(WhileType.INT, WhileType.INT, WhileType.INT)
                    }
                },
                {
                    BinaryOperator.MULTIPLY, new List<Signature>
                    {
                        new Signature(WhileType.INT, WhileType.INT, WhileType.INT)
                    }
                },
                {
                    BinaryOperator.AND, new List<Signature>
                    {
                        new Signature(WhileType.BOOL, WhileType.BOOL, WhileType.BOOL)
                    }
                },
                {
                    BinaryOperator.OR, new List<Signature>
                    {
                        new Signature(WhileType.BOOL, WhileType.BOOL, WhileType.BOOL)
                    }
                },
                {
                    BinaryOperator.LESSER, new List<Signature>
                    {
                        new Signature(WhileType.INT, WhileType.INT, WhileType.BOOL),
                        new Signature(WhileType.STRING, WhileType.STRING, WhileType.BOOL),
                        new Signature(WhileType.BOOL, WhileType.BOOL, WhileType.BOOL)
                    }
                },
                {
                    BinaryOperator.GREATER, new List<Signature>
                    {
                        new Signature(WhileType.INT, WhileType.INT, WhileType.BOOL),
                        new Signature(WhileType.STRING, WhileType.STRING, WhileType.BOOL),
                        new Signature(WhileType.BOOL, WhileType.BOOL, WhileType.BOOL)
                    }
                },
                {
                    BinaryOperator.EQUALS, new List<Signature>
                    {
                        new Signature(WhileType.INT, WhileType.INT, WhileType.BOOL),
                        new Signature(WhileType.STRING, WhileType.STRING, WhileType.BOOL),
                        new Signature(WhileType.BOOL, WhileType.BOOL, WhileType.BOOL)
                    }
                },
                {
                    BinaryOperator.DIFFERENT, new List<Signature>
                    {
                        new Signature(WhileType.INT, WhileType.INT, WhileType.BOOL),
                        new Signature(WhileType.STRING, WhileType.STRING, WhileType.BOOL),
                        new Signature(WhileType.BOOL, WhileType.BOOL, WhileType.BOOL)
                    }
                },
                {
                    BinaryOperator.CONCAT, new List<Signature>
                    {
                        new Signature(WhileType.ANY, WhileType.ANY, WhileType.STRING)
                    }
                }
            };
        }

        public WhileType CheckBinaryOperationTyping(BinaryOperator oper, WhileType left, WhileType right)
        {
            WhileType result;
            if (binaryOperationSignatures.ContainsKey(oper))
            {
                var signatures = binaryOperationSignatures[oper];
                var res = signatures.Find(sig => sig.Match(left, right));
                if (res != null)
                    result = res.Result;
                else
                    throw new SignatureException($"invalid operation {left} {oper} {right}");
            }
            else
            {
                result = left;
            }

            return result;
        }
    }
}