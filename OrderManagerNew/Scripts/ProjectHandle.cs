using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using CadInformation = OrderManagerNew.UserControls.Order_cadBase.CadInformation;


namespace OrderManagerNew
{
    public class ProjectHandle
    {
        public delegate void caseShowEventHandler(int softwareID);
        public event caseShowEventHandler CaseShowEvent;

        public List<CadInformation> Caselist_EZCAD;
        LogRecorder log;

        public ProjectHandle()
        {
            log = new LogRecorder();
        }

        public void LoadEZCADProj(int CaseStatus)
        {
            /*CadInformation cadInfo = new CadInformation
            {
                OrderID = "2005180858-工單1",
                PatientName = "Howwming",
                CreateDate = new DateTime(2020, 5, 19, 9, 44, 52),
                DesignStep = 1,
                CaseDirectoryPath = @"C:\IntewareData\EZCAD\DentDesign\2005180858-工單1-客戶-患者",
                Client = "客戶",
                Technician = "技工",
                Patient = "患者",
                Note = "備註",
                CADversion = new Version("2.1.20512")
            };*/

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

                    if (LoadXml((int)_softwareID.EZCAD, CaseStatus, XmlPath) == false)
                        continue;
                }
            }
            catch(Exception ex)
            {
                log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "ProjectHandle.cs LoadcadProj Exception", ex.Message);
            }

            if (Caselist_EZCAD.Count > 0)
                CaseShowEvent((int)_softwareID.EZCAD);
        }

        bool LoadXml(int SoftwareID, int CaseStatus, string XmlPath)
        {
            XDocument xmlDoc;

            try
            {
                xmlDoc = XDocument.Load(XmlPath);
            }
            catch
            {
                return false;
            }

            if (CaseStatus == (int)_softwareStatus.Installed)
            {
                switch(SoftwareID)
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
                                string xCreateDate = xml.Element("OrderInfo").Element("CreateDate").Value;//ordernumer-custom
                                string xOrderID = xml.Element("OrderInfo").Element("OrderID").Value;//ordernumer-custom
                                string xClient = xml.Element("OrderInfo").Element("Client").Value;//Client
                                string xTechnician = xml.Element("OrderInfo").Element("Technician").Value;//Technicain
                                string xPatient = xml.Element("OrderInfo").Element("Patient").Value;//patient
                                string xOrderNote = xml.Element("OrderInfo").Element("Note").Value;//Note
                                string xDesignStep = xml.Element("DesignInfo").Element("DesignStep").Value;//DesignStep
                                string xVersion = xml.Element("DesignInfo").Element("Version").Value;//Version
                                
                                if (xPatient == "")
                                    xPatient = "No name";
                                
                                CadInformation cadInfo = new CadInformation
                                {
                                    OrderID = xOrderID,
                                    PatientName = xPatient,
                                    CreateDate = DateTime.Parse(xCreateDate, System.Globalization.CultureInfo.InvariantCulture),
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
                            catch(Exception ex)
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
                }
            }

            return true;
        }
    }
}
