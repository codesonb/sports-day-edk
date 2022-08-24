using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;


using EDKv5;

namespace Launcher
{
    class EventSettingPage : wPage
    {
        // constructor
        public EventSettingPage() : base(null)
        {
            Project prj = Project.GetInstance();

            pages = new List<IPageSwitch>()
            {
                new OpenEventPanel(),
                new OpenGroupPanel(),
                new ApplicationPanel(),
                new LaneAssignPanel()
            };

            if (prj.IsEventConfirmed)
                page = 2;
            else if (prj.Events.Length > 0)
                page = 1;

            this.UI = (UIElement)pages[page];

            foreach (IPageSwitch page in pages)
            {
                page.CallPreviousPage += Page_CallPreviousPage;
                page.CallNextPage += Page_CallNextPage;
            }

        }
        public override void In()
        {
            IPage innerPage = this.UI as IPage;
            innerPage?.In();
        }

        private void Page_CallNextPage(object sender, EventArgs e)
        {
            if (page < pages.Count - 1)
            {
                IPage o = pages[page] as IPage;
                o?.BeforeOut();
                FrameworkElement s = (FrameworkElement)pages[++page];
                this.UI = s;
                (s as IPage)?.In();
            }
        }

        private void Page_CallPreviousPage(object sender, EventArgs e)
        {
            if (page > 0)
            {
                IPage o = pages[page] as IPage;
                o?.BeforeOut();
                FrameworkElement s = (FrameworkElement)pages[--page];
                this.UI = s;
                (s as IPage)?.In();
            }
        }

        // fields
        int page = 0;
        List<IPageSwitch> pages;

    }
}
