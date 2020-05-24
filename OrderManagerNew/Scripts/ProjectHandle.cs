using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using CadInformation = OrderManagerNew.UserControls.Order_cadBase.CadInformation;
using TrayInformation = OrderManagerNew.UserControls.Order_tsBase.TrayInformation;
using SplintInformation = OrderManagerNew.UserControls.Order_tsBase.SplintInformation;
using ImplantInformation = OrderManagerNew.UserControls.Order_implantBase.ImplantInformation;
using ImplantCaseInformation = OrderManagerNew.UserControls.Order_case.ImplantCaseInformation;


namespace OrderManagerNew
{
    public class ProjectHandle
    {
        public delegate void caseShowEventHandler(int softwareID);
        public event caseShowEventHandler CaseShowEvent;

        public List<CadInformation> Caselist_EZCAD;
        public List<TrayInformation> Caselist_Tray;
        public List<SplintInformation> Caselist_Splint;
        /// <summary>
        /// ImplantPlanning Dicom專案
        /// </summary>
        public List<ImplantInformation> Caselist_Implant;
        /// <summary>
        /// ImplantPlanning 內部專案
        /// </summary>
        public List<ImplantCaseInformation> Caselist_ImplantCase;
        LogRecorder log;

        public ProjectHandle()
        {
            log = new LogRecorder();
        }

        public void LoadEZCADProj()
        {
            string cad_projectDirectory = Properties.Settings.Default.cad_projectDirectory;
            string cad_exePath = Properties.Settings.Default.cad_exePath;

            if (Directory.Exists(cad_projectDirectory) == false || File.Exists(cad_exePath) == false)
                return;

            Caselist_EZCAD = new List<CadInformation>();
            try
            {
                DirectoryInfo Dinfo = new DirectoryInfo(cad_projectDirectory);
                foreach(DirectoryInfo folder in Dinfo.GetDirectories())
                {
                    string DirectoryName = folder.ToString();
                    string XmlPath = Dinfo.ToString() + DirectoryName + @"\" + DirectoryName + ".xml";

                    if (File.Exists(XmlPath) == false)
                        continue;
                    else
                    {
                        if (LoadXml((int)_softwareID.EZCAD, XmlPath) == false)
                            continue;
                    }
                }
            }
            catch(Exception ex)
            {
                log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "ProjectHandle.cs LoadcadProj Exception", ex.Message);
            }

            if (Caselist_EZCAD.Count > 0)
                CaseShowEvent((int)_softwareID.EZCAD);
        }

        public void LoadTrayProj()
        {
            string tray_projectDirectory = Properties.Settings.Default.tray_projectDirectory;
            string tray_exePath = Properties.Settings.Default.tray_exePath;

            if (Directory.Exists(tray_projectDirectory) == false || File.Exists(tray_exePath) == false)
                return;

            Caselist_Tray = new List<TrayInformation>();
            try
            {
                DirectoryInfo Dinfo = new DirectoryInfo(tray_projectDirectory);
                foreach (DirectoryInfo folder in Dinfo.GetDirectories())
                {
                    string DirectoryName = folder.ToString();
                    string XmlPath = Dinfo.ToString() + DirectoryName + @"\" + DirectoryName + ".tml";

                    if (File.Exists(XmlPath) == false)
                        continue;
                    else
                    {
                        if (LoadXml((int)_softwareID.Tray, XmlPath) == false)
                            continue;
                    }
                }
            }
            catch (Exception ex)
            {
                log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "ProjectHandle.cs LoadtrayProj Exception", ex.Message);
            }

            if (Caselist_Tray.Count > 0)
                CaseShowEvent((int)_softwareID.Tray);
        }

        public void LoadSplintProj()
        {
            string splint_projectDirectory = Properties.Settings.Default.splint_projectDirectory;
            string splint_exePath = Properties.Settings.Default.splint_exePath;

            if (Directory.Exists(splint_projectDirectory) == false || File.Exists(splint_exePath) == false)
                return;

            Caselist_Splint = new List<SplintInformation>();
            try
            {
                DirectoryInfo Dinfo = new DirectoryInfo(splint_projectDirectory);
                foreach (DirectoryInfo folder in Dinfo.GetDirectories())
                {
                    string DirectoryName = folder.ToString();
                    string XmlPath = Dinfo.ToString() + DirectoryName + @"\" + DirectoryName + ".sml";

                    if (File.Exists(XmlPath) == false)
                        continue;
                    else
                    {
                        if (LoadXml((int)_softwareID.Splint, XmlPath) == false)
                            continue;
                    }
                }
            }
            catch (Exception ex)
            {
                log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "ProjectHandle.cs LoadsplintProj Exception", ex.Message);
            }

            if (Caselist_Splint.Count > 0)
                CaseShowEvent((int)_softwareID.Splint);
        }

        public void LoadImplantProj()
        {
            string implant_projectDirectory = Properties.Settings.Default.implant_projectDirectory;
            string implant_exePath = Properties.Settings.Default.implant_exePath;

            if (Directory.Exists(implant_projectDirectory) == false || File.Exists(implant_projectDirectory) == false)
                return;

            //Implant內部專案先讀，讀完後把資料一個一個寫入Implant Dicom專案
            Caselist_ImplantCase = new List<ImplantCaseInformation>();

        }

        bool LoadXml(int SoftwareID, string XmlPath)
        {
            XDocument xmlDoc;
            FileInfo fInfo = new FileInfo(XmlPath);//要取得檔案創建日期和修改日期

            try
            {
                xmlDoc = XDocument.Load(XmlPath);
            }
            catch
            {
                return false;
            }

            switch (SoftwareID)
            {
                case (int)_softwareID.EZCAD:
                    {
                        //判斷是否為Material的XML
                        if (xmlDoc.Element("OrderExport") == null)
                        {
                            return false;
                        }

                        try
                        {
                            XElement xml = xmlDoc.Element("OrderExport");
                            string xOrderID = xml.Element("OrderInfo").Element("OrderID").Value;//ordernumer-custom
                            string xClient = xml.Element("OrderInfo").Element("Client").Value;//Client
                            string xTechnician = xml.Element("OrderInfo").Element("Technician").Value;//Technicain
                            string xPatient = xml.Element("OrderInfo").Element("Patient").Value;//patient
                            string xOrderNote = xml.Element("OrderInfo").Element("Note").Value;//Note
                            string xDesignStep = xml.Element("DesignInfo").Element("DesignStep").Value;//DesignStep
                            string xVersion = xml.Element("DesignInfo").Element("Version").Value;//Version

                            CadInformation cadInfo = new CadInformation
                            {
                                OrderID = xOrderID,
                                PatientName = xPatient,
                                CreateDate = fInfo.CreationTime,
                                ModifyDate = fInfo.LastWriteTime,
                                DesignStep = Convert.ToInt32(xDesignStep),
                                CaseDirectoryPath = Path.GetDirectoryName(XmlPath),
                                Client = xClient,
                                Technician = xTechnician,
                                Note = xOrderNote,
                                CADversion = new Version(xVersion)
                            };
                            Caselist_EZCAD.Add(cadInfo);

                            return true;
                        }
                        catch (Exception ex)
                        {
                            log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "ProjectHandle.cs LoadXml(EZCAD) Exception", ex.Message);
                            return false;
                        }
                    }
                case (int)_softwareID.Implant:
                    {

                        break;
                    }
                case (int)_softwareID.Ortho:
                    {

                        break;
                    }
                case (int)_softwareID.Tray:
                    {
                        if (xmlDoc.Element("GuideExport") == null)
                        {
                            return false;
                        }

                        try
                        {
                            XElement xml = xmlDoc.Element("GuideExport");
                            string xDesignStep = xml.Element("OrderInfo").Element("DesignStep").Value;//DesignStep
                            string xOrderID = xml.Element("ProjectName").Value;//ordernumer-custom

                            TrayInformation trayInfo = new TrayInformation
                            {
                                OrderID = xOrderID,
                                CreateDate = fInfo.CreationTime,
                                ModifyDate = fInfo.LastWriteTime,
                                DesignStep = Convert.ToInt32(xDesignStep),
                                CaseDirectoryPath = Path.GetDirectoryName(XmlPath),
                            };
                            Caselist_Tray.Add(trayInfo);

                            return true;
                        }
                        catch (Exception ex)
                        {
                            log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "ProjectHandle.cs LoadXml(Tray) Exception", ex.Message);
                            return false;
                        }
                    }
                case (int)_softwareID.Splint:
                    {
                        if (xmlDoc.Element("GuideExport") == null)
                        {
                            return false;
                        }

                        try
                        {
                            XElement xml = xmlDoc.Element("GuideExport");
                            string xDesignStep = xml.Element("OrderInfo").Element("DesignStep").Value;//DesignStep
                            string xOrderID = xml.Element("ProjectName").Value;//ordernumer-custom

                            SplintInformation splintInfo = new SplintInformation
                            {
                                OrderID = xOrderID,
                                CreateDate = fInfo.CreationTime,
                                ModifyDate = fInfo.LastWriteTime,
                                DesignStep = Convert.ToInt32(xDesignStep),
                                CaseDirectoryPath = Path.GetDirectoryName(XmlPath),
                            };
                            Caselist_Splint.Add(splintInfo);

                            return true;
                        }
                        catch (Exception ex)
                        {
                            log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "ProjectHandle.cs LoadXml(Splint) Exception", ex.Message);
                            return false;
                        }
                    }
            }

            return true;
        }
    }
}
