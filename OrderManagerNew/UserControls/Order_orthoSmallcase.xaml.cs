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
    /// Order_orthoSmallcase.xaml 的互動邏輯
    /// </summary>
    public partial class Order_orthoSmallcase : UserControl
    {
        private OrthoSmallCaseInformation orthosmallcaseInfo;

        public class OrthoSmallCaseInformation
        {
            public string SmallCaseXmlPath { get; set; }
            public Version SoftwareVer { get; set; }
            public int WorkflowStep { get; set; }
            public DateTime CreateTime { get; set; }

            public OrthoSmallCaseInformation()
            {
                SmallCaseXmlPath = "";
                SoftwareVer = new Version();
                WorkflowStep = -1;
                CreateTime = new DateTime();
            }
        }

        public Order_orthoSmallcase()
        {
            InitializeComponent();
            label_ProjectName.Content = "";
            button_LoadOrthoProject.IsEnabled = false;
        }

        private void Click_ButtonEvent(object sender, RoutedEventArgs e)
        {
            OrderManagerFunctions omFunc = new OrderManagerFunctions();
            omFunc.RunCommandLine(Properties.Settings.Default.ortho_exePath, "-rp \"" + orthosmallcaseInfo.SmallCaseXmlPath + "\"");
        }

        public void SetOrthoSmallCaseInfo(OrthoSmallCaseInformation Import)
        {
            orthosmallcaseInfo = Import;

            if (File.Exists(orthosmallcaseInfo.SmallCaseXmlPath) == true && File.Exists(Properties.Settings.Default.ortho_exePath) == true)
                button_LoadOrthoProject.IsEnabled = true;
            else
                button_LoadOrthoProject.IsEnabled = false;

            label_ProjectName.Content = orthosmallcaseInfo.CreateTime.ToString("yyyy-mm-dd", System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
