
using EDKv5;
using System;

namespace Launcher
{
    class StudentInfoPage : wPage
    {
        public StudentInfoPage() : base(null)
        {
            Project prj = Project.GetInstance();
            pnlInfo = new StudentInfoPanel();
            pnlImport = new StudentImportPanel();
            pnlImport.ScannerObscured += _goAskPage;
            pnlImport.ScannerCompleted += _goListPage;
            pnlImport.ScannerCancelled += _goImport;

            if (prj.StudentsCount > 0)
            {
                this.UI = pnlInfo;
                pnlInfo.Init();
            }
            else
            {
                this.UI = pnlImport;
            }
        }

        StudentImportPanel pnlImport;
        StudentInfoPanel pnlInfo;
        AskInfoPanel pnlAsk;

        void _goImport(object sender, EventArgs e)
        {
            App.Current.Dispatcher.Invoke(new Action(() =>
            {
                this.UI = pnlImport;
                
            }));
        }

        void _goListPage(object sender, EventArgs e)
        {
            App.Current.Dispatcher.Invoke(new Action(() =>
            {
                if (null != pnlInfo) pnlInfo = new StudentInfoPanel();
                this.UI = pnlInfo;
                pnlInfo.Init();
            }));
        }
        void _goAskPage(object sender, EventArgs e)
        {
            App.Current.Dispatcher.Invoke(new Action(() =>
            {
                this.UI = pnlAsk = (AskInfoPanel)sender;
            }));
        }


    }
}
