﻿using System;
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

        //委派到MainWindow.xaml.cs裡面的setSoftwareShow()
        public delegate void softwareLogoShowEventHandler(int softwareID, int currentProgress);
        public event softwareLogoShowEventHandler softwareLogoShowEvent;

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
            log = new LogRecorder();
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
                CloudSoftwareTotal = new List<SoftwareInfo>();
                HLXMLlink = "C:\\InteWare\\HL.xml";    //單機測試
                using (StreamReader oReader = new StreamReader(HLXMLlink, Encoding.GetEncoding("utf-8")))
                {
                    xDoc = XDocument.Load(oReader);
                }

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
                    softDongle.softwareDownloadLink = item.SHyperlink.Replace("\n ", "").Replace("\r ", "").Replace(" ", "");

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
                    softLicense.softwareDownloadLink = item.SHyperlink.Replace("\n ", "").Replace("\r ", "").Replace(" ", "");

                    CloudSoftwareTotal.Add(softLicense);
                }

                if(Properties.Settings.Default.engineerMode == true)
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
        /// <param name="generateVersionXml">是否重建Version.xml並讀取</param>
        /// <returns></returns>
        public void checkExistSoftware(bool generateVersionXml)
        {
            UserSoftwareTotal = new List<SoftwareInfo>();

            if (File.Exists(Properties.Settings.Default.path_EZCAD) == true)
            {
                softwareLogoShowEvent((int)_softwareID.EZCAD, (int)_softwareStatus.Installed);

                if(generateVersionXml == true)
                    RebuildVersionXML(Properties.Settings.Default.path_EZCAD);
            }
            else
            {
                softwareLogoShowEvent((int)_softwareID.EZCAD, (int)_softwareStatus.NotInstall);
            }

            if (File.Exists(Properties.Settings.Default.path_Implant) == true)
            {
                softwareLogoShowEvent((int)_softwareID.Implant, (int)_softwareStatus.Installed);

                if (generateVersionXml == true)
                    RebuildVersionXML(Properties.Settings.Default.path_Implant);
            }
            else
            {
                softwareLogoShowEvent((int)_softwareID.Implant, (int)_softwareStatus.NotInstall);
            }

            if (File.Exists(Properties.Settings.Default.path_Ortho) == true)
            {
                softwareLogoShowEvent((int)_softwareID.Ortho, (int)_softwareStatus.Installed);

                if (generateVersionXml == true)
                    RebuildVersionXML(Properties.Settings.Default.path_Ortho);
            }
            else
            {
                softwareLogoShowEvent((int)_softwareID.Ortho, (int)_softwareStatus.NotInstall);
            }

            if (File.Exists(Properties.Settings.Default.path_Tray) == true)
            {
                softwareLogoShowEvent((int)_softwareID.Tray, (int)_softwareStatus.Installed);

                if (generateVersionXml == true)
                    RebuildVersionXML(Properties.Settings.Default.path_Tray);
            }
            else
            {
                softwareLogoShowEvent((int)_softwareID.Tray, (int)_softwareStatus.NotInstall);
            }

            if (File.Exists(Properties.Settings.Default.path_Splint) == true)
            {
                softwareLogoShowEvent((int)_softwareID.Splint, (int)_softwareStatus.Installed);

                if (generateVersionXml == true)
                    RebuildVersionXML(Properties.Settings.Default.path_Splint);
            }
            else
            {
                softwareLogoShowEvent((int)_softwareID.Splint, (int)_softwareStatus.NotInstall);
            }

            if (File.Exists(Properties.Settings.Default.path_Guide) == true)
            {
                if (generateVersionXml == true)
                    RebuildVersionXML(Properties.Settings.Default.path_Guide);
            }

            if (generateVersionXml == true)
                SoftwareInfoLog(UserSoftwareTotal, "UserSoftwareTotal");
        }

        /// <summary>
        /// 重新輸出Version.xml
        /// </summary>
        /// <param name="softwarePath">軟體執行檔路徑</param>
        /// <returns></returns>
        private void RebuildVersionXML(string softwarePath)
        {
            if (softwarePath == "")
                return;

            //先刪除原先的Version.xml檔
            if (File.Exists(Path.GetDirectoryName(softwarePath) + "Version.xml") == true)
            {
                File.Delete(Path.GetDirectoryName(softwarePath) + @"\Version.xml");
                log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "RebuildVersionXML()", Path.GetDirectoryName(softwarePath) + "Version.xml deleted.");
            }

            //cmdLine生出Version.xml
            FileVersionInfo myFileVersionInfo;
            string arguments = "-v";
            Process processer;

            if (softwarePath.ToLower().IndexOf("ortho") != -1)  //Ortho
            {
                myFileVersionInfo = FileVersionInfo.GetVersionInfo(softwarePath);
                if (haveNewVersion(myFileVersionInfo.FileVersion, "3.1.20325.0") == false)
                {
                    processer = new Process();
                    processer.StartInfo.FileName = softwarePath;
                    processer.StartInfo.Arguments = arguments;
                    processer.Start();
                }
                else
                {
                    log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "RebuildVersionXML()", "Ortho is Old version, can't output version.xml");
                }
            }
            else if(softwarePath.ToLower().IndexOf("implant") != -1)    //ImplantPlanning
            {
                myFileVersionInfo = FileVersionInfo.GetVersionInfo(softwarePath);
                if (haveNewVersion(myFileVersionInfo.FileVersion, "2.1.2.0") == false)
                { 
                    processer = new Process();
                    processer.StartInfo.FileName = softwarePath;
                    processer.StartInfo.Arguments = arguments;
                    processer.Start();
                }
                else
                {
                    log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "RebuildVersionXML()", "ImplantPlanning is Old version, can't output version.xml");
                }
            }
            else if (softwarePath.ToLower().IndexOf("tray") != -1 || softwarePath.ToLower().IndexOf("splint") != -1)
            {
                //Tray、Splint版本號一樣
                myFileVersionInfo = FileVersionInfo.GetVersionInfo(softwarePath);
                if (haveNewVersion(myFileVersionInfo.FileVersion, "1.0.20325.0") == false)
                {
                    processer = new Process();
                    processer.StartInfo.FileName = softwarePath;
                    processer.StartInfo.Arguments = arguments;
                    processer.Start();
                }
                else
                {
                    log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "RebuildVersionXML()", Path.GetFileNameWithoutExtension(softwarePath) + " is Old version, can't output version.xml");
                }
            }
            else if(softwarePath.ToLower().IndexOf("cad") != -1 || softwarePath.ToLower().IndexOf("guide") != -1)
            {
                //EZCAD、Guide版本號一樣
                myFileVersionInfo = FileVersionInfo.GetVersionInfo(softwarePath);
                if (haveNewVersion(myFileVersionInfo.FileVersion, "2.1.20325.0") == false)
                {
                    processer = new Process();
                    processer.StartInfo.FileName = softwarePath;
                    processer.StartInfo.Arguments = arguments;
                    processer.Start();
                }
                else
                {
                    log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "RebuildVersionXML()", Path.GetFileNameWithoutExtension(softwarePath) + " is Old version, can't output version.xml");
                }
            }
            else
            {
                //舊版
                SoftwareInfo installedSoftwareInfo = new SoftwareInfo();
                if (softwarePath.ToLower().IndexOf("ortho") != -1)
                    installedSoftwareInfo.softwareID = (int)_softwareID.Ortho;
                else if (softwarePath.ToLower().IndexOf("implant") != -1)
                    installedSoftwareInfo.softwareID = (int)_softwareID.Implant;
                else if (softwarePath.ToLower().IndexOf("tray") != -1)
                    installedSoftwareInfo.softwareID = (int)_softwareID.Tray;
                else if (softwarePath.ToLower().IndexOf("splint") != -1)
                    installedSoftwareInfo.softwareID = (int)_softwareID.Splint;
                else if (softwarePath.ToLower().IndexOf("guide") != -1)
                    installedSoftwareInfo.softwareID = (int)_softwareID.Guide;
                else if (softwarePath.ToLower().IndexOf("cad") != -1)
                    installedSoftwareInfo.softwareID = (int)_softwareID.EZCAD;
                else
                    return;

                installedSoftwareInfo.softwareInstalled = (int)_softwareStatus.Installed;
                installedSoftwareInfo.softwareName = Path.GetFileNameWithoutExtension(softwarePath);
                myFileVersionInfo = FileVersionInfo.GetVersionInfo(softwarePath);
                installedSoftwareInfo.softwareVersion = myFileVersionInfo.FileVersion;
                installedSoftwareInfo.softwareLicense = (int)_softwareLic.NotSure;

                UserSoftwareTotal.Add(installedSoftwareInfo);
                return;
            }

            //取得Version.xml內容
            if(File.Exists(Path.GetDirectoryName(softwarePath) + @"\Version.xml") == true)
            {
                try
                {
                    XDocument xDoc;
                    using (StreamReader oReader = new StreamReader(Path.GetDirectoryName(softwarePath) + @"\Version.xml", Encoding.GetEncoding("utf-8")))
                    {
                        xDoc = XDocument.Load(oReader);
                    }

                    var installedSoftwareInfoCollection = from q in xDoc.Descendants("SoftwareInfo")
                             select new
                             {
                                 SName = q.Descendants("Name").First().Value,
                                 SVersion = q.Descendants("Version").First().Value,
                                 SType = q.Descendants("Type").First().Value
                             };

                    foreach (var item in installedSoftwareInfoCollection)
                    {
                        SoftwareInfo installedSoftwareInfo = new SoftwareInfo();
                        if (item.SName.ToLower().IndexOf("ortho") != -1)
                            installedSoftwareInfo.softwareID = (int)_softwareID.Ortho;
                        else if (item.SName.ToLower().IndexOf("implant") != -1)
                            installedSoftwareInfo.softwareID = (int)_softwareID.Implant;
                        else if (item.SName.ToLower().IndexOf("tray") != -1)
                            installedSoftwareInfo.softwareID = (int)_softwareID.Tray;
                        else if (item.SName.ToLower().IndexOf("splint") != -1)
                            installedSoftwareInfo.softwareID = (int)_softwareID.Splint;
                        else if (item.SName.ToLower().IndexOf("guide") != -1)
                            installedSoftwareInfo.softwareID = (int)_softwareID.Guide;
                        else if (item.SName.ToLower().IndexOf("cad") != -1)
                            installedSoftwareInfo.softwareID = (int)_softwareID.EZCAD;
                        else
                            break;

                        installedSoftwareInfo.softwareInstalled = (int)_softwareStatus.Installed;
                        installedSoftwareInfo.softwareName = item.SName;
                        installedSoftwareInfo.softwareVersion = item.SVersion;
                        if (item.SType.ToLower() == "license")
                            installedSoftwareInfo.softwareLicense = (int)_softwareLic.License;
                        else if (item.SType.ToLower() == "dongle")
                            installedSoftwareInfo.softwareLicense = (int)_softwareLic.Dongle;

                        UserSoftwareTotal.Add(installedSoftwareInfo);
                    }
                }
                catch (Exception ex)
                {
                    log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "GetVersion.xml_Exception", ex.Message);
                }
            }
            else
            {
                log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "RebuildVersionXML()", softwarePath + " not old version but can't generate Version.xml");
                //給1秒等待軟體生出Version.xml
            }
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
                log.RecordLogContinue(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "softwareLicense", "\"" + Enum.GetName(typeof(_softwareLic), outputInfo[i].softwareLicense) + "\" " + outputInfo[i].softwareLicense.ToString());
                log.RecordLogContinue(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "softwareName", outputInfo[i].softwareName);
                log.RecordLogContinue(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "softwareSize", outputInfo[i].softwareSize.ToString());
                log.RecordLogContinue(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "softwareVersion", outputInfo[i].softwareVersion);
                log.RecordLogContinue(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "softwareDownloadLink", outputInfo[i].softwareDownloadLink);
                log.RecordLogSaperate();
            }
        }
    }
}