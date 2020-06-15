using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
//C# HttpWebRequest保存cookies模擬登入的方法！ http://www.piaoyi.org/c-sharp/c-httpwebrequest-cookies.html

namespace Dll_Airdental
{
    public class Main
    {
        static private bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {return true; }// Always accept
#region 變數
        /// <summary>
        /// Airdental API網址
        /// </summary>
        public string APIPortal = "https://airdental.inteware.com.tw/api/";
        private string ProjectLimit = @"?limit=100";
        /// <summary>
        /// Airdental Cookie
        /// </summary>
        private string CookieStr;
#endregion

#region Class宣告
        /// <summary>
        /// 登入後顯示UserInfo
        /// </summary>
        private class _Login_UserInfo
        {
            [JsonProperty("user_id")]
            public string User_id { get; set; }

            [JsonProperty("lastlogin")]
            public long Lastlogin { get; set; }

            [JsonProperty("usergroup")]
            public string Usergroup { get; set; }
        }
        /// <summary>
        /// User詳細訊息
        /// </summary>
        private class _UserDetail
        {
            [JsonProperty("uid")]
            public string Uid { get; set; }
            [JsonProperty("usergroup")]
            public string Usergroup { get; set; }
            [JsonProperty("email")]
            public string Email { get; set; }
            [JsonProperty("name")]
            public string Name { get; set; }
        }
        /// <summary>
        /// 訂單清單分頁資料
        /// </summary>
        public  class _Pagination
        {
            [JsonProperty("total")]
            public int Total { get; set; }
            [JsonProperty("current")]
            public int Current { get; set; }
            [JsonProperty("pageSize")]
            public int PageSize { get; set; }
        }
        /// <summary>
        /// Ortho訂單
        /// </summary>
        public class _orthoOrder
        {
            [JsonProperty("key")]
            public string _Key { get; set; }
            [JsonProperty("group")]
            public string _group { get; set; }
            [JsonProperty("isAuthor")]
            public bool _isAuthor { get; set; }
            [JsonProperty("stage")]
            public string _stage { get; set; }
            [JsonProperty("stageKey")]
            public string _stageKey { get; set; }
            [JsonProperty("stageOrig")]
            public string _stageOrig { get; set; }
            [JsonProperty("action")]
            public string _action { get; set; }
            [JsonProperty("actionKey")]
            public string _actionKey { get; set; }
            [JsonProperty("instruction")]
            public string _instruction { get; set; }
            [JsonProperty("date")]
            public DateTimeOffset _date { get; set; }
            [JsonProperty("showViewer")]
            public string _showViewer { get; set; }
            [JsonProperty("viewerurl")]
            public string _viewerurl { get; set; }
            [JsonProperty("showRetakes")]
            public bool showRetakes { get; set; }
            [JsonProperty("showDraftEdit")]
            public bool _showDraftEdit { get; set; }
            [JsonProperty("showDetail")]
            public bool _showDetail { get; set; }
            [JsonProperty("showDispatch")]
            public bool _showDispatch { get; set; }
            [JsonProperty("showManufacture")]
            public bool _showManufacture { get; set; }
            [JsonProperty("showBraces")]
            public bool _showBraces { get; set; }
            [JsonProperty("showShip")]
            public bool _showShip { get; set; }
            [JsonProperty("showTransport")]
            public bool _showTransport { get; set; }
            [JsonProperty("showPhotoEdit")]
            public bool _showPhotoEdit { get; set; }
            [JsonProperty("showMeasurement")]
            public bool _showMeasurement { get; set; }
        }
        /// <summary>
        /// ortho專案
        /// </summary>
        public class _orthoProject
        {
            [JsonProperty("key")]
            public string _Key { get; set; }
            [JsonProperty("group")]
            public string _Group { get; set; }
            [JsonProperty("serialnum")]
            public string _SerialNumber { get; set; }
            [JsonProperty("customSerialnum")]
            public string _CustomSerialNumber { get; set; }
            [JsonProperty("patient")]
            public string _Patient { get; set; }
            [JsonProperty("clinic")]
            public string _Clinic { get; set; }
            [JsonProperty("action")]
            public string _Action { get; set; }
            [JsonProperty("actionKey")]
            public string _ActionKey { get; set; }
            [JsonProperty("stage")]
            public string _Stage { get; set; }
            [JsonProperty("stageKey")]
            public string _StageKey { get; set; }
            [JsonProperty("status")]
            public string _Status { get; set; }
            [JsonProperty("doctor")]
            public string _Doctor { get; set; }
            [JsonProperty("date")]
            public DateTimeOffset _Date { get; set; }
            [JsonProperty("instruction")]
            public string _Instruction { get; set; }
            [JsonProperty("patientAvatar")]
            public string _PatientAvatar { get; set; }
            [JsonProperty("txTreatedArch")]
            public string _TxTreatedArch { get; set; }
            [JsonProperty("productType")]
            public string _ProductType { get; set; }
        }
        /// <summary>
        /// ortho專案總覽
        /// </summary>
        public class OrthoTotalProjects
        {
            [JsonProperty("pagination")]
            public _Pagination Pagination { get; set; }
            [JsonProperty("projects")]
            public _orthoProject[] List_orthoProjects { get; set; }
        }

#endregion

        public Main()
        {
            APIPortal = "https://airdental.inteware.com.tw/api/";
            ProjectLimit = @"?limit=100";
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            if (APIPortal.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback =
                        new RemoteCertificateValidationCallback(CheckValidationResult);
            }
        }

#region 登入登出
        /// <summary>
        /// 檢查網路是否正常
        /// </summary>
        public bool CheckServerStatus()
        {
            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes("account=a&password=a");
                string web_login = APIPortal + "login";

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(web_login);
                request.Credentials = CredentialCache.DefaultCredentials;
                request.UserAgent = ".NET Framework Example Client";
                request.Method = "POST";
                request.ContentLength = (long)bytes.Length;
                request.ContentType = "application/x-www-form-urlencoded";
                Stream requestStream = request.GetRequestStream();
                requestStream.Write(bytes, 0, bytes.Length);
                requestStream.Close();
                //Response資料
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                var result = ((HttpWebResponse)response).StatusDescription;
                response.Close();
                return true;
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError)
                    return true;
                else
                    return false;
            }
        }
        /// <summary>
        /// AirDental 登入
        /// </summary>
        /// <param name="loginData">[0]:API網址 [1]:帳號 [2]:密碼</param>
        /// <param name="userDetail">[0]:uid [1]:mail [2]:userName</param>
        /// <param name="except">例外</param>
        /// <returns></returns>
        public WebException Login(string[] loginData,ref string[] userDetail, ref string Importcookie)
        {
            if (loginData[0] != "")
                APIPortal = loginData[0];

            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes("account=" + loginData[1] + "&password=" + loginData[2]);
                string web_login = APIPortal + "login";

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(web_login);
                request.Credentials = CredentialCache.DefaultCredentials;
                request.UserAgent = ".NET Framework Example Client";
                request.Method = "POST";
                request.ContentLength = (long)bytes.Length;
                request.ContentType = "application/x-www-form-urlencoded";
                request.CookieContainer = new CookieContainer();
                Stream requestStream = request.GetRequestStream();
                requestStream.Write(bytes, 0, bytes.Length);
                requestStream.Close();
                //Response資料
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                string WebContent = new StreamReader(response.GetResponseStream()).ReadToEnd();
                response.Close();
                string cookiesstr = request.CookieContainer.GetCookieHeader(request.RequestUri);
                Importcookie = cookiesstr;
                WebException _exception = UserDetailInfo(ref userDetail, cookiesstr);
                if (_exception == null)
                    return null;
                else
                    return _exception;
            }
            catch (WebException ex)
            {
                return ex;
            }
        }
        /// <summary>
        /// User詳細資料
        /// </summary>
        /// <param name="userDetail">[0]:uid [1]:mail [2]:userName</param>
        public WebException UserDetailInfo(ref string[] userDetail,string ImportCookie)
        {
            //https://airdental.inteware.com.tw/api/userinfo
            try
            {   
                string web_Detail = APIPortal + "userinfo";
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(web_Detail);
                request.Credentials = CredentialCache.DefaultCredentials;
                request.Headers.Add("Cookie", ImportCookie);
                request.UserAgent = ".NET Framework Example Client";
                request.Method = "GET";
                //Response資料
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                string WebContent = new StreamReader(response.GetResponseStream()).ReadToEnd();
                response.Close();
                _UserDetail json = JsonConvert.DeserializeObject<_UserDetail>(WebContent);
                userDetail[0] = json.Uid;
                userDetail[1] = json.Usergroup;
                userDetail[2] = json.Email;
                userDetail[3] = json.Name;
                //儲存Cookie
                CookieStr = ImportCookie;
                return null;
            }
            catch(WebException ex)
            {
                return ex;
            }
        }
        /// <summary>
        /// Airdental 登出
        /// </summary>
        /// <param name="except">例外</param>
        /// <returns></returns>
        public WebException Logout()
        {
            try
            {
                string web_logout = APIPortal + "logout";

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(web_logout);
                request.Credentials = CredentialCache.DefaultCredentials;
                request.UserAgent = ".NET Framework Example Client";
                request.Method = "DELETE";
                request.Headers.Add("Cookie", CookieStr);
                //Response資料
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                response.Close();
                return null;
            }
            catch(WebException ex)
            {
                return ex;
            }
        }
#endregion

#region 專案清單
        /// <summary>
        /// 設定每頁最多顯示幾筆
        /// </summary>
        /// <param name="number"></param>
        public void SetProjectLimit(int number)
        {
            if(number > 0)
                ProjectLimit = @"?limit=" + number.ToString();
        }
        /// <summary>
        /// 取得Ortho專案清單
        /// </summary>
        /// <param name="Import">參考的OrthoTotalProjects</param>
        /// <returns></returns>
        public WebException GetOrthoProject(ref OrthoTotalProjects Import)
        {
            //https://airdental.inteware.com.tw/api/project/ortho?limit=100
            try
            {
                string web_orthoProjectLoad = APIPortal + "project/ortho" + ProjectLimit;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(web_orthoProjectLoad);
                request.Credentials = CredentialCache.DefaultCredentials;
                request.Headers.Add("Cookie", CookieStr);
                request.UserAgent = ".NET Framework Example Client";
                request.Method = "GET";
                //Response資料
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                string WebContent = new StreamReader(response.GetResponseStream()).ReadToEnd();
                response.Close();
                Import = JsonConvert.DeserializeObject<OrthoTotalProjects>(WebContent);
                return null;
            }
            catch(WebException ex)
            {
                return ex;
            }
        }
        /// <summary>
        /// 取得Ortho訂單清單
        /// </summary>
        /// <param name="Import">參考的OrderTotalOrders</param>
        /// <param name="pid">Pid</param>
        /// <returns></returns>
        public WebException GetOrthoOrder(ref List<_orthoOrder> Import, string pid)
        {
            //https://airdental.inteware.com.tw/api/project/ortho/history/5dfae5cb7d81ab1580c89922
            try
            {
                string web_orthoOrderLoad = APIPortal + @"project/ortho/history/" + pid;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(web_orthoOrderLoad);
                request.Credentials = CredentialCache.DefaultCredentials;
                request.Headers.Add("Cookie", CookieStr);
                request.UserAgent = ".NET Framework Example Client";
                request.Method = "GET";
                //Response資料
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                string WebContent = new StreamReader(response.GetResponseStream()).ReadToEnd();
                response.Close();
                Import = JsonConvert.DeserializeObject<List<_orthoOrder>>(WebContent);
                return null;
            }
            catch(WebException ex)
            {
                return ex;
            }
        }
#endregion
    }
}
