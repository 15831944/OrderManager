﻿using Ionic.Zip;
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
//C# 本進程執行完畢後再執行下一線程: https://www.itread01.com/content/1542194542.html
namespace OrderManagerLauncher
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class Launcher : Window
    {
        string HLXMLlink = @"https://inteware.com.tw//updateXML//Inteware_om.xml";//newOM.xml網址
        //string HLXMLlink = @"https://inteware.com.tw//updateXML//newOM_Developer.xml";//newOM_Developer.xml網址
        static public bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {   // 總是接受
            return true;
        }
        BackgroundWorker BgWorker_Main;
        BackgroundWorker OrderManagerFunc_BackgroundWorker;
        string DownloadFileName;
        NewOMInfo omInfo, updateInfo;
        bool UpdateOM_Main = false;//OrderManager含Launcher的封裝包

        CountdownEvent latch = new CountdownEvent(1);
        private void RefreshData(CountdownEvent latch)
        {
            latch.Signal();
        }

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
            bool FullRecord = false;

            string systemName = System.Globalization.CultureInfo.CurrentCulture.Name; // 取得電腦語系

            try
            {
                if (systemName == "zh-TW")
                {
                    LocalizationService.SetLanguage("zh-TW");
                }
                else
                {
                    LocalizationService.SetLanguage("en-US");
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            string[] args = Environment.GetCommandLineArgs();
            if (args != null && args.Length > 1)
            {
                foreach (string argument in args)
                {
                    if (argument == "-Rec") //完整記錄模式
                        FullRecord = true;
                    else if (argument == "-VerChk")
                    {
                        foreach(string item in args)
                        {
                            if(item == "-Rec")
                            {
                                FullRecord = true;
                                break;
                            }
                        }
                        if(FullRecord == true)
                            JumpIntoOrderEXE(true);
                        else
                            JumpIntoOrderEXE(false);
                    }
                    else if (argument == "-NeedUpdate")
                    {
                        Properties.Settings.Default.NeedUpdate = true;
                        Properties.Settings.Default.Save();
                    }
                    else if (argument == "-zhTW")
                    {
                        SetLanguage("zh-TW");
                    }
                    else if (argument == "-eng")
                    {
                        SetLanguage("en-US");
                    }
                }
            }

            if (Properties.Settings.Default.NeedUpdate == false) //直接跳過檢查更新
            {
                JumpIntoOrderEXE(false);
            }

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
                updateInfo = new NewOMInfo();
                xDoc = XDocument.Load(HLXMLlink);

                var OrderManagerInfo = from q in xDoc.Descendants("DownloadLink").Descendants("OrderManager")
                                 select new
                                 {
                                    m_Version = q.Descendants("Version").First().Value,
                                    m_HyperLink = q.Descendants("HyperLink").First().Value,
                                 };

                var UpdateInfo = from q in xDoc.Descendants("DownloadLink").Descendants("Updates")
                                       select new
                                       {
                                           m2_Version = q.Descendants("Version").First().Value,
                                           m2_HyperLink = q.Descendants("HyperLink").First().Value,
                                       };

                foreach (var item in OrderManagerInfo)
                {
                    omInfo.VersionFromWeb = new Version(item.m_Version);
                    omInfo.DownloadLink = item.m_HyperLink.Replace("\n ", "").Replace("\r ", "").Replace(" ", ""); ;
                }
                foreach (var item in UpdateInfo)
                {
                    updateInfo.VersionFromWeb = new Version(item.m2_Version);
                    updateInfo.DownloadLink = item.m2_HyperLink.Replace("\n ", "").Replace("\r ", "").Replace(" ", ""); ;
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                Inteware_Messagebox Msg = new Inteware_Messagebox();
                Msg.ShowMessage(TranslationSource.Instance["CannotGetnewOMXML"] + TranslationSource.Instance["Contact"]);
                JumpIntoOrderEXE(false);
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
            bool GoUpdate = false;
            try
            {
                FileVersionInfo verInfo;
                verInfo = FileVersionInfo.GetVersionInfo("Order Manager.exe");
                if(omInfo.VersionFromWeb > new Version(verInfo.FileVersion))
                {
                    GoUpdate = true;
                    UpdateOM_Main = true;
                }
                else if(omInfo.VersionFromWeb == new Version(verInfo.FileVersion) && updateInfo.VersionFromWeb > new Version(verInfo.FileVersion))
                {
                    GoUpdate = true;
                    UpdateOM_Main = false;
                }
            }
            catch (Exception ex)
            {
                Inteware_Messagebox Msg = new Inteware_Messagebox();
                Msg.ShowMessage(ex.Message, "UpdateCheck error");
                GoUpdate = true;
            }

            if (GoUpdate == false) //不用更新
            {
                JumpIntoOrderEXE(false);
            }
            else  //進入更新
            {
                DialogUpdateCheck DlgUpdateChk = new DialogUpdateCheck
                {
                    Owner = this
                };
                DlgUpdateChk.ShowDialog();
                if (DlgUpdateChk.DialogResult == true)   //進入更新
                {
                    if (DlgUpdateChk.NoAutoChk == true)
                        Properties.Settings.Default.NeedUpdate = false;
                    else
                        Properties.Settings.Default.NeedUpdate = true;

                    Properties.Settings.Default.Save();
                    RunCommandLine("Order Manager.exe", "-ExportProps");//匯出Properties
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
                else  //不更新
                {
                    if (DlgUpdateChk.NoAutoChk == true)
                        Properties.Settings.Default.NeedUpdate = false;
                    else
                        Properties.Settings.Default.NeedUpdate = true;

                    Properties.Settings.Default.Save();
                    JumpIntoOrderEXE(false);
                }
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

                string downloadLink;
                if(UpdateOM_Main == true)
                {
                    downloadLink = omInfo.DownloadLink;
                }
                else
                {
                    downloadLink = updateInfo.DownloadLink;
                }

                //Request資料
                HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(downloadLink);
                httpRequest.Credentials = CredentialCache.DefaultCredentials;
                httpRequest.UserAgent = ".NET Framework Example Client";
                httpRequest.Method = "GET";
                
                HttpWebResponse httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                try
                {
                    if (((HttpWebResponse)httpResponse).StatusDescription == "OK" && httpResponse.ContentLength > 1)
                    {
                        // 取得下載的檔名
                        Uri uri = new Uri(downloadLink);
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
                        JumpIntoOrderEXE(false);
                    }
                }
                catch
                {
                    Inteware_Messagebox Msg = new Inteware_Messagebox();
                    Msg.ShowMessage(TranslationSource.Instance["DownloadingError"] + TranslationSource.Instance["Contact"]);
                    JumpIntoOrderEXE(false);
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
                //RunCommandLine(DownloadFileName, "");
                Process processer = new Process();
                processer.StartInfo.FileName = DownloadFileName;
                processer.Start();

                Thread.Sleep(2000);
                Environment.Exit(0);
            }
            else
            {
                JumpIntoOrderEXE(false);
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
                JumpIntoOrderEXE(false);
            }
        }
        void CompletedWork_Unpacking(object sender, RunWorkerCompletedEventArgs e)
        {
            if (File.Exists(DownloadFileName) == true)
                File.Delete(DownloadFileName);
            JumpIntoOrderEXE(false);
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

        void JumpIntoOrderEXE(bool record)
        {
            latch = new CountdownEvent(1);
            Thread thread = new Thread(() =>
            {
                RunCommandLine("Order Manager.exe", "-VerChk");
                RefreshData(latch);
            });
            thread.Start();
            latch.Wait();
            Thread.Sleep(5000);
            Environment.Exit(0);
        }

        /// <summary>
        /// 設定語言
        /// </summary>
        /// <param name="lang">語言</param>
        public void SetLanguage(string lang)
        {
            if(lang == "zh-TW")
                LocalizationService.SetLanguage("zh-TW");
            else
                LocalizationService.SetLanguage("en-US");
        }
    }
}
