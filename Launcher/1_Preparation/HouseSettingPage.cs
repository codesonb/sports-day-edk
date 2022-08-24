using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;


using EDKv5;

namespace Launcher
{
    class HouseSettingPage : wPage
    {
        public HouseSettingPage(HousePanel UI) : base(UI) { hp = UI; }

        HousePanel hp;

        public override void In()
        {
            hp.UpdateValues();
        }
    }
}
