using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EDKv5.Utility
{
    public interface IAskFirstRow
    {
        void Initialize(dynamic[,] table, string[] colNames, Semaphore semaphore);
        bool FirstRowIsHeading { get; }
    }
}
