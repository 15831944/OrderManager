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
            string OriginalPath = "";

            switch (Btn.Name)
            {
                case "Btn_EZCADprogram":
                    {
                        OriginalPath = textbox_EZCAD.Text;
                        break;
                    }
                case "Btn_Implantprogram":
                    {
                        OriginalPath = textbox_Implant.Text;
                        break;
                    }
                case "Btn_Orthoprogram":
                    {
                        OriginalPath = textbox_Ortho.Text;
                        break;
                    }
                case "Btn_Trayprogram":
                    {
                        OriginalPath = textbox_Tray.Text;
                        break;
                    }
                case "Btn_Splintprogram":
                    {
                        OriginalPath = textbox_Splint.Text;
                        break;
                    }
                case "Btn_Guideprogram":
                    {
                        OriginalPath = textbox_Guide.Text;
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

            SearchEXE(OriginalPath, Btn.Name);
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
                        AutoDetectEXE();
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
        /// 自動檢測軟體執行檔路徑並把最常用的磁碟存入 Properties.Settings.Default.systemDisk
        /// </summary>
        public void AutoDetectEXE()
        {
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            List<diskSoftwareNum> diskWithSoftware = new List<diskSoftwareNum>();

            foreach (DriveInfo d in allDrives)  //檢查客戶所有磁碟
            {
                diskSoftwareNum diskInfo = new diskSoftwareNum();
                diskInfo.diskName = d.Name;

                if (File.Exists(d.Name + @"InteWare\EZCAD\Bin\EZCAD.exe") == true)
                {
                    textbox_EZCAD.Text = d.Name + @"InteWare\EZCAD\Bin\EZCAD.exe";
                    diskInfo.softwareCount++;
                }

                if (File.Exists(d.Name + @"InteWare\ImplantPlanning\ImplantPlanning.exe") == true)
                {
                    textbox_Implant.Text = d.Name + @"InteWare\ImplantPlanning\ImplantPlanning.exe";
                    diskInfo.softwareCount++;
                }

                if (File.Exists(d.Name + @"InteWare\OrthoAnalysis\OrthoAnalysis.exe") == true)
                {
                    textbox_Ortho.Text = d.Name + @"InteWare\OrthoAnalysis\OrthoAnalysis.exe";
                    diskInfo.softwareCount++;
                }

                if (File.Exists(d.Name + @"InteWare\EZCAD tray\Bin\EZCAD.tray.exe") == true)
                {
                    textbox_Tray.Text = d.Name + @"InteWare\EZCAD tray\Bin\EZCAD.tray.exe";
                    diskInfo.softwareCount++;
                }

                if (File.Exists(d.Name + @"InteWare\EZCAD splint\Bin\EZCAD.splint.exe") == true)
                {
                    textbox_Splint.Text = d.Name + @"InteWare\EZCAD splint\Bin\EZCAD.splint.exe";
                    diskInfo.softwareCount++;
                }

                if (File.Exists(d.Name + @"InteWare\EZCAD guide\Bin\EZCAD.guide.exe") == true)
                {
                    textbox_Guide.Text = d.Name + @"InteWare\EZCAD guide\Bin\EZCAD.guide.exe";
                    diskInfo.softwareCount++;
                }

                diskWithSoftware.Add(diskInfo);
            }

            diskSoftwareNum disk_most = new diskSoftwareNum();//存最多軟體的磁碟
            for (int i = 0; i < diskWithSoftware.Count; i++)    //開始各磁碟比較，比看哪個磁碟存比較多軟體
            {
                if (i >= 1)
                {
                    if (diskWithSoftware[i].softwareCount > disk_most.softwareCount)
                        disk_most = diskWithSoftware[i];
                }
                else
                    disk_most = diskWithSoftware[i];
            }

            if (disk_most.softwareCount != 0)
                Properties.Settings.Default.systemDisk = disk_most.diskName;
            else
            {
                bool chosen = false;//是否已選中
                foreach(diskSoftwareNum disk in diskWithSoftware)   //一個軟體都沒安裝預設C碟，C碟沒有就D碟，兩個都沒有就用陣列第一筆磁碟
                {
                    if (disk.diskName == @"C:\")
                    {
                        Properties.Settings.Default.systemDisk = @"C:\";
                        chosen = true;
                        break;
                    }
                    if (disk.diskName == @"D:\")
                    {
                        Properties.Settings.Default.systemDisk = @"D:\";
                        chosen = true;
                        break;
                    }
                }

                if (chosen == false)
                    Properties.Settings.Default.systemDisk = diskWithSoftware[0].diskName;

            }

            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// 使用OpenFileDialog去找執行檔位置
        /// </summary>
        /// <param name="originPah">若之前有設定過就從之前exe檔路徑做預設</param>
        /// <param name="softwareName">軟體名稱(名稱是Btn_xxxprogram)</param>
        /// <returns></returns>
        public void SearchEXE(string originPah, string softwareName)
        {
            Microsoft.Win32.OpenFileDialog Dlg = new Microsoft.Win32.OpenFileDialog();
            Dlg.DefaultExt = ".exe";
            Dlg.Filter = "Exe File (.exe)|*.exe";

            if (File.Exists(originPah) == true)
                Dlg.InitialDirectory = System.IO.Path.GetDirectoryName(originPah);
            else if (Directory.Exists("C:\\Inteware\\") == true)
                Dlg.InitialDirectory = "C:\\Inteware\\";

            Nullable<bool> result = Dlg.ShowDialog();
            if (result == true)
            {
                switch (softwareName)
                {
                    case "Btn_EZCADprogram":
                        {
                            textbox_EZCAD.Text = Dlg.FileName;
                            textbox_EZCAD.Focus();
                            break;
                        }
                    case "Btn_Implantprogram":
                        {
                            textbox_Implant.Text = Dlg.FileName;
                            textbox_Implant.Focus();
                            break;
                        }
                    case "Btn_Orthoprogram":
                        {
                            textbox_Ortho.Text = Dlg.FileName;
                            textbox_Ortho.Focus();
                            break;
                        }
                    case "Btn_Trayprogram":
                        {
                            textbox_Tray.Text = Dlg.FileName;
                            textbox_Tray.Focus();
                            break;
                        }
                    case "Btn_Splintprogram":
                        {
                            textbox_Splint.Text = Dlg.FileName;
                            textbox_Splint.Focus();
                            break;
                        }
                    case "Btn_Guideprogram":
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
