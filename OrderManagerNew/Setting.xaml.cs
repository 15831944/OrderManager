using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Path = System.IO.Path;

namespace OrderManagerNew
{
    /// <summary>
    /// Setting.xaml 的互動邏輯
    /// </summary>
    public partial class Setting : Window
    {
        SettingAllSet OriginalSet;
        CountdownEvent latch = new CountdownEvent(1);

        class DiskSoftwareNum
        {
            public string DiskName { get; set; }
            public int SoftwareCount { get; set; }

            public DiskSoftwareNum()
            {
                DiskName = "";
                SoftwareCount = 0;
            }
        }

        class SettingAllSet
        {
            public string Cad_exePath { get; set; }
            public string Implant_exePath { get; set; }
            public string Ortho_exePath { get; set; }
            public string Tray_exePath { get; set; }
            public string Splint_exePath { get; set; }
            public string Guide_exePath { get; set; }
            public string DownloadFolder { get; set; }
            public string LogFolder { get; set; }
            public int Language { get; set; }
            public SettingAllSet()
            {
                Cad_exePath = "";
                Implant_exePath = "";
                Ortho_exePath = "";
                Tray_exePath = "";
                Splint_exePath = "";
                Guide_exePath = "";
                DownloadFolder = "";
                LogFolder = "";
                Language = -1;
            }
        }

        public Setting()
        {
            InitializeComponent();
            OriginalSet = new SettingAllSet();

            if (File.Exists(Properties.Settings.Default.cad_exePath) == true)
                OriginalSet.Cad_exePath = Properties.Settings.Default.cad_exePath;
            if (File.Exists(Properties.Settings.Default.implant_exePath) == true)
                OriginalSet.Implant_exePath = Properties.Settings.Default.implant_exePath;
            if (File.Exists(Properties.Settings.Default.ortho_exePath) == true)
                OriginalSet.Ortho_exePath = Properties.Settings.Default.ortho_exePath;
            if (File.Exists(Properties.Settings.Default.tray_exePath) == true)
                OriginalSet.Tray_exePath = Properties.Settings.Default.tray_exePath;
            if (File.Exists(Properties.Settings.Default.splint_exePath) == true)
                OriginalSet.Splint_exePath = Properties.Settings.Default.splint_exePath;
            if (File.Exists(Properties.Settings.Default.guide_exePath) == true)
                OriginalSet.Guide_exePath = Properties.Settings.Default.guide_exePath;
            if (Directory.Exists(Properties.Settings.Default.DownloadFolder) == true)
                OriginalSet.DownloadFolder = Properties.Settings.Default.DownloadFolder;
            if (Directory.Exists(Properties.Settings.Default.Log_filePath) == true)
                OriginalSet.LogFolder = Properties.Settings.Default.Log_filePath;
            if (Properties.Settings.Default.sysLanguage == "zh-TW")
                OriginalSet.Language = (int)_langSupport.zhTW;
            else
                OriginalSet.Language = (int)_langSupport.English;

            textbox_EZCAD.Text = OriginalSet.Cad_exePath;
            textbox_Implant.Text = OriginalSet.Implant_exePath;
            textbox_Ortho.Text = OriginalSet.Ortho_exePath;
            textbox_Tray.Text = OriginalSet.Tray_exePath;
            textbox_Splint.Text = OriginalSet.Splint_exePath;
            textbox_Guide.Text = OriginalSet.Guide_exePath;
            textbox_Download.Text = OriginalSet.DownloadFolder;
            textbox_Log.Text = OriginalSet.LogFolder;
            comboboxLanguage.SelectedIndex = OriginalSet.Language;
            label_version.Content = "v" + Assembly.GetExecutingAssembly().GetName().Version.ToString();

            if(Properties.OrderManagerProps.Default.OMLatestVer != "")
            {
                Version LatestVer = new Version(Properties.OrderManagerProps.Default.OMLatestVer);
                if (LatestVer > Assembly.GetExecutingAssembly().GetName().Version)
                {
                    OMUpdate(true);
                }
                else
                {
                    OMUpdate(false);
                }
            }
            else
            {
                OMUpdate(false);
            }
        }

        /// <summary>
        /// 切換OrderManager是否有新版本可以更新
        /// </summary>
        /// <param name="CanUpdate"></param>
        private void OMUpdate(bool CanUpdate)
        {
            if(CanUpdate == true)
            {
                label_titlelatestVer.Visibility = Visibility.Visible;
                label_latestversion.Visibility = Visibility.Visible;
                Button_chkVer.Content = TranslationSource.Instance["SoftwareUpdate"];
                updateimage_OM.Visibility = Visibility.Visible;
                label_latestversion.Content = "v" + Properties.OrderManagerProps.Default.OMLatestVer;
            }
            else
            {
                label_titlelatestVer.Visibility = Visibility.Hidden;
                label_latestversion.Visibility = Visibility.Hidden;
                Button_chkVer.Content = TranslationSource.Instance["CheckVer"];
                updateimage_OM.Visibility = Visibility.Hidden;
            }
        }

        private void Click_TitleBar_titlebarButtons(object sender, RoutedEventArgs e)
        {
            if(sender is Button)
            {
                switch (((Button)sender).Name)
                {
                    case "systemButton_Close":              //關閉
                        Close();
                        break;
                }
            }
        }

        private void Click_OpenFilePath(object sender, RoutedEventArgs e)
        {
            if(sender is Button)
            {
                int softwareID = -1;
                string OriginalPath = "";

                switch (((Button)sender).Name)
                {
                    case "Btn_EZCADprogram":
                        {
                            OriginalPath = textbox_EZCAD.Text;
                            softwareID = (int)_softwareID.EZCAD;
                            break;
                        }
                    case "Btn_Implantprogram":
                        {
                            OriginalPath = textbox_Implant.Text;
                            softwareID = (int)_softwareID.Implant;
                            break;
                        }
                    case "Btn_Orthoprogram":
                        {
                            OriginalPath = textbox_Ortho.Text;
                            softwareID = (int)_softwareID.Ortho;
                            break;
                        }
                    case "Btn_Trayprogram":
                        {
                            OriginalPath = textbox_Tray.Text;
                            softwareID = (int)_softwareID.Tray;
                            break;
                        }
                    case "Btn_Splintprogram":
                        {
                            OriginalPath = textbox_Splint.Text;
                            softwareID = (int)_softwareID.Splint;
                            break;
                        }
                    case "Btn_Guideprogram":
                        {
                            OriginalPath = textbox_Guide.Text;
                            softwareID = (int)_softwareID.Guide;
                            break;
                        }
                    case "Btn_Downloadpath":
                        {
                            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
                            {
                                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                                if (result == System.Windows.Forms.DialogResult.OK)
                                {
                                    textbox_Download.Text = dialog.SelectedPath;
                                    textbox_Download.Focus();
                                }
                            }
                            return;
                        }
                    case "Btn_Logpath":
                        {
                            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
                            {
                                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                                if (result == System.Windows.Forms.DialogResult.OK)
                                {
                                    textbox_Log.Text = dialog.SelectedPath;
                                    textbox_Log.Focus();
                                }
                            }
                            return;
                        }
                }

                SearchEXE(OriginalPath, softwareID);
            }
        }
        
        private void Click_systemButton(object sender, RoutedEventArgs e)
        {
            if(sender is Button)
            {
                switch (((Button)sender).Name)
                {
                    case "sysBtn_Yes":
                        {
                            if (File.Exists(textbox_EZCAD.Text) == true)
                                Properties.Settings.Default.cad_exePath = Path.GetFullPath(textbox_EZCAD.Text);
                            else
                                Properties.Settings.Default.cad_exePath = "";
                            if (File.Exists(textbox_Implant.Text) == true)
                                Properties.Settings.Default.implant_exePath = Path.GetFullPath(textbox_Implant.Text);
                            else
                                Properties.Settings.Default.implant_exePath = "";
                            if (File.Exists(textbox_Ortho.Text) == true)
                                Properties.Settings.Default.ortho_exePath = Path.GetFullPath(textbox_Ortho.Text);
                            else
                                Properties.Settings.Default.ortho_exePath = "";
                            if (File.Exists(textbox_Tray.Text) == true)
                                Properties.Settings.Default.tray_exePath = Path.GetFullPath(textbox_Tray.Text);
                            else
                                Properties.Settings.Default.tray_exePath = "";
                            if (File.Exists(textbox_Splint.Text) == true)
                                Properties.Settings.Default.splint_exePath = Path.GetFullPath(textbox_Splint.Text);
                            else
                                Properties.Settings.Default.splint_exePath = "";
                            if (File.Exists(textbox_Guide.Text) == true)
                                Properties.Settings.Default.guide_exePath = Path.GetFullPath(textbox_Guide.Text);
                            else
                                Properties.Settings.Default.guide_exePath = "";

                            if (Directory.Exists(textbox_Download.Text) == true)
                                Properties.Settings.Default.DownloadFolder = textbox_Download.Text;
                            else
                            {
                                Properties.Settings.Default.DownloadFolder = OriginalSet.DownloadFolder;
                            }

                            if (Directory.Exists(textbox_Log.Text) == true)
                                Properties.Settings.Default.Log_filePath = textbox_Log.Text;
                            else
                                {
                                Properties.Settings.Default.Log_filePath = OriginalSet.LogFolder;
                            }

                            //多國語系
                            ComboBoxItem typeItem = (ComboBoxItem)comboboxLanguage.SelectedItem;

                            if (typeItem.Content.ToString() == "繁體中文")
                                Properties.Settings.Default.sysLanguage = "zh-TW";
                            else
                                Properties.Settings.Default.sysLanguage = "en-US";

                            Properties.Settings.Default.Save();
                            this.DialogResult = true;
                            break;
                        }
                    case "sysBtn_Cancel":
                        {
                            //還原
                            Properties.Settings.Default.cad_exePath = OriginalSet.Cad_exePath;
                            Properties.Settings.Default.implant_exePath = OriginalSet.Implant_exePath;
                            Properties.Settings.Default.ortho_exePath = OriginalSet.Ortho_exePath;
                            Properties.Settings.Default.tray_exePath = OriginalSet.Tray_exePath;
                            Properties.Settings.Default.splint_exePath = OriginalSet.Splint_exePath;
                            Properties.Settings.Default.guide_exePath = OriginalSet.Guide_exePath;
                            Properties.Settings.Default.DownloadFolder = OriginalSet.DownloadFolder;
                            Properties.Settings.Default.Log_filePath = OriginalSet.LogFolder;

                            if (OriginalSet.Language == (int)_langSupport.zhTW)
                                LocalizationService.SetLanguage("zh-TW");
                            else
                                LocalizationService.SetLanguage("en-US");

                            this.DialogResult = false;
                            break;
                        }
                    case "sysBtn_AutoDetect":
                        {
                            OrderManagerFunctions omFunc = new OrderManagerFunctions();
                            omFunc.AutoDetectEXE((int)_classFrom.Setting);

                            textbox_EZCAD.Text = Properties.Settings.Default.cad_exePath;
                            textbox_Implant.Text = Properties.Settings.Default.implant_exePath;
                            textbox_Ortho.Text = Properties.Settings.Default.ortho_exePath;
                            textbox_Tray.Text = Properties.Settings.Default.tray_exePath;
                            textbox_Splint.Text = Properties.Settings.Default.splint_exePath;
                            textbox_Guide.Text = Properties.Settings.Default.guide_exePath;
                            break;
                        }
                }
            }
        }

        private void SelectionChanged_Lang(object sender, SelectionChangedEventArgs e)
        {
            if(comboboxLanguage.SelectedIndex == 0)
            {
                LocalizationService.SetLanguage("en-US");
            }
            else if(comboboxLanguage.SelectedIndex == 1)
            {
                LocalizationService.SetLanguage("zh-TW");
            }
            if(label_latestversion != null)
            {
                if (label_latestversion.Visibility == Visibility.Visible)
                {
                    Button_chkVer.Content = TranslationSource.Instance["SoftwareUpdate"];
                }
                else
                {
                    Button_chkVer.Content = TranslationSource.Instance["CheckVer"];
                }
            }
        }
        
        /// <summary>
        /// 使用OpenFileDialog去找執行檔位置
        /// </summary>
        /// <param name="originPah">若之前有設定過就從之前exe檔路徑做預設</param>
        /// <param name="softwareID">軟體名稱 參考_softwareID</param>
        /// <returns></returns>
        public void SearchEXE(string originPah, int softwareID)
        {
            Microsoft.Win32.OpenFileDialog Dlg = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".exe",
                Filter = "Exe File (.exe)|*.exe"
            };

            if (File.Exists(originPah) == true)
                Dlg.InitialDirectory = Path.GetDirectoryName(originPah);
            else if (Directory.Exists(Properties.OrderManagerProps.Default.mostsoftwareDisk + @"IntewareInc\") == true)
                Dlg.InitialDirectory = Properties.OrderManagerProps.Default.mostsoftwareDisk + @"IntewareInc\";
            else
                return;

            Nullable<bool> result = Dlg.ShowDialog();
            if (result == true)
            {
                switch (softwareID)
                {
                    case (int)_softwareID.EZCAD:
                        {
                            textbox_EZCAD.Text = Dlg.FileName;
                            textbox_EZCAD.Focus();
                            break;
                        }
                    case (int)_softwareID.Implant:
                        {
                            textbox_Implant.Text = Dlg.FileName;
                            textbox_Implant.Focus();
                            break;
                        }
                    case (int)_softwareID.Ortho:
                        {
                            textbox_Ortho.Text = Dlg.FileName;
                            textbox_Ortho.Focus();
                            break;
                        }
                    case (int)_softwareID.Tray:
                        {
                            textbox_Tray.Text = Dlg.FileName;
                            textbox_Tray.Focus();
                            break;
                        }
                    case (int)_softwareID.Splint:
                        {
                            textbox_Splint.Text = Dlg.FileName;
                            textbox_Splint.Focus();
                            break;
                        }
                    case (int)_softwareID.Guide:
                        {
                            textbox_Guide.Text = Dlg.FileName;
                            textbox_Guide.Focus();
                            break;
                        }
                }
            }
        }

        private void RefreshData(CountdownEvent latch)
        {
            latch.Signal();
        }

        private void MouseLeftButtonUp_checkVersion(object sender, RoutedEventArgs e)
        {
            string param = "-eng";
            if (Properties.Settings.Default.sysLanguage == "zh-TW")
            {
                param = "-zhTW";
            }

            latch = new CountdownEvent(1);
            Thread thread = new Thread(() =>
            {
            OrderManagerFunctions omFunc = new OrderManagerFunctions();
                omFunc.RunCommandLine("OrderManagerLauncher.exe", param);
                RefreshData(latch);
            });
            thread.Start();
            latch.Wait();
            Thread.Sleep(2000);
            Environment.Exit(0);
        }
    }
}
