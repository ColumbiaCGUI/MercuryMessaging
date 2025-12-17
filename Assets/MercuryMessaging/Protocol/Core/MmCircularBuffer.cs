// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
// This file is part of MercuryMessaging.

using System.Collections;
using System.Collections.Generic;

namespace MercuryMessaging
{
    /// <summary>
    /// Fixed-size circular buffer that automatically overwrites oldest entries
    /// Used for message history tracking with bounded memory
    /// </summary>
    /// <typeparam name="T">Type of items to store</typeparam>
    public class MmCircularBuffer<T> : IEnumerable<T>
    {
        private readonly T[] _buffer;
        private int _head = 0;
        private int _size = 0;
        private readonly int _capacity;

        /// <summary>
        /// Current number of items in buffer
        /// </summary>
        public int Count => _size;

        /// <summary>
        /// Maximum capacity of buffer
        /// </summary>
        public int Capacity => _capacity;

        /// <summary>
        /// Creates circular buffer with specified capacity
        /// </summary>
        /// <param name="capacity">Maximum number of items to store</param>
        public MmCircularBuffer(int capacity)
        {
            if (capacity <= 0)
                throw new System.ArgumentException("Capacity must be positive", nameof(capacity));

            _capacity = capacity;
            _buffer = new T[capacity];
        }

        /// <summary>
        /// Add item to buffer, overwriting oldest if full
        /// </summary>
        /// <param name="item">Item to add</param>
        public void Add(T item)
        {
            _buffer[_head] = item;
            _head = (_head + 1) % _capacity;
            if (_size < _capacity)
                _size++;
        }

        /// <summary>
        /// Insert item at beginning (oldest position) for List.Insert(0, item) compatibility
        /// Maintains compatibility with List.Insert(0, item) pattern
        /// </summary>
        public void Insert(int index, T item)
        {
            if (index != 0)
                throw new System.ArgumentException("MmCircularBuffer only supports Insert(0, item)", nameof(index));

            if (_size == _capacity)
            {
                // When full: replace the oldest item in place (don't advance head)
                int oldestPos = (_capacity + _head - _size) % _capacity;
                _buffer[oldestPos] = item;
                // _head and _size remain unchanged
            }
            else
            {
                // When not full: insert before current oldest without overwriting
                int oldestPos = (_capacity + _head - _size) % _capacity;
                int newOldestPos = (oldestPos - 1 + _capacity) % _capacity;
                _buffer[newOldestPos] = item;
                _size++;
                // _head remains unchanged as it still points to next write position
            }
        }

        /// <summary>
        /// Get item at index (0 = oldest, Count-1 = newest)
        /// Standard list indexing order
        /// </summary>
        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= _size)
                    throw new System.IndexOutOfRangeException();

                // Calculate position of oldest item
                // oldest = (_capacity + _head - _size) % _capacity
                // This works for both full and non-full buffers, including after Insert(0)
                int oldestPos = (_capacity + _head - _size) % _capacity;
                int actualIndex = (oldestPos + index) % _capacity;

                return _buffer[actualIndex];
            }
        }

        /// <summary>
        /// Clear all items from buffer
        /// </summary>
        public void Clear()
        {
            _head = 0;
            _size = 0;
            System.Array.Clear(_buffer, 0, _capacity);
        }

        /// <summary>
        /// Get all items in order (oldest to newest)
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < _size; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
