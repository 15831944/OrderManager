using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
        public event AirD_orthoBaseEventHandler MainSetAirDentalProjectShow;
        //委派到MainWindow.xaml.cs裡面的CloudCaseHandler_Ortho_showDetail()
        public delegate void AirD_orthoBaseEventHandler2(int BaseCaseIndex, int SmallCaseIndex);
        public event AirD_orthoBaseEventHandler2 MainSetSmallOrderDetailShow;

        public Dll_Airdental.Main Airdental;
        public List<AirDental_UserControls.AirD_orthoBase> Projectlist_Ortho;
        /// <summary>
        /// 日誌檔cs
        /// </summary>
        LogRecorder Log;
        Dll_Airdental.Main.OrthoTotalProjects TotalOrthoProjects;
        BackgroundWorker ortho_BackgroundWorker;
        WebException Exception_ortho;
        bool ReceiveOrtho;
        /// <summary>
        /// 訂單清單分頁資料
        /// </summary>
        /*public class _Pagination
        {
            public int Total { get; set; }
            public int Current { get; set; }
            public int PageSize { get; set; }

            public _Pagination()
            {
                Total = -1;
                Current = -1;
                PageSize = -1;
            }
        }
        /// <summary>
        /// orthoCase
        /// </summary>
        public class _orthoCase
        {
            public string _Key { get; set; }
            public string _Group { get; set; }
            public string _SerialNumber { get; set; }
            public string _CustomSerialNumber { get; set; }
            public string _Patient { get; set; }
            public string _Clinic { get; set; }
            public string _Action { get; set; }
            public string _ActionKey { get; set; }
            public string _Stage { get; set; }
            public string _StageKey { get; set; }
            public string _Status { get; set; }
            public string _Doctor { get; set; }
            public DateTimeOffset _Date { get; set; }
            public string _Instruction { get; set; }
            public string _PatientAvatar { get; set; }
            public string _TxTreatedArch { get; set; }
            public string _ProductType { get; set; }

            public _orthoCase()
            {
                _Key = "";
                _Group = "";
                _SerialNumber = "";
                _CustomSerialNumber = "";
                _Patient = "";
                _Clinic = "";
                _Action = "";
                _ActionKey = "";
                _Stage = "";
                _StageKey = "";
                _Status = "";
                _Doctor = "";
                _Date = new DateTimeOffset();
                _Instruction = "";
                _PatientAvatar = "";
                _TxTreatedArch = "";
                _ProductType = "";
            }
        }
        /// <summary>
        /// ortho專案
        /// </summary>
        public class OrthoProject
        {
            public _Pagination Pagination { get; set; }
            public _orthoCase[] List_orthoCase { get; set; }
            public OrthoProject()
            {
                Pagination = new _Pagination();
                List_orthoCase = null;
            }
        }*/

        public AirDentalProjectHandle()
        {
            Log = new LogRecorder();
            Exception_ortho = null;
            ReceiveOrtho = false;
        }

        void DoWork_ortho(object sender, DoWorkEventArgs e)
        {
            Exception_ortho = Airdental.GetOrthoProject(ref TotalOrthoProjects);
            if (Exception_ortho == null)
            {
                ReceiveOrtho = true;
            }
            else
            {
                if (Exception_ortho.Message != "")
                    Handler_snackbarShow(Exception_ortho.Message);
            }
        }
        void CompletedWork_ortho(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if(ReceiveOrtho == true)
                    LoadOrthoProjects();
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
            WebException _exception = Airdental.UserDetailInfo(ref uInfo, Properties.Settings.Default.AirdentalCookie);
            if (Properties.Settings.Default.AirdentalCookie != "" && _exception == null)
            {
                //Cookie還可以用
                Properties.OrderManagerProps.Default.AirD_uid = uInfo[(int)_AirD_LoginDetail.UID];
                return true;
            }
            else
            {
                Properties.Settings.Default.AirdentalCookie = "";
                Properties.Settings.Default.Save();
                return false;
            }
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
                
                AirDental_UserControls.AirD_orthoBase UserControl_orthoProject = new AirDental_UserControls.AirD_orthoBase
                {
                    orthoBase_AirDental = Airdental
                };
                UserControl_orthoProject.SetAirDentalProjectShow += new AirDental_UserControls.AirD_orthoBase.AirD_orthoBaseEventHandler(MainSetAirDentalProjectShow);
                UserControl_orthoProject.SetSmallOrderDetailShow += new AirDental_UserControls.AirD_orthoBase.AirD_orthoBaseEventHandler2(MainSetSmallOrderDetailShow);
                UserControl_orthoProject.SetProjectInfo(orthoProject, count);
                Projectlist_Ortho.Add(UserControl_orthoProject);
                count++;
            }

            AirdentalProjectShowEvent((int)_softwareID.Ortho);
            Mouse.OverrideCursor = Cursors.Arrow;
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

        public void ReceiveOrthoProjects()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            ortho_BackgroundWorker = new BackgroundWorker();
            ortho_BackgroundWorker.DoWork += new DoWorkEventHandler(DoWork_ortho);
            ortho_BackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(CompletedWork_ortho);
            ortho_BackgroundWorker.RunWorkerAsync(this);
        }
    }
}
