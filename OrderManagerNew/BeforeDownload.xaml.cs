using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.IO;
using Path = System.IO.Path;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.ComponentModel;
using System.Timers;  // 名稱空間是 Timers 而不是 Threading
using System.Diagnostics;
using UIDialogs;

namespace OrderManagerNew
{
    public partial class BeforeDownload : Window
    {
        //委派到MainWindow.xaml.cs裡面的setSoftwareShow()
        public delegate void beforedownloadEventHandler();
        public event beforedownloadEventHandler SetHttpResponseOK;
        //委派到MainWindow.xaml.cs裡面的SnackBarShow(string)
        public delegate void beforedownloadEventHandler_snackbar(string message);
        public event beforedownloadEventHandler_snackbar Handler_snackbarShow;

        static public bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {   // 總是接受  
            return true;
        }
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetDiskFreeSpaceEx(string lpDirectoryName, out ulong lpFreeBytesAvailable, out ulong lpTotalNumberOfBytes, out ulong lpTotalNumberOfFreeBytes);

        BackgroundWorker m_BackgroundWorker;        //申明後臺物件
        HttpWebResponse httpResponse;               //共用同一個WebResponse以便清除內部殘留資料
        string http_url;                            //下載網址
        Timer tmr;                                  //計時器(用在GetResponse)
        int currentSoftwareID;                      //軟體ID
        LogRecorder Log;

        public BeforeDownload()
        {
            InitializeComponent();
            http_url = "";
            currentSoftwareID = -1;
            Log = new LogRecorder();
            
        }

        private void Click_TitleBar_titlebarButtons(object sender, RoutedEventArgs e)
        {
            if(sender is Button)
            {
                switch (((Button)sender).Name)
                {
                    case "systemButton_Close":              //關閉
                        Close();
                        break;
                }
            }
        }

        private void Click_OpenFilePath(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    textbox_InstallPath.Text = dialog.SelectedPath + @"\";

                    RemainingSpace(dialog.SelectedPath);
                }
            }
        }

        private void Click_systemButton(object sender, RoutedEventArgs e)
        {
            void GOTODownload()
            {
                try
                {
                    if (Directory.Exists(textbox_InstallPath.Text) != true)
                        Directory.CreateDirectory(textbox_InstallPath.Text);

                    SetPropertiesSoftwarePath(currentSoftwareID, textbox_InstallPath.Text);
                    this.DialogResult = true;
                }
                catch(Exception ex)
                {
                    Inteware_Messagebox Msg = new Inteware_Messagebox();
                    Msg.ShowMessage(ex.Message);
                    this.DialogResult = false;
                }
                
            }

            if(sender is Button)
            {
                switch (((Button)sender).Name)
                {
                    case "sysBtn_Yes":
                        {
                            //檢查客戶端容量是否比軟體檔案所需空間大超過3倍，如果沒有就Messagebox警告
                            if (label_AvailableSpace.Foreground == Brushes.Orange)
                            {
                                Inteware_Messagebox Msg = new Inteware_Messagebox();
                                Msg.ShowMessage(TranslationSource.Instance["SpaceMaynotBeEnough"], TranslationSource.Instance["Warning"], MessageBoxButton.YesNo, MessageBoxImage.Warning);
                                if (Msg.ReturnClickWhitchButton == (int)Inteware_Messagebox._ReturnButtonName.YES)
                                    GOTODownload();
                                else
                                    this.DialogResult = false;
                            }
                            else if (label_AvailableSpace.Foreground == Brushes.Orange)
                            {
                                Inteware_Messagebox Msg = new Inteware_Messagebox();
                                Msg.ShowMessage(TranslationSource.Instance["NoEnoughSpaceToInstall"]);//磁碟空間不足以安裝軟體
                                this.DialogResult = false;
                            }
                            else
                            {
                                GOTODownload();
                            }
                            break;
                        }
                    case "sysBtn_Cancel":
                        {
                            this.DialogResult = false;
                            break;
                        }
                }
            }
        }

        /// <summary>
        /// 用路徑抓出使用者電腦容量
        /// </summary>
        /// <param name="Drive">路徑</param>
        /// <returns></returns>
        bool RemainingSpace(string Drive)
        {
            try
            {
                if (Directory.Exists(Drive) == false)
                {
                    Directory.CreateDirectory(Drive);
                }

                string str = Path.GetPathRoot(Drive);
                bool success = GetDiskFreeSpaceEx(Path.GetPathRoot(Drive), out ulong FreeBytesAvailable, out ulong TotalNumberOfBytes, out ulong TotalNumberOfFreeBytes);

                if (!success)
                {
                    Inteware_Messagebox Msg = new Inteware_Messagebox();
                    Msg.ShowMessage(TranslationSource.Instance["CannnotGetRemainingSpace"]);
                    return false;
                }

                label_AvailableSpace.Tag = TotalNumberOfFreeBytes;
                label_AvailableSpace.Content = ConvertDiskUnit(TotalNumberOfFreeBytes, (int)_diskUnit.MB);
                return true;
            }
            catch(Exception ex)
            {
                Inteware_Messagebox Msg = new Inteware_Messagebox();
                Msg.ShowMessage(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 轉換容量單位
        /// </summary>
        /// <param name="InputBytes">單位是Bytes 使用ulong</param>
        /// <param name="DiskUnit">要顯示哪種單位(參考EnumSummary的_diskUnit)</param>
        /// <returns></returns>
        string ConvertDiskUnit(ulong InputBytes, int DiskUnit)
        {
            string OutputString = "";

            switch(DiskUnit)
            {
                case (int)_diskUnit.KB:
                    {
                        OutputString = Convert.ToSingle(Math.Round(((double)(Int64)InputBytes / 1024.0), 1)).ToString() + "KB";
                        break;
                    }
                case (int)_diskUnit.MB:
                    {
                        OutputString = Convert.ToSingle(Math.Round(((double)(Int64)InputBytes / 1024.0 / 1024.0), 1)).ToString() + "MB";
                        break;
                    }
                case (int)_diskUnit.GB:
                    {
                        OutputString = Convert.ToSingle(Math.Round(((double)(Int64)InputBytes / 1024.0 / 1024.0 / 1024.0), 1)).ToString() + "GB";
                        break;
                    }
                case (int)_diskUnit.TB:
                    {
                        OutputString = Convert.ToSingle(Math.Round(((double)(Int64)InputBytes / 1024.0 / 1024.0 / 1024.0 / 1024.0), 1)).ToString() + "TB";
                        break;
                    }
            }
            return OutputString;
        }
        
        #region 多執行緒處理接收網路下載資料內容
        void DoWork(object sender, DoWorkEventArgs e)
        {
            Log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "BeforeDownload_DoWork", "IntoFunc");
            //跳過https檢測 & Win7 相容
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);

            //Request資料
            try
            {
                HttpWebRequest httpRequest;
                httpRequest = (HttpWebRequest)WebRequest.Create(http_url);
                httpRequest.Credentials = CredentialCache.DefaultCredentials;
                httpRequest.UserAgent = ".NET Framework Example Client";
                httpRequest.Method = "GET";
                httpRequest.Timeout = Properties.Settings.Default.PingTime * 1000;

                Handler_snackbarShow(TranslationSource.Instance["ReceivingData"]); //開始取得資料
                httpResponse = (HttpWebResponse)httpRequest.GetResponse();
            }
            catch (Exception ex)
            {
                Log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "BeforeDownload.xaml.cs_DoWork()_exception", ex.Message);
                e.Cancel = true;
            }
        }
        void CompletedWork(object sender, RunWorkerCompletedEventArgs e)
        {
            Log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "BeforeDownlad_CompletedWork", "IntoFunc");
            if (e.Error != null)
            {
                tmr.Stop();
                Log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "BeforeDownload.xaml.cs_CompletedWork()_Error", e.Error.Message);
                Handler_snackbarShow(TranslationSource.Instance["Error"]);   //錯誤
            }
            else if (e.Cancelled)
            {
                tmr.Stop();
                Log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "BeforeDownload.xaml.cs_CompletedWork()_exception", "e.Cancelled");
                Inteware_Messagebox Msg = new Inteware_Messagebox();
                Msg.ShowMessage(TranslationSource.Instance["ReceivingDataNotResponding"], TranslationSource.Instance["Warning"], MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if(Msg.ReturnClickWhitchButton == (int)Inteware_Messagebox._ReturnButtonName.YES)
                {
                    if (Properties.Settings.Default.PingTime < 60)
                    {
                        //重試，加5秒接收時間
                        Properties.Settings.Default.PingTime += 5;
                        Properties.Settings.Default.Save();
                    }
                }
                Handler_snackbarShow(TranslationSource.Instance["Cancel"]);    //取消(超過時間，也許再讓使用者可以自行增加秒數?)
            }
            else
            {
                tmr.Stop();
                Log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "BeforeDownload.xaml.cs_CompletedWork()", "OK");
                SetHttpResponseOK();
            }
        }
        /// <summary>
        /// 計時器結束事件
        /// </summary>
        void Tmr_Elapsed(object sender, EventArgs e)
        {
            tmr.Stop();
        }
        #endregion

        /// <summary>
        /// 設定UI所顯示的資訊
        /// </summary>
        public bool SetInformation()
        {
            if (currentSoftwareID == -1 || http_url == "")
                return false;

            OrderManagerFunctions omFunc = new OrderManagerFunctions();
            string[] SoftwareNameArray = new string[6] { omFunc.GetSoftwareName(_softwareID.EZCAD),
                omFunc.GetSoftwareName(_softwareID.Implant), omFunc.GetSoftwareName(_softwareID.Ortho),
                omFunc.GetSoftwareName(_softwareID.Tray), omFunc.GetSoftwareName(_softwareID.Splint), omFunc.GetSoftwareName(_softwareID.Guide)};
            label_TitleBar.Content = TranslationSource.Instance["Install"] + "-" + SoftwareNameArray[currentSoftwareID].Replace(" ", ".");
            label_Header.Content = TranslationSource.Instance["AboutToInstall"] + " " + SoftwareNameArray[currentSoftwareID].Replace(" ", ".");
            if(Properties.OrderManagerProps.Default.mostsoftwareDisk != "")
                textbox_InstallPath.Text = Properties.OrderManagerProps.Default.mostsoftwareDisk + @"PrintIn3D\" + SoftwareNameArray[currentSoftwareID] + @"\";
            else
                textbox_InstallPath.Text = @"C:\PrintIn3D\" + SoftwareNameArray[currentSoftwareID] + @"\";
            jlabel_RequireSpace.Content += ":";
            jlabel_AvailableSpace.Content += ":";
            try
            {
                if (((HttpWebResponse)httpResponse).StatusDescription == "OK" && httpResponse.ContentLength > 1)
                {
                    // 取得下載的檔名
                    Uri uri = new Uri(http_url);
                    string downloadfileRealName = Path.GetFileName(uri.LocalPath);
                    label_DownloadFileName.Content = "(" + downloadfileRealName + ")";

                    if (RemainingSpace(textbox_InstallPath.Text) == true)  //客戶電腦剩餘空間
                    {
                        label_RequireSpace.Tag = Convert.ToUInt64(httpResponse.ContentLength);
                        label_RequireSpace.Content = ConvertDiskUnit(Convert.ToUInt64(httpResponse.ContentLength), (int)_diskUnit.MB);

                        if ((ulong)label_AvailableSpace.Tag < (ulong)label_RequireSpace.Tag)
                        {
                            label_AvailableSpace.Foreground = Brushes.Orange;
                            label_AvailableSpace.ToolTip = TranslationSource.Instance["NoEnoughSpaceToInstall"];//磁碟空間不足以安裝軟體
                        }
                        else if ((ulong)label_AvailableSpace.Tag < (ulong)label_RequireSpace.Tag * 3)
                        {
                            label_AvailableSpace.Foreground = Brushes.Orange;
                            label_AvailableSpace.ToolTip = TranslationSource.Instance["SpaceMaynotBeEnough"];//磁碟空間可能不足以安裝軟體
                        }
                        else
                        {
                            label_AvailableSpace.Foreground = Brushes.White;
                            label_AvailableSpace.ToolTip = TranslationSource.Instance["SpaceAvailable"];
                        }
                    }
                    else
                    {
                        Inteware_Messagebox Msg = new Inteware_Messagebox();
                        Msg.ShowMessage(TranslationSource.Instance["CannnotGetRemainingSpace"]);
                        if (httpResponse != null)
                            httpResponse.Close();
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Inteware_Messagebox Msg = new Inteware_Messagebox();
                Msg.ShowMessage(ex.Message, TranslationSource.Instance["NetworkError"]);    //網路連線異常or載點掛掉
                if (httpResponse != null)
                    httpResponse.Close();
                return false;
            }
            if (httpResponse != null)
                httpResponse.Close();
            return true;
        }
        /// <summary>
        /// 用多執行緒去取得httpResponse
        /// </summary>
        /// <param name="Import_http_url">下載網址</param>
        /// <param name="SoftwareID">軟體ID(參考EnumSummary的_softwareID)</param>
        /// <returns></returns>
        public void GethttpResoponse(string Import_http_url, int SoftwareID)
        {
            Log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "BeforeDownload_GethttpResponse", "IntoFunc");
            http_url = Import_http_url;
            currentSoftwareID = SoftwareID;

            m_BackgroundWorker = new BackgroundWorker
            {
                WorkerReportsProgress = false, // 設定可以通告進度
                WorkerSupportsCancellation = true // 設定可以取消
            }; // 例項化後臺物件

            //倒數計時
            tmr = new Timer();
            if (Properties.Settings.Default.PingTime <= 0 || Properties.Settings.Default.PingTime >= 45)
            {
                Properties.Settings.Default.PingTime = 5;
                Properties.Settings.Default.Save();
            }

            tmr.Interval = Properties.Settings.Default.PingTime * 1000;

            tmr.Elapsed += Tmr_Elapsed;  // 使用事件代替委託
            tmr.Start();          // 重啟定時器

            m_BackgroundWorker.DoWork += new DoWorkEventHandler(DoWork);
            m_BackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(CompletedWork);

            m_BackgroundWorker.RunWorkerAsync(this);
        }
        /// <summary>
        /// 設定Properties內各軟體路徑
        /// </summary>
        /// <param name="SoftwareID">軟體ID</param>
        /// <param name="softwarePath">路徑</param>
        public void SetPropertiesSoftwarePath(int SoftwareID, string softwarePath)
        {
            switch (SoftwareID)
            {
                case (int)_softwareID.EZCAD:
                    {
                        Properties.Settings.Default.cad_exePath = softwarePath;
                        break;
                    }
                case (int)_softwareID.Implant:
                    {
                        Properties.Settings.Default.implant_exePath = softwarePath;
                        break;
                    }
                case (int)_softwareID.Ortho:
                    {
                        Properties.Settings.Default.ortho_exePath = softwarePath;
                        break;
                    }
                case (int)_softwareID.Tray:
                    {
                        Properties.Settings.Default.tray_exePath = softwarePath;
                        break;
                    }
                case (int)_softwareID.Splint:
                    {
                        Properties.Settings.Default.splint_exePath = softwarePath;
                        break;
                    }
                case (int)_softwareID.Guide:
                    {
                        Properties.Settings.Default.guide_exePath = softwarePath;
                        break;
                    }
            }
            Properties.Settings.Default.Save();
        }
    }
}
