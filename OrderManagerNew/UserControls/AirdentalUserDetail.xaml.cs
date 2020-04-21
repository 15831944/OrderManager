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
        public AirdentalUserDetail()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public string userName { get; set; }
        public string userPicName { get; set; }
        public string userMail { get; set; }
        public string userPoints { get; set; }
    }
}
