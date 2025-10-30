using System;
using System.Collections.Generic;
using sly.lexer;

namespace sly.parser
{
    /// <summary>
    /// Extension methods for using TokenArrayPool efficiently
    /// </summary>
    public static class TokenArrayPoolExtensions
    {
        /// <summary>
        /// Convert IList to array using pooled memory when possible
        /// </summary>
        public static Token<IN>[] ToPooledArray<IN>(this IList<Token<IN>> list) where IN : struct, Enum
        {
            if (list == null)
                return null;

            var count = list.Count;
            var array = global::sly.parser.parser.TokenArrayPool<IN>.Rent(count);
            
            for (int i = 0; i < count; i++)
            {
                array[i] = list[i];
            }

            return array;
        }

        /// <summary>
        /// Create a slice of tokens using pooled memory
        /// </summary>
        public static Token<IN>[] SliceToPooledArray<IN>(this Token<IN>[] source, int start, int length) 
            where IN : struct, Enum
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            
            if (start < 0 || start >= source.Length)
                throw new ArgumentOutOfRangeException(nameof(start));
            
            if (length < 0 || start + length > source.Length)
                throw new ArgumentOutOfRangeException(nameof(length));

            return global::sly.parser.parser.TokenArrayPool<IN>.RentAndCopy(source, start, length);
        }

        /// <summary>
        /// Convert collection to array using standard ToArray() without pooling
        /// Use this for small, short-lived arrays
        /// </summary>
        public static Token<IN>[] ToArrayUnpooled<IN>(this IEnumerable<Token<IN>> collection) 
            where IN : struct, Enum
        {
            if (collection is IList<Token<IN>> list)
            {
                var array = new Token<IN>[list.Count];
                list.CopyTo(array, 0);
                return array;
            }

            var result = new List<Token<IN>>(collection);
            return result.ToArray();
        }
    }
}

