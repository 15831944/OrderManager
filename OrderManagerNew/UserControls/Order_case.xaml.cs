using System;
using System.Collections.Generic;
using System.IO;
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
            public string ImplantTiiPath { get; set; }
            public string GuideModelPath { get; set; }
            public string GuideCaseDir { get; set; }

            public ImplantCaseInformation()
            {
                OrderName = "";
                ImplantTiiPath = "";
                GuideModelPath = "";
                GuideCaseDir = "";
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
                            omFunc.RunCommandLine(Properties.Settings.Default.implant_exePath, "\"readdii\" \"" + implantcaseInfo.ImplantTiiPath + "\"");
                            break;
                        }
                    case "button_Guide":
                        {
                            string gmlFile = implantcaseInfo.GuideCaseDir + implantcaseInfo.OrderName + "-Guide.gml";
                            string lmgFile = implantcaseInfo.GuideCaseDir + implantcaseInfo.OrderName + ".lmg";

                            if (File.Exists(gmlFile) == true)//有gml就先讀
                                omFunc.RunCommandLine(Properties.Settings.Default.guide_exePath, "guiderpd \"" + gmlFile + "\"");
                            else if (File.Exists(lmgFile) == true)//沒有gml再讀lmg
                                omFunc.RunCommandLine(Properties.Settings.Default.guide_exePath, "guide \"" + lmgFile + "\"");
                            break;
                        }
                    case "button_GuideModelDir":
                        {
                            if(button_GuideModelDir.Opacity == 1)
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
            if (implantcaseInfo.GuideModelPath == "")
            {
                button_GuideModelDir.Opacity = 0.2;
                button_GuideModelDir.ToolTip = null;
            }
            else
            {
                button_GuideModelDir.Opacity = 1;
                button_GuideModelDir.ToolTip = OrderManagerNew.TranslationSource.Instance["GuideModel"];
            }
        }
    }
}
