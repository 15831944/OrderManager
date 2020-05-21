using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OrderManagerNew.UserControls
{
    /// <summary>
    /// Order_case.xaml 的互動邏輯
    /// </summary>
    public partial class Order_case : UserControl
    {
        private ImplantCaseInformation implantcaseInfo;

        public class ImplantCaseInformation
        {
            public string OrderID { get; set; }
            public string GuideModelPath { get; set; }

            public ImplantCaseInformation()
            {
                OrderID = "";
                GuideModelPath = "";
            }
        }

        public Order_case()
        {
            InitializeComponent();
        }

        private void Click_ButtonEvent(object sender, RoutedEventArgs e)
        {
            OrderManagerFunctions omFunc = new OrderManagerFunctions();
            switch (((Button)sender).Name)
            {
                case "button_Implant":
                    {
                        omFunc.RunCommandLine(Properties.Settings.Default.implant_exePath, "");
                        break;
                    }
                case "button_Guide":
                    {
                        omFunc.RunCommandLine(Properties.Settings.Default.guide_exePath, "");
                        break;
                    }
                case "button_GuidePath":
                    {
                        omFunc.RunCommandLine(Properties.Settings.Default.systemDisk + @"Windows\explorer.exe", "\"" + implantcaseInfo.GuideModelPath + "\"");
                        break;
                    }
            }
        }
    }
}
