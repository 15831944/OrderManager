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
using System.Windows.Media.Effects;
using System.Diagnostics;

//Microsoft.Expression.Drawing.dll如果要用多國語言套件: "C:\Program Files (x86)\Microsoft SDKs\Expression\Blend\.NETFramework\v4.5\Libraries"
//抓取程式碼行數: new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString()

namespace OrderManagerNew
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        LogRecorder log;//日誌檔cs
        bool developerMode = false;//開發者模式
        string OrderManagerLanguage;//語系

        public MainWindow()
        {
            InitializeComponent();

            //OrderManager不能多開
            Process[] MyProcess = Process.GetProcessesByName("OrderManager");
            if (MyProcess.Length > 1)
            {
                this.Hide();

                if (MessageBox.Show(OrderManagerNew.TranslationSource.Instance["OMAlreadyRunning"], "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes, MessageBoxOptions.DefaultDesktopOnly) == MessageBoxResult.Yes)
                {
                    try
                    {
                        MyProcess[0].Kill(); //關閉執行中的程式
                    }
                    catch { }
                }
                else
                {
                    this.Close();
                }
            }

            log = new LogRecorder();
            titlebar_OrderManagerVersion.Content = "v" + Assembly.GetExecutingAssembly().GetName().Version.ToString();//TitleBar顯示OrderManager版本
            log.RecordConfigLog("OM Startup", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());

            OrderManagerLanguage = Properties.Settings.Default.sysLanguage;
            LocalizationService.SetLanguage(OrderManagerLanguage);
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
        private void FunctionTable_Click_Setting(object sender, RoutedEventArgs e)
        {
            //主視窗羽化
            var blur = new BlurEffect();
            this.Effect = blur;

            Setting DialogSetting = new Setting();
            DialogSetting.Owner = this;
            DialogSetting.ShowActivated = true;
            DialogSetting.ShowDialog();
            if(DialogSetting.DialogResult == true)
                log.RecordConfigLog("Config changed", new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString());

            //主視窗還原
            this.Effect = null;
            this.OpacityMask = null;
        }

        #endregion


        #region SortTable事件
        private void SortTable_Checked(object sender, RoutedEventArgs e)
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
            switch (txtbox.Name)
            {
                case "textboxPatient":
                    {
                        if(txtbox.Text == "-engineer")
                        {
                            var messageQueue = SnackbarMain.MessageQueue;
                            string message = "";
                            if (developerMode == false)
                            {
                                //開發者模式
                                developerMode = true;
                                message ="Developer Mode";
                                Panel.SetZIndex(Dev_btnGrid, 10);
                            }
                            else
                            {
                                //使用者模式
                                developerMode = false;
                                message = "Customer Mode";
                                Panel.SetZIndex(Dev_btnGrid, -1);
                            }
                            Task.Factory.StartNew(() => messageQueue.Enqueue(message));
                            txtbox.Text = "";
                            Keyboard.ClearFocus();
                        }

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
            switch (radioBtn.Name)
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
                            case (int)_softwareStatus.NotInstall:
                                {
                                    mask_EZCAD.Visibility = Visibility.Visible;
                                    process_EZCAD.Visibility = Visibility.Hidden;
                                    break;
                                }
                            case (int)_softwareStatus.Downloading:
                                {
                                    mask_EZCAD.Visibility = Visibility.Hidden;
                                    process_EZCAD.Visibility = Visibility.Visible;
                                    break;
                                }
                            case (int)_softwareStatus.Installed:
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
                            case (int)_softwareStatus.NotInstall:
                                {
                                    mask_Implant.Visibility = Visibility.Visible;
                                    process_Implant.Visibility = Visibility.Hidden;
                                    break;
                                }
                            case (int)_softwareStatus.Downloading:
                                {
                                    mask_Implant.Visibility = Visibility.Hidden;
                                    process_Implant.Visibility = Visibility.Visible;
                                    break;
                                }
                            case (int)_softwareStatus.Installed:
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
                            case (int)_softwareStatus.NotInstall:
                                {
                                    mask_Ortho.Visibility = Visibility.Visible;
                                    process_Ortho.Visibility = Visibility.Hidden;
                                    break;
                                }
                            case (int)_softwareStatus.Downloading:
                                {
                                    mask_Ortho.Visibility = Visibility.Hidden;
                                    process_Ortho.Visibility = Visibility.Visible;
                                    break;
                                }
                            case (int)_softwareStatus.Installed:
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
                            case (int)_softwareStatus.NotInstall:
                                {
                                    mask_Tray.Visibility = Visibility.Visible;
                                    process_Tray.Visibility = Visibility.Hidden;
                                    break;
                                }
                            case (int)_softwareStatus.Downloading:
                                {
                                    mask_Tray.Visibility = Visibility.Hidden;
                                    process_Tray.Visibility = Visibility.Visible;
                                    break;
                                }
                            case (int)_softwareStatus.Installed:
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
                            case (int)_softwareStatus.NotInstall:
                                {
                                    mask_Splint.Visibility = Visibility.Visible;
                                    process_Splint.Visibility = Visibility.Hidden;
                                    break;
                                }
                            case (int)_softwareStatus.Downloading:
                                {
                                    mask_Splint.Visibility = Visibility.Hidden;
                                    process_Splint.Visibility = Visibility.Visible;
                                    break;
                                }
                            case (int)_softwareStatus.Installed:
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

        private void Dev_Click_Btn(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.path_EZCAD = "";
            Properties.Settings.Default.path_Implant = "";
            Properties.Settings.Default.path_Ortho = "";
            Properties.Settings.Default.path_Tray = "";
            Properties.Settings.Default.path_Splint = "";
            Properties.Settings.Default.path_Guide = "";
            Properties.Settings.Default.sysLanguage = "";
            Properties.Settings.Default.Save();
        }

        private void Closing_MainWindow(object sender, System.ComponentModel.CancelEventArgs e)
        {
            log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "Closing OM", "Manual Shutdown.");
        }
    }
}
