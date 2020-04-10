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
