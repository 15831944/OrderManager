using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Reflection;    //取得OrderManager自身軟體版本

//Microsoft.Expression.Drawing.dll如果要用多國語言套件: "C:\Program Files (x86)\Microsoft SDKs\Expression\Blend\.NETFramework\v4.5\Libraries"

namespace OrderManagerNew
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        LogRecorder log;//日誌檔cs

        public MainWindow()
        {
            InitializeComponent();

            log = new LogRecorder();
            titlebar_OrderManagerVersion.Content = "v" + Assembly.GetExecutingAssembly().GetName().Version.ToString();//TitleBar顯示OrderManager版本
        }

        #region WindowFrame
        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                //systemButton_Maximize.ToolTip = WpfOrderManager.TranslationSource.Instance["SBtn_Restore"];
                this.BorderThickness = new Thickness(0,6,0,0);
            }
            else
            {
                //systemButton_Maximize.ToolTip = WpfOrderManager.TranslationSource.Instance["SBtn_Max"];
                this.BorderThickness = new Thickness(0);
            }
        }
        #endregion

        #region TitleBar事件
        private Point startPos;

        [DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);
        [DllImport("user32.dll")]
        static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
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
        
        private void TitleBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (this.WindowState == WindowState.Maximized && Math.Abs(startPos.Y - e.GetPosition(null).Y) > 2)
                {}
                DragMove();
            }
        }

        private void TitleBar_Click_titlebarButtons(object sender, RoutedEventArgs e)
        {
            Button titleButton = sender as Button;

            if (titleButton == null)
            {
                //是ToggleButton，按鈕是"向下還原"或"最大化"
                this.WindowState = (this.WindowState == WindowState.Normal) ? WindowState.Maximized : WindowState.Normal;
            }
            else
            {
                switch (titleButton.Name)
                {
                    case "systemButton_ContactInteware":    //聯絡客服

                        break;
                    case "systemButton_Minimize":           //最小化
                        this.WindowState = WindowState.Minimized;
                        break;
                    case "systemButton_Close":              //關閉
                        Close();
                        break;
                }
            }
        }
        #endregion

        #region FunctionTable事件
        /// <summary>
        /// 設定SofttwareTable各Icon顯示
        /// </summary>
        /// <param name="currentProgress">(目前進度) 未安裝、下載中、已安裝</param>
        /// <param name="softwareID">(軟體ID) EZCAD、Implant、Ortho、Tray、Splint</param>
        /// <returns></returns>
        void setSoftwareShow(int softwareID, int currentProgress)
        {
            switch (softwareID)
            {
                case (int)_softwareID.EZCAD:
                    {
                        switch (currentProgress)
                        {
                            case (int)_softwareTableStatus.NotInstall:
                                {
                                    mask_EZCAD.Visibility = Visibility.Visible;
                                    process_EZCAD.Visibility = Visibility.Hidden;
                                    break;
                                }
                            case (int)_softwareTableStatus.Downloading:
                                {
                                    mask_EZCAD.Visibility = Visibility.Hidden;
                                    process_EZCAD.Visibility = Visibility.Visible;
                                    break;
                                }
                            case (int)_softwareTableStatus.Installed:
                                {
                                    mask_EZCAD.Visibility = Visibility.Hidden;
                                    process_EZCAD.Visibility = Visibility.Hidden;
                                    break;
                                }
                        }
                        break;
                    }
                case (int)_softwareID.Implant:
                    {
                        switch (currentProgress)
                        {
                            case (int)_softwareTableStatus.NotInstall:
                                {
                                    mask_Implant.Visibility = Visibility.Visible;
                                    process_Implant.Visibility = Visibility.Hidden;
                                    break;
                                }
                            case (int)_softwareTableStatus.Downloading:
                                {
                                    mask_Implant.Visibility = Visibility.Hidden;
                                    process_Implant.Visibility = Visibility.Visible;
                                    break;
                                }
                            case (int)_softwareTableStatus.Installed:
                                {
                                    mask_Implant.Visibility = Visibility.Hidden;
                                    process_Implant.Visibility = Visibility.Hidden;
                                    break;
                                }
                        }
                        break;
                    }
                case (int)_softwareID.Ortho:
                    {
                        switch (currentProgress)
                        {
                            case (int)_softwareTableStatus.NotInstall:
                                {
                                    mask_Ortho.Visibility = Visibility.Visible;
                                    process_Ortho.Visibility = Visibility.Hidden;
                                    break;
                                }
                            case (int)_softwareTableStatus.Downloading:
                                {
                                    mask_Ortho.Visibility = Visibility.Hidden;
                                    process_Ortho.Visibility = Visibility.Visible;
                                    break;
                                }
                            case (int)_softwareTableStatus.Installed:
                                {
                                    mask_Ortho.Visibility = Visibility.Hidden;
                                    process_Ortho.Visibility = Visibility.Hidden;
                                    break;
                                }
                        }
                        break;
                    }
                case (int)_softwareID.Tray:
                    {
                        switch (currentProgress)
                        {
                            case (int)_softwareTableStatus.NotInstall:
                                {
                                    mask_Tray.Visibility = Visibility.Visible;
                                    process_Tray.Visibility = Visibility.Hidden;
                                    break;
                                }
                            case (int)_softwareTableStatus.Downloading:
                                {
                                    mask_Tray.Visibility = Visibility.Hidden;
                                    process_Tray.Visibility = Visibility.Visible;
                                    break;
                                }
                            case (int)_softwareTableStatus.Installed:
                                {
                                    mask_Tray.Visibility = Visibility.Hidden;
                                    process_Tray.Visibility = Visibility.Hidden;
                                    break;
                                }
                        }
                        break;
                    }
                case (int)_softwareID.Splint:
                    {
                        switch (currentProgress)
                        {
                            case (int)_softwareTableStatus.NotInstall:
                                {
                                    mask_Splint.Visibility = Visibility.Visible;
                                    process_Splint.Visibility = Visibility.Hidden;
                                    break;
                                }
                            case (int)_softwareTableStatus.Downloading:
                                {
                                    mask_Splint.Visibility = Visibility.Hidden;
                                    process_Splint.Visibility = Visibility.Visible;
                                    break;
                                }
                            case (int)_softwareTableStatus.Installed:
                                {
                                    mask_Splint.Visibility = Visibility.Hidden;
                                    process_Splint.Visibility = Visibility.Hidden;
                                    break;
                                }
                        }
                        break;
                    }
            }
        }
        void Button_Click(object sender, RoutedEventArgs e)
        {
            setSoftwareShow(0, 2);
        }
        #endregion

        #region SortTable事件
        private void SortTable_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox chkbox = sender as CheckBox;
            switch(chkbox.Name)
            {
                case "checkboxPatient":
                    {
                        break;
                    }
                case "checkboxCase":
                    {
                        break;
                    }
            }
        }

        private void SortTable_UnChecked(object sender, RoutedEventArgs e)
        {
            CheckBox chkbox = sender as CheckBox;
            switch (chkbox.Name)
            {
                case "checkboxPatient":
                    {
                        break;
                    }
                case "checkboxCase":
                    {
                        break;
                    }
            }
        }

        private void SortTable_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox txtbox = sender as TextBox;
            switch(txtbox.Name)
            {
                case "textboxPatient":
                    {
                        break;
                    }
                case "textboxCase":
                    {
                        break;
                    }
            }
        }

        private void SortTable_Click_Filter(object sender, RoutedEventArgs e)
        {
            RadioButton radioBtn = sender as RadioButton;
            switch(radioBtn.Name)
            {
                case "DateFilterAll":
                    {
                        break;
                    }
                case "DateFilterToday":
                    {
                        break;
                    }
                case "DateFilterLW":
                    {
                        break;
                    }
                case "DateFilterL2W":
                    {
                        break;
                    }
                case "SoftwareFilterAll":
                    {
                        break;
                    }
                case "SoftwareFilterCAD":
                    {
                        break;
                    }
                case "SoftwareFilterImplant":
                    {
                        break;
                    }
                case "SoftwareFilterOrtho":
                    {
                        break;
                    }
                case "SoftwareFilterTray":
                    {
                        break;
                    }
                case "SoftwareFilterSplint":
                    {
                        break;
                    }
            }
        }
        #endregion

        
    }
}
