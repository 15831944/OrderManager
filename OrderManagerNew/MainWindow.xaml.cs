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
using System.IO;

//Microsoft.Expression.Drawing.dll如果要用多國語言套件: "C:\Program Files (x86)\Microsoft SDKs\Expression\Blend\.NETFramework\v4.5\Libraries"
//抓取程式碼行數: new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString()

namespace OrderManagerNew
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        LogRecorder log;                //日誌檔cs
        UpdateFunction UpdateFunc;      //軟體更新cs
        bool developerMode = false;     //開發者模式
        string OrderManagerLanguage;    //語系

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
            titlebar_OrderManagerVersion.Content = "v" + Assembly.GetExecutingAssembly().GetName().Version.ToString();  //TitleBar顯示OrderManager版本
            log.RecordConfigLog("OM Startup", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());

            var myMessageQueue = new MaterialDesignThemes.Wpf.SnackbarMessageQueue(TimeSpan.FromMilliseconds(1000));
            SnackbarMain.MessageQueue = myMessageQueue;

            OrderManagerLanguage = Properties.Settings.Default.sysLanguage;
            LocalizationService.SetLanguage(OrderManagerLanguage);

            //檢查有安裝哪些軟體
            UpdateFunc = new UpdateFunction();
            UpdateFunc.softwareLogoShowEvent += new UpdateFunction.softwareLogoShowEventHandler(setSoftwareShow);
            UpdateFunc.checkExistSoftware(true);
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

        private void Closing_MainWindow(object sender, System.ComponentModel.CancelEventArgs e)
        {
            log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "Closing OM", "Manual Shutdown.");
        }

        private void Dev_Click_Btn(object sender, RoutedEventArgs e)
        {
            Button Btn = sender as Button;
            switch (Btn.Name)
            {
                case "DevBtn1":
                    {
                        Properties.Settings.Default.path_EZCAD = "";
                        Properties.Settings.Default.path_Implant = "";
                        Properties.Settings.Default.path_Ortho = "";
                        Properties.Settings.Default.path_Tray = "";
                        Properties.Settings.Default.path_Splint = "";
                        Properties.Settings.Default.path_Guide = "";
                        Properties.Settings.Default.sysLanguage = "";
                        Properties.Settings.Default.Save();

                        UpdateFunc.checkExistSoftware(false);
                        break;
                    }
                case "DevBtn2":
                    {
                        UpdateFunc.loadHLXml();
                        showLog();
                        break;
                    }
                case "DevBtn3":
                    {
                        if (File.Exists("OrderManager.log") == true)
                        {
                            File.Delete("OrderManager.log");
                            SnackBarShow("Log delete success.");
                        }
                        else
                            SnackBarShow("Log not found.");

                        break;
                    }
                case "DevBtn4":
                    {
                        showLog();
                        break;
                    }
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
            GoToSetting("");
        }

        /// <summary>
        /// 設定SofttwareTable各Icon顯示狀態
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
                                    popupbox_EZCAD.IsEnabled = true;
                                    cad_selectPath.Visibility = Visibility.Visible;
                                    cad_download.Visibility = Visibility.Visible;
                                    cad_open.Visibility = Visibility.Collapsed;
                                    cad_demo.Visibility = Visibility.Collapsed;
                                    cad_troubleShooting.Visibility = Visibility.Collapsed;
                                    cad_buyLic.Visibility = Visibility.Collapsed;
                                    cad_unInstall.Visibility = Visibility.Collapsed;
                                    break;
                                }
                            case (int)_softwareStatus.Downloading:
                                {
                                    mask_EZCAD.Visibility = Visibility.Hidden;
                                    process_EZCAD.Visibility = Visibility.Visible;
                                    popupbox_EZCAD.IsEnabled = false;
                                    break;
                                }
                            case (int)_softwareStatus.Installed:
                                {
                                    mask_EZCAD.Visibility = Visibility.Hidden;
                                    process_EZCAD.Visibility = Visibility.Hidden;
                                    popupbox_EZCAD.IsEnabled = true;
                                    cad_selectPath.Visibility = Visibility.Collapsed;
                                    cad_download.Visibility = Visibility.Collapsed;
                                    cad_open.Visibility = Visibility.Visible;
                                    cad_demo.Visibility = Visibility.Visible;
                                    cad_troubleShooting.Visibility = Visibility.Visible;
                                    cad_buyLic.Visibility = Visibility.Visible;
                                    cad_unInstall.Visibility = Visibility.Visible;
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
                                    popupbox_Implant.IsEnabled = true;
                                    implant_selectPath.Visibility = Visibility.Visible;
                                    implant_download.Visibility = Visibility.Visible;
                                    implant_create.Visibility = Visibility.Collapsed;
                                    implant_demo.Visibility = Visibility.Collapsed;
                                    implant_troubleShooting.Visibility = Visibility.Collapsed;
                                    implant_buyLic.Visibility = Visibility.Collapsed;
                                    implant_unInstall.Visibility = Visibility.Collapsed;
                                    break;
                                }
                            case (int)_softwareStatus.Downloading:
                                {
                                    mask_Implant.Visibility = Visibility.Hidden;
                                    process_Implant.Visibility = Visibility.Visible;
                                    popupbox_Implant.IsEnabled = false;
                                    break;
                                }
                            case (int)_softwareStatus.Installed:
                                {
                                    mask_Implant.Visibility = Visibility.Hidden;
                                    process_Implant.Visibility = Visibility.Hidden;
                                    popupbox_Implant.IsEnabled = true;
                                    implant_selectPath.Visibility = Visibility.Collapsed;
                                    implant_download.Visibility = Visibility.Collapsed;
                                    implant_create.Visibility = Visibility.Visible;
                                    implant_demo.Visibility = Visibility.Visible;
                                    implant_troubleShooting.Visibility = Visibility.Visible;
                                    implant_buyLic.Visibility = Visibility.Visible;
                                    implant_unInstall.Visibility = Visibility.Visible;
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
                                    popupbox_Ortho.IsEnabled = true;
                                    ortho_selectPath.Visibility = Visibility.Visible;
                                    ortho_download.Visibility = Visibility.Visible;
                                    ortho_open.Visibility = Visibility.Collapsed;
                                    ortho_demo.Visibility = Visibility.Collapsed;
                                    ortho_troubleShooting.Visibility = Visibility.Collapsed;
                                    ortho_buyLic.Visibility = Visibility.Collapsed;
                                    ortho_unInstall.Visibility = Visibility.Collapsed;
                                    break;
                                }
                            case (int)_softwareStatus.Downloading:
                                {
                                    mask_Ortho.Visibility = Visibility.Hidden;
                                    process_Ortho.Visibility = Visibility.Visible;
                                    popupbox_Ortho.IsEnabled = false;
                                    break;
                                }
                            case (int)_softwareStatus.Installed:
                                {
                                    mask_Ortho.Visibility = Visibility.Hidden;
                                    process_Ortho.Visibility = Visibility.Hidden;
                                    popupbox_Ortho.IsEnabled = true;
                                    ortho_selectPath.Visibility = Visibility.Collapsed;
                                    ortho_download.Visibility = Visibility.Collapsed;
                                    ortho_open.Visibility = Visibility.Visible;
                                    ortho_demo.Visibility = Visibility.Visible;
                                    ortho_troubleShooting.Visibility = Visibility.Visible;
                                    ortho_buyLic.Visibility = Visibility.Visible;
                                    ortho_unInstall.Visibility = Visibility.Visible;
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
                                    popupbox_Tray.IsEnabled = true;
                                    tray_selectPath.Visibility = Visibility.Visible;
                                    tray_download.Visibility = Visibility.Visible;
                                    tray_open.Visibility = Visibility.Collapsed;
                                    tray_demo.Visibility = Visibility.Collapsed;
                                    tray_troubleShooting.Visibility = Visibility.Collapsed;
                                    tray_buyLic.Visibility = Visibility.Collapsed;
                                    tray_unInstall.Visibility = Visibility.Collapsed;
                                    break;
                                }
                            case (int)_softwareStatus.Downloading:
                                {
                                    mask_Tray.Visibility = Visibility.Hidden;
                                    process_Tray.Visibility = Visibility.Visible;
                                    popupbox_Tray.IsEnabled = false;
                                    break;
                                }
                            case (int)_softwareStatus.Installed:
                                {
                                    mask_Tray.Visibility = Visibility.Hidden;
                                    process_Tray.Visibility = Visibility.Hidden;
                                    popupbox_Tray.IsEnabled = true;
                                    tray_selectPath.Visibility = Visibility.Collapsed;
                                    tray_download.Visibility = Visibility.Collapsed;
                                    tray_open.Visibility = Visibility.Visible;
                                    tray_demo.Visibility = Visibility.Visible;
                                    tray_troubleShooting.Visibility = Visibility.Visible;
                                    tray_buyLic.Visibility = Visibility.Visible;
                                    tray_unInstall.Visibility = Visibility.Visible;
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
                                    popupbox_Splint.IsEnabled = true;
                                    splint_selectPath.Visibility = Visibility.Visible;
                                    splint_download.Visibility = Visibility.Visible;
                                    splint_open.Visibility = Visibility.Collapsed;
                                    splint_demo.Visibility = Visibility.Collapsed;
                                    splint_troubleShooting.Visibility = Visibility.Collapsed;
                                    splint_buyLic.Visibility = Visibility.Collapsed;
                                    splint_unInstall.Visibility = Visibility.Collapsed;
                                    break;
                                }
                            case (int)_softwareStatus.Downloading:
                                {
                                    mask_Splint.Visibility = Visibility.Hidden;
                                    process_Splint.Visibility = Visibility.Visible;
                                    popupbox_Splint.IsEnabled = false;
                                    break;
                                }
                            case (int)_softwareStatus.Installed:
                                {
                                    mask_Splint.Visibility = Visibility.Hidden;
                                    process_Splint.Visibility = Visibility.Hidden;
                                    popupbox_Splint.IsEnabled = true;
                                    splint_selectPath.Visibility = Visibility.Collapsed;
                                    splint_download.Visibility = Visibility.Collapsed;
                                    splint_open.Visibility = Visibility.Visible;
                                    splint_demo.Visibility = Visibility.Visible;
                                    splint_troubleShooting.Visibility = Visibility.Visible;
                                    splint_buyLic.Visibility = Visibility.Visible;
                                    splint_unInstall.Visibility = Visibility.Visible;
                                    break;
                                }
                        }
                        break;
                    }
            }
        }

        /// <summary>
        /// 設定SofttwareTable的PopupBox事件
        /// </summary>
        private void SoftwareTable_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            switch (btn.Name)
            {
                case "buyLic":
                    {
                        break;
                    }

                #region EZCAD
                case "cad_selectPath":
                    {
                        GoToSetting("Btn_EZCADprogram");
                        break;
                    }
                case "cad_download":
                    {
                        break;
                    }
                case "cad_open":
                    {
                        break;
                    }
                case "cad_webIntro":
                    {
                        break;
                    }
                case "cad_demo":
                    {
                        break;
                    }
                case "cad_troubleShooting":
                    {
                        break;
                    }
                case "cad_unInstall":
                    {
                        break;
                    }
                #endregion

                #region Implant
                case "implant_selectPath":
                    {
                        GoToSetting("Btn_Implantprogram");
                        break;
                    }
                case "implant_download":
                    {
                        break;
                    }
                case "implant_create":
                    {
                        break;
                    }
                case "implant_webIntro":
                    {
                        break;
                    }
                case "implant_demo":
                    {
                        break;
                    }
                case "implant_troubleShooting":
                    {
                        break;
                    }
                case "implant_unInstall":
                    {
                        break;
                    }
                #endregion

                #region Ortho
                case "ortho_selectPath":
                    {
                        GoToSetting("Btn_Orthoprogram");
                        break;
                    }
                case "ortho_download":
                    {
                        break;
                    }
                case "ortho_open":
                    {
                        break;
                    }
                case "ortho_webIntro":
                    {
                        break;
                    }
                case "ortho_demo":
                    {
                        break;
                    }
                case "ortho_troubleShooting":
                    {
                        break;
                    }
                case "ortho_unInstall":
                    {
                        break;
                    }
                #endregion

                #region Tray
                case "tray_selectPath":
                    {
                        GoToSetting("Btn_Trayprogram");
                        break;
                    }
                case "tray_download":
                    {
                        break;
                    }
                case "tray_open":
                    {
                        break;
                    }
                case "tray_webIntro":
                    {
                        break;
                    }
                case "tray_demo":
                    {
                        break;
                    }
                case "tray_troubleShooting":
                    {
                        break;
                    }
                case "tray_unInstall":
                    {
                        break;
                    }
                #endregion

                #region Splint
                case "splint_selectPath":
                    {
                        GoToSetting("Btn_Splintprogram");
                        break;
                    }
                case "splint_download":
                    {
                        break;
                    }
                case "splint_open":
                    {
                        break;
                    }
                case "splint_webIntro":
                    {
                        break;
                    }
                case "splint_demo":
                    {
                        break;
                    }
                case "splint_troubleShooting":
                    {
                        break;
                    }
                case "splint_unInstall":
                    {
                        break;
                    }
                    #endregion
            }
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

        /// <summary>
        /// 跳出Snackbar訊息
        /// </summary>
        /// <param name="Message"> 要顯示的訊息</param>
        /// <returns></returns>
        private void SnackBarShow(string Message)
        {
            var messageQueue = SnackbarMain.MessageQueue;
            Task.Factory.StartNew(() => messageQueue.Enqueue(Message));
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
                            string message = "";
                            if (developerMode == false)
                            {
                                //開發者模式
                                developerMode = true;
                                message ="Developer Mode";
                                //Panel.SetZIndex(Dev_btnGrid, 10);
                                Thickness Custommargin = Dev_btnGrid.Margin;
                                Custommargin.Bottom = 40;
                                Dev_btnGrid.Margin = Custommargin;
                                Properties.Settings.Default.engineerMode = true;
                            }
                            else
                            {
                                //使用者模式
                                developerMode = false;
                                message = "Customer Mode";
                                //Panel.SetZIndex(Dev_btnGrid, -1);
                                Thickness Custommargin = Dev_btnGrid.Margin;
                                Custommargin.Bottom = -120;
                                Dev_btnGrid.Margin = Custommargin;
                                Properties.Settings.Default.engineerMode = false;
                            }
                            SnackBarShow(message);
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
        /// 記事本開啟Log檔
        /// </summary>
        private void showLog()
        {
            if (File.Exists("OrderManager.log") == true)
            {
                Process OpenLog = new Process();
                OpenLog.StartInfo.FileName = "OrderManager.log";
                OpenLog.Start();
            }
            else
                SnackBarShow("Log not found.");
        }
        

        private void GoToSetting(string softwareName)
        {
            //主視窗羽化
            var blur = new BlurEffect();
            this.Effect = blur;

            Setting DialogSetting = new Setting();
            DialogSetting.Owner = this;
            DialogSetting.ShowActivated = true;

            if (softwareName != "")
                DialogSetting.SearchEXE("", softwareName);

            DialogSetting.ShowDialog();
            if (DialogSetting.DialogResult == true)
            {
                UpdateFunc.checkExistSoftware(true);
                log.RecordConfigLog("FunctionTable_Click_Setting()", "Config changed");
            }

            //主視窗還原
            this.Effect = null;
            this.OpacityMask = null;
        }
    }
}
