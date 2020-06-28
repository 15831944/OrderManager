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
    /// Inteware_Messagebox.xaml 的互動邏輯
    /// </summary>
    public partial class Inteware_Messagebox : Window
    {
        public bool BtnCancel;
        private Point startPos;
        
        public Inteware_Messagebox()
        {
            InitializeComponent();
            label_title.Content = "";
            textblock_content.Text = "";
        }

        /// <summary>
        /// 單純顯示字串，內建顯示ShowDialog()
        /// </summary>
        public void ShowMessage(string message)
        {
            textblock_content.Width += grid_contentImage.Width;
            grid_contentImage.Visibility = Visibility.Collapsed;
            btn_no.Visibility = Visibility.Collapsed;
            btn_cancel.Visibility = Visibility.Collapsed;
            btn_yes.Content = TranslationSource.Instance["OK"];
            textblock_content.Text = message;
            ShowDialog();
        }
        /// <summary>
        /// 顯示字串和標題，內建顯示ShowDialog()
        /// </summary>
        /// <param name="message">內文</param>
        /// <param name="titleMessage">標題</param>
        public void ShowMessage(string message, string titleMessage)
        {
            label_title.Content = titleMessage;
            ShowMessage(message);
        }

        public void ShowMessage(string message, string titleMessage, MessageBoxButton messageBoxButton)
        {

        }

        public void ShowMessage(string message, string titleMessage, MessageBoxButton messageBoxButton, MessageBoxImage messageBoxImage)
        {

        }

        public bool ReturnResult(string message, string titleMessage, MessageBoxButton messageBoxButton, MessageBoxImage messageBoxImage)
        {
            return true;
        }

        private void Click_TitleBar_titlebarButtons(object sender, RoutedEventArgs e)
        {
            if (sender is Button)
            {
                switch (((Button)sender).Name)
                {
                    case "systemButton_ContactInteware":    //聯絡客服
                        OrderManagerFunctions OrderManagerFunc = new OrderManagerFunctions();
                        OrderManagerFunc.RunCommandLine(Properties.HyperLink.Default.ContactInteware, "");
                        break;
                    case "systemButton_Close":              //關閉
                        DialogResult = false;
                        break;
                }
            }
        }

        private void MouseDown_TitleBar(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (e.ClickCount >= 2)
                {
                    this.WindowState = (this.WindowState == WindowState.Normal) ? WindowState.Maximized : WindowState.Normal;
                }
                else
                {
                    startPos = e.GetPosition(null);
                }
            }
        }

        private void MouseMove_TitleBar(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (this.WindowState == WindowState.Maximized && Math.Abs(startPos.Y - e.GetPosition(null).Y) > 2)
                { }
                DragMove();
            }
        }

        private void Click_result(object sender, RoutedEventArgs e)
        {
            if(sender is Button)
            {
                switch(((Button)sender).Name)
                {
                    case "btn_yes":
                        {
                            BtnCancel = false;  //DialogResult為True且BtnCancel為False
                            break;
                        }
                    case "btn_no":
                        {
                            BtnCancel = false;
                            DialogResult = false; //DialogResult為False
                            break;
                        }
                    case "btn_cancel":
                        {
                            BtnCancel = true;  //DialogResult為True且BtnCancel為True
                            break;
                        }
                }
                DialogResult = true;
            }
        }
    }
}
