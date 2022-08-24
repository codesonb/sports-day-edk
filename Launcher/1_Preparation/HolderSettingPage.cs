
using EDKv5;
using System;

namespace Launcher
{
    class HolderSettingPage : wPage
    {
        public HolderSettingPage(ResultHolderPanel ui) : base(ui)
        {
            this.pnlInfo = ui;
        }

        ResultHolderPanel pnlInfo;

    }
}
