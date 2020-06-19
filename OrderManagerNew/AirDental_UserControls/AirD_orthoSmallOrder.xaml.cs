using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.IO;
using MahApps.Metro;
//下載用
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace OrderManagerNew.AirDental_UserControls
{
    /// <summary>
    /// AirD_orthoSmallOrder.xaml 的互動邏輯
    /// </summary>
    public partial class AirD_orthoSmallOrder : UserControl
    {
        //委派到MainWindow.xaml.cs裡面的SnackBarShow(string)
        public delegate void orthoOrder2EventHandler_snackbar(string message);
        public event orthoOrder2EventHandler_snackbar OrderHandler_snackbarShow;

        //委派到Order_orthoBase.xaml.cs裡面的SmallCaseHandler()
        public delegate void orthoOrderEventHandler(int projectIndex);
        public event orthoOrderEventHandler SetOrderCaseShow;

        static public bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {   // 總是接受
            return true;
        }

        public bool IsFocusSmallCase;
        Dll_Airdental.Main AirDental;
        Dll_Airdental.Main._orthoOrder OrderInfo;
        int ItemIndex;
        public BackgroundWorker BgWorker_Download;
        string DownloadFileName;

        public AirD_orthoSmallOrder()
        {
            InitializeComponent();
            ItemIndex = -1;
            progressbar_download.Value = 0;
            background_orthoSmallcase.Visibility = Visibility.Visible;
            DownloadFileName = "";
            BgWorker_Download = null;
        }

        public void SetOrderInfo(Dll_Airdental.Main._orthoOrder Import, int Index)
        {
            OrderInfo = Import;
            if (Import._stageKey.IndexOf("ortho_") == 0)
                Import._stageKey = Import._stageKey.Remove(0, 6);
            label_ProjectName.Content = TranslationSource.Instance[Import._group] + " " + TranslationSource.Instance[Import._actionKey] + TranslationSource.Instance[Import._stageKey];
            label_ProjectName.ToolTip = Import._date.DateTime.ToLongDateString() + Import._date.DateTime.ToLongTimeString();
            ItemIndex = Index;
        }
        
        private void Click_ButtonEvent(object sender, RoutedEventArgs e)
        {
            if(sender is Button)
            {
                switch(((Button)sender).Name)
                {
                    case "button_DownloadOrder":
                        {
                            if(background_orthoSmallcase.Visibility == Visibility.Visible)
                                background_orthoSmallcase.Visibility = Visibility.Hidden;
                            DownloadOrderFile();
                            break;
                        }
                }
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
                        background_orthoSmallcase.Fill = this.FindResource("background_FocusedSmallCase") as SolidColorBrush;
                        background_orthoSmallcase.Stroke = this.FindResource("borderbrush_FocusedSmallCase") as SolidColorBrush;
                        IsFocusSmallCase = true;
                        break;
                    }
                case false:
                    {
                        background_orthoSmallcase.Fill = this.FindResource("background_SmallCase") as SolidColorBrush;
                        background_orthoSmallcase.Stroke = null;
                        IsFocusSmallCase = false;
                        break;
                    }
            }
        }

        private void PMDown_StackPanelMain(object sender, MouseButtonEventArgs e)
        {
            if (e.Source is Button)
            {
                Click_ButtonEvent(e.Source, e);
            }
            else
            {
                SetOrderCaseShow(ItemIndex);
                if (IsFocusSmallCase == false)
                {
                    SetCaseFocusStatus(true);
                }
                else
                {
                    SetCaseFocusStatus(false);
                }
            }
        }

        private void DownloadOrderFile()
        {
            button_DownloadOrder.IsEnabled = false;
            if (System.IO.Directory.Exists(Properties.OrderManagerProps.Default.ortho_projectDirectory) == false)
                return;
            else if (Directory.Exists(Properties.OrderManagerProps.Default.ortho_projectDirectory) == false)
                return;

            progressbar_download.IsIndeterminate = true;

            //檢查抓不抓得到下載檔名
            string Downloadurl = Properties.OrderManagerProps.Default.AirDentalAPI + @"project/ortho/download/" + OrderInfo._Key;
            AirDental = new Dll_Airdental.Main();
            DownloadFileName = AirDental.GetDownloadFileInfo(Downloadurl, Properties.Settings.Default.AirdentalCookie);
            if(DownloadFileName != "")
            {
                string readyDownloadFilePath = Properties.OrderManagerProps.Default.ortho_projectDirectory + DownloadFileName;
                if (File.Exists(readyDownloadFilePath) == true)
                    File.Delete(readyDownloadFilePath);

                //多執行緒下載
                BgWorker_Download = new BackgroundWorker();
                BgWorker_Download.DoWork += new DoWorkEventHandler(DoWork_Download);
                BgWorker_Download.ProgressChanged += new ProgressChangedEventHandler(UpdateProgress_Download);
                BgWorker_Download.RunWorkerCompleted += new RunWorkerCompletedEventHandler(CompletedWork_Download);
                BgWorker_Download.WorkerReportsProgress = true;
                BgWorker_Download.WorkerSupportsCancellation = false;
                progressbar_download.IsIndeterminate = false;
                BgWorker_Download.RunWorkerAsync();
            }
        }

        void DoWork_Download(object sender, DoWorkEventArgs e)
        {
            if(sender is BackgroundWorker)
            {
                double percent = 0.00;
                while (percent < 100.0)
                {
                    ((BackgroundWorker)sender).ReportProgress(Convert.ToInt32(percent * 100));
                    percent += 0.01;
                    System.Threading.Thread.Sleep(1000);
                }
            }
        }
        void UpdateProgress_Download(object sender, ProgressChangedEventArgs e)
        {
            int progress = e.ProgressPercentage;
            this.Dispatcher.Invoke((Action)(() =>
            {
                progressbar_download.Value = progress;
            }));
        }
        void CompletedWork_Download(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                OrderHandler_snackbarShow("Error");   //錯誤 //TODO 多國語系
            }
            else if (e.Cancelled)
            {
                OrderHandler_snackbarShow("Canceled");    //取消 //TODO 多國語系
            }
            else
            {
                OrderHandler_snackbarShow("下載完成");   //下載完成  //TODO 多國語系
                label_ProjectName.Content += "(已下載)";
                button_DownloadOrder.IsEnabled = false;
                BgWorker_Download = null;
            }
        }
    }
}
