using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OrderManagerNew
{
    class AirDentalProjectHandle
    {
        //委派到MainWindow.xaml.cs裡面的SnackBarShow(string)
        public delegate void AirDentalProjHandleEventHandler_snackbar(string message);
        public event AirDentalProjHandleEventHandler_snackbar Handler_snackbarShow;
        //委派到MainWindow.xaml.cs裡面的Handler_SetCaseShow_Airdental
        public delegate void caseShowEventHandler(int softwareID);
        public event caseShowEventHandler AirdentalProjectShowEvent;

        //委派到MainWindow.xaml.cs裡面CloudCaseHandler_Ortho_showSingleProject()
        public delegate void AirD_orthoBaseEventHandler(int projectIndex);
        public event AirD_orthoBaseEventHandler Main_orthoSetAirDentalProjectShow;
        //委派到MainWindow.xaml.cs裡面的CloudCaseHandler_Ortho_showDetail()
        public delegate void AirD_orthoBaseEventHandler2(int BaseCaseIndex, int SmallCaseIndex);
        public event AirD_orthoBaseEventHandler2 Main_orthoSetSmallOrderDetailShow;
        //委派到MainWindow.xaml.cs裡面CloudCaseHandler_Implant_showSingleProject()
        public delegate void AirD_implantBaseEventHandler(int projectIndex);
        public event AirD_implantBaseEventHandler Main_implantSetAirDentalProjectShow;
        //委派到MainWindow.xaml.cs裡面的CloudCaseHandler_Implant_showDetail()
        public delegate void AirD_implantBaseEventHandler2(int BaseCaseIndex, int SmallCaseIndex);
        public event AirD_implantBaseEventHandler2 Main_implantSetSmallOrderDetailShow;
        //委派到MainWindow.xaml.cs裡面CloudCaseHandler_CAD_showSingleProject()
        public delegate void AirD_cadBaseEventHandler(int projectIndex);
        public event AirD_cadBaseEventHandler Main_cadSetAirDentalProjectShow;
        //委派到MainWindow.xaml.cs裡面的CloudCaseHandler_CAD_showDetail()
        public delegate void AirD_cadBaseEventHandler2(int BaseCaseIndex, int SmallCaseIndex);
        public event AirD_cadBaseEventHandler2 Main_cadSetSmallOrderDetailShow;

        public string APIPortal = "https://airdental.inteware.com.tw/api/";
        public Dll_Airdental.Main Airdental;
        public List<AirDental_UserControls.AirD_orthoBase> Projectlist_Ortho;
        public List<AirDental_UserControls.AirD_implantBase> Projectlist_Implant;
        public List<AirDental_UserControls.AirD_cadBase> Projectlist_CAD;
        /// <summary>
        /// 日誌檔cs
        /// </summary>
        LogRecorder Log;
        Dll_Airdental.Main.OrthoTotalProjects TotalOrthoProjects;
        Dll_Airdental.Main.ImplantTotalProjects TotalImplantProjects;
        Dll_Airdental.Main.CADTotalProjects TotalCADProjects;
        BackgroundWorker AirD_BackgroundWorker;
        WebException AirD_Exception;
        bool RogerRoger;

        public AirDentalProjectHandle()
        {
            Log = new LogRecorder();
            AirD_Exception = null;
            RogerRoger = false;
        }

        void DoWork_ortho(object sender, DoWorkEventArgs e)
        {
            AirD_Exception = Airdental.GetOrthoProject(ref TotalOrthoProjects);
            if (AirD_Exception == null)
            {
                RogerRoger = true;
            }
            else
            {
                if (AirD_Exception.Message != "")
                    Handler_snackbarShow(AirD_Exception.Message);
            }
        }
        void DoWork_implant(object sender, DoWorkEventArgs e)
        {
            AirD_Exception = Airdental.GetImplantProject(ref TotalImplantProjects);
            if (AirD_Exception == null)
            {
                RogerRoger = true;
            }
            else
            {
                if (AirD_Exception.Message != "")
                    Handler_snackbarShow(AirD_Exception.Message);
            }
        }
        void DoWork_cad(object sender, DoWorkEventArgs e)
        {
            AirD_Exception = Airdental.GetCADProject(ref TotalCADProjects);
            if (AirD_Exception == null)
            {
                RogerRoger = true;
            }
            else
            {
                if (AirD_Exception.Message != "")
                    Handler_snackbarShow(AirD_Exception.Message);
            }
        }
        void CompletedWork_ortho(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if(RogerRoger == true)
                    LoadOrthoProjects();
            }
            else
            {
                
            }
        }
        void CompletedWork_implant(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (RogerRoger == true)
                    LoadImplantProjects();
            }
            else
            {

            }
        }
        void CompletedWork_cad(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (RogerRoger == true)
                    LoadCADProjects();
            }
            else
            {

            }
        }

        /// <summary>
        /// 檢查Cookie是否還可以用
        /// </summary>
        /// <param name="uInfo">UserInfo[3]</param>
        /// <returns></returns>
        public bool OrderManagerLoginCheck(ref string[] uInfo)
        {
            WebException _exception = Airdental.UserDetailInfo(APIPortal, ref uInfo, Properties.Settings.Default.AirdentalCookie);
            if (Properties.Settings.Default.AirdentalCookie != "" && _exception == null)
            {
                //Cookie還可以用
                Properties.OrderManagerProps.Default.AirD_uid = uInfo[(int)_AirD_LoginDetail.UID];
                Properties.OrderManagerProps.Default.AirDentalAPI = APIPortal;
                return true;
            }
            else
            {
                Properties.Settings.Default.AirdentalCookie = "";
                Properties.Settings.Default.Save();
                return false;
            }
        }
        /// <summary>
        /// 登出
        /// </summary>
        public void UserLogout()
        {
            WebException _exception = Airdental.Logout();
            if (_exception == null)
            {
                Handler_snackbarShow(TranslationSource.Instance["Logout"] + TranslationSource.Instance["Successfully"]);
                Log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "AirDentalProjectHandle_UserLogout()", "Success");
            }
            else
            {
                Handler_snackbarShow(TranslationSource.Instance["Logout"] + TranslationSource.Instance["Successfully"] + "-Cookie error");
                Log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "AirDentalProjectHandle_UserLogout()", "cookie error");
            }
            Properties.Settings.Default.AirdentalCookie = "";
            Properties.Settings.Default.Save();
        }
        private void LoadOrthoProjects()
        {
            int count = 0;
            Projectlist_Ortho = new List<AirDental_UserControls.AirD_orthoBase>();
            foreach (var orthoProject in TotalOrthoProjects.List_orthoProjects)
            {
                //日期過濾
                switch(Properties.OrderManagerProps.Default.DateFilter)
                {
                    case (int)_DateFilter.Today:
                        {
                            if(orthoProject._Date.DateTime.ToLongDateString() != DateTime.Today.ToLongDateString())
                                continue;
                            break;
                        }
                    case (int)_DateFilter.ThisWeek:
                        {
                            if (orthoProject._Date.DateTime < DateTime.Today.AddDays(-7))
                                continue;
                            break;
                        }
                    case (int)_DateFilter.LastTwoWeek:
                        {
                            if (orthoProject._Date.DateTime < DateTime.Today.AddDays(-14))
                                continue;
                            break;
                        }
                }
                if(Properties.OrderManagerProps.Default.PatientNameFilter != "")
                {
                    //姓名過濾
                    if (orthoProject._Patient.ToLower().IndexOf(Properties.OrderManagerProps.Default.PatientNameFilter.ToLower()) == -1)
                        continue;
                }
                else if(Properties.OrderManagerProps.Default.CaseNameFilter != "")
                {
                    //Case名稱過濾
                    if (orthoProject._SerialNumber.ToLower().IndexOf(Properties.OrderManagerProps.Default.CaseNameFilter.ToLower()) == -1)
                        continue;
                }

                AirDental_UserControls.AirD_orthoBase tmpUserControl_orthoProject = new AirDental_UserControls.AirD_orthoBase
                {
                    orthoBase_AirDental = Airdental
                };
                tmpUserControl_orthoProject.ProjectHandler_snackbarShow += new AirDental_UserControls.AirD_orthoBase.AirD_orthoBaseHandleEventHandler_snackbar(Handler_snackbarShow);
                tmpUserControl_orthoProject.SetAirDentalProjectShow += new AirDental_UserControls.AirD_orthoBase.AirD_orthoBaseEventHandler(Main_orthoSetAirDentalProjectShow);
                tmpUserControl_orthoProject.SetSmallOrderDetailShow += new AirDental_UserControls.AirD_orthoBase.AirD_orthoBaseEventHandler2(Main_orthoSetSmallOrderDetailShow);
                tmpUserControl_orthoProject.SetProjectInfo(orthoProject, count);
                Projectlist_Ortho.Add(tmpUserControl_orthoProject);
                count++;
            }

            AirdentalProjectShowEvent((int)_softwareID.Ortho);
            Mouse.OverrideCursor = Cursors.Arrow;
        }
        private void LoadImplantProjects()
        {
            int count = 0;
            Projectlist_Implant = new List<AirDental_UserControls.AirD_implantBase>();
            foreach (var implantProject in TotalImplantProjects.List_implantProjects)
            {
                //日期過濾
                switch (Properties.OrderManagerProps.Default.DateFilter)
                {
                    case (int)_DateFilter.Today:
                        {
                            if (implantProject._Date.DateTime.ToLongDateString() != DateTime.Today.ToLongDateString())
                                continue;
                            break;
                        }
                    case (int)_DateFilter.ThisWeek:
                        {
                            if (implantProject._Date.DateTime < DateTime.Today.AddDays(-7))
                                continue;
                            break;
                        }
                    case (int)_DateFilter.LastTwoWeek:
                        {
                            if (implantProject._Date.DateTime < DateTime.Today.AddDays(-14))
                                continue;
                            break;
                        }
                }
                if (Properties.OrderManagerProps.Default.PatientNameFilter != "")
                {
                    //姓名過濾
                    if (implantProject._Patient.ToLower().IndexOf(Properties.OrderManagerProps.Default.PatientNameFilter.ToLower()) == -1)
                        continue;
                }
                else if (Properties.OrderManagerProps.Default.CaseNameFilter != "")
                {
                    //Case名稱過濾
                    if (implantProject._SerialNumber.ToLower().IndexOf(Properties.OrderManagerProps.Default.CaseNameFilter.ToLower()) == -1)
                        continue;
                }

                AirDental_UserControls.AirD_implantBase tmpUserControl_implantProject = new AirDental_UserControls.AirD_implantBase
                {
                    implantBase_AirDental = Airdental
                };
                tmpUserControl_implantProject.SetAirDentalProjectShow += new AirDental_UserControls.AirD_implantBase.AirD_implantBaseEventHandler(Main_implantSetAirDentalProjectShow);
                tmpUserControl_implantProject.SetSmallOrderDetailShow += new AirDental_UserControls.AirD_implantBase.AirD_implantBaseEventHandler2(Main_implantSetSmallOrderDetailShow);
                tmpUserControl_implantProject.SetProjectInfo(implantProject, count);
                Projectlist_Implant.Add(tmpUserControl_implantProject);
                count++;
            }

            AirdentalProjectShowEvent((int)_softwareID.Implant);
            Mouse.OverrideCursor = Cursors.Arrow;
        }
        private void LoadCADProjects()
        {
            int count = 0;
            Projectlist_CAD = new List<AirDental_UserControls.AirD_cadBase>();
            foreach (var cadProject in TotalCADProjects.List_cadProjects)
            {
                //日期過濾
                switch (Properties.OrderManagerProps.Default.DateFilter)
                {
                    case (int)_DateFilter.Today:
                        {
                            if (cadProject._Date.DateTime.ToLongDateString() != DateTime.Today.ToLongDateString())
                                continue;
                            break;
                        }
                    case (int)_DateFilter.ThisWeek:
                        {
                            if (cadProject._Date.DateTime < DateTime.Today.AddDays(-7))
                                continue;
                            break;
                        }
                    case (int)_DateFilter.LastTwoWeek:
                        {
                            if (cadProject._Date.DateTime < DateTime.Today.AddDays(-14))
                                continue;
                            break;
                        }
                }
                if (Properties.OrderManagerProps.Default.PatientNameFilter != "")
                {
                    //姓名過濾
                    if (cadProject._Patient.ToLower().IndexOf(Properties.OrderManagerProps.Default.PatientNameFilter.ToLower()) == -1)
                        continue;
                }
                else if (Properties.OrderManagerProps.Default.CaseNameFilter != "")
                {
                    //Case名稱過濾
                    if (cadProject._SerialNumber.ToLower().IndexOf(Properties.OrderManagerProps.Default.CaseNameFilter.ToLower()) == -1)
                        continue;
                }

                AirDental_UserControls.AirD_cadBase tmpUserControl_cadProject = new AirDental_UserControls.AirD_cadBase
                {
                    cadBase_AirDental = Airdental
                };
                tmpUserControl_cadProject.SetAirDentalProjectShow += new AirDental_UserControls.AirD_cadBase.AirD_cadBaseEventHandler(Main_cadSetAirDentalProjectShow);
                tmpUserControl_cadProject.SetSmallOrderDetailShow += new AirDental_UserControls.AirD_cadBase.AirD_cadBaseEventHandler2(Main_cadSetSmallOrderDetailShow);
                tmpUserControl_cadProject.SetProjectInfo(cadProject, count);
                Projectlist_CAD.Add(tmpUserControl_cadProject);
                count++;
            }

            AirdentalProjectShowEvent((int)_softwareID.EZCAD);
            Mouse.OverrideCursor = Cursors.Arrow;
        }
        
        public void ReceiveOrthoProjects()
        {
            AirD_Exception = null;
            RogerRoger = false;
            Mouse.OverrideCursor = Cursors.Wait;
            AirD_BackgroundWorker = new BackgroundWorker();
            AirD_BackgroundWorker.DoWork += new DoWorkEventHandler(DoWork_ortho);
            AirD_BackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(CompletedWork_ortho);
            AirD_BackgroundWorker.RunWorkerAsync(this);
        }
        public void ReceiveImplantProjects()
        {
            AirD_Exception = null;
            RogerRoger = false;
            Mouse.OverrideCursor = Cursors.Wait;
            AirD_BackgroundWorker = new BackgroundWorker();
            AirD_BackgroundWorker.DoWork += new DoWorkEventHandler(DoWork_implant);
            AirD_BackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(CompletedWork_implant);
            AirD_BackgroundWorker.RunWorkerAsync(this);
        }
        public void ReceiveCADProjects()
        {
            AirD_Exception = null;
            RogerRoger = false;
            Mouse.OverrideCursor = Cursors.Wait;
            AirD_BackgroundWorker = new BackgroundWorker();
            AirD_BackgroundWorker.DoWork += new DoWorkEventHandler(DoWork_cad);
            AirD_BackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(CompletedWork_cad);
            AirD_BackgroundWorker.RunWorkerAsync(this);
        }

        /// <summary>
        /// 檢查是否有AirDental下載資料夾
        /// </summary>
        public void CheckAirDentalDirExist()
        {
            if(Properties.OrderManagerProps.Default.systemDisk == Properties.OrderManagerProps.Default.mostsoftwareDisk)
            {
                string AirdentalDir = Properties.OrderManagerProps.Default.systemDisk + @"IntewareData\AirDental\";
                if (Directory.Exists(AirdentalDir) == false)
                    Directory.CreateDirectory(AirdentalDir);
                if (Directory.Exists(AirdentalDir + @"CAD\") == false)
                    Directory.CreateDirectory(AirdentalDir + @"CAD\");
                if (Directory.Exists(AirdentalDir + @"Implant\") == false)
                    Directory.CreateDirectory(AirdentalDir + @"Implant\");
                if (Directory.Exists(AirdentalDir + @"Ortho\") == false)
                    Directory.CreateDirectory(AirdentalDir + @"Ortho\");

                Properties.OrderManagerProps.Default.AirD_CAD_Dir = AirdentalDir + @"CAD\";
                Properties.OrderManagerProps.Default.AirD_Implant_Dir = AirdentalDir + @"Implant\";
                Properties.OrderManagerProps.Default.AirD_Ortho_Dir = AirdentalDir + @"Ortho\";
            }
            else
            {
                string SysAirdentalDir = Properties.OrderManagerProps.Default.systemDisk + @"IntewareData\AirDental\";
                string MostDiskAirdentalDir = Properties.OrderManagerProps.Default.mostsoftwareDisk + @"IntewareData\AirDental\";

                if(Directory.Exists(MostDiskAirdentalDir) == true)
                {
                    if (Directory.Exists(MostDiskAirdentalDir + @"CAD\") == false)
                        Directory.CreateDirectory(MostDiskAirdentalDir + @"CAD\");
                    if (Directory.Exists(MostDiskAirdentalDir + @"Implant\") == false)
                        Directory.CreateDirectory(MostDiskAirdentalDir + @"Implant\");
                    if (Directory.Exists(MostDiskAirdentalDir + @"Ortho\") == false)
                        Directory.CreateDirectory(MostDiskAirdentalDir + @"Ortho\");

                    Properties.OrderManagerProps.Default.AirD_CAD_Dir = MostDiskAirdentalDir + @"CAD\";
                    Properties.OrderManagerProps.Default.AirD_Implant_Dir = MostDiskAirdentalDir + @"Implant\";
                    Properties.OrderManagerProps.Default.AirD_Ortho_Dir = MostDiskAirdentalDir + @"Ortho\";
                }
                else
                {
                    if (Directory.Exists(SysAirdentalDir) == false)
                        Directory.CreateDirectory(SysAirdentalDir);
                    if (Directory.Exists(SysAirdentalDir + @"CAD\") == false)
                        Directory.CreateDirectory(SysAirdentalDir + @"CAD\");
                    if (Directory.Exists(SysAirdentalDir + @"Implant\") == false)
                        Directory.CreateDirectory(SysAirdentalDir + @"Implant\");
                    if (Directory.Exists(SysAirdentalDir + @"Ortho\") == false)
                        Directory.CreateDirectory(SysAirdentalDir + @"Ortho\");

                    Properties.OrderManagerProps.Default.AirD_CAD_Dir = SysAirdentalDir + @"CAD\";
                    Properties.OrderManagerProps.Default.AirD_Implant_Dir = SysAirdentalDir + @"Implant\";
                    Properties.OrderManagerProps.Default.AirD_Ortho_Dir = SysAirdentalDir + @"Ortho\";
                }
            }
        }
    }
}
