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
using Order_orthoSmallcase = OrderManagerNew.UserControls.Order_orthoSmallcase;

namespace OrderManagerNew.UserControls
{
    /// <summary>
    /// Order_orthoBase.xaml 的互動邏輯
    /// </summary>
    public partial class Order_orthoBase : UserControl
    {
        private OrthoOuterInformation orthoInfo;
        private bool UnfoldsmallCase = false;   //smallCase目前是否為攤開狀態

        public class OrthoOuterInformation
        {
            public string CaseDirectoryPath { get; set; }
            public string PatientID { get; set; }
            public string PatientName { get; set; }
            public bool PatientSex { get; set; }    //True為男性
            public DateTime PatientBirth { get; set; }
            public string PatientAddress { get; set; }
            public string DentistName { get; set; }
            public string ClinicName { get; set; }
            public DateTime CreateTime { get; set; }
            public DateTime ModifyTime { get; set; }
            public List<Order_orthoSmallcase> List_smallcase { get; set; }

            public OrthoOuterInformation()
            {
                CaseDirectoryPath = "";
                PatientID = "";
                PatientName = "";
                PatientSex = false;
                PatientAddress = "";
                DentistName = "";
                ClinicName = "";
                List_smallcase = new List<Order_orthoSmallcase>();
                PatientBirth = new DateTime();
                CreateTime = new DateTime();
                ModifyTime = new DateTime();
            }
        }

        public Order_orthoBase()
        {
            InitializeComponent();
            label_orderID.Content = "";
            label_patientName.Content = "";
            label_designStep.Content = "";
            label_createDate.Content = "";
            UnfoldsmallCase = false;
        }

        public void SetCaseInfo(OrthoOuterInformation Import)
        {
            orthoInfo = Import;
            string showID = orthoInfo.PatientID + "＿" + orthoInfo.PatientName;
            label_orderID.Content = showID;
            label_patientName.Content = orthoInfo.PatientName;
            if(orthoInfo.PatientBirth != new DateTime())
            {
                int patientAge = DateTime.Today.Year - orthoInfo.PatientBirth.Year;
                label_patientName.Content += "(" + patientAge.ToString() + ")";
                label_patientName.ToolTip = OrderManagerNew.TranslationSource.Instance["PatientNameWithAge"];
            }
            label_createDate.Content = orthoInfo.CreateTime.ToLongDateString();
        }

        private void Click_OpenDir(object sender, RoutedEventArgs e)
        {
            OrderManagerFunctions omFunc = new OrderManagerFunctions();
            omFunc.RunCommandLine(Properties.Settings.Default.systemDisk + @"Windows\explorer.exe", "\"" + System.IO.Path.GetDirectoryName(orthoInfo.CaseDirectoryPath) + "\"");
        }

        private void PMDown_StackPanelMain(object sender, MouseButtonEventArgs e)
        {
            if(e.Source is Button)
            {
                Click_OpenDir(null, null);
            }
            else
            {
                if(UnfoldsmallCase == false)
                {
                    //未被攤開，執行攤開
                    if (orthoInfo.List_smallcase.Count > 0)
                    {
                        foreach (Order_orthoSmallcase OrthoCase in orthoInfo.List_smallcase)
                        {
                            stackpanel_Ortho.Children.Add(OrthoCase);
                        }
                        UnfoldsmallCase = true;
                    }
                }
                else
                {
                    //已被攤開，收回
                    if (orthoInfo.List_smallcase.Count > 0)
                    {
                        stackpanel_Ortho.Children.RemoveRange(1, (stackpanel_Ortho.Children.Count - 1));
                        UnfoldsmallCase = false;
                    }
                }
            }
        }
    }
}
