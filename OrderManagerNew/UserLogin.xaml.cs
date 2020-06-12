using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// UserLogin.xaml 的互動邏輯
    /// </summary>
    public partial class UserLogin : Window
    {
        LogRecorder log;    //日誌檔cs
        /// <summary>
        /// uid、mail、userName
        /// </summary>
        public string[] UserDetail;
        public UserLogin()
        {
            InitializeComponent();
            label_forgotPWD.AddHandler(Button.MouseLeftButtonUpEvent, new MouseButtonEventHandler(MouseLeftButtonUp_ForgotPWD), true);

            log = new LogRecorder();
            log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "UserLogin.cs", "Initial Start");
            string recAcc = Properties.Settings.Default.AirdentalAcc;
            textbox_Account.Text = recAcc;
            textbox_PWD.Text = "";
            passwordbox_PWD.Password = "";
            image_loginFail.Visibility = Visibility.Hidden;
            label_loginFail.Visibility = Visibility.Hidden;
        }

        private void Click_TitleBar_titlebarButtons(object sender, RoutedEventArgs e)
        {
            if(sender is Button)
            {
                switch (((Button)sender).Name)
                {
                    case "systemButton_Close"://關閉
                        Close();
                        break;
                }
            }
        }

        private void Click_OK(object sender, RoutedEventArgs e)
        {
            if(textbox_Account.Text == "")
            {
                textbox_Account.BorderThickness = new Thickness(2);
                textbox_Account.BorderBrush = Brushes.Red;
                return;
            }

            if(textbox_PWD.Text == "")
            {
                passwordbox_PWD.BorderThickness = new Thickness(2);
                textbox_PWD.BorderThickness = new Thickness(2);
                passwordbox_PWD.BorderBrush = Brushes.Red;
                textbox_PWD.BorderBrush = Brushes.Red;
                return;
            }

            Dll_Airdental.Main Airdental = new Dll_Airdental.Main();
            WebException ex = new WebException();
            string[] LoginData = new string[3]{ @"https://airdental.inteware.com.tw/api/", textbox_Account.Text, passwordbox_PWD.Password};//API網址、帳號、密碼
            UserDetail = new string[3] { "", "", "" };//uid、mail、userName
            if (Airdental.Login(LoginData, ref UserDetail, ref ex) == true)
            {
                Properties.Settings.Default.AirdentalAcc = textbox_Account.Text;
                Properties.Settings.Default.Save();
                if(UserDetail[(int)_AirD_LoginDetail.UID] != "")
                {
                    Properties.OrderManagerProps.Default.AirD_uid = UserDetail[(int)_AirD_LoginDetail.UID];
                    DialogResult = true;
                }
                else
                {
                    DialogResult = false;
                }
            }
            else
            {
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    string errMessage = "";
                    if (ex.Response is HttpWebResponse response)
                    {
                        switch (response.StatusCode)
                        {
                            case HttpStatusCode.BadRequest:
                                {
                                    image_loginFail.Visibility = Visibility.Visible;
                                    label_loginFail.Visibility = Visibility.Visible;
                                    passwordbox_PWD.Password = "";
                                    textbox_PWD.Text = "";
                                    textbox_Account.BorderThickness = new Thickness(2);
                                    textbox_Account.BorderBrush = Brushes.Red;
                                    break;
                                }
                            default:
                                {
                                    errMessage = "ErrorCode: " + (object)(int)response.StatusCode + ", " + Enum.GetName(typeof(HttpStatusCode), (int)response.StatusCode);
                                    break;
                                }
                        }
                    }
                    else
                        errMessage = ex.ToString();

                    if (errMessage != "")
                        MessageBox.Show(errMessage);
                }
            }
        }

        private void Keyup_PWD(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                Click_OK(null, null);
            else
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

        private void KeyUp_textboxPWD(object sender, KeyEventArgs e)
        {
            passwordbox_PWD.Password = textbox_PWD.Text;
        }

        private void MouseLeftButtonUp_ForgotPWD(object sender, MouseButtonEventArgs e)
        {
            OrderManagerFunctions omFunc = new OrderManagerFunctions();
            omFunc.RunCommandLine(Properties.HyperLink.Default.ForgetAirdentalPassword, "");
        }

        private void GotFocus_Account(object sender, RoutedEventArgs e)
        {
            if (textbox_Account.BorderBrush == Brushes.Red)
            {
                textbox_Account.BorderThickness = new Thickness(1);
                textbox_Account.BorderBrush = Brushes.Gray;
            }

            if (image_loginFail.Visibility == Visibility.Visible)
            {
                image_loginFail.Visibility = Visibility.Hidden;
                label_loginFail.Visibility = Visibility.Hidden;
            }
        }

        private void GotFocus_PassWD(object sender, RoutedEventArgs e)
        {
            if(passwordbox_PWD.BorderBrush == Brushes.Red)
            {
                passwordbox_PWD.BorderThickness = new Thickness(1);
                textbox_PWD.BorderThickness = new Thickness(1);
                passwordbox_PWD.BorderBrush = Brushes.Gray;
                textbox_PWD.BorderBrush = Brushes.Gray;
            }

            if (image_loginFail.Visibility == Visibility.Visible)
            {
                image_loginFail.Visibility = Visibility.Hidden;
                label_loginFail.Visibility = Visibility.Hidden;
            }
        }

        private void Loaded_UserLogin(object sender, RoutedEventArgs e)
        {
            if(textbox_Account.Text != "")
            {
                if (passwordbox_PWD.Visibility == Visibility.Visible)
                    passwordbox_PWD.Focus();
                else if (textbox_PWD.Visibility == Visibility.Visible)
                    textbox_PWD.Focus();
            }
        }
    }
}
