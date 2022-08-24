using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDKv5.Utility.Scanners
{
    public interface IScanOutput
    {
        int GetColumnCount();
        bool SetCode(byte[] code);
        int SetRow(bool[] cols);    // return error count
        bool Next();
    }
}
