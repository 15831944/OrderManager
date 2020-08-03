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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace UIDialogs
{
    /// <summary>
    /// Inteware_Messagebox.xaml 的互動邏輯
    /// </summary>
    public partial class Inteware_Messagebox : Window
    {
        public enum _ReturnButtonName
        {
            OK = 0,
            YES,
            NO,
            CANCEL
        }
        public enum _Lang
        {
            English = 0,
            TraditionalChinese
        }

        public int ReturnClickWhitchButton;
        private Point startPos;

        public Inteware_Messagebox()
        {
            InitializeComponent();
            label_title.Content = "";
            textblock_content.Text = "";
            ReturnClickWhitchButton = -1;

            string systemName = System.Globalization.CultureInfo.CurrentCulture.Name; // 取得電腦語系
            if (systemName == "zh-TW")
                LocalizationService.SetLanguage("zh-TW");
            else
                LocalizationService.SetLanguage("en-US");
        }
        /// <summary>
        /// 設定語系
        /// </summary>
        /// <param name="language">請用列舉_Lang</param>
        public void SetLanguage(int language)
        {
            switch(language)
            {
                case (int)_Lang.English:
                    {
                        LocalizationService.SetLanguage("en-US");
                        break;
                    }
                case (int)_Lang.TraditionalChinese:
                    {
                        LocalizationService.SetLanguage("zh-TW");
                        break;
                    }
            }
        }
        /// <summary>
        /// 設定語系
        /// </summary>
        /// <param name="lang">LocalizationService.SetLanguage("參數");</param>
        public void SetLanguage(string lang)
        {
            LocalizationService.SetLanguage(lang);
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
        /// <summary>
        /// 顯示字串、標題另外再區分MessageBoxButton，內建顯示ShowDialog()，要找回傳函數請用ReturnClickWhitchButton，用法 if(Msg.ReturnClickWhitchButton == (int)Inteware_Messagebox._ReturnButtonName.YES )
        /// </summary>
        /// <param name="message"></param>
        /// <param name="titleMessage"></param>
        /// <param name="messageBoxButton"></param>
        public void ShowMessage(string message, string titleMessage, MessageBoxButton messageBoxButton)
        {
            label_title.Content = titleMessage;
            textblock_content.Text = message;
            textblock_content.Width += grid_contentImage.Width;
            grid_contentImage.Visibility = Visibility.Collapsed;
            switch (messageBoxButton)
            {
                case MessageBoxButton.OK:
                    {
                        btn_no.Visibility = Visibility.Collapsed;
                        btn_cancel.Visibility = Visibility.Collapsed;
                        btn_yes.Content = TranslationSource.Instance["OK"];
                        break;
                    }
                case MessageBoxButton.OKCancel:
                    {
                        btn_no.Visibility = Visibility.Collapsed;
                        btn_yes.Content = TranslationSource.Instance["OK"];
                        break;
                    }
                case MessageBoxButton.YesNo:
                    {
                        btn_cancel.Visibility = Visibility.Collapsed;
                        break;
                    }
                case MessageBoxButton.YesNoCancel:
                    {
                        break;
                    }
            }
            ShowDialog();
        }
        /// <summary>
        /// 顯示字串、標題、MessageBoxButton還有MessageboxImage，內建顯示ShowDialog()，要找回傳函數請用ReturnClickWhitchButton，用法 if(Msg.ReturnClickWhitchButton == (int)Inteware_Messagebox._ReturnButtonName.YES )
        /// </summary>
        /// <param name="message"></param>
        /// <param name="titleMessage"></param>
        /// <param name="messageBoxButton"></param>
        /// <param name="messageBoxImage"></param>
        public void ShowMessage(string message, string titleMessage, MessageBoxButton messageBoxButton, MessageBoxImage messageBoxImage)
        {
            label_title.Content = titleMessage;
            textblock_content.Text = message;
            switch (messageBoxButton)
            {
                case MessageBoxButton.OK:
                    {
                        btn_no.Visibility = Visibility.Collapsed;
                        btn_cancel.Visibility = Visibility.Collapsed;
                        btn_yes.Content = TranslationSource.Instance["OK"];
                        break;
                    }
                case MessageBoxButton.OKCancel:
                    {
                        btn_no.Visibility = Visibility.Collapsed;
                        btn_yes.Content = TranslationSource.Instance["OK"];
                        break;
                    }
                case MessageBoxButton.YesNo:
                    {
                        btn_cancel.Visibility = Visibility.Collapsed;
                        break;
                    }
                case MessageBoxButton.YesNoCancel:
                    {
                        break;
                    }
            }
            switch (messageBoxImage)
            {
                case MessageBoxImage.Error:
                    {
                        image_content.BeginInit();
                        image_content.Source = (System.Windows.Media.ImageSource)this.Resources["icon_errorDrawingImage"];
                        image_content.EndInit();
                        break;
                    }
                default:    //其它都是用Warning
                    {
                        image_content.BeginInit();
                        image_content.Source = (System.Windows.Media.ImageSource)this.Resources["icon_warningDrawingImage"];
                        image_content.EndInit();
                        break;
                    }
            }
            ShowDialog();
        }

        private void Click_TitleBar_titlebarButtons(object sender, RoutedEventArgs e)
        {
            if (sender is Button)
            {
                switch (((Button)sender).Name)
                {
                    case "systemButton_ContactInteware":    //聯絡客服
                        Process processer = new Process();
                        processer.StartInfo.FileName = @"https://www.facebook.com/messages/t/484154545409477";
                        processer.Start();
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
                    //this.WindowState = (this.WindowState == WindowState.Normal) ? WindowState.Maximized : WindowState.Normal;
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
                DragMove();
            }
        }

        private void Click_result(object sender, RoutedEventArgs e)
        {
            if (sender is Button)
            {
                switch (((Button)sender).Name)
                {
                    case "btn_yes":
                        {
                            if ((string)btn_yes.Content == TranslationSource.Instance["OK"])
                                ReturnClickWhitchButton = (int)_ReturnButtonName.OK;
                            else
                                ReturnClickWhitchButton = (int)_ReturnButtonName.YES;
                            break;
                        }
                    case "btn_no":
                        {
                            ReturnClickWhitchButton = (int)_ReturnButtonName.NO;
                            break;
                        }
                    case "btn_cancel":
                        {
                            ReturnClickWhitchButton = (int)_ReturnButtonName.CANCEL;
                            break;
                        }
                }
                DialogResult = true;
            }
        }

        private void Loaded_Main(object sender, RoutedEventArgs e)
        {
            systemButton_ContactInteware.IsEnabled = false;
        }
    }
}
