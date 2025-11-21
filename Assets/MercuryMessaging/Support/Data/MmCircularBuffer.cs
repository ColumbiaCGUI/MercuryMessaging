// Copyright (c) 2017-2019, Columbia University
// All rights reserved.
// This file is part of MercuryMessaging.

using System.Collections;
using System.Collections.Generic;

namespace MercuryMessaging.Support.Data
{
    /// <summary>
    /// Fixed-size circular buffer that automatically overwrites oldest entries
    /// Used for message history tracking with bounded memory
    /// </summary>
    /// <typeparam name="T">Type of items to store</typeparam>
    public class CircularBuffer<T> : IEnumerable<T>
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
        public CircularBuffer(int capacity)
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
        /// Insert item at beginning (newest position)
        /// Maintains compatibility with List.Insert(0, item) pattern
        /// </summary>
        public void Insert(int index, T item)
        {
            if (index != 0)
                throw new System.ArgumentException("CircularBuffer only supports Insert(0, item)", nameof(index));

            Add(item);
        }

        /// <summary>
        /// Get item at index (0 = newest, Count-1 = oldest)
        /// </summary>
        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= _size)
                    throw new System.IndexOutOfRangeException();

                // Calculate actual position (newest first)
                int actualIndex = (_head - 1 - index + _capacity) % _capacity;
                if (actualIndex < 0) actualIndex += _capacity;

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
        /// Get all items in order (newest to oldest)
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
