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
            if (value == null || (string)value == "")
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
        /// <summary>
        /// 使用者名稱
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 使用者名稱取第一個字
        /// </summary>
        public string UserPicName { get; set; }
        /// <summary>
        /// 使用者Email
        /// </summary>
        public string UserMail { get; set; }
        /// <summary>
        /// 使用者類(醫師、設計師...)
        /// </summary>
        public string Usergroup { get; set; }
        /// <summary>
        /// 使用者點數
        /// </summary>
        public long UserPoints { get; set; }
        /// <summary>
        /// 使用者大頭照
        /// </summary>
        public string UserPicSource { get; set; }

        public AirdentalUserDetail()
        {
            InitializeComponent();
            UserName = "";
            UserPicName = "";
            Usergroup = "";
            UserMail = "";
            UserPoints = 0;
            UserPicSource = "";
            this.DataContext = this;
        }

        public void SetUserPic(string picPath)
        {
            try
            {
                UserPicSource = picPath;
                image_user.BeginInit();
                image_user.Source = new BitmapImage(new Uri(UserPicSource, UriKind.RelativeOrAbsolute));
                image_user.EndInit();

                Panel.SetZIndex(image_user, 1);
            }
            catch
            {
                UserPicSource = "";
                Panel.SetZIndex(image_user, -1);
            }
            lb_userName.Content = UserName;
            lb_Usergroup.Content = TranslationSource.Instance[Usergroup];
            lb_userMail.Content = UserMail;
            lb_points.Content = UserPoints;
        }

        /// <summary>
        /// 重新整理頁面
        /// </summary>
        public void RefreshData()
        {
            try
            {
                image_user.BeginInit();
                image_user.Source = new BitmapImage(new Uri(UserPicSource, UriKind.RelativeOrAbsolute));
                image_user.EndInit();

                Panel.SetZIndex(image_user, 1);
            }
            catch
            {
                UserPicSource = "";
                Panel.SetZIndex(image_user, -1);
            }
            lb_userName.Content = UserName;
            lb_Usergroup.Content = TranslationSource.Instance[Usergroup];
            lb_userMail.Content = UserMail;
            lb_points.Content = UserPoints;
        }

        /// <summary>
        /// Click事件改成LogoutClick事件
        /// </summary>
        private void BtnClick_Logout(object sender, RoutedEventArgs e)
        {
            LogoutClick?.Invoke(this, new RoutedEventArgs());
        }
    }
}
