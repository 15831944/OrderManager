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
using ImplantOuterInformation = OrderManagerNew.UserControls.Order_implantBase.ImplantOuterInformation;
using OrthoOuterInformation = OrderManagerNew.UserControls.Order_orthoBase.OrthoOuterInformation;

namespace OrderManagerNew
{
    public class ProjectHandle
    {
        public delegate void caseShowEventHandler(int softwareID);
        public event caseShowEventHandler CaseShowEvent;

        public List<CadInformation> Caselist_EZCAD;
        public List<TrayInformation> Caselist_Tray;
        public List<SplintInformation> Caselist_Splint;
        public List<ImplantOuterInformation> Caselist_ImplantOuterCase;
        public List<OrthoOuterInformation> Caselist_OrthoOuterCase;

        LogRecorder log;
        CaseSorter Sorter;

        public ProjectHandle()
        {
            log = new LogRecorder();
            Sorter = new CaseSorter();
        }

        /// <summary>
        /// 讀取EZCAD專案
        /// </summary>
        public void LoadEZCADProj()
        {
            string cad_projectDirectory = Properties.OrderManagerProps.Default.cad_projectDirectory;
            string cad_exePath = Properties.Settings.Default.cad_exePath;

            if (Directory.Exists(cad_projectDirectory) == false && File.Exists(cad_exePath) != false)
            {
                OrderManagerFunctions omFunc = new OrderManagerFunctions();
                omFunc.AutoDetectSoftwareProjectPath((int)_softwareID.EZCAD);
                cad_projectDirectory = Properties.OrderManagerProps.Default.cad_projectDirectory;
            }
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

            Sorter.Sort_EZCAD(Caselist_EZCAD, 0, (Caselist_EZCAD.Count - 1), true);
            CaseShowEvent((int)_softwareID.EZCAD);
        }

        /// <summary>
        /// 讀取Tray專案
        /// </summary>
        public void LoadTrayProj()
        {
            string tray_projectDirectory = Properties.OrderManagerProps.Default.tray_projectDirectory;
            string tray_exePath = Properties.Settings.Default.tray_exePath;

            if (Directory.Exists(tray_projectDirectory) == false && File.Exists(tray_exePath) != false)
            {
                OrderManagerFunctions omFunc = new OrderManagerFunctions();
                omFunc.AutoDetectSoftwareProjectPath((int)_softwareID.Tray);
                tray_projectDirectory = Properties.OrderManagerProps.Default.tray_projectDirectory;
            }
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

            Sorter.Sort_Tray(Caselist_Tray, 0, (Caselist_Tray.Count - 1), true);
            CaseShowEvent((int)_softwareID.Tray);
        }

        /// <summary>
        /// 讀取Splint專案
        /// </summary>
        public void LoadSplintProj()
        {
            string splint_projectDirectory = Properties.OrderManagerProps.Default.splint_projectDirectory;
            string splint_exePath = Properties.Settings.Default.splint_exePath;

            if (Directory.Exists(splint_projectDirectory) == false && File.Exists(splint_exePath) != false)
            {
                OrderManagerFunctions omFunc = new OrderManagerFunctions();
                omFunc.AutoDetectSoftwareProjectPath((int)_softwareID.Splint);
                splint_projectDirectory = Properties.OrderManagerProps.Default.splint_projectDirectory;
            }
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

            Sorter.Sort_Splint(Caselist_Splint, 0, (Caselist_Splint.Count - 1), true);
            CaseShowEvent((int)_softwareID.Splint);
        }

        /// <summary>
        /// 讀取ImplantPlanning專案
        /// </summary>
        public void LoadImplantProjV2()
        {
            string implant_projectDirectory = Properties.OrderManagerProps.Default.implant_projectDirectory;
            string implant_exePath = Properties.Settings.Default.implant_exePath;

            if (Directory.Exists(implant_projectDirectory) == false && File.Exists(implant_exePath) != false)
            {
                OrderManagerFunctions omFunc = new OrderManagerFunctions();
                omFunc.AutoDetectSoftwareProjectPath((int)_softwareID.Implant);
                implant_projectDirectory = Properties.OrderManagerProps.Default.implant_projectDirectory;
            }
            if (Directory.Exists(implant_projectDirectory) == false || File.Exists(implant_exePath) == false)
                return;

            //Implant外部清單
            Caselist_ImplantOuterCase = new List<ImplantOuterInformation>();
            
            DirectoryInfo dInfo = new DirectoryInfo(implant_projectDirectory);
            // C:\IntewareData\Implant\2020130102946\202001301037_final_mi\LinkStation\ManufacturingDir\(Guide生出來的物件)
            foreach (DirectoryInfo folder in dInfo.GetDirectories())
            {
                // 這層是C:\IntewareData\Implant\
                
                string XmlPath = folder.FullName + @"\" + folder.ToString() + ".xml";
                if (File.Exists(XmlPath) == false)
                    continue;
                else
                {
                    if (LoadXml((int)_softwareID.Implant, XmlPath) == false)
                        continue;
                }
            }

            Sorter.Sort_Implant(Caselist_ImplantOuterCase, 0, (Caselist_ImplantOuterCase.Count-1), true);
            CaseShowEvent((int)_softwareID.Implant);
        }

        /// <summary>
        /// 讀取OrthoAnalysis專案
        /// </summary>
        public void LoadOrthoProj()
        {
            string ortho_exePath = Properties.Settings.Default.ortho_exePath;
            string ortho_projectDirectory = Properties.OrderManagerProps.Default.ortho_projectDirectory;

            if (Directory.Exists(ortho_projectDirectory) == false && File.Exists(ortho_exePath) != false)
            {
                OrderManagerFunctions omFunc = new OrderManagerFunctions();
                omFunc.AutoDetectSoftwareProjectPath((int)_softwareID.Ortho);
                ortho_projectDirectory = Properties.OrderManagerProps.Default.ortho_projectDirectory;
            }
            if (Directory.Exists(ortho_projectDirectory) == false || File.Exists(ortho_exePath) == false)
                return;

            //Ortho外部清單
            Caselist_OrthoOuterCase = new List<OrthoOuterInformation>();

            DirectoryInfo dInfo = new DirectoryInfo(ortho_projectDirectory);
            // C:\IntewareData\OrthoAnalysisV3\OrthoData\Test_1216\2019-12-16-1543\Test_1216.xml
            foreach(DirectoryInfo folder in dInfo.GetDirectories())
            {
                // 這層是C:\IntewareData\OrthoAnalysisV3\OrthoData\folder\
                string OuterXmlPath = folder.FullName + @"\PatientInfo.xml";
                if (File.Exists(OuterXmlPath) == false)
                    continue;
                else
                {
                    if (LoadXml((int)_softwareID.Ortho, OuterXmlPath) == false)
                        continue;
                }
            }
            
            Sorter.Sort_Ortho(Caselist_OrthoOuterCase, 0, (Caselist_OrthoOuterCase.Count - 1), true);
            CaseShowEvent((int)_softwareID.Ortho);
        }

        /// <summary>
        /// 讀各專案Xml檔內的資料
        /// </summary>
        /// <param name="SoftwareID">軟體ID 參考_SoftwareID</param>
        /// <param name="XmlPath">Xml路徑</param>
        /// <returns></returns>
        private bool LoadXml(int SoftwareID, string XmlPath)
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
                            return false;

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
                                CaseXmlPath = XmlPath,
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
                        if (xmlDoc.Element("ImplantOrderExport") == null)
                            return false;

                        try
                        {
                            XElement xml = xmlDoc.Element("ImplantOrderExport");

                            ImplantOuterInformation tmpImpOuterInfo = new ImplantOuterInformation
                            {
                                OrderNumber = xml.Element("OrderInfo").Element("OrderNo").Value,
                                PatientName = xml.Element("OrderInfo").Element("PatientName").Value,
                                CaseDirectoryPath = Path.GetDirectoryName(XmlPath) + @"\",
                                CreateDate = fInfo.CreationTime,
                                ModifyDate = fInfo.LastWriteTime
                            };
                            try
                            {
                                if (xml.Element("OrderInfo").Element("PatientGender").Value.ToLower() == "male")
                                    tmpImpOuterInfo.Gender = true;
                                else
                                    tmpImpOuterInfo.Gender = false;
                            }
                            catch
                            {
                                tmpImpOuterInfo.Gender = false;
                            }
                            try
                            {
                                string pbirthday = xml.Element("OrderInfo").Element("PatientBirthday").Value + "T00:00:00";
                                tmpImpOuterInfo.PatientBirth = Convert.ToDateTime(pbirthday);
                            }
                            catch
                            {
                                tmpImpOuterInfo.PatientBirth = new DateTime();
                            }
                            try { tmpImpOuterInfo.Clinic = xml.Element("OrderInfo").Element("Clinic").Value; } catch { tmpImpOuterInfo.Clinic = ""; }
                            try { tmpImpOuterInfo.Note = xml.Element("OrderInfo").Element("Note").Value; } catch { tmpImpOuterInfo.Note = ""; }

                            try { tmpImpOuterInfo.SurgicalGoal = xml.Element("CaseInfo").Element("SurgicalGoal").Value; } catch { tmpImpOuterInfo.SurgicalGoal = ""; }
                            try { tmpImpOuterInfo.SurgicalGuide = xml.Element("CaseInfo").Element("SurgicalGuide").Value; } catch { tmpImpOuterInfo.SurgicalGuide = ""; }
                            try { tmpImpOuterInfo.SurgicalOption = xml.Element("CaseInfo").Element("SurgicalOption").Value; } catch { tmpImpOuterInfo.SurgicalOption = ""; }
                            try { tmpImpOuterInfo.Surgicalkit = xml.Element("CaseInfo").Element("Surgicalkit").Value; } catch { tmpImpOuterInfo.Surgicalkit = ""; }

                            try { tmpImpOuterInfo.CBCTPath = xml.Element("ImageData").Element("CBCTPath").Value; } catch { tmpImpOuterInfo.CBCTPath = ""; }
                            try { tmpImpOuterInfo.JawPath = xml.Element("ImageData").Element("JawPath").Value; } catch { tmpImpOuterInfo.JawPath = ""; }
                            try { tmpImpOuterInfo.JawTrayPath = xml.Element("ImageData").Element("JawTrayPath").Value; } catch { tmpImpOuterInfo.JawTrayPath = ""; }
                            try { tmpImpOuterInfo.DenturePath = xml.Element("ImageData").Element("DenturePath").Value; } catch { tmpImpOuterInfo.DenturePath = ""; }
                            tmpImpOuterInfo.XmlfilePath = XmlPath;
                            Caselist_ImplantOuterCase.Add(tmpImpOuterInfo);

                            return true;
                        }
                        catch (Exception ex)
                        {
                            log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "ProjectHandle.cs LoadXml(Implant) Exception", ex.Message);
                            return false;
                        }
                    }
                case (int)_softwareID.Ortho:
                    {
                        if (xmlDoc.Element("CreateProjectInfo") == null)
                            return false;
                        try
                        {
                            XElement xml = xmlDoc.Element("CreateProjectInfo");

                            bool Gender = false;
                            try
                            {
                                if (xml.Element("PatientSex").Value.ToLower() == "true" || xml.Element("PatientSex").Value.ToLower() == "1")
                                    Gender = true;
                                else
                                    Gender = false;
                            }
                            catch { }


                            OrthoOuterInformation orthoInfo = new OrthoOuterInformation
                            {
                                CaseDirectoryPath = Path.GetDirectoryName(XmlPath) + @"\",
                                PatientID = xml.Element("PatientID").Value,
                                PatientName = xml.Element("PatientName").Value,
                                PatientSex = Gender
                        };
                            try { orthoInfo.PatientBirth = Convert.ToDateTime(xml.Element("PatientBday").Value); } catch { orthoInfo.PatientBirth = new DateTime(); }
                            try { orthoInfo.PatientAddress = xml.Element("PatientAddress").Value; } catch { orthoInfo.PatientAddress = ""; }
                            try { orthoInfo.DentistName = xml.Element("DentistName").Value; } catch { orthoInfo.DentistName = ""; }
                            try { orthoInfo.ClinicName = xml.Element("ClinicName").Value; } catch { orthoInfo.ClinicName = ""; }
                            try { orthoInfo.CreateDate = Convert.ToDateTime(xml.Element("CreateTime")?.Value); } catch { orthoInfo.CreateDate = fInfo.CreationTime; }
                            
                            Caselist_OrthoOuterCase.Add(orthoInfo);

                            return true;
                        }
                        catch(Exception ex)
                        {
                            log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "ProjectHandle.cs LoadouterXml(Ortho) Exception", ex.Message);
                            return false;
                        }
                    }
                case (int)_softwareID.Tray:
                    {
                        if (xmlDoc.Element("GuideExport") == null)
                            return false;

                        try
                        {
                            XElement xml = xmlDoc.Element("GuideExport");
                            string xGuideType = xml.Element("OrderInfo").Element("GuideType").Value;
                            string xDesignStep = xml.Element("OrderInfo").Element("DesignStep").Value;//DesignStep
                            string xOrderID = xml.Element("ProjectName").Value;//ordernumer-custom

                            TrayInformation trayInfo = new TrayInformation
                            {
                                OrderID = xOrderID,
                                Brand = xml.Element("OrderInfo").Element("Brand").Value,
                                CreateDate = fInfo.CreationTime,
                                ModifyDate = fInfo.LastWriteTime,
                                GuideType = Convert.ToInt16(xGuideType),
                                DesignStep = Convert.ToInt32(xDesignStep),
                                CaseDirectoryPath = Path.GetDirectoryName(XmlPath),
                                CaseXmlPath = XmlPath
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
                            return false;

                        try
                        {
                            XElement xml = xmlDoc.Element("GuideExport");
                            string xGuideType = xml.Element("OrderInfo").Element("GuideType").Value;
                            string xDesignStep = xml.Element("OrderInfo").Element("DesignStep").Value;//DesignStep
                            string xOrderID = xml.Element("ProjectName").Value;//ordernumer-custom

                            SplintInformation splintInfo = new SplintInformation
                            {
                                OrderID = xOrderID,
                                Brand = xml.Element("OrderInfo").Element("Brand").Value,
                                CreateDate = fInfo.CreationTime,
                                ModifyDate = fInfo.LastWriteTime,
                                GuideType = Convert.ToInt16(xGuideType),
                                DesignStep = Convert.ToInt32(xDesignStep),
                                CaseDirectoryPath = Path.GetDirectoryName(XmlPath),
                                CaseXmlPath = XmlPath
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
