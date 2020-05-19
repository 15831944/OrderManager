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
    /// Order_cadBase.xaml 的互動邏輯
    /// </summary>
    public partial class Order_cadBase : UserControl
    {
        CadInformation cadInfo;

        public class CadInformation
        {
            public string OrderID { get; set; }
            public int DesignStep { get; set; }
            public string PatientName { get; set; }
            public DateTime CreateDate { get; set; }
            public string CaseDirectoryPath { get; set; }

            public string Client { get; set; }
            public string Technician { get; set; }
            public string Patient { get; set; }
            public string Note { get; set; }
            public Version CADversion { get; set; }

            public CadInformation()
            {
                OrderID = "";
                DesignStep = -1;
                PatientName = "";
                CreateDate = new DateTime();
                CaseDirectoryPath = "";
            }
        }

        public Order_cadBase()
        {
            InitializeComponent();
        }

        public void SetCaseInfo(CadInformation Import)
        {
            cadInfo = Import;
            label_orderID.Content = cadInfo.OrderID;
            label_designStep.Content = cadInfo.DesignStep.ToString();
            label_patientName.Content = cadInfo.PatientName;
            label_createDate.Content = cadInfo.CreateDate.ToLongDateString();
        }

        private void Click_FolderOpen(object sender, RoutedEventArgs e)
        {
            if(cadInfo != null && System.IO.Directory.Exists(cadInfo.CaseDirectoryPath) == true)
            {
                OrderManagerFunctions omFunc = new OrderManagerFunctions();
                omFunc.RunCommandLine(Properties.Settings.Default.systemDisk + @"Windows\explorer.exe", cadInfo.CaseDirectoryPath);
            }
        }
    }
}
