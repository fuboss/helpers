using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Code.Tools
{
    [Serializable]
    public struct Tuple<T, E>
    {
        public readonly T First;
        public readonly E Second;

        public Tuple(T first, E second)
        {
            First = first;
            Second = second;
        }

        public bool IsEmpty { get { return Equals(First, default(T)) && Equals(Second, default(E)); } }

        public override string ToString()
        {
            return String.Format("Tuple<{2}, {3}>[First = {0}, Second = {1}]", First, Second, typeof(T).Name, typeof(E).Name);
        }
    }
}
