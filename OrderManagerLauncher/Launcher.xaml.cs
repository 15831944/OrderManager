using Ionic.Zip;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Windows;
using System.Xml.Linq;
using UIDialogs;

namespace OrderManagerLauncher
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class Launcher : Window
    {
        string HLXMLlink = @"https://inteware.com.tw/updateXML/PrintIn_om.xml";//newOM.xml網址
        static public bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {   // 總是接受
            return true;
        }
        BackgroundWorker BgWorker_Main;
        BackgroundWorker OrderManagerFunc_BackgroundWorker;
        bool NeedUpdate;
        string DownloadFileName;
        NewOMInfo omInfo;
        class NewOMInfo
        {
            public Version VersionFromWeb;
            public string DownloadLink;

            public NewOMInfo()
            {
                VersionFromWeb = new Version();
                DownloadLink = "";
            }
        }
        class BackgroundArgs
        {
            public string FileName { get; set; }
            public string Arguments { get; set; }

            public BackgroundArgs()
            {
                FileName = "";
                Arguments = "";
            }
        }

        public Launcher()
        {
            InitializeComponent();
            NeedUpdate = false;
            string systemName = System.Globalization.CultureInfo.CurrentCulture.Name; // 取得電腦語系
            if (systemName == "zh-TW")
                LocalizationService.SetLanguage("zh-TW");
            else
                LocalizationService.SetLanguage("en-US");
            
        }
        /// <summary>
        /// 讀取HL.xml的詳細更新資訊
        /// </summary>
        public void LoadHLXml()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);

            XDocument xDoc;
            try
            {
                omInfo = new NewOMInfo();
                xDoc = XDocument.Load(HLXMLlink);

                var OrderManagerInfo = from q in xDoc.Descendants("DownloadLink").Descendants("OrderManager")
                                 select new
                                 {
                                    m_Version = q.Descendants("Version").First().Value,
                                    m_HyperLink = q.Descendants("HyperLink").First().Value,
                                 };

                foreach (var item in OrderManagerInfo)
                {
                    omInfo.VersionFromWeb = new Version(item.m_Version);
                    omInfo.DownloadLink = item.m_HyperLink.Replace("\n ", "").Replace("\r ", "").Replace(" ", ""); ;
                }
            }
            catch
            {
                Inteware_Messagebox Msg = new Inteware_Messagebox();
                Msg.ShowMessage(TranslationSource.Instance["CannotGetnewOMXML"] + TranslationSource.Instance["Contact"]);
                RunCommandLine("OrderManager.exe", "-VerChk");
                Environment.Exit(0);
            }
        }
        private void Loaded_Launcher(object sender, RoutedEventArgs e)
        {
            BgWorker_Main = new BackgroundWorker();
            BgWorker_Main.DoWork += new DoWorkEventHandler(DoWork_UpdateCheck);
            BgWorker_Main.RunWorkerCompleted += new RunWorkerCompletedEventHandler(CompletedWork_UpdateCheck);
            BgWorker_Main.WorkerReportsProgress = false;
            BgWorker_Main.WorkerSupportsCancellation = false;
            BgWorker_Main.RunWorkerAsync();
        }

        void DoWork_UpdateCheck(object sender, DoWorkEventArgs e)
        {
            //檢查是否有更新
            LoadHLXml();
        }

        void CompletedWork_UpdateCheck(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                FileVersionInfo verInfo;
                verInfo = FileVersionInfo.GetVersionInfo("OrderManager.exe");
                if(omInfo.VersionFromWeb > new Version(verInfo.FileVersion))
                    NeedUpdate = true;
            }
            catch
            {
                NeedUpdate = true;
            }

            if (NeedUpdate == false) //不用更新
            {
                RunCommandLine("OrderManager.exe", "-VerChk");
                Environment.Exit(0);
            }
            else  //進入更新
            {
                RunCommandLine("OrderManager.exe", "-ExportProps");//匯出Properties
                progressbar_update.IsIndeterminate = false;
                label_describe.Content = TranslationSource.Instance["Downloading"];
                BgWorker_Main = new BackgroundWorker();
                BgWorker_Main.DoWork += new DoWorkEventHandler(DoWork_Download);
                BgWorker_Main.ProgressChanged += new ProgressChangedEventHandler(UpdateProgress_Download);
                BgWorker_Main.RunWorkerCompleted += new RunWorkerCompletedEventHandler(CompletedWork_Download);
                BgWorker_Main.WorkerReportsProgress = true;
                BgWorker_Main.WorkerSupportsCancellation = false;
                BgWorker_Main.RunWorkerAsync();
            }
        }

        void DoWork_Download(object sender, DoWorkEventArgs e)
        {
            if (sender is BackgroundWorker)
            {
                BackgroundWorker bw = sender as BackgroundWorker;

                //跳過https檢測 & Win7 相容
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);

                //Request資料
                HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(omInfo.DownloadLink);
                httpRequest.Credentials = CredentialCache.DefaultCredentials;
                httpRequest.UserAgent = ".NET Framework Example Client";
                httpRequest.Method = "GET";
                
                HttpWebResponse httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                try
                {
                    if (((HttpWebResponse)httpResponse).StatusDescription == "OK" && httpResponse.ContentLength > 1)
                    {
                        // 取得下載的檔名
                        Uri uri = new Uri(omInfo.DownloadLink);
                        DownloadFileName = Path.GetFileName(uri.LocalPath);
                        if (File.Exists(DownloadFileName) == true)
                            File.Delete(DownloadFileName);
                        Stream netStream = httpResponse.GetResponseStream();
                        Stream fileStream = new FileStream(DownloadFileName, FileMode.Create);
                        byte[] read = new byte[1024];
                        long progressBarValue = 0;
                        int realReadLen = netStream.Read(read, 0, read.Length);

                        while (realReadLen > 0)
                        {
                            fileStream.Write(read, 0, realReadLen);
                            progressBarValue += realReadLen;
                            double percent = (double)progressBarValue / (double)httpResponse.ContentLength;
                            bw.ReportProgress(Convert.ToInt32(percent * 100));
                            realReadLen = netStream.Read(read, 0, read.Length);
                        }
                        fileStream.Close();
                        netStream.Close();
                        httpResponse.Close();
                    }
                    else
                    {
                        httpResponse.Close();
                        Inteware_Messagebox Msg = new Inteware_Messagebox();
                        Msg.ShowMessage(TranslationSource.Instance["CannotDownloadOM"] + TranslationSource.Instance["Contact"]);
                        RunCommandLine("OrderManager.exe", "-VerChk");
                        Environment.Exit(0);
                    }
                }
                catch
                {
                    Inteware_Messagebox Msg = new Inteware_Messagebox();
                    Msg.ShowMessage(TranslationSource.Instance["DownloadingError"] + TranslationSource.Instance["Contact"]);
                    RunCommandLine("OrderManager.exe", "-VerChk");
                    Environment.Exit(0);
                }
            }
        }
        void UpdateProgress_Download(object sender, ProgressChangedEventArgs e)
        {
            int progress = e.ProgressPercentage;
            this.Dispatcher.Invoke((Action)(() =>
            {
                progressbar_update.Value = progress;
            }));
        }
        void CompletedWork_Download(object sender, RunWorkerCompletedEventArgs e)
        {
            if(Path.GetExtension(DownloadFileName) == ".zip")
            {
                label_describe.Content = TranslationSource.Instance["Updating"];
                progressbar_update.IsIndeterminate = true;
                BgWorker_Main = new BackgroundWorker();
                BgWorker_Main.DoWork += new DoWorkEventHandler(DoWork_Unpacking);
                BgWorker_Main.RunWorkerCompleted += new RunWorkerCompletedEventHandler(CompletedWork_Unpacking);
                BgWorker_Main.WorkerReportsProgress = false;
                BgWorker_Main.WorkerSupportsCancellation = false;
                BgWorker_Main.RunWorkerAsync();
            }
            else if(Path.GetExtension(DownloadFileName) == ".exe")
            {
                RunCommandLine(DownloadFileName, "");
                Thread.Sleep(1000);
                Environment.Exit(0);
            }
            else
            {
                RunCommandLine("OrderManager.exe", "-VerChk");
                Thread.Sleep(1000);
                Environment.Exit(0);
            }
        }

        void DoWork_Unpacking(object sender, DoWorkEventArgs e)
        {
            try
            {
                //解壓縮
                using (var zip = ZipFile.Read(DownloadFileName))
                {
                    foreach (var zipEntry in zip)
                    {
                        zipEntry.Extract(System.Environment.CurrentDirectory, ExtractExistingFileAction.OverwriteSilently);//解壓縮到同一個資料夾
                    }
                }
            }
            catch
            {
                Inteware_Messagebox Msg = new Inteware_Messagebox();
                Msg.ShowMessage(TranslationSource.Instance["UnpackingError"] + TranslationSource.Instance["Contact"]);
                RunCommandLine("OrderManager.exe", "-VerChk");
                Environment.Exit(0);
            }
        }
        void CompletedWork_Unpacking(object sender, RunWorkerCompletedEventArgs e)
        {
            if (File.Exists(DownloadFileName) == true)
                File.Delete(DownloadFileName);
            RunCommandLine("OrderManager.exe", "-VerChk");
            Environment.Exit(0);
        }

        /// <summary>
        /// CommandLine(命令提示字元)
        /// </summary>
        /// <param name="fileName">要開啟的檔案</param>
        /// <param name="arguments">要傳進去的參數</param>
        public void RunCommandLine(string fileName, string arguments)
        {
            OrderManagerFunc_BackgroundWorker = new BackgroundWorker();
            OrderManagerFunc_BackgroundWorker.DoWork += new DoWorkEventHandler(DoWork_Cmd);
            OrderManagerFunc_BackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(CompletedWork_Cmd);
            BackgroundArgs bgArgs = new BackgroundArgs
            {
                FileName = fileName,
                Arguments = arguments
            };
            OrderManagerFunc_BackgroundWorker.RunWorkerAsync(bgArgs);
        }
        void DoWork_Cmd(object sender, DoWorkEventArgs e)
        {
            try
            {
                if (e.Argument is BackgroundArgs)
                {
                    Process processer = new Process();
                    processer.StartInfo.FileName = ((BackgroundArgs)e.Argument).FileName;
                    if (((BackgroundArgs)e.Argument).Arguments != "")
                        processer.StartInfo.Arguments = ((BackgroundArgs)e.Argument).Arguments;
                    processer.Start();
                }
            }
            catch (Exception ex)
            {
                Inteware_Messagebox Msg = new Inteware_Messagebox();
                Msg.ShowMessage(ex.Message);
            }
        }
        void CompletedWork_Cmd(object sender, RunWorkerCompletedEventArgs e)
        {
            OrderManagerFunc_BackgroundWorker = new BackgroundWorker();
        }
    }
}
