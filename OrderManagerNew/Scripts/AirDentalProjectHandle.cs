using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OrderManagerNew
{
    class AirDentalProjectHandle
    {
        //委派到MainWindow.xaml.cs裡面的SnackBarShow(string)
        public delegate void AirDentalProjHandleEventHandler_snackbar(string message);
        public event AirDentalProjHandleEventHandler_snackbar Handler_snackbarShow;

        public Dll_Airdental.Main Airdental;
        /// <summary>
        /// 日誌檔cs
        /// </summary>
        LogRecorder Log;
        Dll_Airdental.Main.OrthoProject TotalOrthoProject;

        /// <summary>
        /// 訂單清單分頁資料
        /// </summary>
        public class _Pagination
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
        }

        public AirDentalProjectHandle()
        {
            Log = new LogRecorder();
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

        public void LoadorthoProjects()
        {
            WebException ex;
            ex = Airdental.GetOrthoProject(ref TotalOrthoProject);
            if(ex == null)
            {

            }
            else
            {
                Handler_snackbarShow(ex.Message);
            }
        }
    }
}
