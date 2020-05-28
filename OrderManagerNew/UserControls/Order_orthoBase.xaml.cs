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

        public class OrthoOuterInformation
        {
            public string PatientID { get; set; }
            public string PatientName { get; set; }
            public string PatientPhone { get; set; }
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
                PatientID = "";
                PatientName = "";
                PatientPhone = "";
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
        }

        public void SetCaseInfo(OrthoOuterInformation Import)
        {
            orthoInfo = Import;
            label_orderID.Content = orthoInfo.PatientID;
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
            if(orthoInfo.List_smallcase.Count > 0)
            {
                foreach (Order_orthoSmallcase OrthoCase in orthoInfo.List_smallcase)
                    stackpanel_Ortho.Children.Add(OrthoCase);
            }
        }
    }
}
