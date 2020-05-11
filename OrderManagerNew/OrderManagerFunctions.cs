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

        public OrderManagerFunctions()
        {
            log = new LogRecorder();
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
