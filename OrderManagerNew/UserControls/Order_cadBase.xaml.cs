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

        /// <summary>
        /// EZCAD設計階段
        /// </summary>
        enum EZCADStep
        {
            DDS_ZAXIS = 0x00000001,
            DDS_MARGIN = 0x00000002,
            DDS_INSERTION = 0x00000004,
            DDS_INNER = 0x00000008,
            DDS_PRECROWN = 0x00000010,
            DDS_CROWN = 0x00000020,
            DDS_COPING = 0x00000040,
            DDS_CONNECTOR = 0x00000080,
            DDS_FINAL = 0x00000100,
            DDS_JIG_POSITION = 0x00000200,
            DDS_ABUTMENT = 0x00000400,
            DDS_CURBACK = 0x00000800,
            DDS_TEMPCROWN = 0x00001000,
            DDS_EMODEL_MARGIN = 0x00002000,
            DDS_CUTBACK = 0x00004000,
            DDS_CUSTOM_CROWN_DATABASE = 0x00008000,
            DDS_ABUTMENT_DEFORM = 0x00010000,
            DDS_ABUTMENT_CUTBACK = 0x00020000,
            DDS_ABUTMENT_SCREW = 0x00040000,
        };

        public class CadInformation
        {
            public string OrderID { get; set; }
            public int DesignStep { get; set; }
            public string PatientName { get; set; }
            public DateTime CreateDate { get; set; }
            public string CaseDirectoryPath { get; set; }

            public string Client { get; set; }
            public string Technician { get; set; }
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

            if ((cadInfo.DesignStep & (int)EZCADStep.DDS_ZAXIS) == 1)
                label_designStep.Content = OrderManagerNew.TranslationSource.Instance["DDS_ZAXIS"];

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
