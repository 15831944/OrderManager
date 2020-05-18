using System;
using System.Collections.Generic;
using System.Globalization;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

//Handling user control events in WPF: https://www.codeproject.com/Questions/424498/Handling-user-control-events-in-WPF

namespace OrderManagerNew.UserControls
{
    [ValueConversion(typeof(string), typeof(string))]
    public class CapitalNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return "";
            string fullName = value.ToString();
            string capitalName = fullName[0].ToString().ToUpper();

            return capitalName;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// AirdentalUserDetail.xaml 的互動邏輯
    /// </summary>
    public partial class AirdentalUserDetail : UserControl
    {
        public event RoutedEventHandler LogoutClick;

        public AirdentalUserDetail()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public string UserName { get; set; }
        public string UserPicName { get; set; }
        public string UserMail { get; set; }
        public string UserPoints { get; set; }

        private void BtnClick_Logout(object sender, RoutedEventArgs e)
        {
            LogoutClick?.Invoke(this, new RoutedEventArgs());
        }
    }
}
