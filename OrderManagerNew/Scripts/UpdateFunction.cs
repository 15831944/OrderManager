using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;                                  //XDocument用
using System.Net;                                   //跳過網路檢查
using System.Net.Security;                          //跳過網路檢查
using System.Security.Cryptography.X509Certificates;//跳過網路檢查
using System.Xml;
using System.Xml.Linq;

namespace OrderManagerNew
{
    public class UpdateFunction
    {
        #region 變數宣告
        readonly string HLXMLlink = @"https://inteware.com.tw/updateXML/PrintIn_om.xml";//HL.xml網址
        //string HLXMLlink = "D:\\IntewareInc\\HLnoLic.xml";    //單機測試
        string downloadfilepath;
        LogRecorder log;    //日誌檔cs
        BackgroundWorker bgWorker_Download;        //申明後臺物件
        /// <summary>
        /// 準備要安裝的軟體Info
        /// </summary>
        public SoftwareInfo readyInstallSoftwareInfo;
        /// <summary>
        /// 準備要解除安裝的軟體Info
        /// </summary>
        public SoftwareInfo readyUninstallSoftwareInfo;

        /// <summary>
        /// 委派到MainWindow.xaml.cs裡面的setSoftwareShow()
        /// </summary>
        /// <param name="softwareID">(軟體ID) 請參考_softwareID</param>
        /// <param name="currentProgress">(目前進度) 未安裝、下載中... 請參考_SoftwareStatus</param>
        /// <param name="downloadPercent">(下載百分比) 100%的值為1.00</param>
        public delegate void softwareLogoShowEventHandler(int softwareID, int currentProgress, double downloadPercent);
        public event softwareLogoShowEventHandler SoftwareLogoShowEvent;
        /// <summary>
        /// 委派到MainWindow.xaml.cs裡面的SnackBarShow(string)
        /// </summary>
        /// <param name="message">顯示訊息</param>
        public delegate void updatefuncEventHandler_snackbar(string message);
        public event updatefuncEventHandler_snackbar Handler_snackbarShow;
        /// <summary>
        /// 設定各單機軟體的軟體更新Button的isEnable狀態
        /// </summary>
        /// <param name="SoftwareID">參考_softwareID</param>
        /// <param name="canUpdate">isEnable開關</param>
        /// <param name="SoftwareVersion">最新軟體版本號</param>
        public delegate void softwareUpdateStatusHandler(int SoftwareID, bool canUpdate, string SoftwareVersion);
        public event softwareUpdateStatusHandler SoftwareUpdateEvent;

        public List<SoftwareInfo> CloudSoftwareTotal { get; set; }  //軟體最新版清單
        #endregion
        public class SoftwareInfo
        {
            public int softwareID;              //參考EnumSummary的_softwareID
            public int softwareInstalled;       //參考EnumSummary的_softwareStatus
            public int softwareLicense;         //參考EnumSummary的_softwareLic
            public double softwareSize;          //軟體大小(以MB計算)
            public string softwareName;         //軟體名稱
            public Version softwareVersion;      //軟體版本
            public string softwarePath;         //軟體路徑
            public string softwareDownloadLink; //軟體下載網址

            public SoftwareInfo()
            {
                softwareID = -1;
                softwareInstalled = -1;
                softwareLicense = -1;
                softwareName = "";
                softwareVersion = new Version();
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
            readyUninstallSoftwareInfo = new SoftwareInfo();
            downloadfilepath = "";
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
                log.RecordLogContinue(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "softwareVersion", outputInfo[i].softwareVersion.ToString());
                log.RecordLogContinue(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "softwareDownloadLink", outputInfo[i].softwareDownloadLink);
                log.RecordLogSaperate();
            }
        }
#region 多執行緒處理下載軟體
        public void StartDownloadSoftware()
        {
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
            if(sender is BackgroundWorker)
            {
                BackgroundWorker bw = sender as BackgroundWorker;

                //跳過https檢測 & Win7 相容
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);

                //Request資料
                HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(readyInstallSoftwareInfo.softwareDownloadLink);
                httpRequest.Credentials = CredentialCache.DefaultCredentials;
                httpRequest.UserAgent = ".NET Framework Example Client";
                httpRequest.Method = "GET";

                Handler_snackbarShow(TranslationSource.Instance["ReceivingData"]); //開始取得資料
                HttpWebResponse httpResponse = (HttpWebResponse)httpRequest.GetResponse();

                try
                {
                    if (((HttpWebResponse)httpResponse).StatusDescription == "OK" && httpResponse.ContentLength > 1)
                    {
                        //覆蓋軟體大小，以直接抓檔案內容最準
                        readyInstallSoftwareInfo.softwareSize = Convert.ToDouble(Math.Round(((double)(Int64)httpResponse.ContentLength / 1024.0 / 1024.0), 1));

                        if (Directory.Exists(Properties.Settings.Default.DownloadFolder) == false)
                        {
                            Properties.Settings.Default.DownloadFolder = Path.GetTempPath() + "IntewareTempFile\\";
                            Properties.Settings.Default.Save();
                            Directory.CreateDirectory(Properties.Settings.Default.DownloadFolder);
                        }

                        // 取得下載的檔名
                        Uri uri = new Uri(readyInstallSoftwareInfo.softwareDownloadLink);
                        downloadfilepath = Properties.Settings.Default.DownloadFolder + @"\" + Path.GetFileName(uri.LocalPath);
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
                            bw.ReportProgress(Convert.ToInt32(percent * 100));
                            realReadLen = netStream.Read(read, 0, read.Length);
                        }
                        fileStream.Close();
                        netStream.Close();
                    }
                    else
                    {

                    }
                    httpResponse.Close();
                }
                catch (Exception ex)
                {
                    Handler_snackbarShow(ex.Message);
                    e.Cancel = true;
                }
            }
        }
        void UpdateProgress_DownloadSoftware(object sender, ProgressChangedEventArgs e)
        {
            int progress = e.ProgressPercentage;
            SoftwareLogoShowEvent(readyInstallSoftwareInfo.softwareID, (int)_softwareStatus.Downloading, (double)(progress/100.0));
        }
        void CompletedWork_DownloadSoftware(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Handler_snackbarShow(TranslationSource.Instance["Download"] + TranslationSource.Instance["Error"]);
            }
            else if (e.Cancelled)
            {
                Handler_snackbarShow(TranslationSource.Instance["Download"] + TranslationSource.Instance["Cancel"]);
            }
            else
            {
                Handler_snackbarShow(TranslationSource.Instance["Order_DownloadCompleted"]);
                SoftwareLogoShowEvent(readyInstallSoftwareInfo.softwareID, (int)_softwareStatus.Installing, 0);
                string downloadPath = GetSoftwarePath(readyInstallSoftwareInfo.softwareID);

                string param = "/quiet APPDIR=\"" + downloadPath + "\"";
                OrderManagerFunctions omFunc = new OrderManagerFunctions();
                omFunc.RunCommandLine(downloadfilepath, param, true);
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
        public void LoadHLXml()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);

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
                    softLicense.softwareVersion = new Version(item.SVersion);
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
        /// OrderManagerLoaded完再開始檢查是否有更新
        /// </summary>
        public void CheckSoftwareHaveNewVersion(int TmpsoftwareID)
        {
            if (CloudSoftwareTotal.Count < 1)   //代表沒有抓到軟體更新資訊
                return;

            FileVersionInfo verInfo;

            switch(TmpsoftwareID)
            {
                case (int)_softwareID.EZCAD:
                    {
                        if (File.Exists(Properties.Settings.Default.cad_exePath) == true)
                        {
                            TmpsoftwareID = (int)_softwareID.EZCAD;
                            verInfo = FileVersionInfo.GetVersionInfo(Properties.Settings.Default.cad_exePath);
                            foreach (SoftwareInfo info in CloudSoftwareTotal)
                            {
                                if (info.softwareID != TmpsoftwareID)
                                    continue;
                                else if (info.softwareVersion > new Version(verInfo.FileVersion))
                                    SoftwareUpdateEvent(TmpsoftwareID, true, info.softwareVersion.ToString());
                            }
                        }
                        break;
                    }
                case (int)_softwareID.Implant:
                    {
                        if (File.Exists(Properties.Settings.Default.implant_exePath) == true)
                        {
                            TmpsoftwareID = (int)_softwareID.Implant;
                            verInfo = FileVersionInfo.GetVersionInfo(Properties.Settings.Default.implant_exePath);
                            foreach (SoftwareInfo info in CloudSoftwareTotal)
                            {
                                if (info.softwareID != TmpsoftwareID)
                                    continue;
                                else if (info.softwareVersion > new Version(verInfo.FileVersion))
                                    SoftwareUpdateEvent(TmpsoftwareID, true, info.softwareVersion.ToString());
                            }
                        }
                        break;
                    }
                case (int)_softwareID.Ortho:
                    {
                        if (File.Exists(Properties.Settings.Default.ortho_exePath) == true)
                        {
                            TmpsoftwareID = (int)_softwareID.Ortho;
                            verInfo = FileVersionInfo.GetVersionInfo(Properties.Settings.Default.ortho_exePath);
                            foreach (SoftwareInfo info in CloudSoftwareTotal)
                            {
                                if (info.softwareID != TmpsoftwareID)
                                    continue;
                                else if (info.softwareVersion > new Version(verInfo.FileVersion))
                                    SoftwareUpdateEvent(TmpsoftwareID, true, info.softwareVersion.ToString());
                            }
                        }
                        break;
                    }
                case (int)_softwareID.Tray:
                    {
                        if (File.Exists(Properties.Settings.Default.tray_exePath) == true)
                        {
                            TmpsoftwareID = (int)_softwareID.Tray;
                            verInfo = FileVersionInfo.GetVersionInfo(Properties.Settings.Default.tray_exePath);
                            foreach (SoftwareInfo info in CloudSoftwareTotal)
                            {
                                if (info.softwareID != TmpsoftwareID)
                                    continue;
                                else if (info.softwareVersion > new Version(verInfo.FileVersion))
                                    SoftwareUpdateEvent(TmpsoftwareID, true, info.softwareVersion.ToString());
                            }
                        }
                        break;
                    }
                case (int)_softwareID.Splint:
                    {
                        if (File.Exists(Properties.Settings.Default.splint_exePath) == true)
                        {
                            TmpsoftwareID = (int)_softwareID.Splint;
                            verInfo = FileVersionInfo.GetVersionInfo(Properties.Settings.Default.splint_exePath);
                            foreach (SoftwareInfo info in CloudSoftwareTotal)
                            {
                                if (info.softwareID != TmpsoftwareID)
                                    continue;
                                else if (info.softwareVersion > new Version(verInfo.FileVersion))
                                    SoftwareUpdateEvent(TmpsoftwareID, true, info.softwareVersion.ToString());
                            }
                        }
                        break;
                    }
                case (int)_softwareID.Guide:
                    {
                        if (File.Exists(Properties.Settings.Default.guide_exePath) == true)
                        {
                            TmpsoftwareID = (int)_softwareID.Guide;
                            verInfo = FileVersionInfo.GetVersionInfo(Properties.Settings.Default.guide_exePath);
                            foreach (SoftwareInfo info in CloudSoftwareTotal)
                            {
                                if (info.softwareID != TmpsoftwareID)
                                    continue;
                                else if (info.softwareVersion > new Version(verInfo.FileVersion))
                                    SoftwareUpdateEvent(TmpsoftwareID, true, info.softwareVersion.ToString());
                            }
                        }
                        break;
                    }
            }
        }
        /// <summary>
        /// 匯出Properties Xml
        /// </summary>
        public void ExportPropertiesXml()
        {
            XmlDocument doc = new XmlDocument();
            XmlNode rootNode = doc.CreateElement("Properties");
            doc.AppendChild(rootNode);
            XmlNode Xnode = doc.CreateElement("sysLanguage");
            Xnode.InnerText = Properties.Settings.Default.sysLanguage;
            rootNode.AppendChild(Xnode);
            Xnode = doc.CreateElement("cad_exePath");
            Xnode.InnerText = Properties.Settings.Default.cad_exePath;
            rootNode.AppendChild(Xnode);
            Xnode = doc.CreateElement("implant_exePath");
            Xnode.InnerText = Properties.Settings.Default.implant_exePath;
            rootNode.AppendChild(Xnode);
            Xnode = doc.CreateElement("ortho_exePath");
            Xnode.InnerText = Properties.Settings.Default.ortho_exePath;
            rootNode.AppendChild(Xnode);
            Xnode = doc.CreateElement("tray_exePath");
            Xnode.InnerText = Properties.Settings.Default.tray_exePath;
            rootNode.AppendChild(Xnode);
            Xnode = doc.CreateElement("splint_exePath");
            Xnode.InnerText = Properties.Settings.Default.splint_exePath;
            rootNode.AppendChild(Xnode);
            Xnode = doc.CreateElement("guide_exePath");
            Xnode.InnerText = Properties.Settings.Default.guide_exePath;
            rootNode.AppendChild(Xnode);
            Xnode = doc.CreateElement("DownloadFolder");
            Xnode.InnerText = Properties.Settings.Default.DownloadFolder;
            rootNode.AppendChild(Xnode);
            Xnode = doc.CreateElement("PingTime");
            Xnode.InnerText = Properties.Settings.Default.PingTime.ToString();
            rootNode.AppendChild(Xnode);
            Xnode = doc.CreateElement("LastSoftwareFilter");
            Xnode.InnerText = Properties.Settings.Default.LastSoftwareFilter.ToString();
            rootNode.AppendChild(Xnode);
            Xnode = doc.CreateElement("AirdentalAcc");
            Xnode.InnerText = Properties.Settings.Default.AirdentalAcc;
            rootNode.AppendChild(Xnode);
            Xnode = doc.CreateElement("AirdentalCookie");
            Xnode.InnerText = Properties.Settings.Default.AirdentalCookie;
            rootNode.AppendChild(Xnode);
            Xnode = doc.CreateElement("showCloudOrderNumbers");
            Xnode.InnerText = Properties.Settings.Default.showCloudOrderNumbers.ToString();
            rootNode.AppendChild(Xnode);
            doc.Save("OrderManagerProps.xml");
        }
        /// <summary>
        /// 匯入Properties
        /// </summary>
        public void ImportPropertiesXml()
        {
            try
            {
                XDocument doc = new XDocument();
                doc = XDocument.Load("OrderManagerProps.xml");
                var props = from q in doc.Descendants("Properties")
                            select new
                            {
                                m_sysLang = q.Descendants("sysLanguage").First().Value,
                                m_cad_exePath = q.Descendants("cad_exePath").First().Value,
                                m_implant_exePath = q.Descendants("implant_exePath").First().Value,
                                m_ortho_exePath = q.Descendants("ortho_exePath").First().Value,
                                m_tray_exePath = q.Descendants("tray_exePath").First().Value,
                                m_splint_exePath = q.Descendants("splint_exePath").First().Value,
                                m_guide_exePath = q.Descendants("guide_exePath").First().Value,
                                m_DownloadFolder = q.Descendants("DownloadFolder").First().Value,
                                m_PingTime = q.Descendants("PingTime").First().Value,
                                m_LastSoftwareFilter = q.Descendants("LastSoftwareFilter").First().Value,
                                m_AirdentalAcc = q.Descendants("AirdentalAcc").First().Value,
                                m_AirdentalCookie = q.Descendants("AirdentalCookie").First().Value,
                                m_showCloudOrderNumbers = q.Descendants("showCloudOrderNumbers").First().Value
                            };
                foreach(var item in props)
                {
                    Properties.Settings.Default.sysLanguage = item.m_sysLang;
                    Properties.Settings.Default.cad_exePath = item.m_cad_exePath;
                    Properties.Settings.Default.implant_exePath = item.m_implant_exePath;
                    Properties.Settings.Default.ortho_exePath = item.m_ortho_exePath;
                    Properties.Settings.Default.tray_exePath = item.m_tray_exePath;
                    Properties.Settings.Default.splint_exePath = item.m_splint_exePath;
                    Properties.Settings.Default.guide_exePath = item.m_guide_exePath;
                    Properties.Settings.Default.DownloadFolder = item.m_DownloadFolder;
                    Properties.Settings.Default.PingTime = int.Parse(item.m_PingTime);
                    Properties.Settings.Default.LastSoftwareFilter = int.Parse(item.m_LastSoftwareFilter);
                    Properties.Settings.Default.AirdentalAcc = item.m_AirdentalAcc;
                    Properties.Settings.Default.AirdentalCookie = item.m_AirdentalCookie;
                    Properties.Settings.Default.showCloudOrderNumbers = int.Parse(item.m_showCloudOrderNumbers);
                    Properties.Settings.Default.Save();
                }
                try
                {
                    doc = null;
                    File.Delete("OrderManagerProps.xml");
                }
                catch { }
                
            }
            catch
            {
                return;
            }
        }
    }
}
