using System;
using System.Collections.Generic;
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
        public AirdentalLogin()
        {
            InitializeComponent();
            textbox_Account.Text = "";
            textbox_PWD.Text = "";
            passwordbox_PWD.Password = "";
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

        private void Click_OK(object sender, RoutedEventArgs e)
        {
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
    }
}
