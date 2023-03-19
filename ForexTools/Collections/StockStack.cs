using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ForexTools.Collections
{
    public class StockStack<T> : ICollection<T>
    {
        public StockStack(int capacity)
        {
            this.Capacity = capacity;
            this.data = new List<T>(capacity);
        }

        public int Capacity { get; }
        private List<T> data;

        public int Count => data.Count;

        public bool IsReadOnly => false;

        public T this[int index]
        {
            get
            {
                if (data.Count == 0 || data.Count < (index + 1))
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }
                int idx = data.Count - (1 + index);
                return data[idx];
            }
        }

        public void OrderBy<TKey>(Func<T, TKey> expression)
        {
            data = data.OrderBy(expression).ToList();
        }

        public void OrderByDescending<TKey>(Func<T, TKey> expression)
        {
            data = data.OrderByDescending(expression).ToList();
        }

        public void Add(T item)
        {
            if (data.Count == Capacity)
            {
                this.data.Remove(data[0]);
            }
            data.Add(item);
        }

        public void Push(T period)
        {
            this.Add(period);
        }

        public void Clear()
        {
            data.Clear();
        }

        public bool Contains(T item)
        {
            return data.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            data.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            return data.Remove(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new StockStackEnumerator<T>(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    internal class StockStackEnumerator<T> : IEnumerator<T>
    {
        private StockStack<T> list;
        private int index;

        public StockStackEnumerator(StockStack<T> list)
        {
            this.list = list;
            index = 0;
        }

        public T Current
        {
            get
            {
                return list[index++];
            }
        }

        object IEnumerator.Current => this.Current;

        public void Dispose()
        {
            list = null;
        }

        public bool MoveNext()
        {
            return index < this.list.Count;
        }

        public void Reset()
        {
            index = 0;
        }
    }
}
