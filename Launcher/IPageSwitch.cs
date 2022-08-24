using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Launcher
{
    interface IPageSwitch
    {
        event EventHandler CallPreviousPage;
        event EventHandler CallNextPage;
    }
}
