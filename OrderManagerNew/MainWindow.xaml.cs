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
using Path = System.IO.Path;
using CadInformation = OrderManagerNew.UserControls.Order_cadBase.CadInformation;
using TrayInformation = OrderManagerNew.UserControls.Order_tsBase.TrayInformation;
using SplintInformation = OrderManagerNew.UserControls.Order_tsBase.SplintInformation;
using ImplantOuterInformation = OrderManagerNew.UserControls.Order_implantBase.ImplantOuterInformation;
using OrthoOuterInformation = OrderManagerNew.UserControls.Order_orthoBase.OrthoOuterInformation;
//Mahapps套件(NuGet下載): MaterialDesignThemes.MahApps v0.0.12


//Microsoft.Expression.Drawing.dll如果要針對多國語言版本: "C:\Program Files (x86)\Microsoft SDKs\Expression\Blend\.NETFramework\v4.5\Libraries"
//抓取程式碼行數: new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString()

//Detect which rectangle has been clicked in Canvas WPF: https://stackoverflow.com/questions/29669169/detect-which-rectangle-has-been-clicked-in-canvas-wpf
//C#的BackgroundWorker--啟動後臺執行緒: https://www.itread01.com/content/1550455929.html
//C#程序以管理員權限運行: https://www.itread01.com/content/1501935602.html

namespace OrderManagerNew
{
    public partial class MainWindow : Window
    {
#region 變數宣告
        /// <summary>
        /// 日誌檔cs
        /// </summary>
        LogRecorder log;
        /// <summary>
        /// 軟體更新cs
        /// </summary>
        UpdateFunction UpdateFunc;
        /// <summary>
        /// 下載前置畫面
        /// </summary>
        BeforeDownload DialogBeforeDownload;
        /// <summary>
        /// OrderManager的函式
        /// </summary>
        OrderManagerFunctions OrderManagerFunc;
        /// <summary>
        /// 專案Case的函式
        /// </summary>
        ProjectHandle ProjHandle;
        /// <summary>
        /// 開發者模式
        /// </summary>
        bool developerMode = true;
        /// <summary>
        /// 是否登入了
        /// </summary>
        bool loginStatus = false;
        /// <summary>
        /// 是否正在顯示UserDetail
        /// </summary>
        bool showUserDetail = false;
        /// <summary>
        /// 判斷安裝時是否有執行檔了
        /// </summary>
        bool haveEXE = false;
        /// <summary>
        /// 記錄使用者按下哪個軟體的SoftwareTable
        /// </summary>
        int CheckedSoftwareID;
        MaterialDesignThemes.Wpf.SnackbarMessageQueue MainsnackbarMessageQueue; //Snackbar
        FileSystemWatcher _watcherEZCAD, _watcherImplant, _watcherOrtho, _watcherTray, _watcherSplint;
#endregion
        
        public MainWindow()
        {
            InitializeComponent();
            string[] args;
            args = Environment.GetCommandLineArgs();
            if(args != null && args.Length > 1)
            {
                foreach(string argument in args)
                {
                    if (argument == "-Rec")
                        Properties.Settings.Default.FullRecord = true;
                }
            }
            
            CheckedSoftwareID = -1;

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

            ProjHandle = new ProjectHandle();
            ProjHandle.CaseShowEvent += new ProjectHandle.caseShowEventHandler(Handler_SetCaseShow);

            OrderManagerFunc = new OrderManagerFunctions();
            OrderManagerFunc.SoftwareLogoShowEvent += new OrderManagerFunctions.softwareLogoShowEventHandler(Handler_setSoftwareShow);

            UpdateFunc = new UpdateFunction();
            UpdateFunc.SoftwareLogoShowEvent += new UpdateFunction.softwareLogoShowEventHandler(Handler_setSoftwareShow);
            UpdateFunc.Handler_snackbarShow += new UpdateFunction.updatefuncEventHandler_snackbar(SnackBarShow);
            UpdateFunc.SoftwareUpdateEvent += new UpdateFunction.softwareUpdateStatusHandler(Handler_SetSoftwareUpdateButtonStatus);
            
            //工程師模式切換
            if (developerMode == true)
            {
                //開發者模式
                string message = "Developer Mode";
                Thickness Custommargin = Dev_btnGrid.Margin;
                Custommargin.Bottom = 40;
                Dev_btnGrid.Margin = Custommargin;
                Properties.Settings.Default.engineerMode = true;
                Properties.Settings.Default.FullRecord = true;
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

#region Watcher事件
        /// <summary>
        /// 安裝軟體時或是刪除軟體時監看軟體資料夾
        /// </summary>
        /// <param name="watcherCommand">Installing、Delete 參考EnumSummary的_watcherCommand</param>
        /// <param name="SoftwareID">軟體ID</param>
        private void Watcher_SoftwareInstall(int watcherCommand, int SoftwareID)
        {
            if (watcherCommand == (int)_watcherCommand.Install)   //安裝中
            {
                FileSystemWatcher watcher = new FileSystemWatcher
                {
                    Path = UpdateFunc.GetSoftwarePath(UpdateFunc.readyInstallSoftwareInfo.softwareID),

                    NotifyFilter = NotifyFilters.Size,
                    //設定是否監控子資料夾
                    IncludeSubdirectories = true,
                    //設定是否啟動元件，此部分必須要設定為 true，不然事件是不會被觸發的
                    EnableRaisingEvents = true
                };
                watcher.Created += new FileSystemEventHandler(Watcher_Installing_Changed);
                watcher.Changed += new FileSystemEventHandler(Watcher_Installing_Changed);
            }
            else if (watcherCommand == (int)_watcherCommand.Delete)
            {
                if (SoftwareID == -1)
                    return;

                FileSystemWatcher watcher = new FileSystemWatcher
                {
                    //softwarePath是執行檔路徑，所以要抓資料夾用Path.GetDirectoryName()
                    Path = Path.GetDirectoryName(UpdateFunc.GetSoftwarePath(SoftwareID)),

                    NotifyFilter = NotifyFilters.FileName,

                    IncludeSubdirectories = true,
                    EnableRaisingEvents = true
                };
                watcher.Deleted += new FileSystemEventHandler(Watcher_Deleting_Changed);
            }
        }

        /// <summary>
        /// 如果資料夾內容大於封裝包就變成"已安裝"
        /// </summary>
        private void Watcher_Installing_Changed(object sender, FileSystemEventArgs e)
        {
            if(sender is FileSystemWatcher)
            {
                FileSystemWatcher watcher = sender as FileSystemWatcher;
                //DirectoryInfo info = new DirectoryInfo(watcher.Path);
                double dirSize = (double)OrderManagerFunc.DirSize(new DirectoryInfo(watcher.Path));
                double LimitSize = UpdateFunc.readyInstallSoftwareInfo.softwareSize;

                string exeName = Path.GetFileName(e.FullPath).ToLower();
                if (exeName.IndexOf("cad.exe") != -1 || exeName.IndexOf("implantplanning.exe") != -1 || exeName.IndexOf("orthoanalysis.exe") != -1
                    || exeName.IndexOf("tray.exe") != -1 || exeName.IndexOf("splint.exe") != -1 || exeName.IndexOf("guide.exe") != -1)
                {
                    DialogBeforeDownload.SetPropertiesSoftewarePath(UpdateFunc.readyInstallSoftwareInfo.softwareID, e.FullPath);
                    haveEXE = true;
                }

                if (dirSize >= LimitSize && haveEXE == true)
                {
                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        haveEXE = false;
                        watcher = new FileSystemWatcher();
                        Handler_setSoftwareShow(UpdateFunc.readyInstallSoftwareInfo.softwareID, (int)_softwareStatus.Installed, 0);
                        System.Threading.Thread.Sleep(1000);
                    }));
                }
            }
        }

        /// <summary>
        /// 監看資料夾內容是否被清除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Watcher_Deleting_Changed(object sender, FileSystemEventArgs e)
        {
            bool HaveDeleted = false;
            string exeName = Path.GetFileName(e.FullPath).ToLower();
                if (HaveDeleted == false && (exeName.IndexOf("cad.exe") != -1 || exeName.IndexOf("implantplanning.exe") != -1 || exeName.IndexOf("orthoanalysis.exe") != -1
                    || exeName.IndexOf("tray.exe") != -1 || exeName.IndexOf("splint.exe") != -1 || exeName.IndexOf("guide.exe") != -1))
                {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    HaveDeleted = true;
                    SetAllSoftwareTableDownloadisEnable(true);
                    Handler_setSoftwareShow(CheckedSoftwareID, (int)_softwareStatus.NotInstall, 0);
                    OrderManagerFunc.AutoDetectEXE((int)_classFrom.MainWindow);
                }));
            }

        }

        /// <summary>
        /// 監看CaseProject
        /// </summary>
        /// <param name="Watcher">固定寫new Watcher()</param>
        /// <param name="Path">資料夾路徑</param>
        private void Watcher_CaseProject(FileSystemWatcher Watcher, string Path)
        {
            if (Directory.Exists(Path) == false)
                return;

            Watcher.Path = Path;
            //設定所要監控的變更類型
            Watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;

            //設定是否監控子資料夾
            Watcher.IncludeSubdirectories = true;

            //設定是否啟動元件，此部分必須要設定為 true，不然事件是不會被觸發的
            Watcher.EnableRaisingEvents = true;

            //設定觸發事件
            Watcher.Created += new FileSystemEventHandler(Watcher_ProjectCreated);
            Watcher.Deleted += new FileSystemEventHandler(Watcher_ProjectDeleted);
        }

        private void Watcher_ProjectCreated(object sender, FileSystemEventArgs e)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                try
                {
                    if ((SoftwareFilterCAD.IsChecked == true) && (e.FullPath.Replace(e.Name, "") == Properties.OrderManagerProps.Default.cad_projectDirectory))
                        ProjHandle.LoadEZCADProj();
                    else if ((SoftwareFilterImplant.IsChecked == true) && (e.FullPath.Replace(e.Name, "") == Properties.OrderManagerProps.Default.implant_projectDirectory))
                        ProjHandle.LoadImplantProj();
                    else if ((SoftwareFilterOrtho.IsChecked == true) && (e.FullPath.Replace(e.Name, "") == Properties.OrderManagerProps.Default.ortho_projectDirectory))
                        ProjHandle.LoadOrthoProj();
                    else if ((SoftwareFilterTray.IsChecked == true) && (e.FullPath.Replace(e.Name, "") == Properties.OrderManagerProps.Default.tray_projectDirectory))
                        ProjHandle.LoadTrayProj();
                    else if ((SoftwareFilterSplint.IsChecked == true) && (e.FullPath.Replace(e.Name, "") == Properties.OrderManagerProps.Default.splint_projectDirectory))
                        ProjHandle.LoadSplintProj();
                }
                catch (Exception ex)
                {
                    log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "Watcher_ProjectCreated_exception", ex.Message);
                }
            }));

        }

        private void Watcher_ProjectDeleted(object sender, FileSystemEventArgs e)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                try
                {
                    if ((SoftwareFilterCAD.IsChecked == true) && (e.FullPath.Replace(e.Name, "") == Properties.OrderManagerProps.Default.cad_projectDirectory))
                        ProjHandle.LoadEZCADProj();
                    else if ((SoftwareFilterImplant.IsChecked == true) && (e.FullPath.Replace(e.Name, "") == Properties.OrderManagerProps.Default.implant_projectDirectory))
                        ProjHandle.LoadImplantProj();
                    else if ((SoftwareFilterOrtho.IsChecked == true) && (e.FullPath.Replace(e.Name, "") == Properties.OrderManagerProps.Default.ortho_projectDirectory))
                        ProjHandle.LoadOrthoProj();
                    else if ((SoftwareFilterTray.IsChecked == true) && (e.FullPath.Replace(e.Name, "") == Properties.OrderManagerProps.Default.tray_projectDirectory))
                        ProjHandle.LoadTrayProj();
                    else if ((SoftwareFilterSplint.IsChecked == true) && (e.FullPath.Replace(e.Name, "") == Properties.OrderManagerProps.Default.splint_projectDirectory))
                        ProjHandle.LoadSplintProj();
                }
                catch (Exception ex)
                {
                    log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "Watcher_ProjectDeleted_exception", ex.Message);
                }
            }));
        }
#endregion

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
            Environment.Exit(0);
        }

        private void Click_Dev_Btn(object sender, RoutedEventArgs e)
        {
            if(sender is Button)
            {
                switch (((Button)sender).Name)
                {
                    case "DevBtn1":
                        {
                            //Reset Properties
                            Properties.Settings.Default.cad_exePath = "";
                            Properties.Settings.Default.implant_exePath = "";
                            Properties.Settings.Default.ortho_exePath = "";
                            Properties.Settings.Default.tray_exePath = "";
                            Properties.Settings.Default.splint_exePath = "";
                            Properties.Settings.Default.guide_exePath = "";
                            Properties.Settings.Default.sysLanguage = "";
                            Properties.Settings.Default.DownloadFolder = "";
                            Properties.OrderManagerProps.Default.mostsoftwareDisk = "";
                            Properties.OrderManagerProps.Default.systemDisk = "";
                            Properties.Settings.Default.engineerMode = false;
                            Properties.Settings.Default.PingTime = 5;
                            Properties.Settings.Default.Save();

                            Properties.OrderManagerProps.Default.cad_projectDirectory = "";
                            Properties.OrderManagerProps.Default.implant_projectDirectory = "";
                            Properties.OrderManagerProps.Default.ortho_projectDirectory = "";
                            Properties.OrderManagerProps.Default.tray_projectDirectory = "";
                            Properties.OrderManagerProps.Default.splint_projectDirectory = "";
                            Properties.OrderManagerProps.Default.DateFilter = (int)_DateFilter.All;
                            Properties.OrderManagerProps.Default.PatientNameFilter = "";
                            Properties.OrderManagerProps.Default.CaseNameFilter = "";

                            Handler_setSoftwareShow((int)_softwareID.EZCAD, (int)_softwareStatus.NotInstall, 0.0);
                            Handler_setSoftwareShow((int)_softwareID.Implant, (int)_softwareStatus.NotInstall, 0.0);
                            Handler_setSoftwareShow((int)_softwareID.Ortho, (int)_softwareStatus.NotInstall, 0.0);
                            Handler_setSoftwareShow((int)_softwareID.Tray, (int)_softwareStatus.NotInstall, 0.0);
                            Handler_setSoftwareShow((int)_softwareID.Splint, (int)_softwareStatus.NotInstall, 0.0);
                            Handler_setSoftwareShow((int)_softwareID.Guide, (int)_softwareStatus.NotInstall, 0.0);

                            SnackBarShow("Reset Properties Success");
                            break;
                        }
                    case "DevBtn2":
                        {
                            //Load HL.xml
                            UpdateFunc.LoadHLXml();
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
                            if (loginStatus == true)
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
                    case "DevBtn7":
                        {
                            OrderManagerFunc.AutoDetectEXE((int)_classFrom.MainWindow);
                            break;
                        }
                    case "DevBtn8":
                        {
                            //reLoad Imp Proj
                            foreach(var childCase in StackPanel_Local.Children)
                            {
                                if(childCase is OrderManagerNew.UserControls.Order_orthoBase)
                                {

                                }
                            }
                            break;
                        }
                    case "DevBtn9":
                        {

                            break;
                        }
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

        private void Loaded_MainWindow(object sender, RoutedEventArgs e)
        {
            UpdateFunc.LoadHLXml();//截取線上HL.xml內的資料
            OrderManagerFunc.DoubleCheckEXEexist();//檢查軟體執行檔是否存在
        }
#endregion

#region TitleBar事件
        private Point startPos;

        [DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);
        [DllImport("user32.dll")]
        static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

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
                {}
                DragMove();
            }
        }

        private void Click_TitleBar_titlebarButtons(object sender, RoutedEventArgs e)
        {
            if(sender is Button)
            {
                switch (((Button)sender).Name)
                {
                    case "systemButton_ContactInteware":    //聯絡客服
                        OrderManagerFunc.RunCommandLine(Properties.HyperLink.Default.ContactInteware, "");
                        break;
                    case "systemButton_Minimize":           //最小化
                        this.WindowState = WindowState.Minimized;
                        break;
                    case "systemButton_Close":              //關閉
                        Close();
                        break;
                    default:
                        OrderManagerFunc.RunCommandLine(Properties.HyperLink.Default.ContactInteware, "");
                        break;
                }
            }
            else
            {
                //是ToggleButton，按鈕是"向下還原"或"最大化"
                this.WindowState = (this.WindowState == WindowState.Normal) ? WindowState.Maximized : WindowState.Normal;
            }
        }
#endregion

#region FunctionTable事件
        private void Click_FunctionTable_Setting(object sender, RoutedEventArgs e)
        {
            GoToSetting(-1);
        }
        
        /// <summary>
        /// 設定SofttwareTable各Icon顯示狀態
        /// </summary>
        /// <param name="softwareID">(軟體ID) EZCAD、Implant、Ortho、Tray、Splint</param>
        /// <param name="currentProgress">(目前進度) 未安裝、下載中... 請參考_SoftwareStatus</param>
        /// <param name="downloadPercent">(下載百分比) 100%的值為1.00</param>
        /// <returns></returns>
        private void Handler_setSoftwareShow(int softwareID, int currentProgress, double downloadPercent)
        {
            CheckedSoftwareID = softwareID;
            switch (softwareID)
            {
                case (int)_softwareID.EZCAD:
                    {
                        SoftwareFilterCAD.IsEnabled = false;
                        mask_EZCAD.Visibility = Visibility.Hidden;
                        process_EZCAD.Visibility = Visibility.Hidden;
                        cad_update.Visibility = Visibility.Collapsed;
                        updateimage_EZCAD.Visibility = Visibility.Hidden;
                        mask2_EZCAD_Installing.Visibility = Visibility.Hidden;
                        progressbar_EZCAD_Installing.Visibility = Visibility.Hidden;
                        switch (currentProgress)
                        {
                            case (int)_softwareStatus.NotInstall:
                                {
                                    mask_EZCAD.Visibility = Visibility.Visible;
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
                                    process_EZCAD.Visibility = Visibility.Visible;
                                    process_EZCAD.EndAngle = 360 - 360 * downloadPercent;
                                    popupbox_EZCAD.IsEnabled = false;
                                    break;
                                }
                            case (int)_softwareStatus.Installed:
                                {
                                    popupbox_EZCAD.IsEnabled = true;
                                    cad_selectPath.Visibility = Visibility.Collapsed;
                                    cad_download.Visibility = Visibility.Collapsed;
                                    cad_open.Visibility = Visibility.Visible;
                                    cad_demo.Visibility = Visibility.Visible;
                                    cad_troubleShooting.Visibility = Visibility.Visible;
                                    cad_buyLic.Visibility = Visibility.Visible;
                                    cad_unInstall.Visibility = Visibility.Visible;
                                    SoftwareFilterCAD.IsEnabled = true;
                                    cad_update.Visibility = Visibility.Visible;
                                    cad_update.IsEnabled = false;
                                    break;
                                }
                            case (int)_softwareStatus.Installing:
                                {
                                    mask2_EZCAD_Installing.Visibility = Visibility.Visible;
                                    progressbar_EZCAD_Installing.Visibility = Visibility.Visible;
                                    break;
                                }
                            case (int)_softwareStatus.Uninstalling:
                                {
                                    mask2_EZCAD_Installing.Visibility = Visibility.Visible;
                                    progressbar_EZCAD_Installing.Visibility = Visibility.Visible;
                                    break;
                                }
                            case (int)_softwareStatus.Updating:
                                {
                                    mask2_EZCAD_Installing.Visibility = Visibility.Visible;
                                    progressbar_EZCAD_Installing.Visibility = Visibility.Visible;
                                    break;
                                }
                        }
                        break;
                    }
                case (int)_softwareID.Implant:
                    {
                        SoftwareFilterImplant.IsEnabled = false;
                        mask_Implant.Visibility = Visibility.Hidden;
                        process_Implant.Visibility = Visibility.Hidden;
                        implant_update.Visibility = Visibility.Collapsed;
                        updateimage_Implant.Visibility = Visibility.Hidden;
                        mask2_Implant_Installing.Visibility = Visibility.Hidden;
                        progressbar_Implant_Installing.Visibility = Visibility.Hidden;
                        switch (currentProgress)
                        {
                            case (int)_softwareStatus.NotInstall:
                                {
                                    mask_Implant.Visibility = Visibility.Visible;
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
                                    process_Implant.Visibility = Visibility.Visible;
                                    process_Implant.EndAngle = 360 - 360 * downloadPercent;
                                    popupbox_Implant.IsEnabled = false;
                                    break;
                                }
                            case (int)_softwareStatus.Installed:
                                {
                                    popupbox_Implant.IsEnabled = true;
                                    implant_selectPath.Visibility = Visibility.Collapsed;
                                    implant_download.Visibility = Visibility.Collapsed;
                                    implant_create.Visibility = Visibility.Visible;
                                    implant_demo.Visibility = Visibility.Visible;
                                    implant_troubleShooting.Visibility = Visibility.Visible;
                                    implant_buyLic.Visibility = Visibility.Visible;
                                    implant_unInstall.Visibility = Visibility.Visible;
                                    SoftwareFilterImplant.IsEnabled = true;
                                    implant_update.Visibility = Visibility.Visible;
                                    implant_update.IsEnabled = false;
                                    break;
                                }
                            case (int)_softwareStatus.Installing:
                                {
                                    mask2_Implant_Installing.Visibility = Visibility.Visible;
                                    progressbar_Implant_Installing.Visibility = Visibility.Visible;
                                    break;
                                }
                            case (int)_softwareStatus.Uninstalling:
                                {
                                    mask2_Implant_Installing.Visibility = Visibility.Visible;
                                    progressbar_Implant_Installing.Visibility = Visibility.Visible;
                                    break;
                                }
                            case (int)_softwareStatus.Updating:
                                {
                                    mask2_Implant_Installing.Visibility = Visibility.Visible;
                                    progressbar_Implant_Installing.Visibility = Visibility.Visible;
                                    break;
                                }
                        }
                        break;
                    }
                case (int)_softwareID.Ortho:
                    {
                        SoftwareFilterOrtho.IsEnabled = false;
                        mask_Ortho.Visibility = Visibility.Hidden;
                        process_Ortho.Visibility = Visibility.Hidden;
                        ortho_update.Visibility = Visibility.Collapsed;
                        updateimage_Ortho.Visibility = Visibility.Hidden;
                        mask2_Ortho_Installing.Visibility = Visibility.Hidden;
                        progressbar_Ortho_Installing.Visibility = Visibility.Hidden;
                        switch (currentProgress)
                        {
                            case (int)_softwareStatus.NotInstall:
                                {
                                    mask_Ortho.Visibility = Visibility.Visible;
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
                                    process_Ortho.Visibility = Visibility.Visible;
                                    process_Ortho.EndAngle = 360 - 360 * downloadPercent;
                                    popupbox_Ortho.IsEnabled = false;
                                    break;
                                }
                            case (int)_softwareStatus.Installed:
                                {
                                    popupbox_Ortho.IsEnabled = true;
                                    ortho_selectPath.Visibility = Visibility.Collapsed;
                                    ortho_download.Visibility = Visibility.Collapsed;
                                    ortho_open.Visibility = Visibility.Visible;
                                    ortho_demo.Visibility = Visibility.Visible;
                                    ortho_troubleShooting.Visibility = Visibility.Visible;
                                    ortho_buyLic.Visibility = Visibility.Visible;
                                    ortho_unInstall.Visibility = Visibility.Visible;
                                    SoftwareFilterOrtho.IsEnabled = true;
                                    ortho_update.Visibility = Visibility.Visible;
                                    ortho_update.IsEnabled = false;
                                    break;
                                }
                            case (int)_softwareStatus.Installing:
                                {
                                    mask2_Ortho_Installing.Visibility = Visibility.Visible;
                                    progressbar_Ortho_Installing.Visibility = Visibility.Visible;
                                    break;
                                }
                            case (int)_softwareStatus.Uninstalling:
                                {
                                    mask2_Ortho_Installing.Visibility = Visibility.Visible;
                                    progressbar_Ortho_Installing.Visibility = Visibility.Visible;
                                    break;
                                }
                            case (int)_softwareStatus.Updating:
                                {
                                    mask2_Ortho_Installing.Visibility = Visibility.Visible;
                                    progressbar_Ortho_Installing.Visibility = Visibility.Visible;
                                    break;
                                }
                        }
                        break;
                    }
                case (int)_softwareID.Tray:
                    {
                        SoftwareFilterTray.IsEnabled = false;
                        mask_Tray.Visibility = Visibility.Hidden;
                        process_Tray.Visibility = Visibility.Hidden;
                        tray_update.Visibility = Visibility.Collapsed;
                        updateimage_Tray.Visibility = Visibility.Hidden;
                        mask2_Tray_Installing.Visibility = Visibility.Hidden;
                        progressbar_Tray_Installing.Visibility = Visibility.Hidden;
                        switch (currentProgress)
                        {
                            case (int)_softwareStatus.NotInstall:
                                {
                                    mask_Tray.Visibility = Visibility.Visible;
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
                                    process_Tray.Visibility = Visibility.Visible;
                                    process_Tray.EndAngle = 360 - 360 * downloadPercent;
                                    popupbox_Tray.IsEnabled = false;
                                    break;
                                }
                            case (int)_softwareStatus.Installed:
                                {
                                    popupbox_Tray.IsEnabled = true;
                                    tray_selectPath.Visibility = Visibility.Collapsed;
                                    tray_download.Visibility = Visibility.Collapsed;
                                    tray_open.Visibility = Visibility.Visible;
                                    tray_demo.Visibility = Visibility.Visible;
                                    tray_troubleShooting.Visibility = Visibility.Visible;
                                    tray_buyLic.Visibility = Visibility.Visible;
                                    tray_unInstall.Visibility = Visibility.Visible;
                                    SoftwareFilterTray.IsEnabled = true;
                                    tray_update.Visibility = Visibility.Visible;
                                    tray_update.IsEnabled = false;
                                    break;
                                }
                            case (int)_softwareStatus.Installing:
                                {
                                    mask2_Tray_Installing.Visibility = Visibility.Visible;
                                    progressbar_Tray_Installing.Visibility = Visibility.Visible;
                                    break;
                                }
                            case (int)_softwareStatus.Uninstalling:
                                {
                                    mask2_Tray_Installing.Visibility = Visibility.Visible;
                                    progressbar_Tray_Installing.Visibility = Visibility.Visible;
                                    break;
                                }
                            case (int)_softwareStatus.Updating:
                                {
                                    mask2_Tray_Installing.Visibility = Visibility.Visible;
                                    progressbar_Tray_Installing.Visibility = Visibility.Visible;
                                    break;
                                }
                        }
                        break;
                    }
                case (int)_softwareID.Splint:
                    {
                        SoftwareFilterSplint.IsEnabled = false;
                        mask_Splint.Visibility = Visibility.Hidden;
                        process_Splint.Visibility = Visibility.Hidden;
                        splint_update.Visibility = Visibility.Collapsed;
                        updateimage_Splint.Visibility = Visibility.Hidden;
                        mask2_Splint_Installing.Visibility = Visibility.Hidden;
                        progressbar_Splint_Installing.Visibility = Visibility.Hidden;
                        switch (currentProgress)
                        {
                            case (int)_softwareStatus.NotInstall:
                                {
                                    mask_Splint.Visibility = Visibility.Visible;
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
                                    process_Splint.Visibility = Visibility.Visible;
                                    process_Splint.EndAngle = 360.0 - 360.0 * downloadPercent;
                                    popupbox_Splint.IsEnabled = false;
                                    break;
                                }
                            case (int)_softwareStatus.Installed:
                                {
                                    popupbox_Splint.IsEnabled = true;
                                    splint_selectPath.Visibility = Visibility.Collapsed;
                                    splint_download.Visibility = Visibility.Collapsed;
                                    splint_open.Visibility = Visibility.Visible;
                                    splint_demo.Visibility = Visibility.Visible;
                                    splint_troubleShooting.Visibility = Visibility.Visible;
                                    splint_buyLic.Visibility = Visibility.Visible;
                                    splint_unInstall.Visibility = Visibility.Visible;
                                    SoftwareFilterSplint.IsEnabled = true;
                                    splint_update.Visibility = Visibility.Visible;
                                    splint_update.IsEnabled = false;
                                    break;
                                }
                            case (int)_softwareStatus.Installing:
                                {
                                    mask2_Splint_Installing.Visibility = Visibility.Visible;
                                    progressbar_Splint_Installing.Visibility = Visibility.Visible;
                                    break;
                                }
                            case (int)_softwareStatus.Uninstalling:
                                {
                                    mask2_Splint_Installing.Visibility = Visibility.Visible;
                                    progressbar_Splint_Installing.Visibility = Visibility.Visible;
                                    break;
                                }
                            case (int)_softwareStatus.Updating:
                                {
                                    mask2_Splint_Installing.Visibility = Visibility.Visible;
                                    progressbar_Splint_Installing.Visibility = Visibility.Visible;
                                    break;
                                }
                        }
                        break;
                    }
                case (int)_softwareID.Guide:
                    {
                        mask_Guide.Visibility = Visibility.Hidden;
                        process_Guide.Visibility = Visibility.Hidden;
                        guide_update.Visibility = Visibility.Collapsed;
                        updateimage_Guide.Visibility = Visibility.Hidden;
                        mask2_Guide_Installing.Visibility = Visibility.Hidden;
                        progressbar_Guide_Installing.Visibility = Visibility.Hidden;
                        switch (currentProgress)
                        {
                            case (int)_softwareStatus.NotInstall:
                                {
                                    mask_Guide.Visibility = Visibility.Visible;
                                    popupbox_Guide.IsEnabled = true;
                                    guide_selectPath.Visibility = Visibility.Visible;
                                    guide_download.Visibility = Visibility.Visible;
                                    guide_open.Visibility = Visibility.Collapsed;
                                    guide_demo.Visibility = Visibility.Collapsed;
                                    guide_troubleShooting.Visibility = Visibility.Collapsed;
                                    guide_buyLic.Visibility = Visibility.Collapsed;
                                    guide_unInstall.Visibility = Visibility.Collapsed;
                                    break;
                                }
                            case (int)_softwareStatus.Downloading:
                                {
                                    process_Guide.Visibility = Visibility.Visible;
                                    process_Guide.EndAngle = 360.0 - 360.0 * downloadPercent;
                                    popupbox_Guide.IsEnabled = false;
                                    break;
                                }
                            case (int)_softwareStatus.Installed:
                                {
                                    popupbox_Guide.IsEnabled = true;
                                    guide_selectPath.Visibility = Visibility.Collapsed;
                                    guide_download.Visibility = Visibility.Collapsed;
                                    guide_open.Visibility = Visibility.Visible;
                                    guide_demo.Visibility = Visibility.Visible;
                                    guide_troubleShooting.Visibility = Visibility.Visible;
                                    guide_buyLic.Visibility = Visibility.Visible;
                                    guide_unInstall.Visibility = Visibility.Visible;
                                    guide_update.Visibility = Visibility.Visible;
                                    guide_update.IsEnabled = false;
                                    break;
                                }
                            case (int)_softwareStatus.Installing:
                                {
                                    mask2_Guide_Installing.Visibility = Visibility.Visible;
                                    progressbar_Guide_Installing.Visibility = Visibility.Visible;
                                    break;
                                }
                            case (int)_softwareStatus.Uninstalling:
                                {
                                    mask2_Guide_Installing.Visibility = Visibility.Visible;
                                    progressbar_Guide_Installing.Visibility = Visibility.Visible;
                                    break;
                                }
                            case (int)_softwareStatus.Updating:
                                {
                                    mask2_Guide_Installing.Visibility = Visibility.Visible;
                                    progressbar_Guide_Installing.Visibility = Visibility.Visible;
                                    break;
                                }
                        }
                        break;
                    }
            }
            //事件
            switch(currentProgress)
            {
                case (int)_softwareStatus.Installed:
                    {
                        SetAllSoftwareTableDownloadisEnable(true);
                        OrderManagerFunc.AutoDetectSoftwareProjectPath(softwareID);
                        UpdateFunc.CheckSoftwareHaveNewVersion(softwareID);
                        break;
                    }
                case (int)_softwareStatus.Installing:
                    {
                        Watcher_SoftwareInstall((int)_watcherCommand.Install, -1);
                        SetAllSoftwareTableDownloadisEnable(false);
                        break;
                    }
                case (int)_softwareStatus.Uninstalling:
                    {
                        Watcher_SoftwareInstall((int)_watcherCommand.Delete, softwareID);
                        SetAllSoftwareTableDownloadisEnable(false);
                        break;
                    }
                case (int)_softwareStatus.Updating:
                    {
                        SetAllSoftwareTableDownloadisEnable(false);
                        break;
                    }
            }

            // CaseFilter RadioButton狀態處理
            if(SoftwareFilterCAD.IsEnabled == false && SoftwareFilterImplant.IsEnabled == false && SoftwareFilterOrtho.IsEnabled == false && SoftwareFilterTray.IsEnabled == false && SoftwareFilterSplint.IsEnabled == false)
            {
                SoftwareFilterCAD.IsChecked = false;
                SoftwareFilterImplant.IsChecked = false;
                SoftwareFilterOrtho.IsChecked = false;
                SoftwareFilterTray.IsChecked = false;
                SoftwareFilterSplint.IsChecked = false;
                StackPanel_Local.Children.Clear();
            }
            else
            {
                if (SoftwareFilterCAD.IsEnabled == true && SoftwareFilterCAD.IsChecked == true)
                    return;
                else if (SoftwareFilterImplant.IsEnabled == true && SoftwareFilterImplant.IsChecked == true)
                    return;
                else if (SoftwareFilterOrtho.IsEnabled == true && SoftwareFilterOrtho.IsChecked == true)
                    return;
                else if (SoftwareFilterTray.IsEnabled == true && SoftwareFilterTray.IsChecked == true)
                    return;
                else if (SoftwareFilterSplint.IsEnabled == true && SoftwareFilterSplint.IsChecked == true)
                    return;
                else if (SoftwareFilterCAD.IsEnabled == true && SoftwareFilterCAD.IsChecked == false)
                    SoftwareFilterCAD.IsChecked = true;
                else if (SoftwareFilterImplant.IsEnabled == true && SoftwareFilterImplant.IsChecked == false)
                    SoftwareFilterImplant.IsChecked = true;
                else if (SoftwareFilterOrtho.IsEnabled == true && SoftwareFilterOrtho.IsChecked == false)
                    SoftwareFilterOrtho.IsChecked = true;
                else if (SoftwareFilterTray.IsEnabled == true && SoftwareFilterTray.IsChecked == false)
                    SoftwareFilterTray.IsChecked = true;
                else if (SoftwareFilterSplint.IsEnabled == true && SoftwareFilterSplint.IsChecked == false)
                    SoftwareFilterSplint.IsChecked = true;
            }
        }

        /// <summary>
        /// 設定各單機軟體更新Button的isEnable狀態
        /// </summary>
        /// <param name="SoftwareID"></param>
        /// <param name="canUpdate"></param>
        private void Handler_SetSoftwareUpdateButtonStatus(int SoftwareID,bool canUpdate)
        {
            switch(SoftwareID)
            {
                //TODO:要再加上Logo右上角小提示
                case (int)_softwareID.EZCAD:
                    {
                        cad_update.IsEnabled = canUpdate;
                        if (cad_update.IsEnabled == true)
                        {
                            updateimage_EZCAD.Visibility = Visibility.Visible;
                            cad_update.FontWeight = FontWeights.Bold;
                            cad_update.Foreground = Brushes.Red;
                        }   
                        else
                        {
                            updateimage_EZCAD.Visibility = Visibility.Hidden;
                            cad_update.FontWeight = FontWeights.Normal;
                            cad_update.Foreground = this.FindResource("Common_DarkBrown") as SolidColorBrush;
                        }
                        break;
                    }
                case (int)_softwareID.Implant:
                    {
                        implant_update.IsEnabled = canUpdate;
                        if (implant_update.IsEnabled == true)
                        {
                            updateimage_Implant.Visibility = Visibility.Visible;
                            implant_update.FontWeight = FontWeights.Bold;
                            implant_update.Foreground = Brushes.Red;
                        }   
                        else
                        {
                            updateimage_Implant.Visibility = Visibility.Hidden;
                            implant_update.FontWeight = FontWeights.Normal;
                            implant_update.Foreground = this.FindResource("Common_DarkBrown") as SolidColorBrush;
                        }
                        break;
                    }
                case (int)_softwareID.Ortho:
                    {
                        ortho_update.IsEnabled = canUpdate;
                        if (ortho_update.IsEnabled == true)
                        {
                            updateimage_Ortho.Visibility = Visibility.Visible;
                            ortho_update.FontWeight = FontWeights.Bold;
                            ortho_update.Foreground = Brushes.Red;
                        }   
                        else
                        {
                            updateimage_Ortho.Visibility = Visibility.Hidden;
                            ortho_update.FontWeight = FontWeights.Normal;
                            ortho_update.Foreground = this.FindResource("Common_DarkBrown") as SolidColorBrush;
                        }
                        break;
                    }
                case (int)_softwareID.Tray:
                    {
                        tray_update.IsEnabled = canUpdate;
                        if (tray_update.IsEnabled == true)
                        {
                            updateimage_Tray.Visibility = Visibility.Visible;
                            tray_update.FontWeight = FontWeights.Bold;
                            tray_update.Foreground = Brushes.Red;
                        }
                        else
                        {
                            updateimage_Tray.Visibility = Visibility.Hidden;
                            tray_update.FontWeight = FontWeights.Normal;
                            tray_update.Foreground = this.FindResource("Common_DarkBrown") as SolidColorBrush;
                        }   
                        break;
                    }
                case (int)_softwareID.Splint:
                    {
                        splint_update.IsEnabled = canUpdate;
                        if (splint_update.IsEnabled == true)
                        {
                            updateimage_Splint.Visibility = Visibility.Visible;
                            splint_update.FontWeight = FontWeights.Bold;
                            splint_update.Foreground = Brushes.Red;
                        }   
                        else
                        {
                            updateimage_Splint.Visibility = Visibility.Hidden;
                            splint_update.FontWeight = FontWeights.Normal;
                            splint_update.Foreground = this.FindResource("Common_DarkBrown") as SolidColorBrush;
                        }   
                        break;
                    }
                case (int)_softwareID.Guide:
                    {
                        guide_update.IsEnabled = canUpdate;
                        if (guide_update.IsEnabled == true)
                        {
                            updateimage_Guide.Visibility = Visibility.Visible;
                            guide_update.FontWeight = FontWeights.Bold;
                            guide_update.Foreground = Brushes.Red;
                        }   
                        else
                        {
                            updateimage_Guide.Visibility = Visibility.Hidden;
                            guide_update.FontWeight = FontWeights.Normal;
                            guide_update.Foreground = this.FindResource("Common_DarkBrown") as SolidColorBrush;
                        }
                        break;
                    }
            }
            
        }
        
        /// <summary>
        /// 設定SofttwareTable的PopupBox事件
        /// </summary>
        private void Click_SoftwareTable(object sender, RoutedEventArgs e)
        {
            if(sender is Button)
            {
                switch (((Button)sender).Name)
                {
                    case "buyLic":
                        {
                            break;
                        }
                    #region EZCAD
                    case "cad_update":
                        {
                            break;
                        }
                    case "cad_selectPath":
                        {
                            GoToSetting((int)_softwareID.EZCAD);
                            break;
                        }
                    case "cad_download":
                        {
                            for (int i = 0; i < UpdateFunc.CloudSoftwareTotal.Count; i++)
                            {
                                if (UpdateFunc.CloudSoftwareTotal[i].softwareID == (int)_softwareID.EZCAD)
                                {
                                    UpdateFunc.readyInstallSoftwareInfo = UpdateFunc.CloudSoftwareTotal[i];
                                    break;
                                }
                            }
                            
                            DialogBeforeDownload = new BeforeDownload();
                            DialogBeforeDownload.SetHttpResponseOK += new BeforeDownload.beforedownloadEventHandler(Handler_ShowBeforeDownload);
                            DialogBeforeDownload.Handler_snackbarShow += new BeforeDownload.beforedownloadEventHandler_snackbar(SnackBarShow);
                            SetAllSoftwareTableDownloadisEnable(false);
                            DialogBeforeDownload.GethttpResoponse(UpdateFunc.readyInstallSoftwareInfo.softwareDownloadLink, UpdateFunc.readyInstallSoftwareInfo.softwareID);
                            break;
                        }
                    case "cad_open":
                        {
                            QueryProductState qPS = new QueryProductState();
                            if (qPS.CheckSoftwareVC((int)_softwareID.EZCAD) == true)
                                OrderManagerFunc.RunCommandLine(Properties.Settings.Default.cad_exePath, "");
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
                            //TODO要再詢問一次是否真的要解除安裝
                            if (MessageBox.Show(OrderManagerNew.TranslationSource.Instance["AreyousureUninstall"] + "EZCAD?", OrderManagerNew.TranslationSource.Instance["Uninstall"], MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes) == MessageBoxResult.Yes)
                            {
                                if (Path.GetExtension(Properties.Settings.Default.cad_exePath) == ".exe")
                                {
                                    string uninstallPath = Path.GetDirectoryName(Properties.Settings.Default.cad_exePath) + @"\Uninstall.lnk";
                                    if (File.Exists(uninstallPath) == true)
                                    {
                                        Handler_setSoftwareShow((int)_softwareID.EZCAD, (int)_softwareStatus.Uninstalling, 0);
                                        OrderManagerFunc.RunCommandLine(uninstallPath, "/quiet");
                                    }
                                    else
                                    {
                                        MessageBox.Show("can't find Uninstall.lnk");    //TODO多國語系
                                    }
                                }
                            }
                            break;
                        }
                    #endregion
                    #region Implant
                    case "implant_update":
                        {
                            break;
                        }
                    case "implant_selectPath":
                        {
                            GoToSetting((int)_softwareID.Implant);
                            break;
                        }
                    case "implant_download":
                        {
                            for (int i = 0; i < UpdateFunc.CloudSoftwareTotal.Count; i++)
                            {
                                if (UpdateFunc.CloudSoftwareTotal[i].softwareID == (int)_softwareID.Implant)
                                {
                                    UpdateFunc.readyInstallSoftwareInfo = UpdateFunc.CloudSoftwareTotal[i];
                                    break;
                                }
                            }
                            
                            DialogBeforeDownload = new BeforeDownload();
                            DialogBeforeDownload.SetHttpResponseOK += new BeforeDownload.beforedownloadEventHandler(Handler_ShowBeforeDownload);
                            DialogBeforeDownload.Handler_snackbarShow += new BeforeDownload.beforedownloadEventHandler_snackbar(SnackBarShow);
                            SetAllSoftwareTableDownloadisEnable(false);
                            DialogBeforeDownload.GethttpResoponse(UpdateFunc.readyInstallSoftwareInfo.softwareDownloadLink, UpdateFunc.readyInstallSoftwareInfo.softwareID);
                            break;
                        }
                    case "implant_create":
                        {
                            QueryProductState qPS = new QueryProductState();
                            if (qPS.CheckSoftwareVC((int)_softwareID.Implant) == true)
                                OrderManagerFunc.RunCommandLine(Properties.Settings.Default.implant_exePath, "");
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
                            if (MessageBox.Show(OrderManagerNew.TranslationSource.Instance["AreyousureUninstall"] + "ImplantPlanning?", OrderManagerNew.TranslationSource.Instance["Uninstall"], MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes) == MessageBoxResult.Yes)
                            {
                                if (Path.GetExtension(Properties.Settings.Default.implant_exePath) == ".exe")
                                {
                                    string uninstallPath = Path.GetDirectoryName(Properties.Settings.Default.implant_exePath) + @"\Uninstall.lnk";
                                    if (File.Exists(uninstallPath) == true)
                                    {
                                        Handler_setSoftwareShow((int)_softwareID.Implant, (int)_softwareStatus.Uninstalling, 0);
                                        OrderManagerFunc.RunCommandLine(uninstallPath, "/quiet");
                                    }
                                    else
                                    {
                                        MessageBox.Show("can't find Uninstall.lnk");    //TODO多國語系
                                    }
                                }
                            }
                            break;
                        }
                    #endregion
                    #region Ortho
                    case "ortho_update":
                        {
                            break;
                        }
                    case "ortho_selectPath":
                        {
                            GoToSetting((int)_softwareID.Ortho);
                            break;
                        }
                    case "ortho_download":
                        {
                            for (int i = 0; i < UpdateFunc.CloudSoftwareTotal.Count; i++)
                            {
                                if (UpdateFunc.CloudSoftwareTotal[i].softwareID == (int)_softwareID.Ortho)
                                {
                                    UpdateFunc.readyInstallSoftwareInfo = UpdateFunc.CloudSoftwareTotal[i];
                                    break;
                                }
                            }
                            
                            DialogBeforeDownload = new BeforeDownload();
                            DialogBeforeDownload.SetHttpResponseOK += new BeforeDownload.beforedownloadEventHandler(Handler_ShowBeforeDownload);
                            DialogBeforeDownload.Handler_snackbarShow += new BeforeDownload.beforedownloadEventHandler_snackbar(SnackBarShow);
                            SetAllSoftwareTableDownloadisEnable(false);
                            DialogBeforeDownload.GethttpResoponse(UpdateFunc.readyInstallSoftwareInfo.softwareDownloadLink, UpdateFunc.readyInstallSoftwareInfo.softwareID);
                            break;
                        }
                    case "ortho_open":
                        {
                            QueryProductState qPS = new QueryProductState();
                            if (qPS.CheckSoftwareVC((int)_softwareID.Ortho) == true)
                                OrderManagerFunc.RunCommandLine(Properties.Settings.Default.ortho_exePath, "");
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
                            if (MessageBox.Show(OrderManagerNew.TranslationSource.Instance["AreyousureUninstall"] + "OrthoAnalysis?", OrderManagerNew.TranslationSource.Instance["Uninstall"], MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes) == MessageBoxResult.Yes)
                            {
                                if (Path.GetExtension(Properties.Settings.Default.ortho_exePath) == ".exe")
                                {
                                    string uninstallPath = Path.GetDirectoryName(Properties.Settings.Default.ortho_exePath) + @"\Uninstall.lnk";
                                    if (File.Exists(uninstallPath) == true)
                                    {
                                        Handler_setSoftwareShow((int)_softwareID.Ortho, (int)_softwareStatus.Uninstalling, 0);
                                        OrderManagerFunc.RunCommandLine(uninstallPath, "/quiet");
                                    }
                                    else
                                    {
                                        MessageBox.Show("can't find Uninstall.lnk");    //TODO多國語系
                                    }
                                }
                            }
                            break;
                        }
                    #endregion
                    #region Tray
                    case "tray_update":
                        {
                            Handler_setSoftwareShow((int)_softwareID.Tray, (int)_softwareStatus.Updating, 0);
                            for (int i = 0; i < UpdateFunc.CloudSoftwareTotal.Count; i++)
                            {
                                if (UpdateFunc.CloudSoftwareTotal[i].softwareID == (int)_softwareID.Tray)
                                {
                                    UpdateFunc.readyInstallSoftwareInfo = UpdateFunc.CloudSoftwareTotal[i];
                                    break;
                                }
                            }

                            DialogBeforeDownload = new BeforeDownload();
                            DialogBeforeDownload.SetHttpResponseOK += new BeforeDownload.beforedownloadEventHandler(Handler_SoftwareUpdate);
                            DialogBeforeDownload.Handler_snackbarShow += new BeforeDownload.beforedownloadEventHandler_snackbar(SnackBarShow);
                            SetAllSoftwareTableDownloadisEnable(false);
                            DialogBeforeDownload.GethttpResoponse(UpdateFunc.readyInstallSoftwareInfo.softwareDownloadLink, UpdateFunc.readyInstallSoftwareInfo.softwareID);
                            break;
                        }
                    case "tray_selectPath":
                        {
                            GoToSetting((int)_softwareID.Tray);
                            break;
                        }
                    case "tray_download":
                        {
                            for (int i = 0; i < UpdateFunc.CloudSoftwareTotal.Count; i++)
                            {
                                if (UpdateFunc.CloudSoftwareTotal[i].softwareID == (int)_softwareID.Tray)
                                {
                                    UpdateFunc.readyInstallSoftwareInfo = UpdateFunc.CloudSoftwareTotal[i];
                                    break;
                                }
                            }
                            
                            DialogBeforeDownload = new BeforeDownload();
                            DialogBeforeDownload.SetHttpResponseOK += new BeforeDownload.beforedownloadEventHandler(Handler_ShowBeforeDownload);
                            DialogBeforeDownload.Handler_snackbarShow += new BeforeDownload.beforedownloadEventHandler_snackbar(SnackBarShow);
                            SetAllSoftwareTableDownloadisEnable(false);
                            DialogBeforeDownload.GethttpResoponse(UpdateFunc.readyInstallSoftwareInfo.softwareDownloadLink, UpdateFunc.readyInstallSoftwareInfo.softwareID);
                            break;
                        }
                    case "tray_open":
                        {
                            QueryProductState qPS = new QueryProductState();
                            if (qPS.CheckSoftwareVC((int)_softwareID.Tray) == true)
                                OrderManagerFunc.RunCommandLine(Properties.Settings.Default.tray_exePath, "");
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
                            if (MessageBox.Show(OrderManagerNew.TranslationSource.Instance["AreyousureUninstall"] + "EZCAD.tray?", OrderManagerNew.TranslationSource.Instance["Uninstall"], MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes) == MessageBoxResult.Yes)
                            {
                                if (Path.GetExtension(Properties.Settings.Default.tray_exePath) == ".exe")
                                {
                                    string uninstallPath = Path.GetDirectoryName(Properties.Settings.Default.tray_exePath) + @"\Uninstall.lnk";
                                    if (File.Exists(uninstallPath) == true)
                                    {
                                        Handler_setSoftwareShow((int)_softwareID.Tray, (int)_softwareStatus.Uninstalling, 0);
                                        OrderManagerFunc.RunCommandLine(uninstallPath, "/quiet");
                                    }
                                    else
                                    {
                                        MessageBox.Show("can't find Uninstall.lnk");    //TODO多國語系
                                    }
                                }
                            }
                            break;
                        }
                    #endregion
                    #region Splint
                    case "splint_update":
                        {
                            break;
                        }
                    case "splint_selectPath":
                        {
                            GoToSetting((int)_softwareID.Splint);
                            break;
                        }
                    case "splint_download":
                        {
                            for (int i = 0; i < UpdateFunc.CloudSoftwareTotal.Count; i++)
                            {
                                if (UpdateFunc.CloudSoftwareTotal[i].softwareID == (int)_softwareID.Splint)
                                {
                                    UpdateFunc.readyInstallSoftwareInfo = UpdateFunc.CloudSoftwareTotal[i];
                                    break;
                                }
                            }
                            
                            DialogBeforeDownload = new BeforeDownload();
                            DialogBeforeDownload.SetHttpResponseOK += new BeforeDownload.beforedownloadEventHandler(Handler_ShowBeforeDownload);
                            DialogBeforeDownload.Handler_snackbarShow += new BeforeDownload.beforedownloadEventHandler_snackbar(SnackBarShow);
                            SetAllSoftwareTableDownloadisEnable(false);
                            DialogBeforeDownload.GethttpResoponse(UpdateFunc.readyInstallSoftwareInfo.softwareDownloadLink, UpdateFunc.readyInstallSoftwareInfo.softwareID);
                            break;
                        }
                    case "splint_open":
                        {
                            QueryProductState qPS = new QueryProductState();
                            if (qPS.CheckSoftwareVC((int)_softwareID.Splint) == true)
                                OrderManagerFunc.RunCommandLine(Properties.Settings.Default.splint_exePath, "");
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
                            if (MessageBox.Show(OrderManagerNew.TranslationSource.Instance["AreyousureUninstall"] + "EZCAD.splint?", OrderManagerNew.TranslationSource.Instance["Uninstall"], MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes) == MessageBoxResult.Yes)
                            {
                                if (Path.GetExtension(Properties.Settings.Default.splint_exePath) == ".exe")
                                {
                                    string uninstallPath = Path.GetDirectoryName(Properties.Settings.Default.splint_exePath) + @"\Uninstall.lnk";
                                    if (File.Exists(uninstallPath) == true)
                                    {
                                        Handler_setSoftwareShow((int)_softwareID.Splint, (int)_softwareStatus.Uninstalling, 0);
                                        OrderManagerFunc.RunCommandLine(uninstallPath, "/quiet");
                                    }
                                    else
                                    {
                                        MessageBox.Show("can't find Uninstall.lnk");    //TODO多國語系
                                    }
                                }
                            }
                            break;
                        }
                    #endregion
                    #region Guide
                    case "guide_update":
                        {
                            break;
                        }
                    case "guide_selectPath":
                        {
                            GoToSetting((int)_softwareID.Guide);
                            break;
                        }
                    case "guide_download":
                        {
                            for (int i = 0; i < UpdateFunc.CloudSoftwareTotal.Count; i++)
                            {
                                //TODO 之後不會分Dongle和License
                                if (UpdateFunc.CloudSoftwareTotal[i].softwareID == (int)_softwareID.Guide)
                                {
                                    UpdateFunc.readyInstallSoftwareInfo = UpdateFunc.CloudSoftwareTotal[i];
                                    break;
                                }
                            }
                            
                            DialogBeforeDownload = new BeforeDownload();
                            DialogBeforeDownload.SetHttpResponseOK += new BeforeDownload.beforedownloadEventHandler(Handler_ShowBeforeDownload);
                            DialogBeforeDownload.Handler_snackbarShow += new BeforeDownload.beforedownloadEventHandler_snackbar(SnackBarShow);
                            SetAllSoftwareTableDownloadisEnable(false);
                            DialogBeforeDownload.GethttpResoponse(UpdateFunc.readyInstallSoftwareInfo.softwareDownloadLink, UpdateFunc.readyInstallSoftwareInfo.softwareID);
                            break;
                        }
                    case "guide_open":
                        {
                            QueryProductState qPS = new QueryProductState();
                            if (qPS.CheckSoftwareVC((int)_softwareID.Guide) == true)
                                OrderManagerFunc.RunCommandLine(Properties.Settings.Default.guide_exePath, "");
                            break;
                        }
                    case "guide_webIntro":
                        {
                            break;
                        }
                    case "guide_demo":
                        {
                            break;
                        }
                    case "guide_troubleShooting":
                        {
                            break;
                        }
                    case "guide_unInstall":
                        {
                            if (MessageBox.Show(OrderManagerNew.TranslationSource.Instance["AreyousureUninstall"] + "EZCAD.guide?", OrderManagerNew.TranslationSource.Instance["Uninstall"], MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes) == MessageBoxResult.Yes)
                            {
                                if (Path.GetExtension(Properties.Settings.Default.guide_exePath) == ".exe")
                                {
                                    string uninstallPath = Path.GetDirectoryName(Properties.Settings.Default.guide_exePath) + @"\Uninstall.lnk";
                                    if (File.Exists(uninstallPath) == true)
                                    {
                                        Handler_setSoftwareShow((int)_softwareID.Guide, (int)_softwareStatus.Uninstalling, 0);
                                        OrderManagerFunc.RunCommandLine(uninstallPath, "/quiet");
                                    }
                                    else
                                    {
                                        MessageBox.Show("can't find Uninstall.lnk");    //TODO多國語系
                                    }
                                }
                            }
                            break;
                        }
                        #endregion
                }
            }
        }

        /// <summary>
        /// 設定各軟體Popupbox內"下載軟體"的isEnable屬性
        /// </summary>
        /// <param name="enable">isEnable屬性</param>
        private void SetAllSoftwareTableDownloadisEnable(bool enable)
        {
            cad_download.IsEnabled = enable;
            implant_download.IsEnabled = enable;
            ortho_download.IsEnabled = enable;
            tray_download.IsEnabled = enable;
            splint_download.IsEnabled = enable;
            guide_download.IsEnabled = enable;

            cad_unInstall.IsEnabled = enable;
            implant_unInstall.IsEnabled = enable;
            ortho_unInstall.IsEnabled = enable;
            tray_unInstall.IsEnabled = enable;
            splint_unInstall.IsEnabled = enable;
            guide_unInstall.IsEnabled = enable;
        }

        /// <summary>
        /// 開始軟體更新流程
        /// </summary>
        private void Handler_SoftwareUpdate()
        {
            //先把原本的軟體exePath改成安裝路徑
            bool canUpdate = false;
            switch(CheckedSoftwareID)
            {
                case (int)_softwareID.EZCAD:
                    {
                        if (Properties.Settings.Default.cad_exePath != "" && Properties.Settings.Default.cad_exePath.IndexOf(".exe") != -1)
                        {
                            Properties.Settings.Default.cad_exePath = OrderManagerFunc.GetUpLevelDirectory(Properties.Settings.Default.cad_exePath, 1) + @"\";
                            canUpdate = true;
                        }
                        break;
                    }
                case (int)_softwareID.Implant:
                    {
                        if(Properties.Settings.Default.implant_exePath != "" && Properties.Settings.Default.implant_exePath.IndexOf(".exe") != -1)
                        {
                            Properties.Settings.Default.implant_exePath = OrderManagerFunc.GetUpLevelDirectory(Properties.Settings.Default.implant_exePath, 0) + @"\";
                            canUpdate = true;
                        }
                        break;
                    }
                case (int)_softwareID.Ortho:
                    {
                        if (Properties.Settings.Default.ortho_exePath != "" && Properties.Settings.Default.ortho_exePath.IndexOf(".exe") != -1)
                        {
                            Properties.Settings.Default.ortho_exePath = OrderManagerFunc.GetUpLevelDirectory(Properties.Settings.Default.ortho_exePath, 0) + @"\";
                            canUpdate = true;
                        }
                        break;
                    }
                case (int)_softwareID.Tray:
                    {
                        if (Properties.Settings.Default.tray_exePath != "" && Properties.Settings.Default.tray_exePath.IndexOf(".exe") != -1)
                        {
                            Properties.Settings.Default.tray_exePath = OrderManagerFunc.GetUpLevelDirectory(Properties.Settings.Default.tray_exePath, 1) + @"\";
                            canUpdate = true;
                        }
                        break;
                    }
                case (int)_softwareID.Splint:
                    {
                        if (Properties.Settings.Default.splint_exePath != "" && Properties.Settings.Default.splint_exePath.IndexOf(".exe") != -1)
                        {
                            Properties.Settings.Default.splint_exePath = OrderManagerFunc.GetUpLevelDirectory(Properties.Settings.Default.splint_exePath, 1) + @"\";
                            canUpdate = true;
                        }
                        break;
                    }
                case (int)_softwareID.Guide:
                    {
                        if (Properties.Settings.Default.guide_exePath != "" && Properties.Settings.Default.guide_exePath.IndexOf(".exe") != -1)
                        {
                            Properties.Settings.Default.guide_exePath = OrderManagerFunc.GetUpLevelDirectory(Properties.Settings.Default.guide_exePath, 1) + @"\";
                            canUpdate = true;
                        }
                        break;
                    }
            }

            if(canUpdate == true)
            {
                haveEXE = false;
                SnackBarShow("Start Download"); //開始下載 //TODO 多國語系
                UpdateFunc.StartDownloadSoftware();
            }
            else
            {
                SnackBarShow("exePath is blank");//TODO 多國語系
                Handler_setSoftwareShow(CheckedSoftwareID, (int)_softwareStatus.Installed, 0);
            }
        }

        /// <summary>
        /// 從網上獲取下載資料成功就顯示BeforeDownload頁面
        /// </summary>
        private void Handler_ShowBeforeDownload()
        {
            haveEXE = false;
            bool DownloadStart = false;
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
                    SnackBarShow("Start Download"); //開始下載 //TODO 多國語系
                    DownloadStart = true;
                }
                else
                    SetAllSoftwareTableDownloadisEnable(true);

                //主視窗還原
                this.Effect = null;
                this.OpacityMask = null;
            }

            if (DownloadStart == true)
                UpdateFunc.StartDownloadSoftware();
        }
        
        private void Click_FunctionTable_User(object sender, RoutedEventArgs e)
        {
            if (loginStatus == false)
            {
                AirdentalLogin DialogLogin = new AirdentalLogin
                {
                    Owner = this
                };
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
        
        private void Click_UserDetail_Logout(object sender, RoutedEventArgs e)
        {
            UserDetailshow(false);
            loginStatus = false;
            SnackBarShow("Logout");
        }

        private void GoToSetting(int softwareID)
        {
            //主視窗羽化
            var blur = new BlurEffect();
            this.Effect = blur;

            Setting DialogSetting = new Setting
            {
                Owner = this,
                ShowActivated = true
            };

            if (softwareID != -1)
                DialogSetting.SearchEXE("", softwareID);

            DialogSetting.ShowDialog();
            if (DialogSetting.DialogResult == true)
            {
                OrderManagerFunc.DoubleCheckEXEexist();
                log.RecordConfigLog("Click_FunctionTable_Setting()", "Config changed");
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
        /// <summary>
        /// 跳出Snackbar訊息
        /// </summary>
        /// <param name="Message"> 要顯示的訊息</param>
        /// <returns></returns>
        private void SnackBarShow(string Message)
        {
            Task.Factory.StartNew(() => MainsnackbarMessageQueue.Enqueue(Message));
        }

        private void ChooseToLoadProj() //日期過濾要加
        {
            if (SoftwareFilterCAD.IsChecked == true)
                ProjHandle.LoadEZCADProj();
            else if (SoftwareFilterImplant.IsChecked == true)
                ProjHandle.LoadImplantProj();
            else if (SoftwareFilterOrtho.IsChecked == true)
                ProjHandle.LoadOrthoProj();
            else if (SoftwareFilterTray.IsChecked == true)
                ProjHandle.LoadTrayProj();
            else if (SoftwareFilterSplint.IsChecked == true)
                ProjHandle.LoadSplintProj();
            else
                StackPanel_Local.Children.Clear();
        }

        private void TextChanged_SortTable(object sender, TextChangedEventArgs e)
        {
            if(sender is TextBox)
            {
                TextBox txtbox = sender as TextBox;
                switch (txtbox.Name)
                {
                    case "textboxPatient":
                        {
                            Properties.OrderManagerProps.Default.PatientNameFilter = textboxPatient.Text;

                            if (textboxPatient.Text == "")
                                checkboxPatient.IsChecked = false;
                            else
                            {
                                checkboxPatient.IsChecked = true;

                                if (txtbox.Text == "-engineer")
                                {
                                    string message = "";
                                    if (developerMode == false)
                                    {
                                        //開發者模式
                                        developerMode = true;
                                        message = "Developer Mode";
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
                            }
                            break;
                        }
                    case "textboxCase":
                        {
                            Properties.OrderManagerProps.Default.CaseNameFilter = textboxCase.Text;

                            if(textboxCase.Text == "")
                                checkboxCase.IsChecked = false;
                            else
                                checkboxCase.IsChecked = true;
                            break;
                        }
                }
                ChooseToLoadProj();
            }
        }

        private void Checked_SortTable_Filter(object sender, RoutedEventArgs e)
        {
            if(sender is RadioButton)
            {
                StackPanel_Local.Children.Clear();
                RadioButton radioBtn = sender as RadioButton;
                switch (radioBtn.Name)
                {
                    case "DateFilterAll":
                        {
                            Properties.OrderManagerProps.Default.DateFilter = (int)_DateFilter.All;
                            ChooseToLoadProj();
                            break;
                        }
                    case "DateFilterToday":
                        {
                            Properties.OrderManagerProps.Default.DateFilter = (int)_DateFilter.Today;
                            ChooseToLoadProj();
                            break;
                        }
                    case "DateFilterLW":
                        {
                            Properties.OrderManagerProps.Default.DateFilter = (int)_DateFilter.ThisWeek;
                            ChooseToLoadProj();
                            break;
                        }
                    case "DateFilterL2W":
                        {
                            Properties.OrderManagerProps.Default.DateFilter = (int)_DateFilter.LastTwoWeek;
                            ChooseToLoadProj();
                            break;
                        }
                    case "SoftwareFilterCAD":
                        {
                            ProjHandle.LoadEZCADProj();
                            break;
                        }
                    case "SoftwareFilterImplant":
                        {
                            ProjHandle.LoadImplantProj();
                            break;
                        }
                    case "SoftwareFilterOrtho":
                        {
                            ProjHandle.LoadOrthoProj();
                            break;
                        }
                    case "SoftwareFilterTray":
                        {
                            ProjHandle.LoadTrayProj();
                            break;
                        }
                    case "SoftwareFilterSplint":
                        {
                            ProjHandle.LoadSplintProj();
                            break;
                        }
                }
            }
            else if(sender is CheckBox)
            {
                switch (((CheckBox)sender).Name)
                {
                    case "checkboxPatient":
                        {
                            if (checkboxPatient.IsChecked == false)
                                Properties.OrderManagerProps.Default.PatientNameFilter = "";
                            else
                                Properties.OrderManagerProps.Default.PatientNameFilter = textboxPatient.Text;
                            break;
                        }
                    case "checkboxCase":
                        {
                            if (checkboxCase.IsChecked == false)
                                Properties.OrderManagerProps.Default.CaseNameFilter = "";
                            else
                                Properties.OrderManagerProps.Default.CaseNameFilter = textboxCase.Text;
                            break;
                        }
                }
                ChooseToLoadProj();
            }
        }
#endregion

#region CaseTable事件
        /// <summary>
        /// 顯示Case
        /// </summary>
        /// <param name="SoftwareID"></param>
        public void Handler_SetCaseShow(int SoftwareID)
        {
            switch(SoftwareID)
            {
                case (int)_softwareID.EZCAD:
                    {
                        StackPanel_Local.Children.Clear();
                        _watcherEZCAD = new FileSystemWatcher();
                        if (ProjHandle.Caselist_EZCAD != null && ProjHandle.Caselist_EZCAD.Count > 0)
                        {
                            int countIndex = 0;
                            foreach (CadInformation cadInfo in ProjHandle.Caselist_EZCAD)
                            {
                                UserControls.Order_cadBase Order_CAD = new UserControls.Order_cadBase();
                                Order_CAD.SetCaseInfo(cadInfo, countIndex);
                                StackPanel_Local.Children.Add(Order_CAD);
                                countIndex++;
                            }
                            Watcher_CaseProject(_watcherEZCAD, Properties.OrderManagerProps.Default.cad_projectDirectory);
                        }
                        break;
                    }
                case (int)_softwareID.Implant:
                    {
                        StackPanel_Local.Children.Clear();
                        _watcherImplant = new FileSystemWatcher();
                        int countIndex = 0;
                        if (ProjHandle.Caselist_ImplantOuterCase != null && ProjHandle.Caselist_ImplantOuterCase.Count > 0)
                        {
                            foreach (ImplantOuterInformation implantInfo in ProjHandle.Caselist_ImplantOuterCase)
                            {
                                UserControls.Order_implantBase Order_Implant = new UserControls.Order_implantBase();
                                Order_Implant.SetCaseInfo(implantInfo, countIndex);
                                StackPanel_Local.Children.Add(Order_Implant);
                                countIndex++;
                            }
                            Watcher_CaseProject(_watcherImplant, Properties.OrderManagerProps.Default.implant_projectDirectory);
                        }
                        break;
                    }
                case (int)_softwareID.Ortho:
                    {
                        StackPanel_Local.Children.Clear();
                        _watcherOrtho = new FileSystemWatcher();
                        int countIndex = 0;
                        if (ProjHandle.Caselist_OrthoOuterCase != null && ProjHandle.Caselist_OrthoOuterCase.Count > 0)
                        {
                            foreach (OrthoOuterInformation orthoInfo in ProjHandle.Caselist_OrthoOuterCase)
                            {
                                UserControls.Order_orthoBase Order_Ortho = new UserControls.Order_orthoBase();
                                Order_Ortho.SetCaseInfo(orthoInfo, countIndex);
                                StackPanel_Local.Children.Add(Order_Ortho);
                                countIndex++;
                            }
                            Watcher_CaseProject(_watcherOrtho, Properties.OrderManagerProps.Default.ortho_projectDirectory);
                        }
                        break;
                    }
                case (int)_softwareID.Tray:
                    {
                        StackPanel_Local.Children.Clear();
                        _watcherTray = new FileSystemWatcher();
                        int countIndex = 0;
                        if (ProjHandle.Caselist_Tray != null && ProjHandle.Caselist_Tray.Count > 0)
                        {
                            foreach (TrayInformation trayInfo in ProjHandle.Caselist_Tray)
                            {
                                UserControls.Order_tsBase Order_Tray = new UserControls.Order_tsBase();
                                Order_Tray.SetTrayCaseInfo(trayInfo, countIndex);
                                StackPanel_Local.Children.Add(Order_Tray);
                                countIndex++;
                            }
                            Watcher_CaseProject(_watcherTray, Properties.OrderManagerProps.Default.tray_projectDirectory);
                        }
                        break;
                    }
                case (int)_softwareID.Splint:
                    {
                        StackPanel_Local.Children.Clear();
                        _watcherSplint = new FileSystemWatcher();
                        int countIndex = 0;
                        if (ProjHandle.Caselist_Splint != null && ProjHandle.Caselist_Splint.Count > 0)
                        {
                            foreach (SplintInformation splintInfo in ProjHandle.Caselist_Splint)
                            {
                                UserControls.Order_tsBase Order_Splint = new UserControls.Order_tsBase();
                                Order_Splint.SetSplintCaseInfo(splintInfo, countIndex);
                                StackPanel_Local.Children.Add(Order_Splint);
                                countIndex++;
                            }
                            Watcher_CaseProject(_watcherSplint, Properties.OrderManagerProps.Default.splint_projectDirectory);
                        }
                        break;
                    }
            }
        }
#endregion
    }
}
