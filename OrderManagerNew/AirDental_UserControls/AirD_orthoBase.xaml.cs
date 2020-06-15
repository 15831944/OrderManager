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

namespace OrderManagerNew.AirDental_UserControls
{
    /// <summary>
    /// AirD_orthoBase.xaml 的互動邏輯
    /// </summary>
    public partial class AirD_orthoBase : UserControl
    {
        //委派到MainWindow.xaml.cs裡面CaseHandler_Ortho_showSingleProject()
        public delegate void AirD_orthoBaseEventHandler(int projectIndex);
        public event AirD_orthoBaseEventHandler SetAirDentalProjectShow;

        public Dll_Airdental.Main orthoBase_AirDental;
        public List<Dll_Airdental.Main._orthoOrder> Orderlist_Ortho;
        public int orthoProjectIndex;
        AirD_orthoProject orthoProjectInfo;
        bool IsFocusCase = false;   //smallCase目前是否為攤開狀態

        public class AirD_orthoOrder
        {
            //內部專案Oid
            public string Oid { get; set; }
            public string Group { get; set; }
            public bool IsAuthor { get; set; }
            public string Stage_String { get; set; }
            public string StageKey { get; set; }
            public string StageOrig { get; set; }
            public string Action { get; set; }
            public string ActionKey { get; set; }
            public string Instruction { get; set; }
            public DateTimeOffset CreateDate { get; set; }
            public string Viewerurl { get; set; }

            public AirD_orthoOrder()
            {
                Oid = "";
                Group = "";
                IsAuthor = false;
                Stage_String = "";
                StageKey = "";
                StageOrig = "";
                Action = "";
                ActionKey = "";
                Instruction = "";
                CreateDate = new DateTimeOffset();
                Viewerurl = "";
            }
        }
        public class AirD_orthoProject
        {
            //外部專案Pid
            public string Pid { get; set; }
            public string Group { get; set; }
            public string SerialNumber { get; set; }
            public string Patient { get; set; }
            public string Clinic { get; set; }
            public string Action_String { get; set; }
            public string ActionKey { get; set; }
            public string Stage_String { get; set; }
            public string StageKey { get; set; }
            public string Doctor { get; set; }
            public DateTimeOffset ModifyDate { get; set; }
            public string Instruction { get; set; }
            public string PatientAvatar { get; set; }
            public string TxTreatedArch { get; set; }
            public string ProductType { get; set; }
            public AirD_orthoOrder[] List_orthoOrder { get; set; }

            public AirD_orthoProject()
            {
                Pid = "";
                Group = "";
                SerialNumber = "";
                Patient = "";
                Clinic = "";
                Action_String = "";
                ActionKey = "";
                Stage_String = "";
                StageKey = "";
                Doctor = "";
                ModifyDate = new DateTimeOffset();
                Instruction = "";
                PatientAvatar = "";
                TxTreatedArch = "";
                ProductType = "";
                List_orthoOrder = null;
            }
        }
        
        public AirD_orthoBase()
        {
            InitializeComponent();
            label_orderID.Content = "";
            label_patientName.Content = "";
            label_designStep.Content = "";
            label_modifyDate.Content = "";
            orthoProjectIndex = -1;
            IsFocusCase = false;
        }

        private void GetOrthoOrder()
        {
            Orderlist_Ortho = new List<Dll_Airdental.Main._orthoOrder>();
            System.Net.WebException Exception_ortho = orthoBase_AirDental.GetOrthoOrder(ref Orderlist_Ortho, orthoProjectInfo.Pid);
            if(Exception_ortho == null)
            {
                LoadOrthoOrders();
            }
        }

        private void LoadOrthoOrders()
        {
            //TODO明天繼續
        }

        public void SetProjectInfo(Dll_Airdental.Main._orthoProject Import, int Index)
        {
            orthoProjectInfo = new AirD_orthoProject
            {
                Pid = Import._Key,
                Group = Import._Group,
                SerialNumber = Import._SerialNumber,
                Patient = Import._Patient,
                Clinic = Import._Clinic,
                Action_String = Import._Action,
                ActionKey = Import._ActionKey,
                Stage_String = Import._Stage,
                StageKey = Import._StageKey,
                Doctor = Import._Doctor,
                ModifyDate = Import._Date,
                Instruction = Import._Instruction,
                PatientAvatar = Import._PatientAvatar,
                TxTreatedArch = Import._TxTreatedArch,
                ProductType = Import._ProductType
            };
            orthoProjectIndex = Index;

            label_orderID.Content = orthoProjectInfo.SerialNumber;
            label_designStep.Content = orthoProjectInfo.Stage_String;
            label_patientName.Content = orthoProjectInfo.Patient;
            label_modifyDate.Content = orthoProjectInfo.ModifyDate.DateTime.ToLongDateString() + orthoProjectInfo.ModifyDate.DateTime.ToLongTimeString();

            /*try
            {
                Dll_Airdental.Main Airdental = new Dll_Airdental.Main();
                string PatientPhoto = Airdental.APIPortal + @"file/ortho/photo/" + orthoProjectInfo.PatientAvatar;
                image_patient.BeginInit();
                image_patient.Source = new BitmapImage(new Uri( PatientPhoto, UriKind.RelativeOrAbsolute));
                image_patient.EndInit();
            }
            catch
            {
                image_patient.BeginInit();
                image_patient.Source = new BitmapImage(new Uri(@"/ImageSource/FunctionTable/icon_O.png", UriKind.RelativeOrAbsolute));
                image_patient.EndInit();
            }*/
        }

        /// <summary>
        /// 設定Case的Focus狀態
        /// </summary>
        /// <param name="isFocused">是否要Focus</param>
        public void SetCaseFocusStatus(bool isFocused)
        {
            switch (isFocused)
            {
                case true:
                    {
                        background_orthoBase.Fill = this.FindResource("background_FocusedCase") as SolidColorBrush;
                        //執行攤開
                        if (orthoProjectInfo.List_orthoOrder != null)
                        {
                            /*foreach (Order_orthoSmallcase OrthoCase in orthoInfo.List_smallcase)
                            {
                                stackpanel_Ortho.Children.Add(OrthoCase);
                            }*/
                        }
                        else
                        {
                            //第一次攤開
                            Mouse.OverrideCursor = Cursors.Wait;
                            GetOrthoOrder();
                            Mouse.OverrideCursor = Cursors.Arrow;
                            /*if (orthoInfo.List_smallcase.Count > 0)
                            {
                                foreach (Order_orthoSmallcase OrthoCase in orthoInfo.List_smallcase)
                                {
                                    stackpanel_Ortho.Children.Add(OrthoCase);
                                }
                            }*/
                        }
                        IsFocusCase = true;
                        break;
                    }
                case false:
                    {
                        background_orthoBase.Fill = Brushes.White;
                        //收回
                        /*if (orthoProjectInfo.List_orthoOrder.Length > 0)
                        {
                            for (int i = 1; i < stackpanel_Ortho.Children.Count; i++)
                            {
                                ((Order_orthoSmallcase)stackpanel_Ortho.Children[i]).SetCaseFocusStatus(false);
                            }

                            stackpanel_Ortho.Children.RemoveRange(1, (stackpanel_Ortho.Children.Count - 1));
                        }*/
                        IsFocusCase = false;
                        break;
                    }
            }
        }

        private void PMDown_StackPanelMain(object sender, MouseButtonEventArgs e)
        {
            if (e.Source is Button)
            {
                Click_AirdentalWeb(e.Source, e);
            }
            else
            {
                SetAirDentalProjectShow(orthoProjectIndex);
                if (IsFocusCase == false)
                {
                    SetCaseFocusStatus(true);
                }
                else
                {
                    SetCaseFocusStatus(false);
                }
            }
        }

        private void Click_AirdentalWeb(object sender, RoutedEventArgs e)
        {

        }
    }
}
