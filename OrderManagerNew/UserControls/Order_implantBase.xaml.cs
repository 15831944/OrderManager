﻿using System;
using System.Collections.Generic;
using System.IO;
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
using ImplantSmallCaseInformation = OrderManagerNew.UserControls.Order_ImplantSmallcase.ImplantSmallCaseInformation;
using Path = System.IO.Path;

namespace OrderManagerNew.UserControls
{
    /// <summary>
    /// Order_implantBase.xaml 的互動邏輯
    /// </summary>
    public partial class Order_implantBase : UserControl
    {
        //委派到MainWindow.xaml.cs裡面的CaseHandler_Implant_showSingleProject()
        public delegate void implantBaseEventHandler(int projectIndex);
        public event implantBaseEventHandler SetBaseProjectShow;
        //委派到MainWindow.xaml.cs裡面的CaseHandler_Ortho_showDetail()
        public delegate void implantBaseEventHandler2(int BaseCaseIndex, int SmallCaseIndex);
        public event implantBaseEventHandler2 SetSmallProjectDetailShow;

        public ImplantOuterInformation implantInfo;
        private bool IsFocusCase;
        public int BaseCaseIndex;
        /// <summary>
        /// ImplantPlanning專案資訊
        /// </summary>
        public class ImplantOuterInformation
        {
            public string OrderNumber { get; set; }
            public string PatientName { get; set; }
            public bool Gender { get; set; }
            public DateTime PatientBirth { get; set; }
            public DateTime CreateDate { get; set; }
            public DateTime ModifyDate { get; set; }
            public string CaseDirectoryPath { get; set; }

            public string SurgicalGoal { get; set; }
            public string SurgicalGuide { get; set; }
            public string SurgicalOption { get; set; }
            public string Surgicalkit { get; set; }

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
                Gender = false;
                PatientBirth = new DateTime();
                CreateDate = new DateTime();
                ModifyDate = new DateTime();
                CaseDirectoryPath = "";

                SurgicalGoal = "";
                SurgicalGuide = "";
                SurgicalOption = "";
                Surgicalkit = "";

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
            IsFocusCase = false;
            BaseCaseIndex = -1;
        }

        /// <summary>
        /// 設定顯示在UserControl上的內容
        /// </summary>
        /// <param name="Import">ImplantInformation清單</param>
        /// <param name="Index">從0開始</param>
        public void SetCaseInfo(ImplantOuterInformation Import, int Index)
        {
            implantInfo = Import;
            label_orderID.Content = implantInfo.OrderNumber;
            label_patientName.Content = implantInfo.PatientName;
            if(implantInfo.PatientBirth != new DateTime())
            {
                int patientAge = DateTime.Today.Year - implantInfo.PatientBirth.Year;
                label_patientName.Content += "(" + patientAge.ToString() + ")";
                label_patientName.ToolTip = TranslationSource.Instance["PatientNameWithAge"];
            }   
            label_createDate.Content = implantInfo.CreateDate.ToLongDateString();
            BaseCaseIndex = Index;
        }

        /// <summary>
        /// 讀取SmallCase資訊
        /// </summary>
        private void LoadSmallCase()
        {
            implantInfo.List_smallcase = new List<UserControls.Order_ImplantSmallcase>();
            int itemIndex = 0;
            foreach (string filename in Directory.GetFiles(implantInfo.CaseDirectoryPath))
            {
                // 這層是C:\IntewareData\Implant\2020130102946\
                //找有幾個tii檔就等於有幾個Implant要給Guide的檔
                if (Path.GetExtension(filename).ToLower() == ".tii")
                {
                    Order_ImplantSmallcase ImplantSmallCase = new Order_ImplantSmallcase();
                    ImplantSmallCase.SetsmallCaseShow += SmallCaseHandler;
                    //記錄內部專案資料夾名稱(就是OrderName)、Guide專案資料夾路徑和檢查是否有從Guide輸出的模型
                    ImplantSmallCaseInformation impInfo = new ImplantSmallCaseInformation
                    {
                        OrderName = Path.GetFileNameWithoutExtension(filename),
                        ImplantTiiPath = filename
                    };
                    impInfo.GuideCaseDir = implantInfo.CaseDirectoryPath + @"\" + impInfo.OrderName + @"\LinkStation\";
                    //TODO 這邊會有bug
                    string tmpGuideModelDir = implantInfo.CaseDirectoryPath + @"\" + impInfo.OrderName + @"\LinkStation\ManufacturingDir\";
                    if (Directory.Exists(tmpGuideModelDir) == true)
                    {
                        string[] guideModel = Directory.GetFiles(tmpGuideModelDir);
                        if (guideModel.Length > 0)
                            impInfo.GuideModelPath = guideModel[0];
                        else
                            impInfo.GuideModelPath = "";
                    }
                    else
                        impInfo.GuideModelPath = "";

                    ImplantSmallCase.SetImplantSmallCaseInfo(impInfo, itemIndex);
                    implantInfo.List_smallcase.Add(ImplantSmallCase);
                    itemIndex++;
                }
            }
        }

        private void SmallCaseHandler(int SmallcaseIndex)
        {
            SetSmallProjectDetailShow(BaseCaseIndex, SmallcaseIndex);  //MainWindow顯示Small Case Detail
            for (int i = 0; i < implantInfo.List_smallcase.Count; i++)
            {
                if (i == SmallcaseIndex)
                    continue;

                implantInfo.List_smallcase[i].SetCaseFocusStatus(false);
            }
        }

        private void Click_OpenDir(object sender, RoutedEventArgs e)
        {
            OrderManagerFunctions omFunc = new OrderManagerFunctions();
            omFunc.RunCommandLine(Properties.OrderManagerProps.Default.systemDisk + @"Windows\explorer.exe", "\"" + Path.GetDirectoryName(implantInfo.CaseDirectoryPath) + "\"");
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
                        background_implantBase.Fill = this.FindResource("background_FocusedCase") as SolidColorBrush;
                        //執行攤開
                        if (implantInfo.List_smallcase.Count > 0)
                        {
                            foreach (Order_ImplantSmallcase ImplantCase in implantInfo.List_smallcase)
                            {
                                stackpanel_Implant.Children.Add(ImplantCase);
                            }
                        }
                        else
                        {
                            Mouse.OverrideCursor = Cursors.Wait;
                            LoadSmallCase();
                            Mouse.OverrideCursor = Cursors.Arrow;
                            if (implantInfo.List_smallcase.Count > 0)
                            {
                                foreach (Order_ImplantSmallcase ImplantCase in implantInfo.List_smallcase)
                                {
                                    stackpanel_Implant.Children.Add(ImplantCase);
                                }
                            }
                        }
                        IsFocusCase = true;
                        break;
                    }
                case false:
                    {
                        background_implantBase.Fill = Brushes.White;
                        //收回
                        if (implantInfo.List_smallcase.Count > 0)
                        {
                            for (int i = 1; i < stackpanel_Implant.Children.Count; i++)
                            {
                                ((Order_ImplantSmallcase)stackpanel_Implant.Children[i]).SetCaseFocusStatus(false);
                            }

                            stackpanel_Implant.Children.RemoveRange(1, (stackpanel_Implant.Children.Count - 1));
                        }
                        IsFocusCase = false;
                        break;
                    }
            }
        }

        private void PMDown_StackPanelMain(object sender, MouseButtonEventArgs e)
        {
            if (e.Source is Button)
            {
                Click_OpenDir(e.Source, e);
            }
            else
            {
                SetBaseProjectShow(BaseCaseIndex);
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
