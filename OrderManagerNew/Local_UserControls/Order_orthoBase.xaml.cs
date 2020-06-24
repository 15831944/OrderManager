using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;

namespace OrderManagerNew.Local_UserControls
{
    /// <summary>
    /// Order_orthoBase.xaml 的互動邏輯
    /// </summary>
    public partial class Order_orthoBase : UserControl
    {
        //委派到MainWindow.xaml.cs裡面CaseHandler_Ortho_showSingleProject()
        public delegate void orthoBaseEventHandler(int projectIndex);
        public event orthoBaseEventHandler SetBaseProjectShow;
        //委派到MainWindow.xaml.cs裡面的CaseHandler_Ortho_showDetail()
        public delegate void orthoBaseEventHandler2(int BaseCaseIndex, int SmallCaseIndex);
        public event orthoBaseEventHandler2 SetSmallProjectDetailShow;

        LogRecorder log;
        private OrthoOuterInformation orthoInfo;
        private bool IsFocusCase = false;   //smallCase目前是否為攤開狀態
        public int BaseCaseIndex;

        public class OrthoOuterInformation
        {
            public string CaseDirectoryPath { get; set; }
            public string PatientID { get; set; }
            public string PatientName { get; set; }
            public bool PatientSex { get; set; }    //True為男性
            public DateTime PatientBirth { get; set; }
            public string PatientAddress { get; set; }
            public string DentistName { get; set; }
            public string ClinicName { get; set; }
            public DateTime CreateDate { get; set; }
            public List<Order_orthoSmallcase> List_smallcase { get; set; }

            public OrthoOuterInformation()
            {
                CaseDirectoryPath = "";
                PatientID = "";
                PatientName = "";
                PatientSex = false;
                PatientAddress = "";
                DentistName = "";
                ClinicName = "";
                List_smallcase = new List<Order_orthoSmallcase>();
                PatientBirth = new DateTime();
                CreateDate = new DateTime();
            }
        }

        public Order_orthoBase()
        {
            InitializeComponent();
            log = new LogRecorder();
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
        /// <param name="Import">OrthoOuterInformation清單</param>
        /// <param name="Index">從0開始</param>
        public void SetCaseInfo(OrthoOuterInformation Import, int Index)
        {
            orthoInfo = Import;
            string showID = orthoInfo.PatientID + "＿" + orthoInfo.PatientName;
            label_orderID.Content = showID;
            label_patientName.Content = orthoInfo.PatientName;
            if(orthoInfo.PatientBirth != new DateTime())
            {
                int patientAge = DateTime.Today.Year - orthoInfo.PatientBirth.Year;
                label_patientName.Content += "(" + patientAge.ToString() + ")";
                label_patientName.ToolTip = TranslationSource.Instance["PatientNameWithAge"];
            }
            label_createDate.Content = orthoInfo.CreateDate.ToString(System.Globalization.CultureInfo.CreateSpecificCulture("en-US"));
            BaseCaseIndex = Index;
        }
        
        /// <summary>
        /// 讀取SmallCase資訊
        /// </summary>
        private void LoadSmallCase()
        {
            orthoInfo.List_smallcase = new List<Local_UserControls.Order_orthoSmallcase>();
            int itemIndex = 0;

            try
            {
                //蒐集OrthoSmallcase然後存進OuterCase
                DirectoryInfo dInfo2 = new DirectoryInfo(orthoInfo.CaseDirectoryPath);
                foreach (DirectoryInfo folder2 in dInfo2.GetDirectories())
                {
                    // 這層是C:\IntewareData\OrthoAnalysisV3\OrthoData\Test_1216\folder2\
                    string SmallXmlPath = folder2.FullName + @"\" + (orthoInfo.PatientID + "_" + orthoInfo.PatientName) + ".xml";
                    if (File.Exists(SmallXmlPath) == false)
                        continue;
                    else
                    {
                        XDocument xmlDoc;
                        FileInfo fInfo = new FileInfo(SmallXmlPath);//要取得檔案創建日期和修改日期

                        try
                        {
                            xmlDoc = XDocument.Load(SmallXmlPath);
                        }
                        catch (Exception ex)
                        {
                            log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "ProjectHandle.cs LoadXml(Ortho smallcase) Exception", ex.Message);
                            continue;
                        }

                        try
                        {
                            var orthodata = EZOrthoDataStructure.ProjectDataWrapper.ProjectDataWrapperDeserialize(SmallXmlPath);
                            int patientAge = DateTime.Today.Year - DateTime.Parse(orthodata.patientInformation.m_PatientBday).Year;

                            Order_orthoSmallcase.OrthoSmallCaseInformation tmpOrthosmallInfo = new Order_orthoSmallcase.OrthoSmallCaseInformation
                            {
                                //tmpOrthosmallInfo.SoftwareVer = new Version(orthodata.File_Version);
                                WorkflowStep = Convert.ToInt16(orthodata.workflowstep),
                                CreateDate = orthodata.patientInformation.m_CreateTime,
                                Describe = orthodata.patientInformation.m_Discribe,
                                ModifyTime = fInfo.LastWriteTime,
                                SmallCaseXmlPath = SmallXmlPath,

                                ProductTypeString = TranslationSource.Instance["ClearAligner"],
                                Name = orthodata.patientInformation.m_PatientName,
                                OrderID = orthodata.patientInformation.m_PatientID,
                                Gender = orthodata.patientInformation.m_PatientSex ? TranslationSource.Instance["Male"] : TranslationSource.Instance["Female"],
                                Age = patientAge.ToString(),
                                Clinic = orthodata.patientInformation.m_ClinicName,
                                Dentist = orthodata.patientInformation.m_DentistName
                            };

                            Order_orthoSmallcase tmporthoSmallcase = new Order_orthoSmallcase();
                            tmporthoSmallcase.SetsmallCaseShow += SmallCaseHandler;
                            tmporthoSmallcase.SetOrthoSmallCaseInfo(tmpOrthosmallInfo, itemIndex);
                            orthoInfo.List_smallcase.Add(tmporthoSmallcase);
                            itemIndex++;
                        }
                        catch (Exception ex)
                        {
                            log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "ProjectHandle.cs LoadXml(Ortho smallcase) Exception2", ex.Message);
                            continue;
                        }
                    }
                }
            }
            catch
            {

            }
        }

        /// <summary>
        /// 使用者點擊SmallCase時事件
        /// </summary>
        /// <param name="SmallcaseIndex">SmallCase的Index</param>
        private void SmallCaseHandler(int SmallcaseIndex)
        {
            SetSmallProjectDetailShow(BaseCaseIndex, SmallcaseIndex);  //MainWindow顯示Small Case Detail
            for (int i = 0; i < orthoInfo.List_smallcase.Count; i++)
            {
                if (i == SmallcaseIndex)
                    continue;

                orthoInfo.List_smallcase[i].SetCaseFocusStatus(false);
            }
        }

        private void Click_OpenDir(object sender, RoutedEventArgs e)
        {
            OrderManagerFunctions omFunc = new OrderManagerFunctions();
            omFunc.RunCommandLine(Properties.OrderManagerProps.Default.systemDisk + @"Windows\explorer.exe", "\"" + System.IO.Path.GetDirectoryName(orthoInfo.CaseDirectoryPath) + "\"");
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
                        if (orthoInfo.List_smallcase.Count > 0)
                        {
                            foreach (Order_orthoSmallcase OrthoCase in orthoInfo.List_smallcase)
                            {
                                stackpanel_Ortho.Children.Add(OrthoCase);
                            }
                        }
                        else
                        {
                            //第一次攤開
                            Mouse.OverrideCursor = Cursors.Wait;
                            LoadSmallCase();
                            Mouse.OverrideCursor = Cursors.Arrow;
                            if (orthoInfo.List_smallcase.Count > 0)
                            {
                                foreach (Order_orthoSmallcase OrthoCase in orthoInfo.List_smallcase)
                                {
                                    stackpanel_Ortho.Children.Add(OrthoCase);
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
                        if (orthoInfo.List_smallcase.Count > 0)
                        {
                            for(int i=1; i< stackpanel_Ortho.Children.Count; i++)
                            {
                                ((Order_orthoSmallcase)stackpanel_Ortho.Children[i]).SetCaseFocusStatus(false);
                            }

                            stackpanel_Ortho.Children.RemoveRange(1, (stackpanel_Ortho.Children.Count - 1));
                        }
                        IsFocusCase = false;
                        break;
                    }
            }
        }

        private void PMDown_StackPanelMain(object sender, MouseButtonEventArgs e)
        {
            if(e.Source is Button)
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
            e.Handled = true;
        }
    }
}
