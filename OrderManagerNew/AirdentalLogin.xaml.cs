using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
    /// AirdentalLogin.xaml 的互動邏輯
    /// </summary>
    public partial class AirdentalLogin : Window
    {
        LogRecorder log;    //日誌檔cs

        public AirdentalLogin()
        {
            InitializeComponent();
            label_forgotPWD.AddHandler(Button.MouseLeftButtonUpEvent, new MouseButtonEventHandler(MouseLeftButtonUp_ForgotPWD), true);

            log = new LogRecorder();
            log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "AirdentalLogin.cs", "Initial Start");

            textbox_Account.Text = "";
            textbox_PWD.Text = "";
            passwordbox_PWD.Password = "";
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

        private void Click_OK(object sender, RoutedEventArgs e)
        {
            bool pass = true;

            if(textbox_Account.Text == "")
            {
                textbox_Account.BorderBrush = Brushes.Red;
                pass = false;
            }

            if(textbox_PWD.Text == "")
            {
                textbox_Account.BorderBrush = Brushes.Red;
                pass = false;
            }

            if(pass == true)
                DialogResult = true;
        }

        private void Keyup_PWD(object sender, KeyEventArgs e)
        {
            textbox_PWD.Text = passwordbox_PWD.Password;
        }

        private void Checked_ShowPWD(object sender, RoutedEventArgs e)
        {
            passwordbox_PWD.Visibility = Visibility.Hidden;
        }

        private void Unchecked_PWD(object sender, RoutedEventArgs e)
        {
            passwordbox_PWD.Visibility = Visibility.Visible;
        }

        private void Textchanged_Account(object sender, TextChangedEventArgs e)
        {
            textbox_Account.BorderBrush = Brushes.Gray;
        }

        private void KeyUp_textboxPWD(object sender, KeyEventArgs e)
        {
            passwordbox_PWD.Password = textbox_PWD.Text;
        }

        private void MouseLeftButtonUp_ForgotPWD(object sender, MouseButtonEventArgs e)
        {
            RunCommandLine(Properties.HyperLink.Default.ForgetAirdentalPassword, "");
        }

        /// <summary>
        /// CommandLine(命令提示字元)
        /// </summary>
        /// <param name="fileName">要開啟的檔案</param>
        /// <param name="arguments">要傳進去的參數</param>
        void RunCommandLine(string fileName, string arguments)
        {
            try
            {
                Process processer = new Process();
                processer.StartInfo.FileName = fileName;
                if (arguments != "")
                    processer.StartInfo.Arguments = arguments;
                processer.Start();
            }
            catch (Exception ex)
            {
                log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "RunCommandLine exception", ex.Message);
            }
        }
    }
}
