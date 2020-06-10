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
using OrthoSmallCaseInformation = OrderManagerNew.UserControls.Order_orthoSmallcase.OrthoSmallCaseInformation;

namespace OrderManagerNew.UserControls
{
    /// <summary>
    /// Detail_ortho.xaml 的互動邏輯
    /// </summary>
    public partial class Detail_ortho : UserControl
    {
        private OrthoSmallCaseInformation orthoInfo;

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

        public Detail_ortho()
        {
            InitializeComponent();
            orthoInfo = new OrthoSmallCaseInformation();
        }

        /// <summary>
        /// 設定Detail顯示資訊
        /// </summary>
        /// <param name="Import">要匯入的OrthoSmallInformation</param>
        public void SetDetailInfo(OrthoSmallCaseInformation Import)
        {
            orthoInfo = Import;
            textbox_ProductType.Text = orthoInfo.ProductTypeString;
            textbox_Workflow.Text = OrderManagerNew.TranslationSource.Instance[Enum.GetName(typeof(OrthoWorkFlow), orthoInfo.WorkflowStep)];
            textbox_Name.Text = orthoInfo.Name;
            textbox_OrderID.Text = orthoInfo.OrderID;
            textbox_Gender.Text = orthoInfo.Gender;
            textbox_Age.Text = orthoInfo.Age;
            textbox_CreateDate.Text = orthoInfo.CreateDate;
            textbox_ModifyDate.Text = orthoInfo.ModifyTime.ToLongTimeString();
            textbox_Clinic.Text = orthoInfo.Clinic;
            textbox_Dentist.Text = orthoInfo.Dentist;
            textbox_Note.Text = orthoInfo.Describe;
        }

        private void Click_systemButton(object sender, RoutedEventArgs e)
        {
            if(sender is Button)
            {
                switch(((Button)sender).Name)
                {
                    case "button_loadProj":
                        {
                            OrderManagerFunctions omFunc = new OrderManagerFunctions();
                            omFunc.RunCommandLine(Properties.Settings.Default.ortho_exePath, "-rp \"-" + orthoInfo.SmallCaseXmlPath + "\"");
                            break;
                        }
                    case "button_openDir":
                        {
                            OrderManagerFunctions omFunc = new OrderManagerFunctions();
                            omFunc.RunCommandLine(Properties.OrderManagerProps.Default.systemDisk + @"Windows\explorer.exe", "\"" + System.IO.Path.GetDirectoryName(orthoInfo.SmallCaseXmlPath) + @"\" + "\"");
                            break;
                        }
                }
            }
        }
    }
}
