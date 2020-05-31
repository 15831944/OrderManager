using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Xml.Linq;
using Order_orthoSmallcase = OrderManagerNew.UserControls.Order_orthoSmallcase;

namespace OrderManagerNew.UserControls
{
    /// <summary>
    /// Order_orthoBase.xaml 的互動邏輯
    /// </summary>
    public partial class Order_orthoBase : UserControl
    {
        LogRecorder log;
        private OrthoOuterInformation orthoInfo;
        private bool UnfoldsmallCase = false;   //smallCase目前是否為攤開狀態

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
            public DateTime CreateTime { get; set; }
            public DateTime ModifyTime { get; set; }
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
                CreateTime = new DateTime();
                ModifyTime = new DateTime();
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
            UnfoldsmallCase = false;
        }

        public void SetCaseInfo(OrthoOuterInformation Import)
        {
            orthoInfo = Import;
            string showID = orthoInfo.PatientID + "＿" + orthoInfo.PatientName;
            label_orderID.Content = showID;
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
            OrderManagerFunctions omFunc = new OrderManagerFunctions();
            omFunc.RunCommandLine(Properties.Settings.Default.systemDisk + @"Windows\explorer.exe", "\"" + System.IO.Path.GetDirectoryName(orthoInfo.CaseDirectoryPath) + "\"");
        }

        private void PMDown_StackPanelMain(object sender, MouseButtonEventArgs e)
        {
            if(e.Source is Button)
            {
                Click_OpenDir(null, null);
            }
            else
            {
                if(UnfoldsmallCase == false)
                {
                    //未被攤開，執行攤開
                    if (orthoInfo.List_smallcase.Count > 0)
                    {
                        foreach (Order_orthoSmallcase OrthoCase in orthoInfo.List_smallcase)
                        {
                            stackpanel_Ortho.Children.Add(OrthoCase);
                        }
                        UnfoldsmallCase = true;
                    }
                    else
                    {
                        LoadSmallCase();

                        if (orthoInfo.List_smallcase.Count > 0)
                        {
                            foreach (Order_orthoSmallcase OrthoCase in orthoInfo.List_smallcase)
                            {
                                stackpanel_Ortho.Children.Add(OrthoCase);
                            }
                            UnfoldsmallCase = true;
                        }
                    } 
                }
                else
                {
                    //已被攤開，收回
                    if (orthoInfo.List_smallcase.Count > 0)
                    {
                        stackpanel_Ortho.Children.RemoveRange(1, (stackpanel_Ortho.Children.Count - 1));
                        UnfoldsmallCase = false;
                    }
                }
            }
        }

        //讀取SmallCase資訊
        private void LoadSmallCase()
        {
            orthoInfo.List_smallcase = new List<UserControls.Order_orthoSmallcase>();
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

                        Order_orthoSmallcase.OrthoSmallCaseInformation tmpOrthosmallInfo = new Order_orthoSmallcase.OrthoSmallCaseInformation
                        {
                            //tmpOrthosmallInfo.SoftwareVer = new Version(orthodata.File_Version);
                            WorkflowStep = Convert.ToInt16(orthodata.workflowstep),
                            CreateTime = orthodata.patientInformation.m_CreateTime,
                            Describe = orthodata.patientInformation.m_Discribe
                        };

                        UserControls.Order_orthoSmallcase tmporthoSmallcase = new UserControls.Order_orthoSmallcase();
                        tmporthoSmallcase.SetOrthoSmallCaseInfo(tmpOrthosmallInfo);
                        orthoInfo.List_smallcase.Add(tmporthoSmallcase);
                    }
                    catch (Exception ex)
                    {
                        log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "ProjectHandle.cs LoadXml(Ortho smallcase) Exception2", ex.Message);
                        continue;
                    }
                }
            }
        }
    }
}
