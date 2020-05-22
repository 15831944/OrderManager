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
            public string OrderName { get; set; }
            public string GuideModelPath { get; set; }
            public string CaseXmlPath { get; set; }

            public ImplantCaseInformation()
            {
                OrderName = "";
                GuideModelPath = "";
                CaseXmlPath = "";
            }
        }

        public Order_case()
        {
            InitializeComponent();
        }

        private void Click_ButtonEvent(object sender, RoutedEventArgs e)
        {
            if(button_Implant.Opacity != 0)
            {
                //ImplantCase
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
            else
            {
                //OrthoCase
            }
        }

        public void SetImplantCaseInfo(ImplantCaseInformation Import)
        {
            implantcaseInfo = Import;
            label_ProjectName.Content = implantcaseInfo.OrderName;
        }
    }
}
