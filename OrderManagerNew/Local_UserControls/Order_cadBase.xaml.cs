using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace OrderManagerNew.Local_UserControls
{
    /// <summary>
    /// Order_cadBase.xaml 的互動邏輯
    /// </summary>
    public partial class Order_cadBase : UserControl
    {
        //委派到MainWindow.xaml.cs裡面的CaseHandler_EZCAD_showSingleProject()
        public delegate void cadBaseEventHandler(int projectIndex);
        public event cadBaseEventHandler SetBaseProjectShow;

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
        
        private CadInformation cadInfo;
        private int ItemIndex;
        public bool IsFocusCase;

        /// <summary>
        /// EZCAD專案資訊
        /// </summary>
        public class CadInformation
        {
            public string OrderID { get; set; }
            public int DesignStep { get; set; }
            public string PatientName { get; set; }
            public DateTime CreateDate { get; set; }
            public DateTime ModifyDate { get; set; }
            public string CaseDirectoryPath { get; set; }
            public string CaseXmlPath { get; set; }

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
                ModifyDate = new DateTime();
                CaseDirectoryPath = "";
                CaseXmlPath = "";
            }
        }

        public Order_cadBase()
        {
            InitializeComponent();
            label_orderID.Content = "";
            label_patientName.Content = "";
            label_designStep.Content = "";
            label_createDate.Content = "";
            ItemIndex = -1;
            IsFocusCase = false;
        }

        /// <summary>
        /// 取得設計階段字串
        /// </summary>
        /// <param name="cadInfoDesignStep">cadInfo.DesignStep</param>
        /// <returns></returns>
        private string GetDesignStep(int cadInfoDesignStep)
        {
            string ShowStep = TranslationSource.Instance["CurrentStep"];

            if ((cadInfoDesignStep & (int)EZCADStep.DDS_FINAL) != 0)
                ShowStep += TranslationSource.Instance["DDS_FINAL"];
            else if ((cadInfoDesignStep & (int)EZCADStep.DDS_ABUTMENT_SCREW) != 0)
                ShowStep += TranslationSource.Instance["DDS_ABUTMENT_SCREW"];
            else if ((cadInfoDesignStep & (int)EZCADStep.DDS_CONNECTOR) != 0)
                ShowStep += TranslationSource.Instance["DDS_CONNECTOR"];
            else if ((cadInfoDesignStep & (int)EZCADStep.DDS_COPING) != 0)
                ShowStep += TranslationSource.Instance["DDS_COPING"];
            else if ((cadInfoDesignStep & (int)EZCADStep.DDS_CUTBACK) != 0)
                ShowStep += TranslationSource.Instance["DDS_CUTBACK"];
            else if ((cadInfoDesignStep & (int)EZCADStep.DDS_TEMPCROWN) != 0)
                ShowStep += TranslationSource.Instance["DDS_TEMPCROWN"];
            else if ((cadInfoDesignStep & (int)EZCADStep.DDS_CROWN) != 0)
                ShowStep += TranslationSource.Instance["DDS_CROWN"];
            else if ((cadInfoDesignStep & (int)EZCADStep.DDS_INNER) != 0)
                ShowStep += TranslationSource.Instance["DDS_INNER"];
            else if ((cadInfoDesignStep & (int)EZCADStep.DDS_ABUTMENT_DEFORM) != 0)
                ShowStep += TranslationSource.Instance["DDS_ABUTMENT_DEFORM"];
            else if ((cadInfoDesignStep & (int)EZCADStep.DDS_ABUTMENT_CUTBACK) != 0)
                ShowStep += TranslationSource.Instance["DDS_ABUTMENT_CUTBACK"];
            else if ((cadInfoDesignStep & (int)EZCADStep.DDS_ABUTMENT) != 0)
                ShowStep += TranslationSource.Instance["DDS_ABUTMENT"];
            else if ((cadInfoDesignStep & (int)EZCADStep.DDS_PRECROWN) != 0)
                ShowStep += TranslationSource.Instance["DDS_PRECROWN"];
            else if ((cadInfoDesignStep & (int)EZCADStep.DDS_INSERTION) != 0)
                ShowStep += TranslationSource.Instance["DDS_INSERTION"];
            else if ((cadInfoDesignStep & (int)EZCADStep.DDS_MARGIN) != 0)
                ShowStep += TranslationSource.Instance["DDS_MARGIN"];
            else if ((cadInfoDesignStep & (int)EZCADStep.DDS_JIG_POSITION) != 0)
                ShowStep += TranslationSource.Instance["DDS_JIG_POSITION"];
            else if ((cadInfoDesignStep & (int)EZCADStep.DDS_ZAXIS) != 0)
                ShowStep += TranslationSource.Instance["DDS_ZAXIS"];
            else
                ShowStep += TranslationSource.Instance["None"];

            return ShowStep;
        }

        /// <summary>
        /// 設定顯示在UserControl上的內容
        /// </summary>
        /// <param name="Import">CadInformation清單</param>
        /// <param name="Index">從0開始</param>
        public void SetCaseInfo(CadInformation Import, int Index)
        {
            cadInfo = Import;
            //label_orderID.Content = cadInfo.OrderID.Substring(cadInfo.OrderID.IndexOf('-') + 1);
            label_orderID.Content = cadInfo.OrderID;
            label_designStep.Content = GetDesignStep((int)cadInfo.DesignStep);
            label_patientName.Content = cadInfo.PatientName;
            label_createDate.Content = cadInfo.CreateDate.ToString(System.Globalization.CultureInfo.CreateSpecificCulture("en-US"));
            ItemIndex = Index;
        }
        
        /// <summary>
        /// 設定Case的Focus狀態
        /// </summary>
        /// <param name="isFocused">是否要Focus</param>
        public void SetCaseFocusStatus(bool isFocused)
        {
            switch(isFocused)
            {
                case true:
                    {
                        background_cadBase.Fill = this.FindResource("background_FocusedCase") as SolidColorBrush;
                        IsFocusCase = true;
                        break;
                    }
                case false:
                    {
                        background_cadBase.Fill = Brushes.White;
                        IsFocusCase = false;
                        break;
                    }
            }
        }

        private void PMDown_StackPanelMain(object sender, MouseButtonEventArgs e)
        {
            if (e.Source is Button)
            {
                Click_buttn_event(e.Source, e);
            }
            else
            {
                SetBaseProjectShow(ItemIndex);
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

        private void Click_buttn_event(object sender, RoutedEventArgs e)
        {
            if(sender is Button buttonName)
            {
                switch(buttonName.Name)
                {
                    case "button_openCAD":
                        {
                            OrderManagerFunctions omFunc = new OrderManagerFunctions();
                            omFunc.RunCommandLine(Properties.Settings.Default.cad_exePath, "-openrpd \"" + cadInfo.CaseXmlPath + "\"");
                            break;
                        }
                    case "button_openDir":
                        {
                            if (cadInfo != null && System.IO.Directory.Exists(cadInfo.CaseDirectoryPath) == true)
                            {
                                OrderManagerFunctions omFunc = new OrderManagerFunctions();
                                omFunc.RunCommandLine(Properties.OrderManagerProps.Default.systemDisk + @"Windows\explorer.exe", "\"" + cadInfo.CaseDirectoryPath + "\"");
                            }
                            break;
                        }
                }
            }
        }
    }
}
