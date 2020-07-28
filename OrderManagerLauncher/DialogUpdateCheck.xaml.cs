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

namespace OrderManagerLauncher
{
    /// <summary>
    /// DialogUpdateCheck.xaml 的互動邏輯
    /// </summary>
    public partial class DialogUpdateCheck : Window
    {
        public bool NoAutoChk = false;

        public DialogUpdateCheck()
        {
            InitializeComponent();
            label_title.Content = TranslationSource.Instance["NewVerAvailable"] + TranslationSource.Instance["NewVerAvailable2"];
        }

        private void Click_TitleBar_titlebarButtons(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Click_systemButton(object sender, RoutedEventArgs e)
        {
            if(sender is Button buttonName)
            {
                if (checkbox_NoautoCheckUpdate.IsChecked == true)
                    NoAutoChk = true;
                else
                    NoAutoChk = false;

                switch (buttonName.Name)
                {
                    case "button_yes":
                        {
                            DialogResult = true;
                            break;
                        }
                    case "button_no":
                        {
                            DialogResult = false;
                            break;
                        }
                }
            }
        }
    }
}
