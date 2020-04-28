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
using System.Windows.Media.Animation;

//Microsoft.Expression.Drawing.dll如果要針對多國語言版本: "C:\Program Files (x86)\Microsoft SDKs\Expression\Blend\.NETFramework\v4.5\Libraries"
//抓取程式碼行數: new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString()

//Detect which rectangle has been clicked in Canvas WPF: https://stackoverflow.com/questions/29669169/detect-which-rectangle-has-been-clicked-in-canvas-wpf
//C#的BackgroundWorker--啟動後臺執行緒: https://www.itread01.com/content/1550455929.html

namespace OrderManagerNew
{
    public partial class MainWindow : Window
    {
        #region 變數宣告
        LogRecorder log;                            //日誌檔cs
        UpdateFunction UpdateFunc;                  //軟體更新cs
        BeforeDownload DialogBeforeDownload;        //下載前置畫面
        bool developerMode = true;                  //開發者模式
        bool loginStatus = false;                   //是否登入了
        bool showUserDetail = false;                //是否正在顯示UserDetail
        MaterialDesignThemes.Wpf.SnackbarMessageQueue MainsnackbarMessageQueue; //Snackbar
        #endregion
        
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

            //初始化LogRecorder
            log = new LogRecorder();
            titlebar_OrderManagerVersion.Content = "v" + Assembly.GetExecutingAssembly().GetName().Version.ToString();  //TitleBar顯示OrderManager版本
            log.RecordConfigLog("OM Startup", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());

            //設定Snackbar顯示時間
            var myMessageQueue = new MaterialDesignThemes.Wpf.SnackbarMessageQueue(TimeSpan.FromMilliseconds(1000));
            SnackbarMain.MessageQueue = myMessageQueue;
            MainsnackbarMessageQueue = SnackbarMain.MessageQueue;

            MediaTimeline.DesiredFrameRateProperty.OverrideMetadata(typeof(System.Windows.Media.Animation.Timeline), new FrameworkPropertyMetadata(1000));    //設定動畫流暢度
            LocalizationService.SetLanguage(Properties.Settings.Default.sysLanguage);   //設定語系

            
            UpdateFunc = new UpdateFunction();
            UpdateFunc.softwareLogoShowEvent += new UpdateFunction.softwareLogoShowEventHandler(Handler_setSoftwareShow);
            UpdateFunc.Handler_snackbarShow += new UpdateFunction.updatefuncEventHandler_snackbar(SnackBarShow);
            UpdateFunc.checkExistSoftware(true);    //檢查有安裝哪些軟體
            UpdateFunc.loadHLXml();                 //截取線上HL.xml內的資料

            //工程師模式切換
            if (developerMode == true)
            {
                //開發者模式
                string message = "Developer Mode";
                Thickness Custommargin = Dev_btnGrid.Margin;
                Custommargin.Bottom = 40;
                Dev_btnGrid.Margin = Custommargin;
                Properties.Settings.Default.engineerMode = true;
                SnackBarShow(message);
            }
            else
            {
                //使用者模式
                Thickness Custommargin = Dev_btnGrid.Margin;
                Custommargin.Bottom = -120;
                Dev_btnGrid.Margin = Custommargin;
                Properties.Settings.Default.engineerMode = false;
            }
            
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
                        //Reset Properties
                        Properties.Settings.Default.path_EZCAD = "";
                        Properties.Settings.Default.path_Implant = "";
                        Properties.Settings.Default.path_Ortho = "";
                        Properties.Settings.Default.path_Tray = "";
                        Properties.Settings.Default.path_Splint = "";
                        Properties.Settings.Default.path_Guide = "";
                        Properties.Settings.Default.sysLanguage = "";
                        Properties.Settings.Default.DownloadFolder = "";
                        Properties.Settings.Default.engineerMode = false;
                        Properties.Settings.Default.PingTime = 5;
                        Properties.Settings.Default.Save();
                        UpdateFunc.checkExistSoftware(false);
                        SnackBarShow("Reset Properties Success");
                        break;
                    }
                case "DevBtn2":
                    {
                        //Load HL.xml
                        UpdateFunc.loadHLXml();
                        break;
                    }
                case "DevBtn3":
                    {
                        //Delete Log
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
                        //Show Log
                        if (File.Exists("OrderManager.log") == true)
                        {
                            Process OpenLog = new Process();
                            OpenLog.StartInfo.FileName = "OrderManager.log";
                            OpenLog.Start();
                        }
                        else
                            SnackBarShow("Log not found.");
                        break;
                    }
                case "DevBtn5":
                    {
                        //LoginStatus
                        if(loginStatus == true)
                        {
                            loginStatus = false;
                            SnackBarShow("loginStatus = false");
                        }
                        else
                        {
                            loginStatus = true;
                            SnackBarShow("loginStatus = true");
                        }
                        break;
                    }
                case "DevBtn6":
                    {
                        //Splint Download
                        SetAllSoftwareTableDownloadisEnable(false);
                        DialogBeforeDownload = new BeforeDownload();
                        DialogBeforeDownload.SetHttpResponseOK += new BeforeDownload.beforedownloadEventHandler(Handler_ShowBeforeDownload);
                        DialogBeforeDownload.Handler_snackbarShow += new BeforeDownload.beforedownloadEventHandler_snackbar(SnackBarShow);
                        DialogBeforeDownload.GethttpResoponse(@"https://www.dropbox.com/s/dj6p305a3ckkjxz/EZCAD.Splint_1.0.20214.0.exe?dl=1", (int)_softwareID.Splint);

                        break;
                    }
            }
        }

        private void PMouseDown_Main(object sender, MouseButtonEventArgs e)
        {
            //點擊UserDetail以外的地方就收回
            var mouseWasDownOnUserDetail = e.Source as UserControls.AirdentalUserDetail;
            var mouseWasDownOnomFunction_User = e.Source as Button;
            if (loginStatus == true)
            {
                if (mouseWasDownOnomFunction_User != null && mouseWasDownOnomFunction_User.Name == "omFunction_User")
                    return;

                if (mouseWasDownOnUserDetail == null)
                    UserDetailshow(false);
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
        /// <param name="downloadPercent">(下載百分比) 100%的值為1.00</param>
        /// <returns></returns>
        void Handler_setSoftwareShow(int softwareID, int currentProgress, double downloadPercent)
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
                                    process_EZCAD.EndAngle = 360 - 360 * downloadPercent;
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
                                    process_Implant.EndAngle = 360 - 360 * downloadPercent;
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
                                    process_Ortho.EndAngle = 360 - 360 * downloadPercent;
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
                                    process_Tray.EndAngle = 360 - 360 * downloadPercent;
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
                                    process_Splint.EndAngle = 360 - 360 * downloadPercent;
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
                        UpdateFunc.readyInstallSoftwareID = (int)_softwareID.Splint;
                        SetAllSoftwareTableDownloadisEnable(false);
                        DialogBeforeDownload = new BeforeDownload();
                        DialogBeforeDownload.SetHttpResponseOK += new BeforeDownload.beforedownloadEventHandler(Handler_ShowBeforeDownload);
                        DialogBeforeDownload.Handler_snackbarShow += new BeforeDownload.beforedownloadEventHandler_snackbar(SnackBarShow);
                        DialogBeforeDownload.GethttpResoponse(@"https://www.dropbox.com/s/dj6p305a3ckkjxz/EZCAD.Splint_1.0.20214.0.exe?dl=1" , (int)_softwareID.Splint);
                        
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

        void SetAllSoftwareTableDownloadisEnable(bool enable)
        {
            cad_download.IsEnabled = enable;
            implant_download.IsEnabled = enable;
            ortho_download.IsEnabled = enable;
            tray_download.IsEnabled = enable;
            splint_download.IsEnabled = enable;
        }

        /// <summary>
            /// 從網上獲取下載資料成功就顯示BeforeDownload頁面
            /// </summary>
        void Handler_ShowBeforeDownload()
        {
            bool DownloadStart = false;
            SetAllSoftwareTableDownloadisEnable(true);
            if (DialogBeforeDownload.SetInformation() == true)
            {
                //主視窗羽化
                var blur = new BlurEffect();
                this.Effect = blur;

                DialogBeforeDownload.Owner = this;
                DialogBeforeDownload.ShowActivated = true;
                DialogBeforeDownload.ShowDialog();
                if(DialogBeforeDownload.DialogResult == true)
                {
                    SetAllSoftwareTableDownloadisEnable(false);
                    SnackBarShow("Start Download"); //開始下載 //TODO 多國語系
                    DownloadStart = true;
                }

                //主視窗還原
                this.Effect = null;
                this.OpacityMask = null;
            }

            if (DownloadStart == true)
                UpdateFunc.StartDownloadSoftware();
        }
        
        private void FunctionTable_Click_User(object sender, RoutedEventArgs e)
        {
            if (loginStatus == false)
            {
                AirdentalLogin DialogLogin = new AirdentalLogin();
                DialogLogin.Owner = this;
                DialogLogin.ShowDialog();
            }
            else
            {
                if (showUserDetail == false)
                    UserDetailshow(true);
                else
                    UserDetailshow(false);
            }
        }
        
        private void UserDetail_Click_Logout(object sender, RoutedEventArgs e)
        {
            UserDetailshow(false);
            loginStatus = false;
            SnackBarShow("Logout");
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

        /// <summary>
        /// 是否顯示UserDetail
        /// </summary>
        /// <param name="show"> 是否顯示</param>
        /// <returns></returns>
        private void UserDetailshow(bool show)
        {
            if (show == true)
            {
                Storyboard sb = (Storyboard)TryFindResource("UserDetailOpen");
                foreach (var animation in sb.Children)
                {
                    Storyboard.SetTargetName(animation, "usercontrolUserDetail");
                    Storyboard.SetTarget(animation, this.usercontrolUserDetail);
                }
                sb.Begin();
                showUserDetail = true;
            }
            else
            {
                Storyboard sb = (Storyboard)TryFindResource("UserDetailClose");
                foreach (var animation in sb.Children)
                {
                    Storyboard.SetTargetName(animation, "usercontrolUserDetail");
                    Storyboard.SetTarget(animation, this.usercontrolUserDetail);
                }
                sb.Begin();
                showUserDetail = false;
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
            Task.Factory.StartNew(() => MainsnackbarMessageQueue.Enqueue(Message));
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
        
        
    }
}
