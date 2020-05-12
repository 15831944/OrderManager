using System;
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
        public event softwareLogoShowEventHandler softwareLogoShowEvent;

        public OrderManagerFunctions()
        {
            log = new LogRecorder();
        }

        class diskSoftwareInfo
        {
            public string diskName { get; set; }
            public int softwareCount { get; set; }

            public diskSoftwareInfo()
            {
                diskName = "";
                softwareCount = 0;
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
        /// 自動檢測軟體執行檔路徑並把最常用的磁碟存入 Properties.Settings.Default.systemDisk
        /// </summary>
        /// <param name="classfrom">哪個class呼叫的，參考 _classFrom</param>
        public void AutoDetectEXE(int classfrom)
        {
            string cad_exePath = Properties.Settings.Default.cad_exePath;
            string implant_exePath = Properties.Settings.Default.implant_exePath;
            string ortho_exePath = Properties.Settings.Default.ortho_exePath;
            string tray_exePath = Properties.Settings.Default.tray_exePath;
            string splint_exePath = Properties.Settings.Default.splint_exePath;
            string guide_exePath = Properties.Settings.Default.guide_exePath;
            string systemDisk = Properties.Settings.Default.systemDisk;
            string[] array_exePath = { cad_exePath, implant_exePath, ortho_exePath, tray_exePath, splint_exePath, guide_exePath };
            diskSoftwareInfo disk_most = new diskSoftwareInfo();//存最多軟體的磁碟

            //全部軟體都有安裝
            if (File.Exists(cad_exePath) == true && File.Exists(implant_exePath) == true && File.Exists(ortho_exePath) == true
                && File.Exists(tray_exePath) == true && File.Exists(splint_exePath) == true && File.Exists(guide_exePath) == true)
            {
                List<diskSoftwareInfo> calcDiskwithSoftware = new List<diskSoftwareInfo>();
                for(int i=0; i < array_exePath.Count(); i++)
                {
                    diskSoftwareInfo dInfo = new diskSoftwareInfo();
                    dInfo.diskName = Path.GetPathRoot(array_exePath[i]);
                    dInfo.softwareCount++;

                    if (i == 0)
                        calcDiskwithSoftware.Add(dInfo);
                    else
                    {
                        bool SameDisk = false;
                        for(int j=0; j<calcDiskwithSoftware.Count; j++)
                        {
                            if (calcDiskwithSoftware[j].diskName == dInfo.diskName)
                            {
                                calcDiskwithSoftware[j].softwareCount++;
                                SameDisk = true;
                            }
                        }

                        if(SameDisk == false)
                            calcDiskwithSoftware.Add(dInfo);
                    }
                }
                //統整出systemDisk
                for(int i=0; i<calcDiskwithSoftware.Count; i++)
                {
                    if (i >= 1)
                    {
                        if (calcDiskwithSoftware[i].softwareCount > disk_most.softwareCount)
                            disk_most = calcDiskwithSoftware[i];
                    }
                    else
                        disk_most = calcDiskwithSoftware[i];
                }

                Properties.Settings.Default.systemDisk = disk_most.diskName;
                Properties.Settings.Default.Save();

                //已經有systemDisk資料
                if (classfrom == (int)_classFrom.MainWindow)
                {
                    for (int i = 0; i < array_exePath.Count(); i++)
                        softwareLogoShowEvent(i, (int)_softwareStatus.Installed, 0.0);
                }
            }
            else
            {
                //先找已儲存到Properties內的exePath
                List<diskSoftwareInfo> calcDiskwithSoftware2 = new List<diskSoftwareInfo>();
                if (File.Exists(cad_exePath) == true)
                {
                    diskSoftwareInfo dInfo = new diskSoftwareInfo();
                    dInfo.diskName = Path.GetPathRoot(cad_exePath);
                    dInfo.softwareCount++;
                    calcDiskwithSoftware2.Add(dInfo);

                    if (classfrom == (int)_classFrom.MainWindow)
                        softwareLogoShowEvent((int)_softwareID.EZCAD, (int)_softwareStatus.Installed, 0.0);
                }
                if (File.Exists(implant_exePath) == true)
                {
                    diskSoftwareInfo dInfo = new diskSoftwareInfo();
                    dInfo.diskName = Path.GetPathRoot(implant_exePath);
                    dInfo.softwareCount++;
                    calcDiskwithSoftware2.Add(dInfo);

                    if (classfrom == (int)_classFrom.MainWindow)
                        softwareLogoShowEvent((int)_softwareID.Implant, (int)_softwareStatus.Installed, 0.0);
                }
                if (File.Exists(ortho_exePath) == true)
                {
                    diskSoftwareInfo dInfo = new diskSoftwareInfo();
                    dInfo.diskName = Path.GetPathRoot(ortho_exePath);
                    dInfo.softwareCount++;
                    calcDiskwithSoftware2.Add(dInfo);

                    if (classfrom == (int)_classFrom.MainWindow)
                        softwareLogoShowEvent((int)_softwareID.Ortho, (int)_softwareStatus.Installed, 0.0);
                }
                if (File.Exists(tray_exePath) == true)
                {
                    diskSoftwareInfo dInfo = new diskSoftwareInfo();
                    dInfo.diskName = Path.GetPathRoot(tray_exePath);
                    dInfo.softwareCount++;
                    calcDiskwithSoftware2.Add(dInfo);

                    if (classfrom == (int)_classFrom.MainWindow)
                        softwareLogoShowEvent((int)_softwareID.Tray, (int)_softwareStatus.Installed, 0.0);
                }
                if (File.Exists(splint_exePath) == true)
                {
                    diskSoftwareInfo dInfo = new diskSoftwareInfo();
                    dInfo.diskName = Path.GetPathRoot(splint_exePath);
                    dInfo.softwareCount++;
                    calcDiskwithSoftware2.Add(dInfo);

                    if (classfrom == (int)_classFrom.MainWindow)
                        softwareLogoShowEvent((int)_softwareID.Splint, (int)_softwareStatus.Installed, 0.0);
                }
                if (File.Exists(guide_exePath) == true)
                {
                    diskSoftwareInfo dInfo = new diskSoftwareInfo();
                    dInfo.diskName = Path.GetPathRoot(guide_exePath);
                    dInfo.softwareCount++;
                    calcDiskwithSoftware2.Add(dInfo);

                    if (classfrom == (int)_classFrom.MainWindow)
                        softwareLogoShowEvent((int)_softwareID.Guide, (int)_softwareStatus.Installed, 0.0);
                }
                
                DriveInfo[] allDrives = DriveInfo.GetDrives();
                foreach (DriveInfo d in allDrives)  //檢查客戶所有磁碟
                {
                    diskSoftwareInfo diskInfo = new diskSoftwareInfo();
                    diskInfo.diskName = d.Name;
                    
                    if (File.Exists(cad_exePath) == false && File.Exists(d.Name + @"InteWare\EZCAD\Bin\EZCAD.exe") == true)
                    {
                        if (Properties.Settings.Default.cad_exePath == "")
                            Properties.Settings.Default.cad_exePath = d.Name + @"InteWare\EZCAD\Bin\EZCAD.exe";

                        if (classfrom == (int)_classFrom.MainWindow)
                            softwareLogoShowEvent((int)_softwareID.EZCAD, (int)_softwareStatus.Installed, 0.0);
                        diskInfo.softwareCount++;
                    }
                    if (File.Exists(implant_exePath) == false && File.Exists(d.Name + @"InteWare\ImplantPlanning\ImplantPlanning.exe") == true)
                    {
                        if (Properties.Settings.Default.implant_exePath == "")
                            Properties.Settings.Default.implant_exePath = d.Name + @"InteWare\ImplantPlanning\ImplantPlanning.exe";

                        if (classfrom == (int)_classFrom.MainWindow)
                            softwareLogoShowEvent((int)_softwareID.Implant, (int)_softwareStatus.Installed, 0.0);
                        diskInfo.softwareCount++;
                    }
                    if (File.Exists(ortho_exePath) == false && File.Exists(d.Name + @"InteWare\OrthoAnalysis\OrthoAnalysis.exe") == true)
                    {
                        if (Properties.Settings.Default.ortho_exePath == "")
                            Properties.Settings.Default.ortho_exePath = d.Name + @"InteWare\OrthoAnalysis\OrthoAnalysis.exe";

                        if (classfrom == (int)_classFrom.MainWindow)
                            softwareLogoShowEvent((int)_softwareID.Ortho, (int)_softwareStatus.Installed, 0.0);
                        diskInfo.softwareCount++;
                    }
                    if (File.Exists(tray_exePath) == false && File.Exists(d.Name + @"InteWare\EZCAD tray\Bin\EZCAD.tray.exe") == true)
                    {
                        if (Properties.Settings.Default.tray_exePath == "")
                            Properties.Settings.Default.tray_exePath = d.Name + @"InteWare\EZCAD tray\Bin\EZCAD.tray.exe";

                        if (classfrom == (int)_classFrom.MainWindow)
                            softwareLogoShowEvent((int)_softwareID.Tray, (int)_softwareStatus.Installed, 0.0);
                        diskInfo.softwareCount++;
                    }
                    if (File.Exists(splint_exePath) == false && File.Exists(d.Name + @"InteWare\EZCAD splint\Bin\EZCAD.splint.exe") == true)
                    {
                        if (Properties.Settings.Default.splint_exePath == "")
                            Properties.Settings.Default.splint_exePath = d.Name + @"InteWare\EZCAD splint\Bin\EZCAD.splint.exe";

                        if (classfrom == (int)_classFrom.MainWindow)
                            softwareLogoShowEvent((int)_softwareID.Splint, (int)_softwareStatus.Installed, 0.0);
                        diskInfo.softwareCount++;
                    }
                    if (File.Exists(guide_exePath) == false && File.Exists(d.Name + @"InteWare\EZCAD guide\Bin\EZCAD.guide.exe") == true)
                    {
                        if (Properties.Settings.Default.guide_exePath == "")
                            Properties.Settings.Default.guide_exePath = d.Name + @"InteWare\EZCAD guide\Bin\EZCAD.guide.exe";

                        if (classfrom == (int)_classFrom.MainWindow)
                            softwareLogoShowEvent((int)_softwareID.Guide, (int)_softwareStatus.Installed, 0.0);
                        diskInfo.softwareCount++;
                    }

                    calcDiskwithSoftware2.Add(diskInfo);
                }

                //分類list內資料並整理成sortedInfoList
                List<diskSoftwareInfo> sortedInfoList = new List<diskSoftwareInfo>();
                for (int i=0; i< calcDiskwithSoftware2.Count; i++)
                {
                    if(i == 0)
                        sortedInfoList.Add(calcDiskwithSoftware2[0]);
                    else
                    {
                        bool SameDisk = false;
                        for (int j = 0; j < sortedInfoList.Count; j++)
                        {
                            if (calcDiskwithSoftware2[i].diskName == sortedInfoList[j].diskName)
                            {
                                sortedInfoList[j].softwareCount++;
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
                        if (sortedInfoList[i].softwareCount > disk_most.softwareCount)
                            disk_most = sortedInfoList[i];
                    }
                    else
                        disk_most = sortedInfoList[i];
                }

                if (disk_most.softwareCount != 0)
                    Properties.Settings.Default.systemDisk = disk_most.diskName;
                else
                {
                    //一個軟體都沒安裝預設C碟，C碟沒有就D碟，兩個都沒有就用陣列第一筆磁碟
                    bool chosen = false;    //是否已選中
                    foreach (diskSoftwareInfo disk in sortedInfoList)
                    {
                        if (disk.diskName == @"C:\")
                        {
                            Properties.Settings.Default.systemDisk = @"C:\";
                            chosen = true;
                            break;
                        }
                        if (disk.diskName == @"D:\")
                        {
                            Properties.Settings.Default.systemDisk = @"D:\";
                            chosen = true;
                            break;
                        }
                    }

                    if (chosen == false)
                        Properties.Settings.Default.systemDisk = sortedInfoList[0].diskName;
                }

                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// 自動偵測各軟體專案檔路徑
        /// </summary>
        public void DetectSoftwareProjectPath()
        {
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
                    string cad_projectPath = xmlDoc.Element("SystemSetting").Element("SystemPath").Element("DesignFilePath").Value;
                    if (cad_projectPath[cad_projectPath.Length - 1].ToString() != @"\")
                        cad_projectPath += @"\";

                    Properties.Settings.Default.cad_projectPath = cad_projectPath;

                    if (!Directory.Exists(cad_projectPath))
                        Directory.CreateDirectory(cad_projectPath);

                    /*_watch_EZCADProject.Path = cad_projectPath;
                    MyFileSystemWatcher(_watch_EZCADProject, cad_projectPath);

                    if (RecordAll == true)
                        log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "LoadEZCADProject()", "IntoFunc");
                    LoadEZCADProject();*/
                }
                catch (Exception ex)
                {
                    log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "DetectSoftwareProjectPath()_CAD", ex.Message);
                }
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
                    string tray_projectPath = xmlDoc.Element("SystemSetting").Element("DesignFilePath").Value;
                    if (tray_projectPath[tray_projectPath.Length - 1].ToString() != @"\")
                        tray_projectPath += @"\";

                    Properties.Settings.Default.tray_projectPath = tray_projectPath;

                    if (!Directory.Exists(tray_projectPath))
                        Directory.CreateDirectory(tray_projectPath);

                    /*_watch_TrayProject.Path = tray_projectPath;
                    MyFileSystemWatcher(_watch_TrayProject, tray_projectPath);

                    if (RecordAll == true)
                        log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "LoadTrayProject()", "IntoFunc");
                    LoadTrayProject();*/
                }
                catch (Exception ex)
                {
                    log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "DetectSoftwareProjectPath()_tray", ex.Message);
                }
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
                    string splint_projectPath = xmlDoc.Element("SystemSetting").Element("DesignFilePath").Value;
                    if (splint_projectPath[splint_projectPath.Length - 1].ToString() != @"\")
                        splint_projectPath += @"\";

                    Properties.Settings.Default.splint_projectPath = splint_projectPath;

                    if (!Directory.Exists(splint_projectPath))
                        Directory.CreateDirectory(splint_projectPath);

                    /*_watch_SplintProject.Path = splint_projectPath;
                    MyFileSystemWatcher(_watch_SplintProject, splint_projectPath);

                    if (RecordAll == true)
                        log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "LoadSplintProject()", "IntoFunc");
                    LoadSplintProject();*/
                }
                catch (Exception ex)
                {
                    log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "DetectSoftwareProjectPath()_splint", ex.Message);
                }
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
                    string ortho_projectPath = xmlDoc.Element("OrthoExport").Element("ProjectInfo").Element("PrjDataPath").Value;
                    if (ortho_projectPath[ortho_projectPath.Length - 1].ToString() != @"\")
                        ortho_projectPath += @"\";

                    Properties.Settings.Default.ortho_projectPath = ortho_projectPath;

                    if (!Directory.Exists(ortho_projectPath))
                        Directory.CreateDirectory(ortho_projectPath);

                    /*_watch_OrthoProject.Path = ortho_projectPath;
                    MyFileSystemWatcher(_watch_OrthoProject, ortho_projectPath);

                    if (RecordAll == true)
                        log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "LoadOrthoProject()", "IntoFunc");
                    LoadOrthoProject();*/
                }
                catch (Exception ex)
                {
                    log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "DetectSoftwareProjectPath()_ortho", ex.Message);
                }
            }

            Properties.Settings.Default.Save();
        }
    }
}
