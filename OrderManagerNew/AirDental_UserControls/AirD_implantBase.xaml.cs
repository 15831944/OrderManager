﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace OrderManagerNew.AirDental_UserControls
{
    /// <summary>
    /// AirD_implantBase.xaml 的互動邏輯
    /// </summary>
    public partial class AirD_implantBase : UserControl
    {
        //委派到MainWindow.xaml.cs裡面的SnackBarShow(string)
        public delegate void AirD_implantBaseHandleEventHandler_snackbar(string message);
        public event AirD_implantBaseHandleEventHandler_snackbar ProjectHandler_snackbarShow;
        //委派到MainWindow.xaml.cs裡面CaseHandler_Ortho_showSingleProject()
        public delegate void AirD_implantBaseEventHandler(int projectIndex);
        public event AirD_implantBaseEventHandler SetAirDentalProjectShow;
        //委派到MainWindow.xaml.cs裡面的CaseHandler_Ortho_showDetail()
        public delegate void AirD_implantBaseEventHandler2(int BaseCaseIndex, int SmallCaseIndex);
        public event AirD_implantBaseEventHandler2 SetSmallOrderDetailShow;

        public Dll_Airdental.Main implantBase_AirDental;
        public List<Dll_Airdental.Main._implantOrder> Orderlist_Implant;
        public int implantProjectIndex;
        AirD_implantProject implantProjectInfo;
        bool IsFocusCase = false;   //smallCase目前是否為攤開狀態

        public class AirD_implantProject
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
            public List<AirD_implantSmallOrder> List_implantOrder { get; set; }

            public AirD_implantProject()
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
                List_implantOrder = null;
            }
        }

        public AirD_implantBase()
        {
            InitializeComponent();
        }

        private void GetImplantOrder()
        {
            Orderlist_Implant = new List<Dll_Airdental.Main._implantOrder>();
            System.Net.WebException Exception_implant = implantBase_AirDental.GetImplantOrder(ref Orderlist_Implant, implantProjectInfo.Pid);
            if (Exception_implant == null)
            {
                LoadImplantOrders();
            }
        }

        private void LoadImplantOrders()
        {
            implantProjectInfo.List_implantOrder = new List<AirD_implantSmallOrder>();
            if (Properties.Settings.Default.showCloudOrderNumbers < 1)
                Properties.Settings.Default.showCloudOrderNumbers = 5;

            int totalCount = -1;
            if (Orderlist_Implant.Count < Properties.Settings.Default.showCloudOrderNumbers)
                totalCount = Orderlist_Implant.Count;
            else
                totalCount = Properties.Settings.Default.showCloudOrderNumbers;

            for (int i = 0; i < totalCount; i++)
            {
                AirDental_UserControls.AirD_implantSmallOrder TmpImplantSmallOrder = new AirD_implantSmallOrder();
                TmpImplantSmallOrder.SetOrderCaseShow += new AirD_implantSmallOrder.implantOrderEventHandler(SmallOrderHandler);
                TmpImplantSmallOrder.OrderHandler_snackbarShow += new AirD_implantSmallOrder.implantOrder2EventHandler_snackbar(ProjectHandler_snackbarShow);
                TmpImplantSmallOrder.SetOrderInfo(Orderlist_Implant[i], i);
                implantProjectInfo.List_implantOrder.Add(TmpImplantSmallOrder);
            }
        }

        public void SetProjectInfo(Dll_Airdental.Main._implantProject Import, int Index)
        {
            implantProjectInfo = new AirD_implantProject
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
                PatientAvatar = Import._PatientAvatar,
            };
            implantProjectIndex = Index;

            label_orderID.Content = implantProjectInfo.SerialNumber;
            if (implantProjectInfo.StageKey.IndexOf("guide_") == 0)
                implantProjectInfo.StageKey = implantProjectInfo.StageKey.Remove(0, 6);
            label_designStep.Content = TranslationSource.Instance[implantProjectInfo.Group] + " " + TranslationSource.Instance[implantProjectInfo.ActionKey] + TranslationSource.Instance[implantProjectInfo.StageKey];
            label_patientName.Content = implantProjectInfo.Patient;
            label_modifyDate.Content = implantProjectInfo.ModifyDate.DateTime.ToString(System.Globalization.CultureInfo.CreateSpecificCulture("en-US"));
            label_designStep.ToolTip = label_designStep.Content;
        }

        /// <summary>
        /// 使用者點擊SmallCase時事件
        /// </summary>
        /// <param name="SmallcaseIndex">SmallCase的Index</param>
        private void SmallOrderHandler(int SmallorderIndex)
        {
            SetSmallOrderDetailShow(implantProjectIndex, SmallorderIndex);  //MainWindow顯示Small Case Detail
            for (int i = 0; i < implantProjectInfo.List_implantOrder.Count; i++)
            {
                if (i == SmallorderIndex)
                    continue;

                implantProjectInfo.List_implantOrder[i].SetCaseFocusStatus(false);
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
                        if (implantProjectInfo.List_implantOrder != null)
                        {
                            foreach (AirD_implantSmallOrder implantOrder in implantProjectInfo.List_implantOrder)
                            {
                                stackpanel_Ortho.Children.Add(implantOrder);
                            }
                        }
                        else
                        {
                            //第一次攤開
                            Mouse.OverrideCursor = Cursors.Wait;
                            GetImplantOrder();
                            Mouse.OverrideCursor = Cursors.Arrow;
                            if (implantProjectInfo.List_implantOrder != null)
                            {
                                foreach (AirD_implantSmallOrder implantOrder in implantProjectInfo.List_implantOrder)
                                {
                                    stackpanel_Ortho.Children.Add(implantOrder);
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
                        if (implantProjectInfo.List_implantOrder != null)
                        {
                            for (int i = 1; i < stackpanel_Ortho.Children.Count; i++)
                            {
                                ((AirD_implantSmallOrder)stackpanel_Ortho.Children[i]).SetCaseFocusStatus(false);
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
            string WebUrl = Properties.OrderManagerProps.Default.AirDentalAPI.Remove(Properties.OrderManagerProps.Default.AirDentalAPI.LastIndexOf("api/")) + @"project/implant/" + implantProjectInfo.Pid;
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
                SetAirDentalProjectShow(implantProjectIndex);
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
