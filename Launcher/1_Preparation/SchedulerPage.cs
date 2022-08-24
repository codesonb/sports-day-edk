using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;


using EDKv5;

namespace Launcher
{
    class SchedulerPage : wPage
    {
        public SchedulerPage() : base(null)
        {
            UI = spnl = new SchedulerPanel();
        }

        SchedulerPanel spnl;

        public override void In()
        {
            Project prj = Project.GetInstance();
            if (!prj.IsEventConfirmed)
            {
                MessageBox.Show("This page will be available after events are confirmed.", "EDKv5", MessageBoxButton.OK);
                return;
            }
            else if (null == prj.Schedule)
            {
                prj.Schedule = new Schedule();
                prj.Schedule.Init();
            }
            spnl.Update_CompetingGroupList(prj.Schedule);
        }

    }
}
