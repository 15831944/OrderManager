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
using Order_ImplantSmallcase = OrderManagerNew.UserControls.Order_ImplantSmallcase;

namespace OrderManagerNew.UserControls
{
    /// <summary>
    /// Order_implantBase.xaml 的互動邏輯
    /// </summary>
    public partial class Order_implantBase : UserControl
    {
        private ImplantOuterInformation implantInfo;
        private bool UnfoldsmallCase = false;   //smallCase目前是否為攤開狀態

        /// <summary>
        /// ImplantPlanning專案資訊
        /// </summary>
        public class ImplantOuterInformation
        {
            public string OrderNumber { get; set; }
            public string PatientName { get; set; }
            public DateTime PatientBirth { get; set; }
            public DateTime CreateDate { get; set; }
            public DateTime ModifyDate { get; set; }
            public string CaseDirectoryPath { get; set; }

            public string Clinic { get; set; }
            public string Note { get; set; }
            public string CBCTPath { get; set; }
            public string JawPath { get; set; }
            public string JawTrayPath { get; set; }
            public string DenturePath { get; set; }
            public List<Order_ImplantSmallcase> List_smallcase { get; set; }

            public ImplantOuterInformation()
            {
                OrderNumber = "";
                PatientName = "";
                PatientBirth = new DateTime();
                CreateDate = new DateTime();
                ModifyDate = new DateTime();
                CaseDirectoryPath = "";

                Clinic = "";
                Note = "";
                CBCTPath = "";
                JawPath = "";
                JawTrayPath = "";
                DenturePath = "";
                List_smallcase = new List<Order_ImplantSmallcase>();
            }
        }

        public Order_implantBase()
        {
            InitializeComponent();
            label_orderID.Content = "";
            label_patientName.Content = "";
            label_designStep.Content = "";
            label_createDate.Content = "";
            UnfoldsmallCase = false;
        }

        public void SetCaseInfo(ImplantOuterInformation Import)
        {
            implantInfo = Import;
            label_orderID.Content = implantInfo.OrderNumber;
            label_patientName.Content = implantInfo.PatientName;
            if(implantInfo.PatientBirth != new DateTime())
            {
                int patientAge = DateTime.Today.Year - implantInfo.PatientBirth.Year;
                label_patientName.Content += "(" + patientAge.ToString() + ")";
                label_patientName.ToolTip = OrderManagerNew.TranslationSource.Instance["PatientNameWithAge"];
            }   
            label_createDate.Content = implantInfo.CreateDate.ToLongDateString();
        }

        private void Click_OpenDir(object sender, RoutedEventArgs e)
        {
            OrderManagerFunctions omFunc = new OrderManagerFunctions();
            omFunc.RunCommandLine(Properties.Settings.Default.systemDisk + @"Windows\explorer.exe", "\"" + System.IO.Path.GetDirectoryName(implantInfo.CaseDirectoryPath) + "\"");
        }

        private void PMDown_StackPanelMain(object sender, MouseButtonEventArgs e)
        {
            if (e.Source is Button)
            {
                Click_OpenDir(null, null);
            }
            else
            {
                if (UnfoldsmallCase == false)
                {
                    //未被攤開，執行攤開
                    if (implantInfo.List_smallcase.Count > 0)
                    {
                        foreach (Order_ImplantSmallcase ImplantCase in implantInfo.List_smallcase)
                        {
                            stackpanel_Implant.Children.Add(ImplantCase);
                        }
                        UnfoldsmallCase = true;
                    }
                }
                else
                {
                    //已被攤開，收回
                    if (implantInfo.List_smallcase.Count > 0)
                    {
                        stackpanel_Implant.Children.RemoveRange(1, (stackpanel_Implant.Children.Count - 1));
                        UnfoldsmallCase = false;
                    }
                }
            }
        }
    }
}
