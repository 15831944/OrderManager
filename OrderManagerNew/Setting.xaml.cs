using System;
using System.Collections.Generic;
using System.IO;
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
using Path = System.IO.Path;

namespace OrderManagerNew
{
    /// <summary>
    /// Setting.xaml 的互動邏輯
    /// </summary>
    public partial class Setting : Window
    {
        class diskSoftwareNum
        {
            public string diskName { get; set; }
            public int softwareCount { get; set; }

            public diskSoftwareNum()
            {
                diskName = "";
                softwareCount = 0;
            }
        }

        public Setting()
        {
            InitializeComponent();

            textbox_EZCAD.Text = Properties.Settings.Default.cad_exePath;
            textbox_Implant.Text = Properties.Settings.Default.implant_exePath;
            textbox_Ortho.Text = Properties.Settings.Default.ortho_exePath;
            textbox_Tray.Text = Properties.Settings.Default.tray_exePath;
            textbox_Splint.Text = Properties.Settings.Default.splint_exePath;
            textbox_Guide.Text = Properties.Settings.Default.guide_exePath;


            if (Properties.Settings.Default.DownloadFolder == "")
                textbox_Download.Text = Properties.Settings.Default.DownloadFolder = System.IO.Path.GetTempPath() + @"IntewareTempFile\";
            else
                textbox_Download.Text = Properties.Settings.Default.DownloadFolder;

            if (Properties.Settings.Default.sysLanguage == "zh-TW")
                comboboxLanguage.SelectedIndex = 1;
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
            Button Btn = sender as Button;
            int softwareID = -1;
            string OriginalPath = "";

            switch (Btn.Name)
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
                            if(result == System.Windows.Forms.DialogResult.OK)
                            {
                                textbox_Download.Text = dialog.SelectedPath;
                                textbox_Download.Focus();
                            }
                        }
                        return;
                    }
            }

            SearchEXE(OriginalPath, softwareID);
        }
        
        private void Click_systemButton(object sender, RoutedEventArgs e)
        {
            Button Btn = sender as Button;
            switch(Btn.Name)
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
                            Properties.Settings.Default.DownloadFolder = System.IO.Path.GetTempPath() + "IntewareTempFile\\";

                            /*if (Directory.Exists(System.IO.Path.GetTempPath() + "IntewareTempFile\\") == false)
                            {
                                System.IO.Directory.CreateDirectory(System.IO.Path.GetTempPath() + "IntewareTempFile\\");
                            }*/
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
                        this.DialogResult = false;
                        break;
                    }
                case "sysBtn_AutoDetect":
                    {
                        OrderManagerFunctions omFunc = new OrderManagerFunctions();
                        omFunc.AutoDetectEXE((int)_classFrom.Setting);//TODO如果原本properties全部都沒有，AutoDetect偵測到並寫入textbox，再按取消,則properties會寫入(bug)

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
        }
        
        /// <summary>
        /// 使用OpenFileDialog去找執行檔位置
        /// </summary>
        /// <param name="originPah">若之前有設定過就從之前exe檔路徑做預設</param>
        /// <param name="softwareID">軟體名稱 參考_softwareID</param>
        /// <returns></returns>
        public void SearchEXE(string originPah, int softwareID)
        {
            Microsoft.Win32.OpenFileDialog Dlg = new Microsoft.Win32.OpenFileDialog();
            Dlg.DefaultExt = ".exe";
            Dlg.Filter = "Exe File (.exe)|*.exe";

            if (File.Exists(originPah) == true)
                Dlg.InitialDirectory = System.IO.Path.GetDirectoryName(originPah);
            else if (Directory.Exists(Properties.Settings.Default.systemDisk + @"Inteware\") == true)
                Dlg.InitialDirectory = Properties.Settings.Default.systemDisk + @"Inteware\";

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
    }
}
