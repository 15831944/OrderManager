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
        public Setting()
        {
            InitializeComponent();

            textbox_EZCAD.Text = Properties.Settings.Default.path_EZCAD;
            textbox_Implant.Text = Properties.Settings.Default.path_Implant;
            textbox_Ortho.Text = Properties.Settings.Default.path_Ortho;
            textbox_Tray.Text = Properties.Settings.Default.path_Tray;
            textbox_Splint.Text = Properties.Settings.Default.path_Splint;
            textbox_Guide.Text = Properties.Settings.Default.path_Guide;

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
            }

            Microsoft.Win32.OpenFileDialog Dlg = new Microsoft.Win32.OpenFileDialog();
            Dlg.DefaultExt = ".exe";
            Dlg.Filter = "Exe File (.exe)|*.exe";

            if(File.Exists(OriginalPath) == true)
                Dlg.InitialDirectory = System.IO.Path.GetDirectoryName(OriginalPath);
            else
                Dlg.InitialDirectory = "c:\\Inteware\\";

            Nullable<bool> result = Dlg.ShowDialog();
            if (result == true)
            {
                switch (Btn.Name)
                {
                    case "Btn_EZCADprogram":
                        {
                            textbox_EZCAD.Text = Dlg.FileName;
                            break;
                        }
                    case "Btn_Implantprogram":
                        {
                            textbox_Implant.Text = Dlg.FileName;
                            break;
                        }
                    case "Btn_Orthoprogram":
                        {
                            textbox_Ortho.Text = Dlg.FileName;
                            break;
                        }
                    case "Btn_Trayprogram":
                        {
                            textbox_Tray.Text = Dlg.FileName;
                            break;
                        }
                    case "Btn_Splintprogram":
                        {
                            textbox_Splint.Text = Dlg.FileName;
                            break;
                        }
                    case "Btn_Guideprogram":
                        {
                            textbox_Guide.Text = Dlg.FileName;
                            break;
                        }
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
                        if (File.Exists(textbox_EZCAD.Text) == true)
                            Properties.Settings.Default.path_EZCAD = Path.GetFullPath(textbox_EZCAD.Text);
                        if (File.Exists(textbox_Implant.Text) == true)
                            Properties.Settings.Default.path_Implant = Path.GetFullPath(textbox_Implant.Text);
                        if (File.Exists(textbox_Ortho.Text) == true)
                            Properties.Settings.Default.path_Ortho = Path.GetFullPath(textbox_Ortho.Text);
                        if (File.Exists(textbox_Tray.Text) == true)
                            Properties.Settings.Default.path_Tray = Path.GetFullPath(textbox_Tray.Text);
                        if (File.Exists(textbox_Splint.Text) == true)
                            Properties.Settings.Default.path_Splint = Path.GetFullPath(textbox_Splint.Text);
                        if (File.Exists(textbox_Guide.Text) == true)
                            Properties.Settings.Default.path_Guide = Path.GetFullPath(textbox_Guide.Text);
                        
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
                        if (File.Exists(@"C:\InteWare\EZCAD\Bin\EZCAD.exe") == true)
                            textbox_EZCAD.Text = @"C:\InteWare\EZCAD\Bin\EZCAD.exe";

                        if (File.Exists(@"C:\InteWare\ImplantPlanning\ImplantPlanning.exe") == true)
                            textbox_Implant.Text = @"C:\InteWare\ImplantPlanning\ImplantPlanning.exe";

                        if (File.Exists(@"C:\InteWare\OrthoAnalysis\OrthoAnalysis.exe") == true)
                            textbox_Ortho.Text = @"C:\InteWare\OrthoAnalysis\OrthoAnalysis.exe";

                        if (File.Exists(@"C:\InteWare\EZCAD tray\Bin\EZCAD.tray.exe") == true)
                            textbox_Tray.Text = @"C:\InteWare\EZCAD tray\Bin\EZCAD.tray.exe";

                        if (File.Exists(@"C:\InteWare\EZCAD splint\Bin\EZCAD.splint.exe") == true)
                            textbox_Splint.Text = @"C:\InteWare\EZCAD splint\Bin\EZCAD.splint.exe";

                        if (File.Exists(@"C:\InteWare\EZCAD guide\Bin\EZCAD.guide.exe") == true)
                            textbox_Guide.Text = @"C:\InteWare\EZCAD guide\Bin\EZCAD.guide.exe";

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
    }
}
