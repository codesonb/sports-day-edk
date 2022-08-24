using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EDKv5.Utility
{
    public interface IManualMatch
    {
        void Initialize(Tuple<string, int>[] keys, string[] values, Semaphore semaphore);
        short[] Result { get; }
        bool Cancelled { get; }
    }
}
