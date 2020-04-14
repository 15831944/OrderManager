using System;
using System.Collections.Generic;
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
        LogRecorder log;//日誌檔cs
        string HLXMLlink = @"https://inteware.com.tw/updateXML/HL.xml";//HL.xml網址
        List<SoftwareInfo> UserSoftwareTotal;
        List<SoftwareInfo> CloudSoftwareTotal;

        public class SoftwareInfo
        {
            public int softwareID;              //參考EnumSummary的_softwareID
            public int softwareInstalled;       //參考EnumSummary的_softwareStatus
            public int softwareLicense;         //參考EnumSummary的_softwareLic
            public float softwareSize;          //軟體大小
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
                softwareSize = 0f;
                softwareDownloadLink = "";
            }
        }

        static public bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {   // 總是接受
            return true;
        }

        public UpdateFunction()
        {
            log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "UpdateFunction.cs", "Initial Start");
            CloudSoftwareTotal = new List<SoftwareInfo>();
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
                HLXMLlink = "C:\\InteWare\\HL.xml";    //單機測試
                xDoc = XDocument.Load(HLXMLlink);

                var SoftwareHL_Dongle = from q in xDoc.Descendants("Software").Descendants("Dongle").Descendants("Item")
                                   select new
                                   {
                                       SName = q.Descendants("SoftwareName").First().Value,
                                       SVersion = q.Descendants("LatestVersion").First().Value,
                                       SHyperlink = q.Descendants("HyperLink").First().Value,
                                       SDescription = q.Descendants("Description").First().Value,
                                       SSize = q.Descendants("Size").First().Value,
                                   };

                var SoftwareHL_License = from q in xDoc.Descendants("Software").Descendants("License").Descendants("Item")
                                   select new
                                   {
                                       SName = q.Descendants("SoftwareName").First().Value,
                                       SVersion = q.Descendants("LatestVersion").First().Value,
                                       SHyperlink = q.Descendants("HyperLink").First().Value,
                                       SDescription = q.Descendants("Description").First().Value,
                                       SSize = q.Descendants("Size").First().Value,
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

                foreach (var item in SoftwareHL_Dongle)
                {
                    SoftwareInfo softDongle = new SoftwareInfo();
                    if (item.SName.ToLower().IndexOf("ortho") != -1)
                        softDongle.softwareID = (int)_softwareID.Ortho;
                    else if (item.SName.ToLower().IndexOf("implant") != -1)
                        softDongle.softwareID = (int)_softwareID.Implant;
                    else if (item.SName.ToLower().IndexOf("tray") != -1)
                        softDongle.softwareID = (int)_softwareID.Tray;
                    else if (item.SName.ToLower().IndexOf("splint") != -1)
                        softDongle.softwareID = (int)_softwareID.Splint;
                    else if (item.SName.ToLower().IndexOf("guide") != -1)
                        softDongle.softwareID = (int)_softwareID.Guide;
                    else if (item.SName.ToLower().IndexOf("cad") != -1)
                        softDongle.softwareID = (int)_softwareID.EZCAD;
                    else
                        break;

                    softDongle.softwareInstalled = (int)_softwareStatus.Cloud;
                    softDongle.softwareLicense = (int)_softwareLic.Dongle;
                    softDongle.softwareName = item.SName;
                    softDongle.softwareSize = float.Parse(item.SSize);
                    softDongle.softwareVersion = item.SVersion;
                    softDongle.softwareDownloadLink = item.SHyperlink;

                    CloudSoftwareTotal.Add(softDongle);
                }

                foreach (var item in SoftwareHL_License)
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
                    softLicense.softwareLicense = (int)_softwareLic.License;
                    softLicense.softwareName = item.SName;
                    softLicense.softwareSize = float.Parse(item.SSize);
                    softLicense.softwareVersion = item.SVersion;
                    softLicense.softwareDownloadLink = item.SHyperlink;

                    CloudSoftwareTotal.Add(softLicense);
                }

                SoftwareInfoLog(CloudSoftwareTotal, "CloudSoftwareTotal");
            }
            catch (Exception ex)
            {
                log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "UpdateFunction.cs Initial exception", ex.Message);
            }
        }

        /// <summary>
        /// 檢查使用者安裝哪些軟體
        /// </summary>
        public void checkExistSoftware()
        {
            UserSoftwareTotal = new List<SoftwareInfo>();
            SoftwareInfo ClientSofteware = new SoftwareInfo();

            if (File.Exists(Properties.Settings.Default.path_EZCAD) == true)
            {

            }

        }

        private void SoftwareInfoLog(List<SoftwareInfo> outputInfo, string InfoName)
        {
            log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "UpdateFunction.cs SoftwareInfoLog()", InfoName + " Total:" + outputInfo.Count);
            for(int i=0; i<outputInfo.Count; i++)
            {
                log.RecordLogContinue(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "SoftwareID", outputInfo[i].softwareID.ToString());
                log.RecordLogContinue(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "softwareInstalled", outputInfo[i].softwareInstalled.ToString());
                log.RecordLogContinue(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "softwareLicense", outputInfo[i].softwareLicense.ToString());
                log.RecordLogContinue(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "softwareName", outputInfo[i].softwareName);
                log.RecordLogContinue(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "softwareSize", outputInfo[i].softwareSize.ToString());
                log.RecordLogContinue(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "softwareVersion", outputInfo[i].softwareVersion);
                log.RecordLogContinue(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "softwareDownloadLink", outputInfo[i].softwareDownloadLink);
                log.RecordLogSaperate();
            }
        }
    }
}
