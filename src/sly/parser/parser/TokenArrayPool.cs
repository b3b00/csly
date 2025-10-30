using System;
using System.Buffers;
using sly.lexer;

namespace sly.parser.parser
{
    /// <summary>
    /// Helper class to manage token array pooling for parsing operations
    /// Reduces allocations by reusing token arrays
    /// </summary>
    public static class TokenArrayPool<IN> where IN : struct, Enum
    {
        private static readonly ArrayPool<Token<IN>> Pool = ArrayPool<Token<IN>>.Shared;

        /// <summary>
        /// Rent a token array from the pool
        /// </summary>
        /// <param name="minimumLength">Minimum required length</param>
        /// <returns>Rented array (may be larger than requested)</returns>
        public static Token<IN>[] Rent(int minimumLength)
        {
            return Pool.Rent(minimumLength);
        }

        /// <summary>
        /// Return a token array to the pool
        /// </summary>
        /// <param name="array">Array to return</param>
        /// <param name="clearArray">Whether to clear the array before returning</param>
        public static void Return(Token<IN>[] array, bool clearArray = false)
        {
            if (array != null)
            {
                Pool.Return(array, clearArray);
            }
        }

        /// <summary>
        /// Copy tokens from source array to a pooled array
        /// </summary>
        public static Token<IN>[] RentAndCopy(Token<IN>[] source, int startIndex, int length)
        {
            var rented = Rent(length);
            Array.Copy(source, startIndex, rented, 0, length);
            return rented;
        }
    }
}

