using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
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

namespace OrderManagerNew
{
    /// <summary>
    /// ReleaseNote.xaml 的互動邏輯
    /// </summary>
    public partial class ReleaseNote : Window
    {
        private string DocUrl;
        private BackgroundWorker Bgworker;
        private String MainContent;
        OrderManagerFunctions omFunc;
        public ReleaseNote()
        {
            InitializeComponent();
            omFunc = new OrderManagerFunctions();
            MainContent = "";
            DocUrl = "";
        }

        public void SetCurrentSoftware(_softwareID SoftwareID)
        {
            DocUrl = "";
            string UrlCADLog = "https://www.dropbox.com/s/ra9luek6imfh8po/EZCAD%20Release%20Notes.txt?dl=1";
            string UrlImplant = "https://www.dropbox.com/s/qhw1jcsuzeofkrb/ImplantPlanning%20Release%20Notes.txt?dl=1";
            string UrlGuide = "https://www.dropbox.com/s/hiaol85rxp7u1ns/EZCAD%20guide%20Release%20Notes.txt?dl=1";
            string UrlOrthoLog = "https://www.dropbox.com/s/gx9rec4tw4n9fr8/OrthoAnalysis%20Release%20Notes.txt?dl=1";
            string UrlTrayLog = "https://www.dropbox.com/s/bs7vohkc9nddcj6/EZCAD%20tray%20Release%20Notes.txt?dl=1";
            string UrlSplintLog = "https://www.dropbox.com/s/fmk1h08ov3b9xpt/EZCAD%20splint%20Release%20Notes.txt?dl=1";

            label_loading.Visibility = Visibility.Visible;
            progressbar_loading.Visibility = Visibility.Visible;

            switch (SoftwareID)
            {
                case _softwareID.EZCAD:
                    {
                        label_title.Content = omFunc.GetSoftwareName(_softwareID.EZCAD) + "'s " + TranslationSource.Instance["ReleaseNote"];
                        DocUrl = UrlCADLog;
                        break;
                    }
                case _softwareID.Implant:
                    {
                        label_title.Content = omFunc.GetSoftwareName(_softwareID.Implant) + "'s " + TranslationSource.Instance["ReleaseNote"];
                        DocUrl = UrlImplant;
                        break;
                    }
                case _softwareID.Ortho:
                    {
                        label_title.Content = omFunc.GetSoftwareName(_softwareID.Ortho) + "'s " + TranslationSource.Instance["ReleaseNote"];
                        DocUrl = UrlOrthoLog;
                        break;
                    }
                case _softwareID.Tray:
                    {
                        label_title.Content = omFunc.GetSoftwareName(_softwareID.Tray) + "'s " + TranslationSource.Instance["ReleaseNote"];
                        DocUrl = UrlTrayLog;
                        break;
                    }
                case _softwareID.Splint:
                    {
                        label_title.Content = omFunc.GetSoftwareName(_softwareID.Splint) + "'s " + TranslationSource.Instance["ReleaseNote"];
                        DocUrl = UrlSplintLog;
                        break;
                    }
                case _softwareID.Guide:
                    {
                        label_title.Content = omFunc.GetSoftwareName(_softwareID.Guide) + "'s " + TranslationSource.Instance["ReleaseNote"];
                        DocUrl = UrlGuide;
                        break;
                    }
            }

            if (DocUrl != "")
            {
                Bgworker = new BackgroundWorker();
                Bgworker.DoWork += new DoWorkEventHandler(DoWork_Download);
                Bgworker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(CompletedWork_Download);
                Bgworker.WorkerReportsProgress = false;
                Bgworker.WorkerSupportsCancellation = false;
                Bgworker.RunWorkerAsync();
            }
        }

        private void Click_TitleBar_titlebarButtons(object sender, RoutedEventArgs e)
        {
            if (sender is Button)
            {
                switch (((Button)sender).Name)
                {
                    case "systemButton_Close":              //關閉
                        Close();
                        break;
                }
            }
        }

        void DoWork_Download(object sender, DoWorkEventArgs e)
        {
            var webRequest = WebRequest.Create(DocUrl);

            try
            {
                using (var response = webRequest.GetResponse())
                using (var content = response.GetResponseStream())
                using (var reader = new StreamReader(content))
                {
                    MainContent = reader.ReadToEnd();
                }
            }
            catch(Exception ex)
            {
                MainContent = ex.Message;
            }
        }

        void CompletedWork_Download(object sender, RunWorkerCompletedEventArgs e)
        {
            label_loading.Visibility = Visibility.Hidden;
            progressbar_loading.Visibility = Visibility.Hidden;
            if(e.Error != null)
            {
                textbox_relNote.Text = e.Error.Message;
            }
            else
            {
                textbox_relNote.Text = MainContent;
            }
        }
    }
}
