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
        public static CookieContainer cookies = new CookieContainer();
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
        /// AirDental登入
        /// </summary>
        /// <param name="loginData">[0]:API網址 [1]:帳號 [2]:密碼</param>
        /// <param name="userDetail">[0]:uid [1]:mail [2]:userName</param>
        /// <param name="except"></param>
        /// <returns></returns>
        public bool Login(string[] loginData,ref string[] userDetail, ref WebException except)
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
                request.CookieContainer = cookies;
                Stream requestStream = request.GetRequestStream();
                requestStream.Write(bytes, 0, bytes.Length);
                requestStream.Close();
                //Response資料
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                string WebContent = new StreamReader(response.GetResponseStream()).ReadToEnd();
                response.Close();
                UserDetailInfo(ref userDetail[0], ref userDetail[1], ref userDetail[2]);

                return true;
            }
            catch (WebException ex)
            {
                except = ex;
                return false;
            }
        }

        private void UserDetailInfo(ref string _uid, ref string _mail, ref string _UserName)
        {
            //https://airdental.inteware.com.tw/api/userinfo
            string web_Detail = APIPortal + "userinfo";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(web_Detail);
            request.Credentials = CredentialCache.DefaultCredentials;
            request.CookieContainer = cookies;
            request.UserAgent = ".NET Framework Example Client";
            request.Method = "GET";
            //Response資料
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            string WebContent = new StreamReader(response.GetResponseStream()).ReadToEnd();
            response.Close();
            //CloudFileConnector.OrderLists json = JsonConvert.DeserializeObject<CloudFileConnector.OrderLists>(end);
            _UserDetail json = JsonConvert.DeserializeObject<_UserDetail>(WebContent);
            _uid = json.Uid;
            _mail = json.Email;
            _UserName = json.Name;
        }
    }
}
