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
    /// Order_tsBase.xaml 的互動邏輯
    /// </summary>
    public partial class Order_tsBase : UserControl
    {
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

        private TrayInformation trayInfo;
        private SplintInformation splintInfo;

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
            public string Brand { get; set; }
            public int GuideType { get; set; }
            public DateTime CreateDate { get; set; }
            public DateTime ModifyDate { get; set; }
            public string CaseDirectoryPath { get; set; }

            public TrayInformation()
            {
                OrderID = "";
                DesignStep = -1;
                Brand = "";
                GuideType = -1;
                CreateDate = new DateTime();
                CaseDirectoryPath = "";
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
            public string Brand { get; set; }
            public int GuideType { get; set; }
            public DateTime CreateDate { get; set; }
            public DateTime ModifyDate { get; set; }
            public string CaseDirectoryPath { get; set; }

            public SplintInformation()
            {
                OrderID = "";
                DesignStep = -1;
                Brand = "";
                GuideType = -1;
                CreateDate = new DateTime();
                ModifyDate = new DateTime();
                CaseDirectoryPath = "";
            }
        }

        public Order_tsBase()
        {
            InitializeComponent();

            label_orderID.Content = "";
            label_patientName.Content = "";
            label_designStep.Content = "";
            label_createDate.Content = "";
        }

        /// <summary>
        /// 取得Tray設計階段字串
        /// </summary>
        /// <param name="trayDesignStep">trayInfo.DesignStep</param>
        /// <returns></returns>
        private string GetTrayDesignStep(int trayDesignStep)
        {
            string ShowStep = OrderManagerNew.TranslationSource.Instance["CurrentStep"];

            if ((trayDesignStep & (int)TrayStep.GDS_MODELEDIT) == 0)
                ShowStep += OrderManagerNew.TranslationSource.Instance["GDS_MODELEDIT"];
            else if((trayDesignStep & (int)TrayStep.GDS_BLOCKOUT) == 0)
                ShowStep += OrderManagerNew.TranslationSource.Instance["GDS_BLOCKOUT"];
            else if ((trayDesignStep & (int)TrayStep.GDS_MARGIN) == 0)
                ShowStep += OrderManagerNew.TranslationSource.Instance["GDS_MARGIN"];
            else if ((trayDesignStep & (int)TrayStep.GDS_GUIDECREATE_T) == 0)
                ShowStep += OrderManagerNew.TranslationSource.Instance["GDS_GUIDECREATE_T"];
            else if ((trayDesignStep & (int)TrayStep.GDS_POSTPROCESS) == 0)
                ShowStep += OrderManagerNew.TranslationSource.Instance["GDS_POSTPROCESS"];
            else if ((trayDesignStep & (int)TrayStep.GDS_OUTPUT) == 0)
                ShowStep += OrderManagerNew.TranslationSource.Instance["GDS_OUTPUT"];
            else
                ShowStep += OrderManagerNew.TranslationSource.Instance["None"];

            return ShowStep;
        }

        /// <summary>
        /// 取得Splint設計階段字串
        /// </summary>
        /// <param name="splintDesignStep">splintInfo.DesignStep</param>
        /// <returns></returns>
        private string GetSplintDesignStep(int splintDesignStep)
        {
            string ShowStep = OrderManagerNew.TranslationSource.Instance["CurrentStep"];

            if ((splintDesignStep & (int)SplintStep.GDS_MODELEDIT) == 0)
                ShowStep += OrderManagerNew.TranslationSource.Instance["GDS_MODELEDIT"];
            else if ((splintDesignStep & (int)SplintStep.GDS_BLOCKOUT) == 0)
                ShowStep += OrderManagerNew.TranslationSource.Instance["GDS_BLOCKOUT"];
            else if ((splintDesignStep & (int)SplintStep.GDS_MARGIN) == 0)
                ShowStep += OrderManagerNew.TranslationSource.Instance["GDS_MARGIN"];
            else if ((splintDesignStep & (int)SplintStep.GDS_GUIDECREATE_S) == 0)
                ShowStep += OrderManagerNew.TranslationSource.Instance["GDS_GUIDECREATE_S"];
            else if ((splintDesignStep & (int)SplintStep.GDS_POSTPROCESS) == 0)
                ShowStep += OrderManagerNew.TranslationSource.Instance["GDS_POSTPROCESS"];
            else if ((splintDesignStep & (int)SplintStep.GDS_OUTPUT) == 0)
                ShowStep += OrderManagerNew.TranslationSource.Instance["GDS_OUTPUT"];
            else if ((splintDesignStep & (int)SplintStep.GDS_SPLINT_CREATION) == 0)
                ShowStep += OrderManagerNew.TranslationSource.Instance["GDS_SPLINT_CREATION"];
            else if ((splintDesignStep & (int)SplintStep.GDS_SPLINT_TOOLKIT) == 0)
                ShowStep += OrderManagerNew.TranslationSource.Instance["GDS_SPLINT_TOOLKIT"];
            else
                ShowStep += OrderManagerNew.TranslationSource.Instance["None"];

            return ShowStep;
        }

        /// <summary>
        /// 設定顯示在UserControl上的內容
        /// </summary>
        /// <param name="Import"></param>
        public void SetTrayCaseInfo(TrayInformation Import)
        {
            trayInfo = Import;
            label_orderID.Content = trayInfo.OrderID.Substring(trayInfo.OrderID.IndexOf('-') + 1);
            label_designStep.Content = GetTrayDesignStep((int)trayInfo.DesignStep);
            label_createDate.Content = trayInfo.CreateDate.ToLongDateString();
        }

        /// <summary>
        /// 設定顯示在UserControl上的內容
        /// </summary>
        /// <param name="Import"></param>
        public void SetSplintCaseInfo(SplintInformation Import)
        {
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(@"/ImageSource/FunctionTable/icon_S.png", UriKind.Relative);
            bitmap.EndInit();
            image_Main.Source = bitmap;
            splintInfo = Import;
            label_orderID.Content = splintInfo.OrderID.Substring(splintInfo.OrderID.IndexOf('-') + 1);
            label_designStep.Content = GetSplintDesignStep((int)splintInfo.DesignStep);
            label_createDate.Content = splintInfo.CreateDate.ToLongDateString();
        }

        private void Click_FolderOpen(object sender, RoutedEventArgs e)
        {
            if (trayInfo != null && System.IO.Directory.Exists(trayInfo.CaseDirectoryPath) == true)
            {
                OrderManagerFunctions omFunc = new OrderManagerFunctions();
                omFunc.RunCommandLine(Properties.Settings.Default.systemDisk + @"Windows\explorer.exe", "\"" + trayInfo.CaseDirectoryPath + "\"");
            }
            else if(splintInfo != null && System.IO.Directory.Exists(splintInfo.CaseDirectoryPath) == true)
            {
                OrderManagerFunctions omFunc = new OrderManagerFunctions();
                omFunc.RunCommandLine(Properties.Settings.Default.systemDisk + @"Windows\explorer.exe", "\"" + splintInfo.CaseDirectoryPath + "\"");
            }
        }
    }
}
