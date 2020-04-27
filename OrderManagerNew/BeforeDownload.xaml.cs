using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using Path = System.IO.Path;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.ComponentModel;
using System.Timers;  // 名稱空間是 Timers 而不是 Threading

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

        private BackgroundWorker m_BackgroundWorker;//申明後臺物件
        HttpWebResponse httpResponse;               //共用同一個WebResponse以便清除內部殘留資料
        string http_url;                            //下載網址
        Timer tmr;                                  //計時器(用在GetResponse)
        int currentSoftwareID;                      //軟體ID

        public BeforeDownload()
        {
            InitializeComponent();
            http_url = "";
            currentSoftwareID = -1;
        }

        private void TitleBar_Click_titlebarButtons(object sender, RoutedEventArgs e)
        {
            Button titleButton = sender as Button;

            switch (titleButton.Name)
            {
                case "systemButton_Close":              //關閉
                    Close();
                    break;
            }
        }

        private void Click_OpenFilePath(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    textbox_InstallPath.Text = dialog.SelectedPath;

                    RemainingSpace(dialog.SelectedPath);
                }
            }
        }

        private void Click_systemButton(object sender, RoutedEventArgs e)
        {
            Button Btn = sender as Button;
            switch(Btn.Name)
            {
                case "sysBtn_Yes":
                    {
                        this.DialogResult = true;
                        break;
                    }
                case "sysBtn_Cancel":
                    {
                        this.DialogResult = false;
                        break;
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
            if (Directory.Exists(Drive) == false)
            {
                System.IO.Directory.CreateDirectory(Drive);
            }

            ulong FreeBytesAvailable;
            ulong TotalNumberOfBytes;
            ulong TotalNumberOfFreeBytes;
            string str = Path.GetPathRoot(Drive);
            bool success = GetDiskFreeSpaceEx(Path.GetPathRoot(Drive), out FreeBytesAvailable, out TotalNumberOfBytes, out TotalNumberOfFreeBytes);

            if (!success)
            {
                MessageBox.Show("Can't get space! try use adnim mode"); //TODO 多國語系
                return false;
            }

            label_AvailableSpace.Content = convertDiskUnit(TotalNumberOfFreeBytes);
            return true;
        }

        /// <summary>
        /// 轉換容量單位
        /// </summary>
        /// <param name="InputBytes">單位是Bytes 使用ulong</param>
        /// <returns></returns>
        string convertDiskUnit(ulong InputBytes)
        {
            string OutputString = "";

            if (((double)(Int64)InputBytes / 1024.0 / 1024.0) < 1.0)//低於1MB就用KB
                OutputString = Convert.ToSingle(Math.Round(((double)(Int64)InputBytes / 1024.0), 1)).ToString() + "KB";
            else if (((double)(Int64)InputBytes / 1024.0 / 1024.0 / 1024.0) < 1.0)//低於1GB就用MB
                OutputString = Convert.ToSingle(Math.Round(((double)(Int64)InputBytes / 1024.0 / 1024.0), 1)).ToString() + "MB";
            else if (((double)(Int64)InputBytes / 1024.0 / 1024.0 / 1024.0 / 1024.0) < 1.0)//低於1TB就用GB
                OutputString = Convert.ToSingle(Math.Round(((double)(Int64)InputBytes / 1024.0 / 1024.0 / 1024.0), 1)).ToString() + "GB";
            else
                OutputString = Convert.ToSingle(Math.Round(((double)(Int64)InputBytes / 1024.0 / 1024.0 / 1024.0 / 1024.0), 1)).ToString() + "TB";

            return OutputString;
        }

        /// <summary>
        /// 設定UI所顯示的資訊
        /// </summary>
        public bool SetInformation()
        {
            if (currentSoftwareID == -1 || http_url == "")
            {
                MessageBox.Show("SoftwareID is -1");
                return false;
            }
                
            string[] SoftwareNameArray = new string[6] {"EZCAD", "ImplantPlanning", "OrthoAnalysis", "EZCAD Tray", "EZCAD Splint", "EZCAD Guide"};

            label_TitleBar.Content = OrderManagerNew.TranslationSource.Instance["Install"] + "-" + SoftwareNameArray[currentSoftwareID].Replace(" ", ".");
            label_Header.Content = OrderManagerNew.TranslationSource.Instance["AboutToInstall"] + " " + SoftwareNameArray[currentSoftwareID].Replace(" ", ".");
            textbox_InstallPath.Text = @"C:\InteWare\" + SoftwareNameArray[currentSoftwareID];
            
            try
            {
                if (((HttpWebResponse)httpResponse).StatusDescription == "OK" && httpResponse.ContentLength > 1)
                {
                    // 取得下載的檔名
                    Uri uri = new Uri(http_url);
                    string downloadfileRealName = System.IO.Path.GetFileName(uri.LocalPath);
                    
                    if (RemainingSpace(textbox_InstallPath.Text) == true)  //客戶電腦剩餘空間
                        label_RequireSpace.Content = convertDiskUnit(Convert.ToUInt64(httpResponse.ContentLength));
                    else
                    {
                        MessageBox.Show("Can't get user remaining space");//無法獲取客戶電腦剩餘空間 //TODO 多國語系
                        httpResponse.Close();
                        return false;
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Network error");   //網路連線異常or載點掛掉 //TODO 多國語系
                httpResponse.Close();
                return false;
            }
            httpResponse.Close();
            return true;
        }

        void DoWork(object sender, DoWorkEventArgs e)
        {

            //倒數計時3秒
            tmr = new Timer();
            if(Properties.Settings.Default.PingTime != 0)
                tmr.Interval = Properties.Settings.Default.PingTime * 1000;
            else
            {
                tmr.Interval = 5000;
                Properties.Settings.Default.PingTime = 5;
                Properties.Settings.Default.Save();
            }
                

            tmr.Elapsed += tmr_Elapsed;  // 使用事件代替委託
            tmr.Start();          // 重啟定時器

            //跳過https檢測 & Win7 相容
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            System.Net.ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);

            //Request資料
            HttpWebRequest httpRequest;
            httpRequest = (HttpWebRequest)WebRequest.Create(http_url);
            httpRequest.Credentials = CredentialCache.DefaultCredentials;
            httpRequest.UserAgent = ".NET Framework Example Client";
            httpRequest.Method = "GET";

            try
            {
                Handler_snackbarShow("Get httpRequest Response Start..."); //開始取得資料 //TODO 多國語系
                httpResponse = (HttpWebResponse)httpRequest.GetResponse();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Network error");   //網路連線異常or載點掛掉 //TODO 多國語系
                e.Cancel = true;
            }
        }

        void tmr_Elapsed(object sender, EventArgs e)
        {
            httpResponse.Close();
            tmr.Dispose();
            Handler_snackbarShow("can't get network response, please restart ordermanager and try again"); //超過5秒回應時間 //TODO 多國語系
            DialogResult = false;
        }

        void CompletedWork(object sender, RunWorkerCompletedEventArgs e)
        {
            SetHttpResponseOK();
            if (e.Error != null)
            {
                tmr.Dispose();
                MessageBox.Show("Error");   //錯誤 //TODO 多國語系
            }
            else if (e.Cancelled)
            {
                tmr.Dispose();
                MessageBox.Show("Canceled");    //取消 //TODO 多國語系
            }
            else
            {
                tmr.Dispose();
            }
        }

        /// <summary>
        /// 用多執行緒去取得httpResponse
        /// </summary>
        /// <param name="http_url">下載網址</param>
        /// <param name="SoftwareID">軟體ID(參考EnumSummary的_softwareID)</param>
        /// <returns></returns>
        public void GethttpResoponse(string Import_http_url, int SoftwareID)
        {
            http_url = Import_http_url;
            currentSoftwareID = SoftwareID;

            m_BackgroundWorker = new BackgroundWorker(); // 例項化後臺物件

            m_BackgroundWorker.WorkerReportsProgress = false; // 設定可以通告進度
            m_BackgroundWorker.WorkerSupportsCancellation = true; // 設定可以取消

            m_BackgroundWorker.DoWork += new DoWorkEventHandler(DoWork);
            m_BackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(CompletedWork);

            m_BackgroundWorker.RunWorkerAsync(this);
        }
    }
}
