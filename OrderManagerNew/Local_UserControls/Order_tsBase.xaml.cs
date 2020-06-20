using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OrderManagerNew.Local_UserControls
{
    /// <summary>
    /// Order_tsBase.xaml 的互動邏輯
    /// </summary>
    public partial class Order_tsBase : UserControl
    {
        //委派到MainWindow.xaml.cs裡面的CaseHandler_EZCAD_showSingleProject()
        public delegate void tsBaseEventHandler(int projectIndex);
        public event tsBaseEventHandler SetBaseProjectShow;

        /// <summary>
        /// Tray設計階段
        /// </summary>
        enum TrayStep
        {
            GDS_MODELEDIT = 0x00000001,
            GDS_BLOCKOUT = 0x00000002,
            GDS_MARGIN = 0x00000004,
            GDS_GUIDECREATE_T = 0x00000008,
            GDS_POSTPROCESS = 0x00000010,
            GDS_OUTPUT = 0x00000020,
        };

        /// <summary>
        /// Splint設計階段
        /// </summary>
        enum SplintStep
        {
            GDS_MODELEDIT = 0x00000001,
            GDS_BLOCKOUT = 0x00000002,
            GDS_MARGIN = 0x00000004,
            GDS_GUIDECREATE_S = 0x00000008,
            GDS_POSTPROCESS = 0x00000010,
            GDS_OUTPUT = 0x00000020,
            GDS_SPLINT_CREATION = 0x00000040,
            GDS_SPLINT_TOOLKIT = 0x00000080,
        };

        public TrayInformation trayInfo;
        public SplintInformation splintInfo;
        private int ItemIndex;
        public bool IsFocusCase;

        /// <summary>
        /// Tray專案資訊
        /// </summary>
        public class TrayInformation
        {
            /// <summary>
            /// 在tml裡的tag是<ProjectName>
            /// </summary>
            public string OrderID { get; set; }
            public int DesignStep { get; set; }
            public string DesignStepString { get; set; }
            public string Brand { get; set; }
            public int GuideType { get; set; }
            public string GuideTypeString { get; set; }
            public DateTime CreateDate { get; set; }
            public DateTime ModifyDate { get; set; }
            public string CaseDirectoryPath { get; set; }
            public string CaseXmlPath { get; set; }
            public TrayInformation()
            {
                OrderID = "";
                DesignStep = -1;
                DesignStepString = "";
                Brand = "";
                GuideType = -1;
                CreateDate = new DateTime();
                CaseDirectoryPath = "";
                CaseXmlPath = "";
                GuideTypeString = "";
            }
        }
        
        /// <summary>
        /// Splint專案資訊
        /// </summary>
        public class SplintInformation
        {
            /// <summary>
            /// 在sml裡的tag是<ProjectName>
            /// </summary>
            public string OrderID { get; set; }
            public int DesignStep { get; set; }
            public string DesignStepString { get; set; }
            public string Brand { get; set; }
            public int GuideType { get; set; }
            public string GuideTypeString { get; set; }
            public DateTime CreateDate { get; set; }
            public DateTime ModifyDate { get; set; }
            public string CaseDirectoryPath { get; set; }
            public string CaseXmlPath { get; set; }

            public SplintInformation()
            {
                OrderID = "";
                DesignStep = -1;
                DesignStepString = "";
                Brand = "";
                GuideType = -1;
                CreateDate = new DateTime();
                ModifyDate = new DateTime();
                CaseDirectoryPath = "";
                CaseXmlPath = "";
                GuideTypeString = "";
            }
        }

        public Order_tsBase()
        {
            InitializeComponent();
            trayInfo = null;
            splintInfo = null;
            label_orderID.Content = "";
            label_patientName.Content = "";
            label_designStep.Content = "";
            label_createDate.Content = "";
            ItemIndex = -1;
            IsFocusCase = false;
        }

        /// <summary>
        /// 取得Tray設計階段字串
        /// </summary>
        /// <param name="trayDesignStep">trayInfo.DesignStep</param>
        /// <returns></returns>
        private string GetTrayDesignStep(int trayDesignStep)
        {
            string ShowStep = "";

            if ((trayDesignStep & (int)TrayStep.GDS_MODELEDIT) == 0)
                ShowStep += TranslationSource.Instance["GDS_MODELEDIT"];
            else if((trayDesignStep & (int)TrayStep.GDS_BLOCKOUT) == 0)
                ShowStep += TranslationSource.Instance["GDS_BLOCKOUT"];
            else if ((trayDesignStep & (int)TrayStep.GDS_MARGIN) == 0)
                ShowStep += TranslationSource.Instance["GDS_MARGIN"];
            else if ((trayDesignStep & (int)TrayStep.GDS_GUIDECREATE_T) == 0)
                ShowStep += TranslationSource.Instance["GDS_GUIDECREATE_T"];
            else if ((trayDesignStep & (int)TrayStep.GDS_POSTPROCESS) == 0)
                ShowStep += TranslationSource.Instance["GDS_POSTPROCESS"];
            else if ((trayDesignStep & (int)TrayStep.GDS_OUTPUT) == 0)
                ShowStep += TranslationSource.Instance["GDS_OUTPUT"];
            else
                ShowStep += TranslationSource.Instance["None"];

            return ShowStep;
        }

        /// <summary>
        /// 取得Splint設計階段字串
        /// </summary>
        /// <param name="splintDesignStep">splintInfo.DesignStep</param>
        /// <returns></returns>
        private string GetSplintDesignStep(int splintDesignStep)
        {
            string ShowStep = "";

            if ((splintDesignStep & (int)SplintStep.GDS_MODELEDIT) == 0)
                ShowStep += TranslationSource.Instance["GDS_MODELEDIT"];
            else if ((splintDesignStep & (int)SplintStep.GDS_BLOCKOUT) == 0)
                ShowStep += TranslationSource.Instance["GDS_BLOCKOUT"];
            else if ((splintDesignStep & (int)SplintStep.GDS_MARGIN) == 0)
                ShowStep += TranslationSource.Instance["GDS_MARGIN"];
            else if ((splintDesignStep & (int)SplintStep.GDS_GUIDECREATE_S) == 0)
                ShowStep += TranslationSource.Instance["GDS_GUIDECREATE_S"];
            else if ((splintDesignStep & (int)SplintStep.GDS_POSTPROCESS) == 0)
                ShowStep += TranslationSource.Instance["GDS_POSTPROCESS"];
            else if ((splintDesignStep & (int)SplintStep.GDS_OUTPUT) == 0)
                ShowStep += TranslationSource.Instance["GDS_OUTPUT"];
            else if ((splintDesignStep & (int)SplintStep.GDS_SPLINT_CREATION) == 0)
                ShowStep += TranslationSource.Instance["GDS_SPLINT_CREATION"];
            else if ((splintDesignStep & (int)SplintStep.GDS_SPLINT_TOOLKIT) == 0)
                ShowStep += TranslationSource.Instance["GDS_SPLINT_TOOLKIT"];
            else
                ShowStep += TranslationSource.Instance["None"];

            return ShowStep;
        }

        /// <summary>
        /// 設定顯示在UserControl上的內容
        /// </summary>
        /// <param name="Import">TrayInformation清單</param>
        /// <param name="Index">從0開始</param>
        public void SetTrayCaseInfo(TrayInformation Import, int Index)
        {
            trayInfo = Import;
            trayInfo.DesignStepString = GetTrayDesignStep((int)trayInfo.DesignStep);
            label_orderID.Content = trayInfo.OrderID.Substring(trayInfo.OrderID.IndexOf('-') + 1);
            label_designStep.Content = TranslationSource.Instance["CurrentStep"] + trayInfo.DesignStepString;
            label_createDate.Content = trayInfo.CreateDate.ToString(System.Globalization.CultureInfo.CreateSpecificCulture("en-US"));
            ItemIndex = Index;
        }

        /// <summary>
        /// 設定顯示在UserControl上的內容
        /// </summary>
        /// <param name="Import">SplintInformation清單</param>
        /// <param name="Index">從0開始</param>
        public void SetSplintCaseInfo(SplintInformation Import, int Index)
        {
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(@"/ImageSource/FunctionTable/icon_S.png", UriKind.Relative);
            bitmap.EndInit();
            image_Main.Source = bitmap;

            splintInfo = Import;
            splintInfo.DesignStepString = GetSplintDesignStep((int)splintInfo.DesignStep);
            label_orderID.Content = splintInfo.OrderID.Substring(splintInfo.OrderID.IndexOf('-') + 1);
            label_designStep.Content = TranslationSource.Instance["CurrentStep"] + splintInfo.DesignStepString;
            label_createDate.Content = splintInfo.CreateDate.ToString(System.Globalization.CultureInfo.CreateSpecificCulture("en-US"));
            ItemIndex = Index;
        }

        private void Click_FolderOpen(object sender, RoutedEventArgs e)
        {
            if (trayInfo != null && System.IO.Directory.Exists(trayInfo.CaseDirectoryPath) == true)
            {
                OrderManagerFunctions omFunc = new OrderManagerFunctions();
                omFunc.RunCommandLine(Properties.OrderManagerProps.Default.systemDisk + @"Windows\explorer.exe", "\"" + trayInfo.CaseDirectoryPath + "\"");
            }
            else if(splintInfo != null && System.IO.Directory.Exists(splintInfo.CaseDirectoryPath) == true)
            {
                OrderManagerFunctions omFunc = new OrderManagerFunctions();
                omFunc.RunCommandLine(Properties.OrderManagerProps.Default.systemDisk + @"Windows\explorer.exe", "\"" + splintInfo.CaseDirectoryPath + "\"");
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
                        background_tsBase.Fill = this.FindResource("background_FocusedCase") as SolidColorBrush;
                        IsFocusCase = true;
                        break;
                    }
                case false:
                    {
                        background_tsBase.Fill = Brushes.White;
                        IsFocusCase = false;
                        break;
                    }
            }
        }

        private void PMDown_StackPanelMain(object sender, MouseButtonEventArgs e)
        {
            if (e.Source is Button)
            {
                Click_FolderOpen(e.Source, e);
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
    }
}
