using System;
using System.Collections;
using System.Collections.Generic;

namespace Saro.BT.Utility
{
    public class FixedSizeStack<T> : IEnumerable<T>, IEnumerable
    {
        private readonly T[] m_Container;

        public int Capacity { get; private set; }
        public int Count { get; private set; }

        public FixedSizeStack(int capacity)
        {
            Count = 0;
            Capacity = capacity;
            this.m_Container = new T[capacity];
        }

        public void Clear()
        {
            Count = 0;
            for (int i = 0; i < m_Container.Length; i++)
            {
                m_Container[i] = default;
            }
        }

        public void FastClear()
        {
            Count = 0;
        }

        public T Peek()
        {
            return m_Container[Count - 1];
        }

        public T Pop()
        {
            return m_Container[--Count];
        }

        public void Push(T value)
        {
            m_Container[Count++] = value;
        }

        public T this[int index]
        {
            get { return m_Container[index]; }
        }

        public T GetValueAt(int index)
        {
            return m_Container[index];
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        public struct Enumerator : IEnumerator<T>, IEnumerator
        {
            public T Current => m_Array[m_Position];

            object IEnumerator.Current => m_Array[m_Position];

            FixedSizeStack<T> m_Array;
            private int m_Position;

            internal Enumerator(FixedSizeStack<T> array)
            {
                if (array == null) throw new Exception();
                this.m_Array = array;
                m_Position = -1;
            }

            void IDisposable.Dispose()
            {

            }

            public bool MoveNext()
            {
                m_Position++;
                return m_Position < m_Array.Count;
            }

            void IEnumerator.Reset()
            {
                m_Position = -1;
            }
        }
    }
}

