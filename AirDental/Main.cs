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

        public bool Login(string id, string passwd, ref WebException except)
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
                except = ex;
                return false;
            }
        }
    }
}
