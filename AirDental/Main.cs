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

namespace Dll_Airdental
{
    public class Main
    {
        static private bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {return true; }// Always accept

        /// <summary>
        /// Airdental API網址
        /// </summary>
        public string APIPortal;
        /// <summary>
        /// Airdental Cookie
        /// </summary>
        private string CookieStr;
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

            _Login_UserInfo()
            {
                User_id = "";
                Lastlogin = -1;
                Usergroup = "";
            }
        }
        /// <summary>
        /// User詳細訊息
        /// </summary>
        private class _UserDetail
        {
            [JsonProperty("uid")]
            public string Uid { get; set; }
            [JsonProperty("email")]
            public string Email { get; set; }
            [JsonProperty("name")]
            public string Name { get; set; }

            _UserDetail()
            {
                Uid = "";
                Email = "";
                Name = "";
            }
        }

        public Main()
        {
            APIPortal = "https://airdental.inteware.com.tw/api/";
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            if (APIPortal.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback =
                        new RemoteCertificateValidationCallback(CheckValidationResult);
            }
        }

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
        public bool Login(string[] loginData,ref string[] userDetail, ref string Importcookie, ref WebException except)
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
                if (UserDetailInfo(ref userDetail, cookiesstr, ref except) == true)
                    return true;
                else
                    return false;
            }
            catch (WebException ex)
            {
                except = ex;
                return false;
            }
        }

        /// <summary>
        /// User詳細資料
        /// </summary>
        /// <param name="userDetail">[0]:uid [1]:mail [2]:userName</param>
        public bool UserDetailInfo(ref string[] userDetail,string ImportCookie,ref WebException except)
        {
            try
            {
                //https://airdental.inteware.com.tw/api/userinfo
                string web_Detail = APIPortal + "userinfo";
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(web_Detail);
                request.Credentials = CredentialCache.DefaultCredentials;
                //request.CookieContainer = ImportCookie;
                //string cookiesstr = request.CookieContainer.GetCookieHeader(request.RequestUri);
                request.Headers.Add("Cookie", ImportCookie);
                request.UserAgent = ".NET Framework Example Client";
                request.Method = "GET";
                //Response資料
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                string WebContent = new StreamReader(response.GetResponseStream()).ReadToEnd();
                response.Close();
                _UserDetail json = JsonConvert.DeserializeObject<_UserDetail>(WebContent);
                userDetail[0] = json.Uid;
                userDetail[1] = json.Email;
                userDetail[2] = json.Name;
                //儲存Cookie
                CookieStr = ImportCookie;
                return true;
            }
            catch(WebException ex)
            {
                except = ex;
                return false;
            }
        }

        /// <summary>
        /// Airdental 登出
        /// </summary>
        /// <param name="except">例外</param>
        /// <returns></returns>
        public bool Logout(ref WebException except)
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
                return true;
            }
            catch(WebException ex)
            {
                except = ex;
                return false;
            }
        }
    }
}
