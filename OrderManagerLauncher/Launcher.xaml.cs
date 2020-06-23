using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Windows;

namespace OrderManagerLauncher
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class Launcher : Window
    {
        static public bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {   // 總是接受
            return true;
        }
        BackgroundWorker BgWorker_Main;
        BackgroundWorker OrderManagerFunc_BackgroundWorker;

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

            string systemName = System.Globalization.CultureInfo.CurrentCulture.Name; // 取得電腦語系
            if (systemName == "zh-TW")
                LocalizationService.SetLanguage("zh-TW");
            else
                LocalizationService.SetLanguage("en-US");
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
            int counter = 0;
            while(counter < 2)
            {
                Thread.Sleep(1000);
                counter++;
            }
        }

        void CompletedWork_UpdateCheck(object sender, RunWorkerCompletedEventArgs e)
        {
            RunCommandLine("OrderManager.exe", "-ExportProps");
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

        void DoWork_Download(object sender, DoWorkEventArgs e)
        {
            if (sender is BackgroundWorker)
            {
                //跳過https檢測 & Win7 相容
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);

                try
                {
                    int counter = 1;
                    while (counter <100)
                    {
                        double percent = (double)counter / (double)100;
                        ((BackgroundWorker)sender).ReportProgress(Convert.ToInt32(percent * 100));
                        Thread.Sleep(20);
                        counter++;
                    }

                    /*string fileUrl = Properties.OrderManagerProps.Default.AirDentalAPI + @"project/ortho/download/" + OrderInfo._Key;
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
                    response.Close();*/
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
                progressbar_update.Value = progress;
            }));
        }
        void CompletedWork_Download(object sender, RunWorkerCompletedEventArgs e)
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

        void DoWork_Unpacking(object sender, DoWorkEventArgs e)
        {
            int counter = 0;
            while (counter < 2)
            {
                Thread.Sleep(1000);
                counter++;
            }
        }
        void CompletedWork_Unpacking(object sender, RunWorkerCompletedEventArgs e)
        {
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
                MessageBox.Show(ex.Message);
            }
        }
        void CompletedWork_Cmd(object sender, RunWorkerCompletedEventArgs e)
        {
            OrderManagerFunc_BackgroundWorker = new BackgroundWorker();
        }
    }
}
