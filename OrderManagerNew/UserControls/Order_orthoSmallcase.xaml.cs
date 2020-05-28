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

        public enum OrthoWorkFlow : int
        {
            Develope = -2,
            General = -1,
            Model_Position,//模型擺放
            Model_Edit,//模型編修
            Teeth_Define,//齒位定義
            Crown_Margin_Adjust,//調整牙冠
            Crown_Postprocess,//牙冠後修
            IB_DefineAxis,//定義齒軸坐標系(未分離)
            Finish,//牙冠分離結束 這個當作分隔線 以上或以下步驟的順序不可亂動
            SetupManage,//排牙療程總管理頁面
            Printing,//輸出模型
            CompareScanModel,//與掃描模型比較
            Measurement,//與其他排牙療程比較
            Treatment_Planning,//Ezortho排牙規劃
            Shell_Creation,
            Bracket_Treatment
        }

        public class OrthoSmallCaseInformation
        {
            public string SmallCaseXmlPath { get; set; }
            //public Version SoftwareVer { get; set; }
            public int WorkflowStep { get; set; }
            public string CreateTime { get; set; }
            public string Describe { get; set; }

            public OrthoSmallCaseInformation()
            {
                SmallCaseXmlPath = "";
                //SoftwareVer = new Version();
                WorkflowStep = -1;
                CreateTime = "";
                Describe = "";
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

            label_ProjectName.Content = orthosmallcaseInfo.CreateTime;
        }
    }
}
