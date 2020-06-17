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
    /// AirD_cadBasexaml.xaml 的互動邏輯
    /// </summary>
    public partial class AirD_cadBase : UserControl
    {
        //委派到MainWindow.xaml.cs裡面CaseHandler_Ortho_showSingleProject()
        public delegate void AirD_cadBaseEventHandler(int projectIndex);
        public event AirD_cadBaseEventHandler SetAirDentalProjectShow;
        //委派到MainWindow.xaml.cs裡面的CaseHandler_Ortho_showDetail()
        public delegate void AirD_cadBaseEventHandler2(int BaseCaseIndex, int SmallCaseIndex);
        public event AirD_cadBaseEventHandler2 SetSmallOrderDetailShow;

        public Dll_Airdental.Main cadBase_AirDental;
        public List<Dll_Airdental.Main._cadOrder> Orderlist_CAD;
        public int cadProjectIndex;
        AirD_cadProject cadProjectInfo;
        bool IsFocusCase = false;   //smallCase目前是否為攤開狀態

        public class AirD_cadProject
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
            public string Status { get; set; }
            public string Doctor { get; set; }
            public DateTimeOffset ModifyDate { get; set; }
            public string Instruction { get; set; }
            public string PatientAvatar { get; set; }
            public string ArchTreat { get; set; }
            public List<AirD_cadSmallOrder> List_cadOrder { get; set; }

            public AirD_cadProject()
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
                Status = "";
                Doctor = "";
                ModifyDate = new DateTimeOffset();
                Instruction = "";
                PatientAvatar = "";
                ArchTreat = "";
                List_cadOrder = null;
            }
        }

        public AirD_cadBase()
        {
            InitializeComponent();
        }

        private void GetCADOrder()
        {
            Orderlist_CAD = new List<Dll_Airdental.Main._cadOrder>();
            System.Net.WebException Exception_implant = cadBase_AirDental.GetCADOrder(ref Orderlist_CAD, cadProjectInfo.Pid);
            if (Exception_implant == null)
            {
                LoadCADOrders();
            }
        }

        private void LoadCADOrders()
        {
            cadProjectInfo.List_cadOrder = new List<AirD_cadSmallOrder>();

            if (Properties.Settings.Default.showCloudOrderNumbers < 1)
                Properties.Settings.Default.showCloudOrderNumbers = 5;

            int totalCount = -1;
            if (Orderlist_CAD.Count < Properties.Settings.Default.showCloudOrderNumbers)
                totalCount = Orderlist_CAD.Count;
            else
                totalCount = Properties.Settings.Default.showCloudOrderNumbers;

            for (int i = 0; i < totalCount; i++)
            {
                AirDental_UserControls.AirD_cadSmallOrder TmpCADSmallOrder = new AirD_cadSmallOrder();
                TmpCADSmallOrder.SetOrderCaseShow += new AirD_cadSmallOrder.cadOrderEventHandler(SmallOrderHandler);
                TmpCADSmallOrder.SetOrderInfo(Orderlist_CAD[i], i);
                cadProjectInfo.List_cadOrder.Add(TmpCADSmallOrder);
            }
        }

        public void SetProjectInfo(Dll_Airdental.Main._cadProject Import, int Index)
        {
            cadProjectInfo = new AirD_cadProject
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
                Status = Import._Status,
                Doctor = Import._Doctor,
                ModifyDate = Import._Date,
                Instruction = Import._Instruction,
                ArchTreat = Import._archTreat,
                PatientAvatar = Import._PatientAvatar,
            };
            cadProjectIndex = Index;

            label_orderID.Content = cadProjectInfo.SerialNumber;
            if (cadProjectInfo.StageKey.IndexOf("prostheses_") == 0)
                cadProjectInfo.StageKey = cadProjectInfo.StageKey.Remove(0, 11);
            label_designStep.Content = TranslationSource.Instance[cadProjectInfo.Group] + " " + TranslationSource.Instance[cadProjectInfo.ActionKey] + TranslationSource.Instance[cadProjectInfo.StageKey];
            label_patientName.Content = cadProjectInfo.Patient;
            label_modifyDate.Content = cadProjectInfo.ModifyDate.DateTime.ToLongDateString() + cadProjectInfo.ModifyDate.DateTime.ToLongTimeString();
            label_designStep.ToolTip = label_designStep.Content;
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
        /// 使用者點擊SmallCase時事件
        /// </summary>
        /// <param name="SmallcaseIndex">SmallCase的Index</param>
        private void SmallOrderHandler(int SmallorderIndex)
        {
            SetSmallOrderDetailShow(cadProjectIndex, SmallorderIndex);  //MainWindow顯示Small Case Detail
            for (int i = 0; i < cadProjectInfo.List_cadOrder.Count; i++)
            {
                if (i == SmallorderIndex)
                    continue;

                cadProjectInfo.List_cadOrder[i].SetCaseFocusStatus(false);
            }
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
                        if (cadProjectInfo.List_cadOrder != null)
                        {
                            foreach (AirD_cadSmallOrder cadOrder in cadProjectInfo.List_cadOrder)
                            {
                                stackpanel_Ortho.Children.Add(cadOrder);
                            }
                        }
                        else
                        {
                            //第一次攤開
                            Mouse.OverrideCursor = Cursors.Wait;
                            GetCADOrder();
                            Mouse.OverrideCursor = Cursors.Arrow;
                            if (cadProjectInfo.List_cadOrder != null)
                            {
                                foreach (AirD_cadSmallOrder cadOrder in cadProjectInfo.List_cadOrder)
                                {
                                    stackpanel_Ortho.Children.Add(cadOrder);
                                }
                            }
                        }
                        IsFocusCase = true;
                        break;
                    }
                case false:
                    {
                        background_orthoBase.Fill = Brushes.White;
                        //收回
                        if (cadProjectInfo.List_cadOrder != null)
                        {
                            for (int i = 1; i < stackpanel_Ortho.Children.Count; i++)
                            {
                                ((AirD_cadSmallOrder)stackpanel_Ortho.Children[i]).SetCaseFocusStatus(false);
                            }

                            stackpanel_Ortho.Children.RemoveRange(1, (stackpanel_Ortho.Children.Count - 1));
                        }
                        IsFocusCase = false;
                        break;
                    }
            }
        }


        private void Click_AirdentalWeb(object sender, RoutedEventArgs e)
        {

        }

        private void PMDown_StackPanelMain(object sender, MouseButtonEventArgs e)
        {
            if (e.Source is Button)
            {
                Click_AirdentalWeb(e.Source, e);
            }
            else
            {
                SetAirDentalProjectShow(cadProjectIndex);
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
    }
}
