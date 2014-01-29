using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestOnvif
{
    public class CircularBuffer<T> where T : new()
    {
        private T[] buffer;

        private Object locker = new Object();

        private volatile int indexOut;
        private volatile int indexIn;

        private volatile int count;

        private const int BUFFER_SIZE = 8;

        public int Count
        {
            get { return count; }
            private set { count = value; }
        }

        private volatile bool isComplete;

        public bool IsComplete
        {
            get { return isComplete; }
            set { isComplete = value; }
        }

        public CircularBuffer()
        {
            buffer = new T[BUFFER_SIZE];
            indexOut = 0;
            indexIn = 0;
            count = 0;
        }

        public void Add(T t)
        {
            lock (locker)
            {
                buffer[indexIn] = t;
                indexIn = (indexIn + 1) % BUFFER_SIZE;
                count = (count + 1) % BUFFER_SIZE;

                if (count == 0)
                    Logger.Write("Buffer full droping frames", EnumLoggerType.DebugLog);
            }
        }

        public bool TryGet(out T t)
        {
            t = new T();
            lock (locker)
            {
                if (count > 0)//(indexOut != (indexIn ))
                {
                    t = buffer[indexOut];
                    indexOut = (indexOut + 1) % BUFFER_SIZE;
                    count = (count - 1) % BUFFER_SIZE;

                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public T Get()
        {
            lock (locker)
            {
                if (count > 0)
                {
                    int index = indexOut;
                    indexOut = (indexOut + 1) % BUFFER_SIZE;
                    count = (count - 1) % BUFFER_SIZE;

                    return buffer[index];
                }
                else
                {
                    return default(T);
                }
            }
        }
    }
}
