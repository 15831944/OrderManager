using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using Path = System.IO.Path;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace OrderManagerNew
{
    public partial class BeforeDownload : Window
    {
        static public bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {   // 總是接受  
            return true;
        }
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetDiskFreeSpaceEx(string lpDirectoryName,
        out ulong lpFreeBytesAvailable,
        out ulong lpTotalNumberOfBytes,
        out ulong lpTotalNumberOfFreeBytes);

        public BeforeDownload()
        {
            InitializeComponent();
        }

        private void TitleBar_Click_titlebarButtons(object sender, RoutedEventArgs e)
        {
            Button titleButton = sender as Button;

            switch (titleButton.Name)
            {
                case "systemButton_Close":              //關閉
                    Close();
                    break;
            }
        }

        private void Click_OpenFilePath(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    textbox_InstallPath.Text = dialog.SelectedPath;

                    RemainingSpace(dialog.SelectedPath);
                }
            }
        }

        private void Click_systemButton(object sender, RoutedEventArgs e)
        {
            Button Btn = sender as Button;
            switch(Btn.Name)
            {
                case "sysBtn_Yes":
                    {
                        this.DialogResult = true;
                        break;
                    }
                case "sysBtn_Cancel":
                    {
                        this.DialogResult = false;
                        break;
                    }
            }
        }

        /// <summary>
        /// 用路徑抓出使用者電腦容量
        /// </summary>
        /// <param name="Drive">路徑</param>
        /// <returns></returns>
        bool RemainingSpace(string Drive)
        {
            if (Directory.Exists(Drive) == false)
            {
                System.IO.Directory.CreateDirectory(Drive);
            }

            ulong FreeBytesAvailable;
            ulong TotalNumberOfBytes;
            ulong TotalNumberOfFreeBytes;
            string str = Path.GetPathRoot(Drive);
            bool success = GetDiskFreeSpaceEx(Path.GetPathRoot(Drive), out FreeBytesAvailable, out TotalNumberOfBytes, out TotalNumberOfFreeBytes);

            if (!success)
            {
                MessageBox.Show("Can't get space! try use adnim mode"); //TODO 多國語系
                return false;
            }

            label_AvailableSpace.Content = convertDiskUnit(TotalNumberOfFreeBytes);
            return true;
        }

        /// <summary>
        /// 轉換容量單位
        /// </summary>
        /// <param name="InputBytes">單位是Bytes 使用ulong</param>
        /// <returns></returns>
        string convertDiskUnit(ulong InputBytes)
        {
            string OutputString = "";

            if (((double)(Int64)InputBytes / 1024.0 / 1024.0) < 1.0)//低於1MB就用KB
                OutputString = Convert.ToSingle(Math.Round(((double)(Int64)InputBytes / 1024.0), 1)).ToString() + "KB";
            else if (((double)(Int64)InputBytes / 1024.0 / 1024.0 / 1024.0) < 1.0)//低於1GB就用MB
                OutputString = Convert.ToSingle(Math.Round(((double)(Int64)InputBytes / 1024.0 / 1024.0), 1)).ToString() + "MB";
            else if (((double)(Int64)InputBytes / 1024.0 / 1024.0 / 1024.0 / 1024.0) < 1.0)//低於1TB就用GB
                OutputString = Convert.ToSingle(Math.Round(((double)(Int64)InputBytes / 1024.0 / 1024.0 / 1024.0), 1)).ToString() + "GB";
            else
                OutputString = Convert.ToSingle(Math.Round(((double)(Int64)InputBytes / 1024.0 / 1024.0 / 1024.0 / 1024.0), 1)).ToString() + "TB";

            return OutputString;
        }
        
        /// <summary>
        /// 設定UI所顯示的資訊
        /// </summary>
        /// <param name="http_url">下載檔網址</param>
        /// <param name="SoftwareID">軟體ID(參考EnumSummary的_softwareID)</param>
        /// <returns></returns>
        public bool SetInformation(string http_url, int SoftwareID)
        {
            string softwareName = "";
            switch (SoftwareID)
            {
                case (int)_softwareID.EZCAD:
                    {
                        softwareName = "EZCAD";
                        break;
                    }
                case (int)_softwareID.Implant:
                    {
                        softwareName = "ImplantPlanning";
                        break;
                    }
                case (int)_softwareID.Ortho:
                    {
                        softwareName = "OrthoAnalysis";
                        break;
                    }
                case (int)_softwareID.Tray:
                    {
                        softwareName = "EZCAD Tray";
                        break;
                    }
                case (int)_softwareID.Splint:
                    {
                        softwareName = "EZCAD Splint";
                        break;
                    }
                case (int)_softwareID.Guide:
                    {
                        softwareName = "EZCAD Guide";
                        break;
                    }
            }

            label_TitleBar.Content = OrderManagerNew.TranslationSource.Instance["Install"] + "-" + softwareName.Replace(" ", ".");
            label_Header.Content = OrderManagerNew.TranslationSource.Instance["AboutToInstall"] + " " + softwareName.Replace(" ", ".");
            textbox_InstallPath.Text = @"C:\InteWare\" + softwareName;

            //跳過https檢測 & Win7 相容
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            System.Net.ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);

            /* 檢查User電腦空間 ************************************/

            //Request資料
            HttpWebRequest httpRequest;
            httpRequest = (HttpWebRequest)WebRequest.Create(http_url);
            httpRequest.Credentials = CredentialCache.DefaultCredentials;
            httpRequest.UserAgent = ".NET Framework Example Client";
            httpRequest.Method = "GET";

            try
            {
                HttpWebResponse httpResponse = (HttpWebResponse)httpRequest.GetResponse();

                if (((HttpWebResponse)httpResponse).StatusDescription == "OK" && httpResponse.ContentLength > 1)
                {
                    // 取得下載的檔名
                    Uri uri = new Uri(http_url);
                    string downloadfileRealName = System.IO.Path.GetFileName(uri.LocalPath);
                    
                    if (RemainingSpace(textbox_InstallPath.Text) == true)  //客戶電腦剩餘空間
                        label_RequireSpace.Content = convertDiskUnit(Convert.ToUInt64(httpResponse.ContentLength));
                    else
                        return false;
                }
            }
            catch
            {
                MessageBox.Show("Network error");   //網路連線異常or載點掛掉 //TODO 多國語系
                return false;
            }

            return true;
        }
    }
}
