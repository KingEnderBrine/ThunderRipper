using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThunderClassGenerator.Utilities
{
    public struct EmptyEnumerator<T> : IEnumerator<T>
    {
        public T Current => default;
        object IEnumerator.Current => default;

        public bool MoveNext() => false;
        public void Dispose() { }
        public void Reset() { }
    }
}
