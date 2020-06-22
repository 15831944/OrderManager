using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace OrderManagerNew.AirDental_UserControls
{
    /// <summary>
    /// AirD_implantSmallOrder.xaml 的互動邏輯
    /// </summary>
    public partial class AirD_implantSmallOrder : UserControl
    {
        //委派到MainWindow.xaml.cs裡面的SnackBarShow(string)
        public delegate void implantOrder2EventHandler_snackbar(string message);
        public event implantOrder2EventHandler_snackbar OrderHandler_snackbarShow;
        //委派到Order_orthoBase.xaml.cs裡面的SmallCaseHandler()
        public delegate void implantOrderEventHandler(int projectIndex);
        public event implantOrderEventHandler SetOrderCaseShow;
        static public bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {   // 總是接受
            return true;
        }

        public bool IsFocusSmallCase;
        Dll_Airdental.Main AirDental;
        Dll_Airdental.Main._implantOrder OrderInfo;
        int ItemIndex;
        BackgroundWorker BgWorker_Download;
        string DownloadFileName;

        public AirD_implantSmallOrder()
        {
            InitializeComponent();
            ItemIndex = -1;
            progressbar_download.Value = 0;
            background_orthoSmallcase.Visibility = Visibility.Visible;
            DownloadFileName = "";
            button_openDir.IsEnabled = false;
        }

        public void SetOrderInfo(Dll_Airdental.Main._implantOrder Import, int Index)
        {
            OrderInfo = Import;
            if (Import._stageKey.IndexOf("guide_") == 0)
                Import._stageKey = Import._stageKey.Remove(0, 6);
            label_ProjectName.Content = TranslationSource.Instance[Import._group] + " " + TranslationSource.Instance[Import._actionKey] + TranslationSource.Instance[Import._stageKey];
            label_ProjectName.ToolTip = Import._date.ToString(System.Globalization.CultureInfo.CreateSpecificCulture("en-US"));
            ItemIndex = Index;
        }

        private void Click_ButtonEvent(object sender, RoutedEventArgs e)
        {
            if (sender is Button)
            {
                switch (((Button)sender).Name)
                {
                    case "button_DownloadOrder":
                        {
                            if (background_orthoSmallcase.Visibility == Visibility.Visible)
                                background_orthoSmallcase.Visibility = Visibility.Hidden;
                            DownloadOrderFile();
                            break;
                        }
                    case "button_openDir":
                        {
                            OrderManagerFunctions omFunc = new OrderManagerFunctions();
                            omFunc.RunCommandLine(Properties.OrderManagerProps.Default.systemDisk + @"Windows\explorer.exe", "/select,\"" + DownloadFileName + "\"");
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
            e.Handled = true;
        }

        private void DownloadOrderFile()
        {
            button_DownloadOrder.IsEnabled = false;
            button_openDir.IsEnabled = false;
            if (Directory.Exists(Properties.OrderManagerProps.Default.AirD_Implant_Dir) == false)
            {
                OrderHandler_snackbarShow(TranslationSource.Instance["DownloadDirNotFound"]);
                return;
            }

            string Downloadurl = "";
            try
            {
                //檢查抓不抓得到下載檔名
                Downloadurl = Properties.OrderManagerProps.Default.AirDentalAPI + @"project/implant/download/" + OrderInfo._Key;
                AirDental = new Dll_Airdental.Main();
                DownloadFileName = AirDental.GetDownloadFileInfo(Downloadurl, Properties.Settings.Default.AirdentalCookie);
            }
            catch (Exception ex)
            {
                OrderHandler_snackbarShow(ex.Message);
                return;
            }

            if (DownloadFileName != "")
            {
                try
                {
                    DownloadFileName = Properties.OrderManagerProps.Default.AirD_Implant_Dir + DownloadFileName;
                    DownloadFileName = DownloadFileName.Insert(DownloadFileName.LastIndexOf("."), "_" + (ItemIndex + 1).ToString());
                    if (File.Exists(DownloadFileName) == true)
                        File.Delete(DownloadFileName);
                    progressbar_download.Value = 0.0;
                    if (((string)label_ProjectName.Content).IndexOf(TranslationSource.Instance["Order_Downloaded"]) != -1)
                        label_ProjectName.Content = ((string)label_ProjectName.Content).Remove(((string)label_ProjectName.Content).IndexOf("(已下載)"));
                }
                catch (Exception ex)
                {
                    OrderHandler_snackbarShow(ex.Message);
                    return;
                }

                //多執行緒下載
                BgWorker_Download = new BackgroundWorker();
                BgWorker_Download.DoWork += new DoWorkEventHandler(DoWork_Download);
                BgWorker_Download.ProgressChanged += new ProgressChangedEventHandler(UpdateProgress_Download);
                BgWorker_Download.RunWorkerCompleted += new RunWorkerCompletedEventHandler(CompletedWork_Download);
                BgWorker_Download.WorkerReportsProgress = true;
                BgWorker_Download.WorkerSupportsCancellation = false;
                BgWorker_Download.RunWorkerAsync();
            }
        }

        void DoWork_Download(object sender, DoWorkEventArgs e)
        {
            if (sender is BackgroundWorker)
            {
                //跳過https檢測 & Win7 相容
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);

                try
                {
                    string fileUrl = Properties.OrderManagerProps.Default.AirDentalAPI + @"project/implant/download/" + OrderInfo._Key;
                    //Response資料
                    HttpWebResponse response = AirDental.GetDownloadFileResponse(fileUrl, Properties.Settings.Default.AirdentalCookie);
                    if (response != null)
                    {
                        // 取得下載的檔名
                        Uri uri = new Uri(fileUrl);
                        Stream netStream = response.GetResponseStream();
                        Stream fileStream = new FileStream(DownloadFileName, FileMode.Create, FileAccess.Write);
                        byte[] read = new byte[1024];
                        long progressBarValue = 0;
                        int realReadLen = netStream.Read(read, 0, read.Length);

                        while (realReadLen > 0)
                        {
                            fileStream.Write(read, 0, realReadLen);
                            progressBarValue += realReadLen;
                            double percent = (double)progressBarValue / (double)response.ContentLength;
                            ((BackgroundWorker)sender).ReportProgress(Convert.ToInt32(percent * 100));
                            realReadLen = netStream.Read(read, 0, read.Length);
                        }
                        fileStream.Close();
                        netStream.Close();
                    }
                    response.Close();
                }
                catch (WebException ex)
                {
                    Console.WriteLine(ex.Message);
                    e.Cancel = true;
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
                OrderHandler_snackbarShow(TranslationSource.Instance["Download"] + TranslationSource.Instance["Error"]);
            }
            else if (e.Cancelled)
            {
                OrderHandler_snackbarShow(TranslationSource.Instance["Download"] + TranslationSource.Instance["Cancel"]);
            }
            else
            {
                OrderHandler_snackbarShow(TranslationSource.Instance["Order_DownloadCompleted"]);
                if (((string)label_ProjectName.Content).IndexOf(TranslationSource.Instance["Order_Downloaded"]) == -1)
                    label_ProjectName.Content += TranslationSource.Instance["Order_Downloaded"];
                button_DownloadOrder.IsEnabled = true;
                button_openDir.IsEnabled = true;
            }
        }
    }
}
