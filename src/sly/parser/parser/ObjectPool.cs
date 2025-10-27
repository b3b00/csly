using System;
using System.Collections.Concurrent;

namespace sly.parser.parser
{
    /// <summary>
    /// Generic object pool to reduce allocations during parsing
    /// </summary>
    /// <typeparam name="T">Type of objects to pool</typeparam>
    public class ObjectPool<T> where T : class, new()
    {
        private ConcurrentBag<T> _objects = new ConcurrentBag<T>();
        private readonly Func<T> _objectGenerator;
        private readonly Action<T> _resetAction;
        private readonly int _maxSize;
        private int _currentSize;

        public ObjectPool(Func<T> objectGenerator = null, Action<T> resetAction = null, int maxSize = 100)
        {
            _objectGenerator = objectGenerator ?? (() => new T());
            _resetAction = resetAction;
            _maxSize = maxSize;
            _currentSize = 0;
        }

        public T Get()
        {
            if (_objects.TryTake(out T item))
            {
                return item;
            }
            return _objectGenerator();
        }

        public void Return(T item)
        {
            if (item == null || _currentSize >= _maxSize)
                return;

            _resetAction?.Invoke(item);
            _objects.Add(item);
            System.Threading.Interlocked.Increment(ref _currentSize);
        }

        public void Clear()
        {
            _objects = new ConcurrentBag<T>();
            _currentSize = 0;
        }
    }
}

