﻿using Newtonsoft.Json;
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
            public string user_id { get; set; }

            [JsonProperty("lastlogin")]
            public long lastlogin { get; set; }

            [JsonProperty("usergroup")]
            public string usergroup { get; set; }

            _Login_UserInfo()
            {
                user_id = "";
                lastlogin = -1;
                usergroup = "";
            }
        }
        /// <summary>
        /// User詳細訊息
        /// </summary>
        private class _UserDetail
        {
            [JsonProperty("uid")]
            public string uid { get; set; }
            [JsonProperty("email")]
            public string email { get; set; }
            [JsonProperty("name")]
            public string name { get; set; }

            _UserDetail()
            {
                uid = "";
                email = "";
                name = "";
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
        /// 登入
        /// </summary>
        /// <param name="id">帳號</param>
        /// <param name="passwd">密碼</param>
        /// <param name="uid">uid</param>
        /// <param name="mail">email</param>
        /// <param name="UserName">user名稱</param>
        /// <param name="except">例外</param>
        /// <returns></returns>
        public bool Login(string id, string passwd,ref string uid, ref string mail, ref string UserName, ref WebException except)
        {
            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes("account=" + id + "&password=" + passwd);
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
                UserDetailInfo(ref uid, ref mail, ref UserName);

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
            _uid = json.uid;
            _mail = json.email;
            _UserName = json.name;
        }
    }
}
