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
    /// Order_implantBase.xaml 的互動邏輯
    /// </summary>
    public partial class Order_implantBase : UserControl
    {
        private ImplantInformation implantInfo;

        /// <summary>
        /// ImplantPlanning專案資訊
        /// </summary>
        public class ImplantInformation
        {
            public string OrderNumber { get; set; }
            public string PatientName { get; set; }
            public DateTime PatientBirth { get; set; }
            public DateTime CreateDate { get; set; }
            public DateTime ModifyDate { get; set; }
            public string CaseDirectoryPath { get; set; }

            public string Client { get; set; }
            public string CBCTPath { get; set; }
            public string JawPath { get; set; }
            public List<OrderManagerNew.UserControls.Order_case> List_case { get; set; }

            public ImplantInformation()
            {
                OrderNumber = "";
                PatientName = "";
                PatientBirth = new DateTime();
                CreateDate = new DateTime();
                ModifyDate = new DateTime();
                CaseDirectoryPath = "";
                List_case = new List<Order_case>();
            }
        }

        public Order_implantBase()
        {
            InitializeComponent();
            label_orderID.Content = "";
            label_patientName.Content = "";
            label_designStep.Content = "";
            label_createDate.Content = "";
        }

        public void SetCaseInfo(ImplantInformation Import)
        {
            implantInfo = Import;

        }
    }
}
