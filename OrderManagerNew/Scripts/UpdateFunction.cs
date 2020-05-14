using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;                                  //XDocument用
using System.Net;                                   //跳過網路檢查
using System.Net.Security;                          //跳過網路檢查
using System.Security.Cryptography.X509Certificates;//跳過網路檢查
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace OrderManagerNew
{
    public class UpdateFunction
    {
        #region 變數宣告
        //string HLXMLlink = @"https://inteware.com.tw/updateXML/HLnoLic.xml";//HL.xml網址
        string HLXMLlink = "D:\\Inteware\\HLnoLic.xml";    //單機測試//TODO之後要換到網上
        string downloadfilepath;
        LogRecorder log;    //日誌檔cs
        BackgroundWorker bgWorker_Download;        //申明後臺物件
        /// <summary>
        /// 準備要安裝的軟體Info
        /// </summary>
        public SoftwareInfo readyInstallSoftwareInfo;

        /// <summary>
        /// 委派到MainWindow.xaml.cs裡面的setSoftwareShow()
        /// </summary>
        /// <param name="softwareID">(軟體ID) 請參考_softwareID</param>
        /// <param name="currentProgress">(目前進度) 未安裝、下載中... 請參考_SoftwareStatus</param>
        /// <param name="downloadPercent">(下載百分比) 100%的值為1.00</param>
        public delegate void softwareLogoShowEventHandler(int softwareID, int currentProgress, double downloadPercent);
        public event softwareLogoShowEventHandler softwareLogoShowEvent;
        /// <summary>
        /// 委派到MainWindow.xaml.cs裡面的SnackBarShow(string)
        /// </summary>
        /// <param name="message">顯示訊息</param>
        public delegate void updatefuncEventHandler_snackbar(string message);
        public event updatefuncEventHandler_snackbar Handler_snackbarShow;
        
        public List<SoftwareInfo> CloudSoftwareTotal { get; set; }  //軟體最新版清單
        #endregion
        public class SoftwareInfo
        {
            public int softwareID;              //參考EnumSummary的_softwareID
            public int softwareInstalled;       //參考EnumSummary的_softwareStatus
            public int softwareLicense;         //參考EnumSummary的_softwareLic
            public double softwareSize;          //軟體大小(以MB計算)
            public string softwareName;         //軟體名稱
            public string softwareVersion;      //軟體版本
            public string softwarePath;         //軟體路徑
            public string softwareDownloadLink; //軟體下載網址

            public SoftwareInfo()
            {
                softwareID = -1;
                softwareInstalled = -1;
                softwareLicense = -1;
                softwareName = "";
                softwareVersion = "";
                softwarePath = "";
                softwareSize = 0;
                softwareDownloadLink = "";
            }
        }

        static public bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {   // 總是接受
            return true;
        }

        public UpdateFunction()
        {
            log = new LogRecorder();
            log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "UpdateFunction.cs", "Initial Start");
            CloudSoftwareTotal = new List<SoftwareInfo>();
            readyInstallSoftwareInfo = new SoftwareInfo();
            downloadfilepath = "";
        }
        
        /// <summary>
        /// 有新版本回傳True
        /// </summary>
        /// <param name="CurrentVersion">現有版本</param>
        /// <param name="latestVersion">最新版本</param>
        /// <returns></returns>
        private bool haveNewVersion(string currentVersion, string latestVersion)
        {
            if (currentVersion == "")
                return true;

            //版本樣子 1.2.3.4
            string[] List_currentVersion = currentVersion.Split('.');
            string[] List_latestVersion = latestVersion.Split('.');

            //如果版本樣子是1.2.3則要自動更改為1.2.3.0
            if (List_currentVersion.Length == 3)
            {
                currentVersion += ".0";
                List_currentVersion = currentVersion.Split('.');
            }

            if (List_latestVersion.Length == 3)
            {
                latestVersion += ".0";
                List_latestVersion = latestVersion.Split('.');
            }
            
            try
            {
                if (currentVersion.ToLower().Replace(" ", "") == "oldversion")
                    return true;

                if (currentVersion == "" || List_currentVersion.Length < 3)
                    return false;

                //判斷[0]
                if (Convert.ToInt16(List_latestVersion[0]) > Convert.ToInt16(List_currentVersion[0]))
                    return true;
                else if (Convert.ToInt16(List_currentVersion[0]) > Convert.ToInt16(List_latestVersion[0]))
                    return false;

                //判斷[1]
                if (Convert.ToInt16(List_latestVersion[1]) > Convert.ToInt16(List_currentVersion[1]))
                    return true;
                else if (Convert.ToInt16(List_currentVersion[1]) > Convert.ToInt16(List_latestVersion[1]))
                    return false;

                //判斷[2]
                if (Convert.ToInt16(List_latestVersion[2]) > Convert.ToInt16(List_currentVersion[2]))
                    return true;
                else if (Convert.ToInt16(List_currentVersion[2]) > Convert.ToInt16(List_latestVersion[2]))
                    return false;

                //判斷[3]
                if (Convert.ToInt16(List_latestVersion[3]) > Convert.ToInt16(List_currentVersion[3]))
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "Exception", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Log檔記錄 軟體Info清單內容
        /// </summary>
        /// <param name="outputInfo">List<SoftwareInfo> 軟體Info清單</param>
        /// <param name="InfoName">軟體Info清單名稱</param>
        /// <returns></returns>
        private void SoftwareInfoLog(List<SoftwareInfo> outputInfo, string InfoName)
        {
            log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "UpdateFunction.cs SoftwareInfoLog()", InfoName + " Total:" + outputInfo.Count);
            for(int i=0; i<outputInfo.Count; i++)
            {
                log.RecordLogContinue(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "SoftwareID", "\"" + Enum.GetName(typeof(_softwareID), outputInfo[i].softwareID) + "\" " + outputInfo[i].softwareID.ToString());
                log.RecordLogContinue(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "softwareInstalled", "\"" + Enum.GetName(typeof(_softwareStatus), outputInfo[i].softwareInstalled) + "\" " + outputInfo[i].softwareInstalled.ToString());
                log.RecordLogContinue(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "softwareName", outputInfo[i].softwareName);
                log.RecordLogContinue(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "softwareSize", outputInfo[i].softwareSize.ToString());
                log.RecordLogContinue(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "softwareVersion", outputInfo[i].softwareVersion);
                log.RecordLogContinue(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "softwareDownloadLink", outputInfo[i].softwareDownloadLink);
                log.RecordLogSaperate();
            }
        }

        #region 多執行緒處理下載軟體
        public void StartDownloadSoftware()
        {
            if (readyInstallSoftwareInfo.softwareID == -1)
            {
                Handler_snackbarShow("No SoftwareID");    //NoSoftwareID停止下載//TODO多國語系
                return;
            }

            //開始下載多執行緒
            bgWorker_Download = new BackgroundWorker();
            bgWorker_Download.DoWork += new DoWorkEventHandler(DoWork_DownloadSoftware);
            bgWorker_Download.ProgressChanged += new ProgressChangedEventHandler(UpdateProgress_DownloadSoftware);
            bgWorker_Download.RunWorkerCompleted += new RunWorkerCompletedEventHandler(CompletedWork_DownloadSoftware);
            bgWorker_Download.WorkerReportsProgress = true;
            bgWorker_Download.WorkerSupportsCancellation = true;
            bgWorker_Download.RunWorkerAsync(this);
        }

        void DoWork_DownloadSoftware(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bw = sender as BackgroundWorker;

            //跳過https檢測 & Win7 相容
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            System.Net.ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
            
            //Request資料
            HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(readyInstallSoftwareInfo.softwareDownloadLink);
            httpRequest.Credentials = CredentialCache.DefaultCredentials;
            httpRequest.UserAgent = ".NET Framework Example Client";
            httpRequest.Method = "GET";

            Handler_snackbarShow("Get httpRequest Response Start..."); //開始取得資料 //TODO 多國語系
            HttpWebResponse httpResponse = (HttpWebResponse)httpRequest.GetResponse();

            try
            {
                if (((HttpWebResponse)httpResponse).StatusDescription == "OK" && httpResponse.ContentLength > 1)
                {
                    //覆蓋軟體大小，以直接抓檔案內容最準
                    readyInstallSoftwareInfo.softwareSize = Convert.ToDouble(Math.Round(((double)(Int64)httpResponse.ContentLength / 1024.0 / 1024.0), 1));
                    
                    if (Directory.Exists(Properties.Settings.Default.DownloadFolder) == false)
                    {
                        Properties.Settings.Default.DownloadFolder = System.IO.Path.GetTempPath() + "IntewareTempFile\\";
                        Properties.Settings.Default.Save();
                        System.IO.Directory.CreateDirectory(Properties.Settings.Default.DownloadFolder);
                    }
                        
                    // 取得下載的檔名
                    Uri uri = new Uri(readyInstallSoftwareInfo.softwareDownloadLink);
                    downloadfilepath = Properties.Settings.Default.DownloadFolder + @"\" + System.IO.Path.GetFileName(uri.LocalPath);
                    Stream netStream = httpResponse.GetResponseStream();
                    Stream fileStream = new FileStream(downloadfilepath, FileMode.Create);
                    byte[] read = new byte[1024];
                    long progressBarValue = 0;
                    int realReadLen = netStream.Read(read, 0, read.Length);

                    while (realReadLen > 0)
                    {
                        fileStream.Write(read, 0, realReadLen);
                        //realReadLen 是一個封包大小，progressBarValue會一直累加
                        progressBarValue += realReadLen;
                        double percent = (double)progressBarValue / (double)httpResponse.ContentLength;
                        bw.ReportProgress(Convert.ToInt32(percent*100));
                        realReadLen = netStream.Read(read, 0, read.Length);
                    }
                    netStream.Close();
                    fileStream.Close();
                }
                else
                {

                }
                httpResponse.Close();
            }
            catch (Exception ex)
            {
                Handler_snackbarShow(ex.Message);   //網路連線異常or載點掛掉 //TODO 多國語系
                e.Cancel = true;
            }
        }

        void UpdateProgress_DownloadSoftware(object sender, ProgressChangedEventArgs e)
        {
            int progress = e.ProgressPercentage;
            softwareLogoShowEvent(readyInstallSoftwareInfo.softwareID, (int)_softwareStatus.Downloading, (double)(progress/100.0));
        }

        void CompletedWork_DownloadSoftware(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Handler_snackbarShow("Error");   //錯誤 //TODO 多國語系
            }
            else if (e.Cancelled)
            {
                Handler_snackbarShow("Canceled");    //取消 //TODO 多國語系
            }
            else
            {
                Handler_snackbarShow("下載完成");   //下載完成  //TODO 多國語系
                softwareLogoShowEvent(readyInstallSoftwareInfo.softwareID, (int)_softwareStatus.Installing, 0);
                string downloadPath = GetSoftwarePath(readyInstallSoftwareInfo.softwareID);

                string param = "/quiet APPDIR=\"" + downloadPath + "\"";
                RunCommandLine(downloadfilepath, param);
            }
        }
        #endregion

        /// <summary>
        /// 回傳軟體路徑
        /// </summary>
        /// <param name="SoftwareID">軟體ID</param>
        /// <returns></returns>
        public string GetSoftwarePath(int SoftwareID)
        {
            switch (SoftwareID)
            {
                case (int)_softwareID.EZCAD:
                    return Properties.Settings.Default.cad_exePath;
                case (int)_softwareID.Implant:
                    return Properties.Settings.Default.implant_exePath;
                case (int)_softwareID.Ortho:
                    return Properties.Settings.Default.ortho_exePath;
                case (int)_softwareID.Tray:
                    return Properties.Settings.Default.tray_exePath;
                case (int)_softwareID.Splint:
                    return Properties.Settings.Default.splint_exePath;
                case (int)_softwareID.Guide:
                    return Properties.Settings.Default.guide_exePath;
                default:
                    return "";
            }
        }

        /// <summary>
        /// 讀取HL.xml的詳細更新資訊
        /// </summary>
        public void loadHLXml()
        {
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            System.Net.ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);

            XDocument xDoc;
            try
            {
                log.RecordLogContinue(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "UpdateFunction.cs", "load HL.xml Start");
                CloudSoftwareTotal = new List<SoftwareInfo>();
                xDoc = XDocument.Load(HLXMLlink);

                var SoftwareHL = from q in xDoc.Descendants("Software").Descendants("Item")
                                   select new
                                   {
                                       SName = q.Descendants("SoftwareName").First().Value,
                                       SVersion = q.Descendants("LatestVersion").First().Value,
                                       SHyperlink = q.Descendants("HyperLink").First().Value,
                                       SDescription = q.Descendants("Description").First().Value,
                                   };

                var OthersHL = from q in xDoc.Descendants("Others").Descendants("Item")
                               select new
                               {
                                   OSupportSoftwareName = q.Descendants("SupportSoftware").First().Value,
                                   OProduct = q.Descendants("product").First().Value,
                                   OVersion = q.Descendants("LatestVersion").First().Value,
                                   OHyperlink = q.Descendants("HyperLink").First().Value,
                                   ODescription = q.Descendants("Description").First().Value,
                                   OSize = q.Descendants("Size").First().Value
                               };

                foreach (var item in SoftwareHL)
                {
                    SoftwareInfo softLicense = new SoftwareInfo();
                    if (item.SName.ToLower().IndexOf("ortho") != -1)
                        softLicense.softwareID = (int)_softwareID.Ortho;
                    else if (item.SName.ToLower().IndexOf("implant") != -1)
                        softLicense.softwareID = (int)_softwareID.Implant;
                    else if (item.SName.ToLower().IndexOf("tray") != -1)
                        softLicense.softwareID = (int)_softwareID.Tray;
                    else if (item.SName.ToLower().IndexOf("splint") != -1)
                        softLicense.softwareID = (int)_softwareID.Splint;
                    else if (item.SName.ToLower().IndexOf("guide") != -1)
                        softLicense.softwareID = (int)_softwareID.Guide;
                    else if (item.SName.ToLower().IndexOf("cad") != -1)
                        softLicense.softwareID = (int)_softwareID.EZCAD;
                    else
                        break;

                    softLicense.softwareInstalled = (int)_softwareStatus.Cloud;
                    softLicense.softwareName = item.SName;
                    softLicense.softwareVersion = item.SVersion;
                    softLicense.softwareDownloadLink = item.SHyperlink.Replace("\n ", "").Replace("\r ", "").Replace(" ", "");

                    CloudSoftwareTotal.Add(softLicense);
                }

                if (Properties.Settings.Default.engineerMode == true)
                    SoftwareInfoLog(CloudSoftwareTotal, "CloudSoftwareTotal");
            }
            catch (Exception ex)
            {
                log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "UpdateFunction.cs Initial exception", ex.Message);
            }
        }
        
        /// <summary>
        /// CommandLine(命令提示字元)
        /// </summary>
        /// <param name="fileName">要開啟的檔案</param>
        /// <param name="arguments">要傳進去的參數</param>
        public void RunCommandLine(string fileName, string arguments)
        {
            try
            {
                Process processer = new Process();
                processer.StartInfo.FileName = fileName;
                if(arguments != "")
                    processer.StartInfo.Arguments = arguments;
                processer.Start();
            }
            catch(Exception ex)
            {
                Handler_snackbarShow(ex.Message);
                log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "RunCommandLine exception", ex.Message);
            }
        }
    }
}
