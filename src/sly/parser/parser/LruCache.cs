using System;
using System.Collections.Generic;

namespace sly.parser.parser
{
    /// <summary>
    /// LRU (Least Recently Used) Cache implementation for memoization
    /// Provides better memory management than unlimited Dictionary
    /// </summary>
    /// <typeparam name="TKey">Cache key type</typeparam>
    /// <typeparam name="TValue">Cache value type</typeparam>
    public class LruCache<TKey, TValue>
    {
        private readonly int _capacity;
        private readonly Dictionary<TKey, LinkedListNode<CacheItem>> _cache;
        private readonly LinkedList<CacheItem> _lruList;

        private class CacheItem
        {
            public TKey Key { get; set; }
            public TValue Value { get; set; }
        }

        public LruCache(int capacity)
        {
            if (capacity <= 0)
                throw new ArgumentException("Capacity must be greater than 0", nameof(capacity));

            _capacity = capacity;
            _cache = new Dictionary<TKey, LinkedListNode<CacheItem>>(capacity);
            _lruList = new LinkedList<CacheItem>();
        }

        /// <summary>
        /// Get value from cache, returns true if found
        /// </summary>
        public bool TryGetValue(TKey key, out TValue value)
        {
            if (_cache.TryGetValue(key, out var node))
            {
                // Move to front (most recently used)
                _lruList.Remove(node);
                _lruList.AddFirst(node);
                
                value = node.Value.Value;
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Add or update value in cache
        /// </summary>
        public void Set(TKey key, TValue value)
        {
            if (_cache.TryGetValue(key, out var existingNode))
            {
                // Update existing
                existingNode.Value.Value = value;
                _lruList.Remove(existingNode);
                _lruList.AddFirst(existingNode);
                return;
            }

            // Add new item
            if (_cache.Count >= _capacity)
            {
                // Remove least recently used
                var lruNode = _lruList.Last;
                _lruList.RemoveLast();
                _cache.Remove(lruNode.Value.Key);
            }

            var newItem = new CacheItem { Key = key, Value = value };
            var newNode = _lruList.AddFirst(newItem);
            _cache[key] = newNode;
        }

        /// <summary>
        /// Clear all cached items
        /// </summary>
        public void Clear()
        {
            _cache.Clear();
            _lruList.Clear();
        }

        /// <summary>
        /// Get current number of cached items
        /// </summary>
        public int Count => _cache.Count;

        /// <summary>
        /// Get cache capacity
        /// </summary>
        public int Capacity => _capacity;
    }
}

