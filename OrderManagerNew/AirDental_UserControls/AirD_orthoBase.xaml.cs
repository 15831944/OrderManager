using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace OrderManagerNew.AirDental_UserControls
{
    /// <summary>
    /// AirD_orthoBase.xaml 的互動邏輯
    /// </summary>
    public partial class AirD_orthoBase : UserControl
    {
        //委派到MainWindow.xaml.cs裡面的SnackBarShow(string)
        public delegate void AirD_orthoBaseHandleEventHandler_snackbar(string message);
        public event AirD_orthoBaseHandleEventHandler_snackbar ProjectHandler_snackbarShow;

        //委派到MainWindow.xaml.cs裡面CaseHandler_Ortho_showSingleProject()
        public delegate void AirD_orthoBaseEventHandler(int projectIndex);
        public event AirD_orthoBaseEventHandler SetAirDentalProjectShow;
        //委派到MainWindow.xaml.cs裡面的CaseHandler_Ortho_showDetail()
        public delegate void AirD_orthoBaseEventHandler2(int BaseCaseIndex, int SmallCaseIndex);
        public event AirD_orthoBaseEventHandler2 SetSmallOrderDetailShow;

        public Dll_Airdental.Main orthoBase_AirDental;
        public List<Dll_Airdental.Main._orthoOrder> Orderlist_Ortho;
        public int orthoProjectIndex;
        AirD_orthoProject orthoProjectInfo;
        bool IsFocusCase = false;   //smallCase目前是否為攤開狀態
        
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
            public List<AirD_orthoSmallOrder> List_orthoOrder { get; set; }

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
            orthoProjectInfo.List_orthoOrder = new List<AirD_orthoSmallOrder>();
            if (Properties.Settings.Default.showCloudOrderNumbers < 1)
                Properties.Settings.Default.showCloudOrderNumbers = 5;

            int totalCount = -1;
            if (Orderlist_Ortho.Count < Properties.Settings.Default.showCloudOrderNumbers)
                totalCount = Orderlist_Ortho.Count;
            else
                totalCount = Properties.Settings.Default.showCloudOrderNumbers;

            for(int i=0; i<totalCount; i++)
            {
                AirDental_UserControls.AirD_orthoSmallOrder TmpOrthoSmallOrder = new AirD_orthoSmallOrder();
                TmpOrthoSmallOrder.OrderHandler_snackbarShow += new AirD_orthoSmallOrder.orthoOrder2EventHandler_snackbar(ProjectHandler_snackbarShow);
                TmpOrthoSmallOrder.SetOrderCaseShow += new AirD_orthoSmallOrder.orthoOrderEventHandler(SmallOrderHandler);
                TmpOrthoSmallOrder.SetOrderInfo(Orderlist_Ortho[i], i);
                orthoProjectInfo.List_orthoOrder.Add(TmpOrthoSmallOrder);
            }
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
            if (orthoProjectInfo.StageKey.IndexOf("ortho_") == 0)
                orthoProjectInfo.StageKey = orthoProjectInfo.StageKey.Remove(0, 6);
            label_designStep.Content = TranslationSource.Instance[orthoProjectInfo.Group] + " " + TranslationSource.Instance[orthoProjectInfo.ActionKey] + TranslationSource.Instance[orthoProjectInfo.StageKey];
            label_patientName.Content = orthoProjectInfo.Patient;
            label_modifyDate.Content = orthoProjectInfo.ModifyDate.ToString(System.Globalization.CultureInfo.CreateSpecificCulture("en-US"));
            label_designStep.ToolTip = label_designStep.Content;
        }

        /// <summary>
        /// 使用者點擊SmallCase時事件
        /// </summary>
        /// <param name="SmallcaseIndex">SmallCase的Index</param>
        private void SmallOrderHandler(int SmallorderIndex)
        {
            SetSmallOrderDetailShow(orthoProjectIndex, SmallorderIndex);  //MainWindow顯示Small Case Detail
            for (int i = 0; i < orthoProjectInfo.List_orthoOrder.Count; i++)
            {
                if (i == SmallorderIndex)
                    continue;

                orthoProjectInfo.List_orthoOrder[i].SetCaseFocusStatus(false);
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
                        if (orthoProjectInfo.List_orthoOrder != null)
                        {
                            foreach (AirD_orthoSmallOrder OrthoOrder in orthoProjectInfo.List_orthoOrder)
                            {
                                stackpanel_Ortho.Children.Add(OrthoOrder);
                            }
                        }
                        else
                        {
                            //第一次攤開
                            Mouse.OverrideCursor = Cursors.Wait;
                            GetOrthoOrder();
                            Mouse.OverrideCursor = Cursors.Arrow;
                            if (orthoProjectInfo.List_orthoOrder != null)
                            {
                                foreach (AirD_orthoSmallOrder OrthoOrder in orthoProjectInfo.List_orthoOrder)
                                {
                                    stackpanel_Ortho.Children.Add(OrthoOrder);
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
                        if (orthoProjectInfo.List_orthoOrder != null)
                        {
                            for (int i = 1; i < stackpanel_Ortho.Children.Count; i++)
                            {
                                ((AirD_orthoSmallOrder)stackpanel_Ortho.Children[i]).SetCaseFocusStatus(false);
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
            string WebUrl = Properties.OrderManagerProps.Default.AirDentalAPI.Remove(Properties.OrderManagerProps.Default.AirDentalAPI.LastIndexOf("api/")) + @"project/ortho/" + orthoProjectInfo.Pid;
            OrderManagerFunctions omFunc = new OrderManagerFunctions();
            omFunc.RunCommandLine(WebUrl, "");
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
            e.Handled = true;
        }
    }
}
