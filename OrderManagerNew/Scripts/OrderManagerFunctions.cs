﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace OrderManagerNew
{
    public class OrderManagerFunctions
    {
        LogRecorder log;

        /// <summary>
        /// 委派到MainWindow.xaml.cs裡面的setSoftwareShow()
        /// </summary>
        /// <param name="softwareID">(軟體ID) 請參考_softwareID</param>
        /// <param name="currentProgress">(目前進度) 未安裝、下載中... 請參考_softwareStatus</param>
        /// <param name="downloadPercent">(下載百分比) 100%的值為1.00</param>
        public delegate void softwareLogoShowEventHandler(int softwareID, int currentProgress, double downloadPercent);
        public event softwareLogoShowEventHandler SoftwareLogoShowEvent;

        /// <summary>
        /// 委派到MainWindow.xaml.cs裡面的SnackBarShow(string)
        /// </summary>
        /// <param name="message">顯示訊息</param>
        public delegate void updatefuncEventHandler_snackbar(string message);
        public event updatefuncEventHandler_snackbar Handler_snackbarShow;

        public OrderManagerFunctions()
        {
            log = new LogRecorder();
        }

        class DiskSoftwareInfo
        {
            public string DiskName { get; set; }
            public int SoftwareCount { get; set; }

            public DiskSoftwareInfo()
            {
                DiskName = "";
                SoftwareCount = 0;
            }
        }

        /// <summary>
        /// 資料夾容量大小
        /// </summary>
        /// <param name="d">創建用 new DirectoryInfo("路徑")</param>
        /// <returns></returns>
        public long DirSize(DirectoryInfo d)
        {
            long size = 0;
            // Add file sizes.
            FileInfo[] fis = d.GetFiles();
            foreach (FileInfo fi in fis)
            {
                size += fi.Length;
            }
            // Add subdirectory sizes.
            DirectoryInfo[] dis = d.GetDirectories();
            foreach (DirectoryInfo di in dis)
            {
                size += DirSize(di);
            }
            return size;
        }

        /// <summary>
        /// 再次檢查軟體執行檔是否存在，exePath路徑不會生
        /// </summary>
        public void DoubleCheckEXEexist()
        {
            if(File.Exists(Properties.Settings.Default.cad_exePath) == true)
            {
                SoftwareLogoShowEvent((int)_softwareID.EZCAD, (int)_softwareStatus.Installed, 0.0);
            }
            else
            {
                Properties.Settings.Default.cad_exePath = "";
                SoftwareLogoShowEvent((int)_softwareID.EZCAD, (int)_softwareStatus.NotInstall, 0.0);
            }
            if (File.Exists(Properties.Settings.Default.implant_exePath) == true)
            {
                SoftwareLogoShowEvent((int)_softwareID.Implant, (int)_softwareStatus.Installed, 0.0);
            }
            else
            {
                Properties.Settings.Default.implant_exePath = "";
                SoftwareLogoShowEvent((int)_softwareID.Implant, (int)_softwareStatus.NotInstall, 0.0);
            }
            if (File.Exists(Properties.Settings.Default.ortho_exePath) == true)
            {
                SoftwareLogoShowEvent((int)_softwareID.Ortho, (int)_softwareStatus.Installed, 0.0);
            }
            else
            {
                Properties.Settings.Default.ortho_exePath = "";
                SoftwareLogoShowEvent((int)_softwareID.Ortho, (int)_softwareStatus.NotInstall, 0.0);
            }
            if (File.Exists(Properties.Settings.Default.tray_exePath) == true)
            {
                SoftwareLogoShowEvent((int)_softwareID.Tray, (int)_softwareStatus.Installed, 0.0);
            }
            else
            {
                Properties.Settings.Default.tray_exePath = "";
                SoftwareLogoShowEvent((int)_softwareID.Tray, (int)_softwareStatus.NotInstall, 0.0);
            }
            if (File.Exists(Properties.Settings.Default.splint_exePath) == true)
            {
                SoftwareLogoShowEvent((int)_softwareID.Splint, (int)_softwareStatus.Installed, 0.0);
            }
            else
            {
                Properties.Settings.Default.splint_exePath = "";
                SoftwareLogoShowEvent((int)_softwareID.Splint, (int)_softwareStatus.NotInstall, 0.0);
            }
            if (File.Exists(Properties.Settings.Default.guide_exePath) == true)
            {
                SoftwareLogoShowEvent((int)_softwareID.Guide, (int)_softwareStatus.Installed, 0.0);
            }
            else
            {
                Properties.Settings.Default.guide_exePath = "";
                SoftwareLogoShowEvent((int)_softwareID.Guide, (int)_softwareStatus.NotInstall, 0.0);
            }

            AutoDetectSoftwareProjectPath();
        }

        /// <summary>
        /// 自動檢測軟體執行檔路徑(偵測並寫入exePath)並把最常用的磁碟存入 Properties.Settings.Default.mostsoftwareDisk
        /// </summary>
        /// <param name="classfrom">哪個class呼叫的，參考 _classFrom</param>
        public void AutoDetectEXE(int classfrom)
        {
            log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "OrderManagerFunctions.cs AutoDetectEXE()", "IntoFunction");

            string cad_exePath = Properties.Settings.Default.cad_exePath;
            string implant_exePath = Properties.Settings.Default.implant_exePath;
            string ortho_exePath = Properties.Settings.Default.ortho_exePath;
            string tray_exePath = Properties.Settings.Default.tray_exePath;
            string splint_exePath = Properties.Settings.Default.splint_exePath;
            string guide_exePath = Properties.Settings.Default.guide_exePath;
            string mostsoftwareDisk = Properties.Settings.Default.mostsoftwareDisk;
            string[] array_exePath = { cad_exePath, implant_exePath, ortho_exePath, tray_exePath, splint_exePath, guide_exePath };
            DiskSoftwareInfo disk_most = new DiskSoftwareInfo();//存最多軟體的磁碟

            //全部軟體都有安裝
            if (File.Exists(cad_exePath) == true && File.Exists(implant_exePath) == true && File.Exists(ortho_exePath) == true
                && File.Exists(tray_exePath) == true && File.Exists(splint_exePath) == true && File.Exists(guide_exePath) == true)
            {
                List<DiskSoftwareInfo> calcDiskwithSoftware = new List<DiskSoftwareInfo>();
                for(int i=0; i < array_exePath.Count(); i++)
                {
                    DiskSoftwareInfo dInfo = new DiskSoftwareInfo
                    {
                        DiskName = Path.GetPathRoot(array_exePath[i])
                    };
                    dInfo.SoftwareCount++;

                    if (i == 0)
                        calcDiskwithSoftware.Add(dInfo);
                    else
                    {
                        bool SameDisk = false;
                        for(int j=0; j<calcDiskwithSoftware.Count; j++)
                        {
                            if (calcDiskwithSoftware[j].DiskName == dInfo.DiskName)
                            {
                                calcDiskwithSoftware[j].SoftwareCount++;
                                SameDisk = true;
                            }
                        }

                        if(SameDisk == false)
                            calcDiskwithSoftware.Add(dInfo);
                    }
                }
                //統整出mostsoftwareDisk
                for(int i=0; i<calcDiskwithSoftware.Count; i++)
                {
                    if (i >= 1)
                    {
                        if (calcDiskwithSoftware[i].SoftwareCount > disk_most.SoftwareCount)
                            disk_most = calcDiskwithSoftware[i];
                    }
                    else
                        disk_most = calcDiskwithSoftware[i];
                }

                Properties.Settings.Default.mostsoftwareDisk = disk_most.DiskName;

                //已經有mostsoftwareDisk資料
                if (classfrom == (int)_classFrom.MainWindow)
                {
                    for (int i = 0; i < array_exePath.Count(); i++)
                        SoftwareLogoShowEvent(i, (int)_softwareStatus.Installed, 0.0);
                }
            }
            else
            {
                //先找已儲存到Properties內的exePath
                List<DiskSoftwareInfo> calcDiskwithSoftware2 = new List<DiskSoftwareInfo>();
                if (File.Exists(cad_exePath) == true)
                {
                    DiskSoftwareInfo dInfo = new DiskSoftwareInfo
                    {
                        DiskName = Path.GetPathRoot(cad_exePath)
                    };
                    dInfo.SoftwareCount++;
                    calcDiskwithSoftware2.Add(dInfo);

                    if (classfrom == (int)_classFrom.MainWindow)
                        SoftwareLogoShowEvent((int)_softwareID.EZCAD, (int)_softwareStatus.Installed, 0.0);
                }
                else
                {
                    Properties.Settings.Default.cad_exePath = "";
                    Properties.OrderManagerProps.Default.cad_projectDirectory = "";
                }
                if (File.Exists(implant_exePath) == true)
                {
                    DiskSoftwareInfo dInfo = new DiskSoftwareInfo
                    {
                        DiskName = Path.GetPathRoot(implant_exePath)
                    };
                    dInfo.SoftwareCount++;
                    calcDiskwithSoftware2.Add(dInfo);

                    if (classfrom == (int)_classFrom.MainWindow)
                        SoftwareLogoShowEvent((int)_softwareID.Implant, (int)_softwareStatus.Installed, 0.0);
                }
                else
                {
                    Properties.Settings.Default.implant_exePath = "";
                    Properties.OrderManagerProps.Default.implant_projectDirectory = "";
                }
                if (File.Exists(ortho_exePath) == true)
                {
                    DiskSoftwareInfo dInfo = new DiskSoftwareInfo
                    {
                        DiskName = Path.GetPathRoot(ortho_exePath)
                    };
                    dInfo.SoftwareCount++;
                    calcDiskwithSoftware2.Add(dInfo);

                    if (classfrom == (int)_classFrom.MainWindow)
                        SoftwareLogoShowEvent((int)_softwareID.Ortho, (int)_softwareStatus.Installed, 0.0);
                }
                else
                {
                    Properties.Settings.Default.ortho_exePath = "";
                    Properties.OrderManagerProps.Default.ortho_projectDirectory = "";
                }
                if (File.Exists(tray_exePath) == true)
                {
                    DiskSoftwareInfo dInfo = new DiskSoftwareInfo
                    {
                        DiskName = Path.GetPathRoot(tray_exePath)
                    };
                    dInfo.SoftwareCount++;
                    calcDiskwithSoftware2.Add(dInfo);

                    if (classfrom == (int)_classFrom.MainWindow)
                        SoftwareLogoShowEvent((int)_softwareID.Tray, (int)_softwareStatus.Installed, 0.0);
                }
                else
                {
                    Properties.Settings.Default.tray_exePath = "";
                    Properties.OrderManagerProps.Default.tray_projectDirectory = "";
                }
                if (File.Exists(splint_exePath) == true)
                {
                    DiskSoftwareInfo dInfo = new DiskSoftwareInfo
                    {
                        DiskName = Path.GetPathRoot(splint_exePath)
                    };
                    dInfo.SoftwareCount++;
                    calcDiskwithSoftware2.Add(dInfo);

                    if (classfrom == (int)_classFrom.MainWindow)
                        SoftwareLogoShowEvent((int)_softwareID.Splint, (int)_softwareStatus.Installed, 0.0);
                }
                else
                {
                    Properties.Settings.Default.splint_exePath = "";
                    Properties.OrderManagerProps.Default.splint_projectDirectory = "";
                }
                if (File.Exists(guide_exePath) == true)
                {
                    DiskSoftwareInfo dInfo = new DiskSoftwareInfo
                    {
                        DiskName = Path.GetPathRoot(guide_exePath)
                    };
                    dInfo.SoftwareCount++;
                    calcDiskwithSoftware2.Add(dInfo);

                    if (classfrom == (int)_classFrom.MainWindow)
                        SoftwareLogoShowEvent((int)_softwareID.Guide, (int)_softwareStatus.Installed, 0.0);
                }
                else
                    Properties.Settings.Default.guide_exePath = "";

                DriveInfo[] allDrives = DriveInfo.GetDrives();
                foreach (DriveInfo d in allDrives)  //檢查客戶所有磁碟
                {
                    try
                    {
                        DiskSoftwareInfo diskInfo = new DiskSoftwareInfo
                        {
                            DiskName = d.Name
                        };

                        if (File.Exists(cad_exePath) == false && File.Exists(d.Name + @"IntewareInc\EZCAD\Bin\EZCAD.exe") == true)
                        {
                            if (Properties.Settings.Default.cad_exePath == "")
                                Properties.Settings.Default.cad_exePath = d.Name + @"IntewareInc\EZCAD\Bin\EZCAD.exe";

                            if (classfrom == (int)_classFrom.MainWindow)
                                SoftwareLogoShowEvent((int)_softwareID.EZCAD, (int)_softwareStatus.Installed, 0.0);
                            diskInfo.SoftwareCount++;
                        }
                        if (File.Exists(implant_exePath) == false && File.Exists(d.Name + @"IntewareInc\ImplantPlanning\ImplantPlanning.exe") == true)
                        {
                            if (Properties.Settings.Default.implant_exePath == "")
                                Properties.Settings.Default.implant_exePath = d.Name + @"IntewareInc\ImplantPlanning\ImplantPlanning.exe";

                            if (classfrom == (int)_classFrom.MainWindow)
                                SoftwareLogoShowEvent((int)_softwareID.Implant, (int)_softwareStatus.Installed, 0.0);
                            diskInfo.SoftwareCount++;
                        }
                        if (File.Exists(ortho_exePath) == false && File.Exists(d.Name + @"IntewareInc\OrthoAnalysis\OrthoAnalysis.exe") == true)
                        {
                            if (Properties.Settings.Default.ortho_exePath == "")
                                Properties.Settings.Default.ortho_exePath = d.Name + @"IntewareInc\OrthoAnalysis\OrthoAnalysis.exe";

                            if (classfrom == (int)_classFrom.MainWindow)
                                SoftwareLogoShowEvent((int)_softwareID.Ortho, (int)_softwareStatus.Installed, 0.0);
                            diskInfo.SoftwareCount++;
                        }
                        if (File.Exists(tray_exePath) == false && File.Exists(d.Name + @"IntewareInc\EZCAD tray\Bin\EZCAD.tray.exe") == true)
                        {
                            if (Properties.Settings.Default.tray_exePath == "")
                                Properties.Settings.Default.tray_exePath = d.Name + @"IntewareInc\EZCAD tray\Bin\EZCAD.tray.exe";

                            if (classfrom == (int)_classFrom.MainWindow)
                                SoftwareLogoShowEvent((int)_softwareID.Tray, (int)_softwareStatus.Installed, 0.0);
                            diskInfo.SoftwareCount++;
                        }
                        if (File.Exists(splint_exePath) == false && File.Exists(d.Name + @"IntewareInc\EZCAD splint\Bin\EZCAD.splint.exe") == true)
                        {
                            if (Properties.Settings.Default.splint_exePath == "")
                                Properties.Settings.Default.splint_exePath = d.Name + @"IntewareInc\EZCAD splint\Bin\EZCAD.splint.exe";

                            if (classfrom == (int)_classFrom.MainWindow)
                                SoftwareLogoShowEvent((int)_softwareID.Splint, (int)_softwareStatus.Installed, 0.0);
                            diskInfo.SoftwareCount++;
                        }
                        if (File.Exists(guide_exePath) == false && File.Exists(d.Name + @"IntewareInc\EZCAD guide\Bin\EZCAD.guide.exe") == true)
                        {
                            if (Properties.Settings.Default.guide_exePath == "")
                                Properties.Settings.Default.guide_exePath = d.Name + @"IntewareInc\EZCAD guide\Bin\EZCAD.guide.exe";

                            if (classfrom == (int)_classFrom.MainWindow)
                                SoftwareLogoShowEvent((int)_softwareID.Guide, (int)_softwareStatus.Installed, 0.0);
                            diskInfo.SoftwareCount++;
                        }

                        calcDiskwithSoftware2.Add(diskInfo);

                        if (Directory.Exists(Properties.Settings.Default.systemDisk) == false && File.Exists(diskInfo.DiskName + @"Windows\explorer.exe") == true)
                            Properties.Settings.Default.systemDisk = diskInfo.DiskName;
                    }
                    catch(Exception ex)
                    {
                        log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "foreach to check every Disk exception", ex.Message);
                    }
                }

                //分類list內資料並整理成sortedInfoList
                List<DiskSoftwareInfo> sortedInfoList = new List<DiskSoftwareInfo>();
                for (int i=0; i< calcDiskwithSoftware2.Count; i++)
                {
                    if(i == 0)
                        sortedInfoList.Add(calcDiskwithSoftware2[0]);
                    else
                    {
                        bool SameDisk = false;
                        for (int j = 0; j < sortedInfoList.Count; j++)
                        {
                            if (calcDiskwithSoftware2[i].DiskName == sortedInfoList[j].DiskName)
                            {
                                sortedInfoList[j].SoftwareCount++;
                                SameDisk = true;
                            }
                        }

                        if (SameDisk == false)
                            sortedInfoList.Add(calcDiskwithSoftware2[i]);
                    }
                }

                //開始各磁碟比較，比看哪個磁碟存比較多軟體
                for (int i = 0; i < sortedInfoList.Count; i++)
                {
                    if (i >= 1)
                    {
                        if (sortedInfoList[i].SoftwareCount > disk_most.SoftwareCount)
                            disk_most = sortedInfoList[i];
                    }
                    else
                        disk_most = sortedInfoList[i];
                }

                if (disk_most.SoftwareCount != 0)
                    Properties.Settings.Default.mostsoftwareDisk = disk_most.DiskName;
                else
                {
                    //一個軟體都沒安裝預設C碟，C碟沒有就D碟，兩個都沒有就用陣列第一筆磁碟
                    bool chosen = false;    //是否已選中
                    foreach (DiskSoftwareInfo disk in sortedInfoList)
                    {
                        if (disk.DiskName == @"C:\")
                        {
                            Properties.Settings.Default.mostsoftwareDisk = @"C:\";
                            chosen = true;
                            break;
                        }
                        if (disk.DiskName == @"D:\")
                        {
                            Properties.Settings.Default.mostsoftwareDisk = @"D:\";
                            chosen = true;
                            break;
                        }
                    }

                    if (chosen == false)
                        Properties.Settings.Default.mostsoftwareDisk = sortedInfoList[0].DiskName;
                }
            }

            //記錄到log檔內
            log.RecordLogContinue(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "cad_exePath", "\t\"" + Properties.Settings.Default.cad_exePath + "\"");
            log.RecordLogContinue(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "imp_exePath", "\t\"" + Properties.Settings.Default.implant_exePath + "\"");
            log.RecordLogContinue(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "ortho_exePath", "\t\"" + Properties.Settings.Default.ortho_exePath + "\"");
            log.RecordLogContinue(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "tray_exePath", "\t\"" + Properties.Settings.Default.tray_exePath + "\"");
            log.RecordLogContinue(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "splint_exePath", "\t\"" + Properties.Settings.Default.splint_exePath + "\"");
            log.RecordLogContinue(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "guide_exePath", "\t\"" + Properties.Settings.Default.guide_exePath + "\"");
            log.RecordLogContinue(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "mostswDisk", "\t\"" + Properties.Settings.Default.mostsoftwareDisk + "\"");
            log.RecordLogContinue(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "systemDisk", "\t\"" + Properties.Settings.Default.systemDisk + "\"");
            log.RecordLogContinue(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "OrderManagerFunctions.cs AutoDetectEXE()", "Detect finish.");
            log.RecordLogSaperate();

            //沒安裝的軟體Logo變灰，有安裝的常亮
            if (classfrom == (int)_classFrom.MainWindow)
            {
                DoubleCheckEXEexist();
            }
        }

        /// <summary>
        /// 自動偵測各軟體專案檔路徑
        /// </summary>
        public void AutoDetectSoftwareProjectPath()
        {
            log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "OrderManagerFunctions.cs AutoDetectSoftwareProjectPath()", "IntoFunction");

            XDocument xmlDoc = new XDocument();

            if (Properties.Settings.Default.cad_exePath != "")
            {
                try
                {
                    string xmlPath = Path.GetDirectoryName(Properties.Settings.Default.cad_exePath) + @"\SystemSetting.xml";
                    using (StreamReader oReader = new StreamReader(xmlPath, Encoding.GetEncoding("utf-8")))
                    {
                        xmlDoc = XDocument.Load(oReader);
                    }
                    string cad_projectDirectory = xmlDoc.Element("SystemSetting").Element("SystemPath").Element("DesignFilePath").Value;
                    if (cad_projectDirectory[cad_projectDirectory.Length - 1].ToString() != @"\")
                        cad_projectDirectory += @"\";

                    Properties.OrderManagerProps.Default.cad_projectDirectory = cad_projectDirectory;

                    if (!Directory.Exists(cad_projectDirectory))
                        Directory.CreateDirectory(cad_projectDirectory);

                    /*_watch_EZCADProject.Path = cad_projectDirectory;
                    MyFileSystemWatcher(_watch_EZCADProject, cad_projectDirectory);

                    if (RecordAll == true)
                        log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "LoadEZCADProject()", "IntoFunc");
                    LoadEZCADProject();*/
                }
                catch (Exception ex)
                {
                    Properties.OrderManagerProps.Default.cad_projectDirectory = "";
                    log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "AutoDetectSoftwareProjectPath()_CAD", ex.Message);
                }
            }
            else
            {
                Properties.OrderManagerProps.Default.cad_projectDirectory = "";
            }
            if (Properties.Settings.Default.tray_exePath != "")
            {
                try
                {
                    string xmlPath = Path.GetDirectoryName(Properties.Settings.Default.tray_exePath) + @"\SystemSetting.xml";
                    using (StreamReader oReader = new StreamReader(xmlPath, Encoding.GetEncoding("utf-8")))
                    {
                        xmlDoc = XDocument.Load(oReader);
                    }
                    string tray_projectDirectory = xmlDoc.Element("SystemSetting").Element("DesignFilePath").Value;
                    if (tray_projectDirectory[tray_projectDirectory.Length - 1].ToString() != @"\")
                        tray_projectDirectory += @"\";

                    Properties.OrderManagerProps.Default.tray_projectDirectory = tray_projectDirectory;

                    if (!Directory.Exists(tray_projectDirectory))
                        Directory.CreateDirectory(tray_projectDirectory);

                    /*_watch_TrayProject.Path = tray_projectDirectory;
                    MyFileSystemWatcher(_watch_TrayProject, tray_projectDirectory);

                    if (RecordAll == true)
                        log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "LoadTrayProject()", "IntoFunc");
                    LoadTrayProject();*/
                }
                catch (Exception ex)
                {
                    Properties.OrderManagerProps.Default.tray_projectDirectory = "";
                    log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "AutoDetectSoftwareProjectPath()_tray", ex.Message);
                }
            }
            else
            {
                Properties.OrderManagerProps.Default.tray_projectDirectory = "";
            }
            if (Properties.Settings.Default.splint_exePath != "")
            {
                try
                {
                    string xmlPath = Path.GetDirectoryName(Properties.Settings.Default.splint_exePath) + @"\SystemSetting.xml";
                    using (StreamReader oReader = new StreamReader(xmlPath, Encoding.GetEncoding("utf-8")))
                    {
                        xmlDoc = XDocument.Load(oReader);
                    }
                    string splint_projectDirectory = xmlDoc.Element("SystemSetting").Element("DesignFilePath").Value;
                    if (splint_projectDirectory[splint_projectDirectory.Length - 1].ToString() != @"\")
                        splint_projectDirectory += @"\";

                    Properties.OrderManagerProps.Default.splint_projectDirectory = splint_projectDirectory;

                    if (!Directory.Exists(splint_projectDirectory))
                        Directory.CreateDirectory(splint_projectDirectory);

                    /*_watch_SplintProject.Path = splint_projectDirectory;
                    MyFileSystemWatcher(_watch_SplintProject, splint_projectDirectory);

                    if (RecordAll == true)
                        log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "LoadSplintProject()", "IntoFunc");
                    LoadSplintProject();*/
                }
                catch (Exception ex)
                {
                    Properties.OrderManagerProps.Default.splint_projectDirectory = "";
                    log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "AutoDetectSoftwareProjectPath()_splint", ex.Message);
                }
            }
            else
            {
                Properties.OrderManagerProps.Default.splint_projectDirectory = "";
            }
            if (Properties.Settings.Default.ortho_exePath != "")
            {
                try
                {
                    string xmlPath = Path.GetDirectoryName(Properties.Settings.Default.ortho_exePath) + @"\SystemPara.xml";
                    using (StreamReader oReader = new StreamReader(xmlPath, Encoding.GetEncoding("utf-8")))
                    {
                        xmlDoc = XDocument.Load(oReader);
                    }
                    string ortho_projectDirectory = xmlDoc.Element("OrthoExport").Element("ProjectInfo").Element("PrjDataPath").Value;
                    if (ortho_projectDirectory[ortho_projectDirectory.Length - 1].ToString() != @"\")
                        ortho_projectDirectory += @"\";

                    Properties.OrderManagerProps.Default.ortho_projectDirectory = ortho_projectDirectory;

                    if (!Directory.Exists(ortho_projectDirectory))
                        Directory.CreateDirectory(ortho_projectDirectory);

                    /*_watch_OrthoProject.Path = ortho_projectDirectory;
                    MyFileSystemWatcher(_watch_OrthoProject, ortho_projectDirectory);

                    if (RecordAll == true)
                        log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "LoadOrthoProject()", "IntoFunc");
                    LoadOrthoProject();*/
                }
                catch (Exception ex)
                {
                    Properties.OrderManagerProps.Default.ortho_projectDirectory = "";
                    log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "AutoDetectSoftwareProjectPath()_ortho", ex.Message);
                }
            }
            else
            {
                Properties.OrderManagerProps.Default.ortho_projectDirectory = "";
            }
            if (Properties.Settings.Default.implant_exePath != "")
            {
                //Implant比較特殊，舊版Implant的專案檔路徑是OrderManager給的
                try
                {
                    bool foundImplantPath = false;
                    DriveInfo[] allDrives = DriveInfo.GetDrives();
                    foreach (DriveInfo d in allDrives)  //檢查客戶所有磁碟
                    {
                        try
                        {
                            if(Directory.Exists(d.Name + @"IntewareData\Implant\") == true)
                            {
                                Properties.OrderManagerProps.Default.implant_projectDirectory = d.Name + @"IntewareData\Implant\";
                                foundImplantPath = true;
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "foreach to check every Disk exception", ex.Message);
                        }
                    }

                    if(foundImplantPath == false)
                    {
                        try
                        {
                            if (Properties.Settings.Default.mostsoftwareDisk != "")
                            {
                                Directory.CreateDirectory(Properties.Settings.Default.mostsoftwareDisk + @"IntewareData\Implant\");
                                Properties.OrderManagerProps.Default.implant_projectDirectory = Properties.Settings.Default.mostsoftwareDisk + @"IntewareData\Implant\";
                                goto createtosysDirectorySuccess;
                            }
                            else
                                goto createtosysDirectoryFailed;
                        }
                        catch (Exception ex)
                        {
                            log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "foundImplantPath=false_1 exception", ex.Message);
                            goto createtosysDirectoryFailed;
                        }

                        createtosysDirectoryFailed:
                        bool chosen = false;    //是否已選中
                        foreach (DriveInfo d in allDrives)  //檢查客戶所有磁碟
                        {
                            try
                            {
                                if (d.Name == @"C:\")
                                {
                                    Properties.OrderManagerProps.Default.implant_projectDirectory = d.Name + @"IntewareData\Implant\";
                                    chosen = true;
                                    break;
                                }
                                if (d.Name == @"D:\")
                                {
                                    Properties.OrderManagerProps.Default.implant_projectDirectory = d.Name + @"IntewareData\Implant\";
                                    chosen = true;
                                    break;
                                }
                            }
                            catch (Exception ex)
                            {
                                log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "foundImplantPath=false_2 exception", ex.Message);
                            }
                        }

                        if (chosen == false)
                        {
                            foreach (DriveInfo d in allDrives)
                            {
                                try
                                {
                                    Directory.CreateDirectory(d.Name + @"IntewareData\Implant\");
                                    Properties.OrderManagerProps.Default.implant_projectDirectory = d.Name + @"IntewareData\Implant\";
                                    break;
                                }
                                catch (Exception ex)
                                {
                                    log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "foundImplantPath=false_3 exception", ex.Message);
                                }
                            }
                        }
                        createtosysDirectorySuccess:;
                    }
                }
                catch (Exception ex)
                {
                    Properties.OrderManagerProps.Default.implant_projectDirectory = Properties.Settings.Default.mostsoftwareDisk + @"IntewareData\Implant\";
                    if(Directory.Exists(Properties.OrderManagerProps.Default.implant_projectDirectory) == false)
                        Directory.CreateDirectory(Properties.Settings.Default.mostsoftwareDisk + @"IntewareData\Implant\");

                    log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "AutoDetectSoftwareProjectPath()_implant", ex.Message);
                }
            }
            else
            {
                Properties.OrderManagerProps.Default.implant_projectDirectory = Properties.Settings.Default.mostsoftwareDisk + @"IntewareData\Implant\";
                if (Directory.Exists(Properties.OrderManagerProps.Default.implant_projectDirectory) == false)
                    Directory.CreateDirectory(Properties.Settings.Default.mostsoftwareDisk + @"IntewareData\Implant\");
            }
            Properties.Settings.Default.Save();

            //記錄到log檔內
            log.RecordLogContinue(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "cad_projectDirectory", "\t\"" + Properties.OrderManagerProps.Default.cad_projectDirectory + "\"");
            log.RecordLogContinue(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "implant_projectDirectory", "\t\"" + Properties.OrderManagerProps.Default.implant_projectDirectory + "\"");
            log.RecordLogContinue(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "ortho_projectDirectory", "\t\"" + Properties.OrderManagerProps.Default.ortho_projectDirectory + "\"");
            log.RecordLogContinue(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "tray_projectDirectory", "\t\"" + Properties.OrderManagerProps.Default.tray_projectDirectory + "\"");
            log.RecordLogContinue(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "splint_projectDirectory", "\t\"" + Properties.OrderManagerProps.Default.splint_projectDirectory + "\"");
            log.RecordLogContinue(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "OrderManagerFunctions.cs AutoDetectSoftwareProjectPath()", "Detect finish.");
            log.RecordLogSaperate();
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
                if (arguments != "")
                    processer.StartInfo.Arguments = arguments;
                processer.Start();
            }
            catch (Exception ex)
            {
                Handler_snackbarShow(ex.Message);
                log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "RunCommandLine exception", ex.Message);
            }
        }
    }
}
