using System;
using sly.lexer;
using sly.parser.parser;

namespace sly.parser
{
    /// <summary>
    /// Provides a disposable scope for using pooled token arrays
    /// Ensures arrays are returned to the pool when disposed
    /// </summary>
    /// <typeparam name="IN">Token type</typeparam>
    public struct PooledTokenArray<IN> : IDisposable where IN : struct, Enum
    {
        private Token<IN>[] _array;
        private readonly int _actualLength;
        private bool _disposed;

        /// <summary>
        /// The pooled array. May be larger than the requested size.
        /// Use Length property to get the actual useful length.
        /// </summary>
        public Token<IN>[] Array => _array;

        /// <summary>
        /// The actual length of useful data in the array
        /// </summary>
        public int Length => _actualLength;

        internal PooledTokenArray(Token<IN>[] array, int actualLength)
        {
            _array = array;
            _actualLength = actualLength;
            _disposed = false;
        }

        /// <summary>
        /// Create a pooled array from a source with copying
        /// </summary>
        public static PooledTokenArray<IN> FromCopy(Token<IN>[] source, int start, int length)
        {
            var array = global::sly.parser.parser.TokenArrayPool<IN>.RentAndCopy(source, start, length);
            return new PooledTokenArray<IN>(array, length);
        }

        /// <summary>
        /// Create a pooled array with a specific size
        /// </summary>
        public static PooledTokenArray<IN> Rent(int minimumLength)
        {
            var array = global::sly.parser.parser.TokenArrayPool<IN>.Rent(minimumLength);
            return new PooledTokenArray<IN>(array, minimumLength);
        }

        /// <summary>
        /// Get a span view of the actual data (not the excess capacity)
        /// </summary>
        public Span<Token<IN>> AsSpan()
        {
            return new Span<Token<IN>>(_array, 0, _actualLength);
        }

        /// <summary>
        /// Return the array to the pool
        /// </summary>
        public void Dispose()
        {
            if (!_disposed && _array != null)
            {
                global::sly.parser.parser.TokenArrayPool<IN>.Return(_array, clearArray: true);
                _array = null;
                _disposed = true;
            }
        }
    }

    /// <summary>
    /// Factory methods for creating PooledTokenArray instances
    /// </summary>
    public static class PooledTokenArray
    {
        /// <summary>
        /// Rent a pooled token array
        /// </summary>
        public static PooledTokenArray<IN> Rent<IN>(int minimumLength) where IN : struct, Enum
        {
            return PooledTokenArray<IN>.Rent(minimumLength);
        }

        /// <summary>
        /// Create a pooled array from a slice of another array
        /// </summary>
        public static PooledTokenArray<IN> FromSlice<IN>(Token<IN>[] source, int start, int length) 
            where IN : struct, Enum
        {
            return PooledTokenArray<IN>.FromCopy(source, start, length);
        }
    }
}

