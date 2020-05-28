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
using ImplantSmallCaseInformation = OrderManagerNew.UserControls.Order_ImplantSmallcase.ImplantSmallCaseInformation;
using OrthoOuterInformation = OrderManagerNew.UserControls.Order_orthoBase.OrthoOuterInformation;
using OrthoSmallCaseInformation = OrderManagerNew.UserControls.Order_orthoSmallcase.OrthoSmallCaseInformation;

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

        public ProjectHandle()
        {
            log = new LogRecorder();
        }

        /// <summary>
        /// 讀取EZCAD專案
        /// </summary>
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

        /// <summary>
        /// 讀取Tray專案
        /// </summary>
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

        /// <summary>
        /// 讀取Splint專案
        /// </summary>
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

        /// <summary>
        /// 讀取ImplantPlanning專案
        /// </summary>
        public void LoadImplantProj()
        {
            string implant_projectDirectory = Properties.Settings.Default.implant_projectDirectory;
            string implant_exePath = Properties.Settings.Default.implant_exePath;

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

                    Caselist_ImplantOuterCase[Caselist_ImplantOuterCase.Count - 1].List_smallcase = new List<UserControls.Order_ImplantSmallcase>();
                    foreach (string filename in Directory.GetFiles(folder.FullName))
                    {
                        // 這層是C:\IntewareData\Implant\2020130102946\
                        //找有幾個tii檔就等於有幾個Implant要給Guide的檔
                        if (Path.GetExtension(filename).ToLower() == ".tii")
                        {
                            OrderManagerNew.UserControls.Order_ImplantSmallcase ImplantSmallCase = new OrderManagerNew.UserControls.Order_ImplantSmallcase();
                            //記錄內部專案資料夾名稱(就是OrderName)、Guide專案資料夾路徑和檢查是否有從Guide輸出的模型
                            ImplantSmallCaseInformation impInfo = new ImplantSmallCaseInformation
                            {
                                OrderName = Path.GetFileNameWithoutExtension(filename),
                                ImplantTiiPath = filename
                            };
                            impInfo.GuideCaseDir = folder.FullName + @"\" + impInfo.OrderName + @"\LinkStation\";
                            //TODO 這邊會有bug
                            string tmpGuideModelDir = folder.FullName + @"\" + impInfo.OrderName + @"\LinkStation\ManufacturingDir\";
                            if (Directory.Exists(tmpGuideModelDir) == true)
                            {
                                string[] guideModel = Directory.GetFiles(tmpGuideModelDir);
                                if (guideModel.Length > 0)
                                    impInfo.GuideModelPath = guideModel[0];
                                else
                                    impInfo.GuideModelPath = "";
                            }
                            else
                                impInfo.GuideModelPath = "";

                            ImplantSmallCase.SetImplantSmallCaseInfo(impInfo);
                            Caselist_ImplantOuterCase[Caselist_ImplantOuterCase.Count - 1].List_smallcase.Add(ImplantSmallCase);
                        }
                    }
                }
            }

            if(Caselist_ImplantOuterCase.Count > 0)
                CaseShowEvent((int)_softwareID.Implant);
        }

        /// <summary>
        /// 讀取OrthoAnalysis專案
        /// </summary>
        public void LoadOrthoProj()
        {
            string ortho_exePath = Properties.Settings.Default.ortho_exePath;
            string ortho_projectDirectory = Properties.Settings.Default.ortho_projectDirectory;

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

                    Caselist_OrthoOuterCase[Caselist_OrthoOuterCase.Count - 1].List_smallcase = new List<UserControls.Order_orthoSmallcase>();
                    //蒐集OrthoSmallcase然後存進OuterCase
                    DirectoryInfo dInfo2 = new DirectoryInfo(folder.FullName + @"\");
                    foreach (DirectoryInfo folder2 in dInfo2.GetDirectories())
                    {
                        // 這層是C:\IntewareData\OrthoAnalysisV3\OrthoData\Test_1216\folder2\
                        string SmallXmlPath = folder2.FullName + @"\" + folder + ".xml";
                        if (File.Exists(SmallXmlPath) == false)
                            continue;
                        else
                        {
                            XDocument xmlDoc;
                            FileInfo fInfo = new FileInfo(SmallXmlPath);//要取得檔案創建日期和修改日期

                            try
                            {
                                xmlDoc = XDocument.Load(SmallXmlPath);
                            }
                            catch(Exception ex)
                            {
                                log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "ProjectHandle.cs LoadXml(Ortho smallcase) Exception", ex.Message);
                                continue;
                            }

                            try
                            {
                                var orthodata = EZOrthoDataStructure.ProjectDataWrapper.ProjectDataWrapperDeserialize(SmallXmlPath);

                                OrthoSmallCaseInformation tmpOrthosmallInfo = new OrthoSmallCaseInformation();
                                //tmpOrthosmallInfo.SoftwareVer = new Version(orthodata.File_Version);
                                tmpOrthosmallInfo.WorkflowStep = Convert.ToInt16(orthodata.workflowstep);
                                tmpOrthosmallInfo.CreateTime = orthodata.patientInformation.m_CreateTime;
                                tmpOrthosmallInfo.Describe = orthodata.patientInformation.m_Discribe;

                                UserControls.Order_orthoSmallcase tmporthoSmallcase = new UserControls.Order_orthoSmallcase();
                                tmporthoSmallcase.SetOrthoSmallCaseInfo(tmpOrthosmallInfo);
                                Caselist_OrthoOuterCase[Caselist_OrthoOuterCase.Count - 1].List_smallcase.Add(tmporthoSmallcase);
                            }
                            catch(Exception ex)
                            {
                                log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "ProjectHandle.cs LoadXml(Ortho smallcase) Exception2", ex.Message);
                                continue;
                            }
                        }
                    }
                }
            }

            if (Caselist_OrthoOuterCase.Count > 0)
                CaseShowEvent((int)_softwareID.Ortho);
        }

        /// <summary>
        /// 讀各專案Xml檔內的資料
        /// </summary>
        /// <param name="SoftwareID">軟體ID 參考_SoftwareID</param>
        /// <param name="XmlPath">Xml路徑</param>
        /// <returns></returns>
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
                            ImplantOuterInformation tmpImpOuterInfo = new ImplantOuterInformation();

                            XElement xml = xmlDoc.Element("ImplantOrderExport");
                            tmpImpOuterInfo.OrderNumber = xml.Element("OrderInfo").Element("OrderNo").Value;
                            tmpImpOuterInfo.PatientName = xml.Element("OrderInfo").Element("PatientName").Value;
                            string pbirthday = xml.Element("OrderInfo").Element("PatientBirthday").Value + "T00:00:00";
                            tmpImpOuterInfo.PatientBirth = Convert.ToDateTime(pbirthday);
                            tmpImpOuterInfo.Clinic = xml.Element("OrderInfo").Element("Clinic").Value;
                            tmpImpOuterInfo.Note = xml.Element("OrderInfo").Element("Note").Value;

                            tmpImpOuterInfo.CBCTPath = xml.Element("ImageData").Element("CBCTPath").Value;
                            tmpImpOuterInfo.JawPath = xml.Element("ImageData").Element("JawPath").Value;
                            tmpImpOuterInfo.JawTrayPath = xml.Element("ImageData").Element("JawTrayPath").Value;
                            tmpImpOuterInfo.DenturePath = xml.Element("ImageData").Element("DenturePath").Value;
                            tmpImpOuterInfo.CreateDate = fInfo.CreationTime;
                            tmpImpOuterInfo.ModifyDate = fInfo.LastWriteTime;

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
                            if (xml.Element("PatientSex").Value.ToLower() == "true")
                                Gender = true;
                            else
                                Gender = false;

                            OrthoOuterInformation orthoInfo = new OrthoOuterInformation();
                            orthoInfo.PatientID = xml.Element("PatientID").Value;
                            orthoInfo.PatientName = xml.Element("PatientName").Value;
                            orthoInfo.PatientPhone = xml.Element("PatientPhone").Value;
                            orthoInfo.PatientSex = Gender;
                            orthoInfo.PatientBirth = Convert.ToDateTime(xml.Element("PatientBday").Value);
                            orthoInfo.PatientAddress = xml.Element("PatientAddress").Value;
                            orthoInfo.DentistName = xml.Element("DentistName").Value;
                            orthoInfo.ClinicName = xml.Element("ClinicName").Value;
                            orthoInfo.CreateTime = Convert.ToDateTime(xml.Element("CreateTime")?.Value);
                            orthoInfo.ModifyTime = fInfo.LastWriteTime;
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
                            return false;

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
