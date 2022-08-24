using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using EDKv5;

namespace Launcher
{
    /// <summary>
    /// Interaction logic for StudentInfoPanel.xaml
    /// </summary>
    public partial class StudentInfoPanel : UserControl
    {
        public StudentInfoPanel()
        {
            InitializeComponent();
        }


        public void Init()
        {
            Project prj = Project.GetInstance();
            Dictionary<Class, List<Student>> clsStus = prj.ClassStudents;
            tvStundets.ItemsSource = clsStus;
        }  

    }
}
