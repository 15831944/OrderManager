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
    class UpdateFunction
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

            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            System.Net.ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);

            
        }

        /// <summary>
        /// 讀取HL.xml的詳細更新資訊
        /// </summary>
        private void loadHLXml()
        {
            XDocument xDoc;
            try
            {
                log.RecordLogContinue(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "UpdateFunction.cs", "load HL.xml Start");
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
                }

                /*foreach (var item in SoftwareHL_License)
                {
                    SoftwareInfo tmpSHL = new SoftwareInfo();
                    tmpSHL.SoftwareName = item.SName;
                    tmpSHL.SoftwareVersion = item.SVersion;
                    tmpSHL.SoftwareType = 0;
                    tmpSHL.SoftwareHyperLink = item.SHyperlink.Replace("\n ", "").Replace("\r ", "").Replace(" ", "");
                    tmpSHL.SoftwareDescription = item.SDescription;
                    tmpSHL.SoftwareSize = float.Parse(item.SSize);
                    TotalSHL.Add(tmpSHL);
                }*/
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

    }
}
