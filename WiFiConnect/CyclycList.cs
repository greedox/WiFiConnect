using System.Collections.Generic;

namespace WiFiConnect
{
    public class CyclicList<T>: List<T>
    {
        public int Current { get; private set; }
        public T GetCurrent()
        {
            return this[Current];
        }

        public T GetNext()
        {
            T current = GetCurrent();
            Current = (Current + 1) % this.Count;
            return current;
        }
    }
}
