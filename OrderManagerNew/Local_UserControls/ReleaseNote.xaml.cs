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
        public ReleaseNote()
        {
            InitializeComponent();
            MainContent = "";
            DocUrl = "";
        }

        public void SetCurrentSoftware(_softwareID SoftwareID)
        {
            string UrlOrthoLog = "https://www.dropbox.com/s/dkwbrhz4bk2w65v/OrthoAnalysis%20Release%20Note.txt?dl=1";

            label_loading.Visibility = Visibility.Visible;
            progressbar_loading.Visibility = Visibility.Visible;

            switch (SoftwareID)
            {
                case _softwareID.EZCAD:
                    break;
                case _softwareID.Implant:
                    break;
                case _softwareID.Ortho:
                    {
                        label_title.Content = "PrintIn Aligner's " + TranslationSource.Instance["ReleaseNote"];
                        DocUrl = UrlOrthoLog;
                        Bgworker = new BackgroundWorker();
                        Bgworker.DoWork += new DoWorkEventHandler(DoWork_Download);
                        Bgworker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(CompletedWork_Download);
                        Bgworker.WorkerReportsProgress = false;
                        Bgworker.WorkerSupportsCancellation = false;
                        Bgworker.RunWorkerAsync();
                        break;
                    }
                case _softwareID.Tray:
                    break;
                case _softwareID.Splint:
                    break;
                case _softwareID.Guide:
                    break;
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
