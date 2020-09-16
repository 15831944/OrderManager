using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Reflection;    //取得OrderManager自身軟體版本
using System.Windows.Media.Effects;
using System.Diagnostics;
using System.IO;
using System.Windows.Media.Animation;
using Path = System.IO.Path;
using CadInformation = OrderManagerNew.Local_UserControls.Order_cadBase.CadInformation;
using TrayInformation = OrderManagerNew.Local_UserControls.Order_tsBase.TrayInformation;
using SplintInformation = OrderManagerNew.Local_UserControls.Order_tsBase.SplintInformation;
using ImplantOuterInformation = OrderManagerNew.Local_UserControls.Order_implantBase.ImplantOuterInformation;
using OrthoOuterInformation = OrderManagerNew.Local_UserControls.Order_orthoBase.OrthoOuterInformation;
using System.Threading;
using UIDialogs;
using System.Windows.Shapes;
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
        /// Airdental通道
        /// </summary>
        AirDentalProjectHandle AirDentalProjHandle;
        /// <summary>
        /// 開發者模式
        /// </summary>
        bool developerMode = false;
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
        bool isInstalling;
        MaterialDesignThemes.Wpf.SnackbarMessageQueue MainsnackbarMessageQueue; //Snackbar
#endregion
        
        public MainWindow()
        {
            InitializeComponent();
            CheckedSoftwareID = -1;
            isInstalling = false;
            updateimage_Setting.Visibility = Visibility.Hidden;
            try
            {
                if(Properties.Settings.Default.Log_filePath == "" || Directory.Exists(Properties.Settings.Default.Log_filePath) == false)
                {
                    //我的文件資料夾路徑
                    string MyDocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    Properties.Settings.Default.Log_filePath = MyDocumentsPath + @"\Inteware OrderManager\";
                    if (Directory.Exists(Properties.Settings.Default.Log_filePath) == false)
                    {
                        Directory.CreateDirectory(Properties.Settings.Default.Log_filePath);
                    }
                    Properties.Settings.Default.Save();
                }

                //初始化LogRecorder
                log = new LogRecorder();
                titlebar_OrderManagerVersion.Content = "v" + Assembly.GetExecutingAssembly().GetName().Version.ToString();  //TitleBar顯示OrderManager版本
                log.RecordConfigLog("OM Startup", Assembly.GetExecutingAssembly().GetName().Version.ToString());
            }
            catch(Exception ex)
            {
                Inteware_Messagebox Msg = new Inteware_Messagebox();
                Msg.ShowMessage(ex.Message, "Init LogRecorder error", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(0);
            }

            if(Properties.Settings.Default.DownloadFolder == "" || Directory.Exists(Properties.Settings.Default.DownloadFolder) == false)
            {
                Properties.Settings.Default.DownloadFolder = Properties.Settings.Default.Log_filePath;
                Properties.Settings.Default.Save();
            }
            
                //設定Snackbar顯示時間
                var myMessageQueue = new MaterialDesignThemes.Wpf.SnackbarMessageQueue(TimeSpan.FromMilliseconds(2000));
                SnackbarMain.MessageQueue = myMessageQueue;
                MainsnackbarMessageQueue = SnackbarMain.MessageQueue;
                Timeline.DesiredFrameRateProperty.OverrideMetadata(typeof(System.Windows.Media.Animation.Timeline), new FrameworkPropertyMetadata(500));    //設定動畫流暢度
            
            try
            {
                //專案Function
                ProjHandle = new ProjectHandle();
                ProjHandle.CaseShowEvent += new ProjectHandle.caseShowEventHandler(Handler_SetCaseShow);
            }
            catch(Exception ex)
            {
                Inteware_Messagebox Msg = new Inteware_Messagebox();
                Msg.ShowMessage(ex.Message, "Init ProjectHandle error", MessageBoxButton.OK, MessageBoxImage.Error);
                log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "Init ProjectHandle error", ex.Message);
                Environment.Exit(0);
            }

            try
            {
                if (Properties.Settings.Default.sysLanguage == "")
                    Properties.Settings.Default.sysLanguage = "en-US";

                LocalizationService.SetLanguage(Properties.Settings.Default.sysLanguage);   //設定語系
            }
            catch (Exception ex)
            {
                Inteware_Messagebox Msg = new Inteware_Messagebox();
                Msg.ShowMessage(ex.Message, "Init SetLanguage error", MessageBoxButton.OK, MessageBoxImage.Error);
                log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "Init SetLanguage error", ex.Message);
            }

            try
            {
                //OrderManager常用Function
                OrderManagerFunc = new OrderManagerFunctions();
                OrderManagerFunc.SoftwareLogoShowEvent += new OrderManagerFunctions.softwareLogoShowEventHandler(Handler_setSoftwareShow);
                OrderManagerFunc.SoftwareVersionShowEvent += new OrderManagerFunctions.softwareLogoShowEventHandler2(Handler_setSoftwareVersion);
            }
            catch(Exception ex)
            {
                Inteware_Messagebox Msg = new Inteware_Messagebox();
                Msg.ShowMessage(ex.Message, "Init OrderManagerFunctions error", MessageBoxButton.OK, MessageBoxImage.Error);
                log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "Init Inteware_Messagebox error", ex.Message);
                Environment.Exit(0);
            }
            
            try
            {
                //更新Function
                UpdateFunc = new UpdateFunction();
                UpdateFunc.SoftwareLogoShowEvent += new UpdateFunction.softwareLogoShowEventHandler(Handler_setSoftwareShow);
                UpdateFunc.Handler_snackbarShow += new UpdateFunction.updatefuncEventHandler_snackbar(SnackBarShow);
                UpdateFunc.SoftwareUpdateEvent += new UpdateFunction.softwareUpdateStatusHandler(Handler_SetSoftwareUpdateButtonStatus);
                UpdateFunc.OMUpdateEvent += new UpdateFunction.omUpdateHandler(Handler_SetOMUpate);
            }
            catch(Exception ex)
            {
                Inteware_Messagebox Msg = new Inteware_Messagebox();
                Msg.ShowMessage(ex.Message, "Init UpdateFunction error", MessageBoxButton.OK, MessageBoxImage.Error);
                log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "Init UpdateFunction error", ex.Message);
                Environment.Exit(0);
            }

            try
            {
                //Airdental通道
                AirDentalProjHandle = new AirDentalProjectHandle
                {
                    Airdental = new Dll_Airdental.Main()
                };
                AirDentalProjHandle.Handler_snackbarShow += new AirDentalProjectHandle.AirDentalProjHandleEventHandler_snackbar(SnackBarShow);
                AirDentalProjHandle.AirdentalProjectShowEvent += new AirDentalProjectHandle.caseShowEventHandler(Handler_SetCaseShow_Airdental);
                AirDentalProjHandle.Main_orthoSetAirDentalProjectShow += new AirDentalProjectHandle.AirD_orthoBaseEventHandler(CloudCaseHandler_Ortho_showSingleProject);
                AirDentalProjHandle.Main_orthoSetSmallOrderDetailShow += new AirDentalProjectHandle.AirD_orthoBaseEventHandler2(CloudCaseHandler_Ortho_showDetail);
                AirDentalProjHandle.Main_implantSetAirDentalProjectShow += new AirDentalProjectHandle.AirD_implantBaseEventHandler(CloudCaseHandler_Implant_showSingleProject);
                AirDentalProjHandle.Main_implantSetSmallOrderDetailShow += new AirDentalProjectHandle.AirD_implantBaseEventHandler2(CloudCaseHandler_Implant_showDetail);
                AirDentalProjHandle.Main_cadSetAirDentalProjectShow += new AirDentalProjectHandle.AirD_cadBaseEventHandler(CloudCaseHandler_CAD_showSingleProject);
                AirDentalProjHandle.Main_cadSetSmallOrderDetailShow += new AirDentalProjectHandle.AirD_cadBaseEventHandler2(CloudCaseHandler_CAD_showDetail);
            }
            catch(Exception ex)
            {
                Inteware_Messagebox Msg = new Inteware_Messagebox();
                Msg.ShowMessage(ex.Message, "Init AirDentalProjectHandle error", MessageBoxButton.OK, MessageBoxImage.Error);
                log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "Init AirDentalProjectHandle error", ex.Message);
                Environment.Exit(0);
            }
            
            string[] args;
            args = Environment.GetCommandLineArgs();
            if(args != null && args.Length > 1)
            {
                foreach(string argument in args)
                {
                    if (argument == "-VerChk")
                    {
                        if (File.Exists("OrderManagerProps.xml") == true)
                            UpdateFunc.ImportPropertiesXml();
                        
                    }   
                    else if (argument == "-ExportProps")
                    {
                        UpdateFunc.ExportPropertiesXml();
                        Thread.Sleep(1000);
                        Environment.Exit(0);
                    }   
                }
            }

            //OrderManager不能多開
            Process[] MyProcess = Process.GetProcessesByName("OrderManager");
            if (MyProcess.Length > 1)
            {
                this.Visibility = Visibility.Collapsed;
                MyProcess[0].Kill(); //關閉執行中的程式
            }

            try
            {
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
            catch(Exception ex)
            {
                Inteware_Messagebox Msg = new Inteware_Messagebox();
                Msg.ShowMessage(ex.Message, "DeveloperMode switch error", MessageBoxButton.OK, MessageBoxImage.Error);
                log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "DeveloperMode switch error", ex.Message);
                Environment.Exit(0);
            }
        }

        private void Loaded_MainWindow(object sender, RoutedEventArgs e)
        {
            UpdateFunc.LoadHLXml();//截取線上HL.xml內的資料
            OrderManagerFunc.DoubleCheckEXEexist(false);//檢查軟體執行檔是否存在
            //檢查Cookie是否還可以用
            string[] uInfo = new string[4];
            if (AirDentalProjHandle.OrderManagerLoginCheck(ref uInfo) == true)
            {
                LoginSuccess(uInfo);
            }
            else
            {
                tabitem_Cloud.ToolTip = TranslationSource.Instance["PleaseLoginFirst"];
                tabitem_Cloud.IsEnabled = false;
            }
            DateFilterAll.IsChecked = true;
            // CaseFilter 復原之前最後選擇的軟體排序
            if (Properties.Settings.Default.LastSoftwareFilter >= (int)_softwareID.EZCAD && Properties.Settings.Default.LastSoftwareFilter < (int)_softwareID.All)
            {
                switch (Properties.Settings.Default.LastSoftwareFilter)
                {
                    case (int)_softwareID.EZCAD:
                        {
                            if (SoftwareFilterCAD.IsEnabled == true)
                                SoftwareFilterCAD.IsChecked = true;
                            else
                                ChangeSoftwareFilter();
                            break;
                        }
                    case (int)_softwareID.Implant:
                        {
                            if (SoftwareFilterImplant.IsEnabled == true)
                                SoftwareFilterImplant.IsChecked = true;
                            else
                                ChangeSoftwareFilter();
                            break;
                        }
                    case (int)_softwareID.Ortho:
                        {
                            if (SoftwareFilterOrtho.IsEnabled == true)
                                SoftwareFilterOrtho.IsChecked = true;
                            else
                                ChangeSoftwareFilter();
                            break;
                        }
                    case (int)_softwareID.Tray:
                        {
                            if (SoftwareFilterTray.IsEnabled == true)
                                SoftwareFilterTray.IsChecked = true;
                            else
                                ChangeSoftwareFilter();
                            break;
                        }
                    case (int)_softwareID.Splint:
                        {
                            if (SoftwareFilterSplint.IsEnabled == true)
                                SoftwareFilterSplint.IsChecked = true;
                            else
                                ChangeSoftwareFilter();
                            break;
                        }
                }
            }
            else
                ChangeSoftwareFilter();

            if(Properties.Settings.Default.cad_exePath == "" && 
                Properties.Settings.Default.implant_exePath == "" && 
                Properties.Settings.Default.ortho_exePath == "" && 
                Properties.Settings.Default.tray_exePath == "" &&
                Properties.Settings.Default.splint_exePath == "" &&
                Properties.Settings.Default.guide_exePath == "")
            {
                Inteware_Messagebox Msg = new Inteware_Messagebox();
                Msg.ShowMessage(TranslationSource.Instance["InitNoSoftwareAsk"], TranslationSource.Instance["AutoDetect"], MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if(Msg.ReturnClickWhitchButton == (int)Inteware_Messagebox._ReturnButtonName.YES )
                {
                    OrderManagerFunc.AutoDetectEXE((int)_classFrom.MainWindow);
                    ChangeSoftwareFilter();
                }
            }

            //檢查OrderManager是否為最新版本
            UpdateFunc.CheckOMHaveNewVersion();

            log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "Loaded_MainWindow", "fin func");
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
                isInstalling = true;
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
                UpdateFunc.readyUninstallSoftwareInfo.softwareID = SoftwareID;
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

                if (UpdateFunc.readyInstallSoftwareInfo.softwareID == (int)_softwareID.EZCAD && File.Exists(Properties.Settings.Default.cad_exePath + @"Bin\EZCAD.exe") == true)
                {
                    Properties.Settings.Default.cad_exePath += @"Bin\EZCAD.exe";
                    Properties.Settings.Default.Save();
                    FileVersionInfo verInfo = FileVersionInfo.GetVersionInfo(Properties.Settings.Default.cad_exePath);
                    if(verInfo.FileVersion != null)
                    {
                        bool isOldVer = false;
                        foreach (UpdateFunction.SoftwareInfo info in UpdateFunc.CloudSoftwareTotal)
                        {
                            if (info.softwareID != (int)_softwareID.EZCAD)
                                continue;
                            else if (info.softwareVersion > new Version(verInfo.FileVersion))
                            {
                                isOldVer = true;
                                break;
                            }
                        }
                        if (isOldVer == false)
                        {
                            this.Dispatcher.Invoke((Action)(() =>
                            {
                                Handler_setSoftwareShow((int)_softwareID.EZCAD, (int)_softwareStatus.Installed, 0);
                                string snackStr = TranslationSource.Instance["Install"] + " " + OrderManagerFunc.GetSoftwareName(_softwareID.EZCAD)
                                + " " + TranslationSource.Instance["Successfully"];
                                SnackBarShow(snackStr);
                                OrderManagerFunc.AutoDetectEXE((int)_classFrom.MainWindow);
                                isInstalling = false;
                                ChangeSoftwareFilter();
                                watcher = new FileSystemWatcher();
                            }));
                        }
                    }
                }
                else if (UpdateFunc.readyInstallSoftwareInfo.softwareID == (int)_softwareID.Implant && File.Exists(Properties.Settings.Default.implant_exePath + @"ImplantPlanning.exe") == true)
                {
                    Properties.Settings.Default.implant_exePath += @"ImplantPlanning.exe";
                    Properties.Settings.Default.Save();
                    FileVersionInfo verInfo = FileVersionInfo.GetVersionInfo(Properties.Settings.Default.implant_exePath);
                    if(verInfo.FileVersion != null)
                    {
                        bool isOldVer = false;
                        foreach (UpdateFunction.SoftwareInfo info in UpdateFunc.CloudSoftwareTotal)
                        {
                            if (info.softwareID != (int)_softwareID.Implant)
                                continue;
                            else if (info.softwareVersion > new Version(verInfo.FileVersion))
                            {
                                isOldVer = true;
                                break;
                            }
                        }
                        if (isOldVer == false)
                        {
                            this.Dispatcher.Invoke((Action)(() =>
                            {
                                Handler_setSoftwareShow((int)_softwareID.Implant, (int)_softwareStatus.Installed, 0);
                                string snackStr = TranslationSource.Instance["Install"] + " " + OrderManagerFunc.GetSoftwareName(_softwareID.Implant)
                                + " " + TranslationSource.Instance["Successfully"];
                                SnackBarShow(snackStr);
                                OrderManagerFunc.AutoDetectEXE((int)_classFrom.MainWindow);
                                isInstalling = false;
                                ChangeSoftwareFilter();
                                watcher = new FileSystemWatcher();
                            }));
                        }
                    }
                }
                else if (UpdateFunc.readyInstallSoftwareInfo.softwareID == (int)_softwareID.Ortho && File.Exists(Properties.Settings.Default.ortho_exePath + @"OrthoAnalysis.exe") == true)
                {
                    Properties.Settings.Default.ortho_exePath += @"OrthoAnalysis.exe";
                    Properties.Settings.Default.Save();
                    FileVersionInfo verInfo = FileVersionInfo.GetVersionInfo(Properties.Settings.Default.ortho_exePath);
                    if(verInfo.FileVersion != null)
                    {
                        bool isOldVer = false;
                        foreach (UpdateFunction.SoftwareInfo info in UpdateFunc.CloudSoftwareTotal)
                        {
                            if (info.softwareID != (int)_softwareID.Ortho)
                                continue;
                            else if (info.softwareVersion > new Version(verInfo.FileVersion))
                            {
                                isOldVer = true;
                                break;
                            }
                        }
                        if (isOldVer == false)
                        {
                            this.Dispatcher.Invoke((Action)(() =>
                            {
                                Handler_setSoftwareShow((int)_softwareID.Ortho, (int)_softwareStatus.Installed, 0);
                                string snackStr = TranslationSource.Instance["Install"] + " " + OrderManagerFunc.GetSoftwareName(_softwareID.Ortho)
                                + " " + TranslationSource.Instance["Successfully"];
                                SnackBarShow(snackStr);
                                OrderManagerFunc.AutoDetectEXE((int)_classFrom.MainWindow);
                                isInstalling = false;
                                ChangeSoftwareFilter();
                                watcher = new FileSystemWatcher();
                            }));
                        }
                    }
                }
                else if (UpdateFunc.readyInstallSoftwareInfo.softwareID == (int)_softwareID.Tray && File.Exists(Properties.Settings.Default.tray_exePath + @"Bin\EZCAD.tray.exe") == true)
                {
                    Properties.Settings.Default.tray_exePath += @"Bin\EZCAD.tray.exe";
                    Properties.Settings.Default.Save();
                    FileVersionInfo verInfo = FileVersionInfo.GetVersionInfo(Properties.Settings.Default.tray_exePath);
                    if(verInfo.FileVersion != null)
                    {
                        bool isOldVer = false;
                        foreach (UpdateFunction.SoftwareInfo info in UpdateFunc.CloudSoftwareTotal)
                        {
                            if (info.softwareID != (int)_softwareID.Tray)
                                continue;
                            else if (info.softwareVersion > new Version(verInfo.FileVersion))
                            {
                                isOldVer = true;
                                break;
                            }
                        }
                        if (isOldVer == false)
                        {
                            this.Dispatcher.Invoke((Action)(() =>
                            {
                                Handler_setSoftwareShow((int)_softwareID.Tray, (int)_softwareStatus.Installed, 0);
                                string snackStr = TranslationSource.Instance["Install"] + " " + OrderManagerFunc.GetSoftwareName(_softwareID.Tray)
                                + " " + TranslationSource.Instance["Successfully"];
                                SnackBarShow(snackStr);
                                OrderManagerFunc.AutoDetectEXE((int)_classFrom.MainWindow);
                                isInstalling = false;
                                ChangeSoftwareFilter();
                                watcher = new FileSystemWatcher();
                            }));
                        }
                    }
                }
                else if (UpdateFunc.readyInstallSoftwareInfo.softwareID == (int)_softwareID.Splint && File.Exists(Properties.Settings.Default.tray_exePath + @"Bin\EZCAD.splint.exe") == true)
                {
                    Properties.Settings.Default.splint_exePath += @"Bin\EZCAD.splint.exe";
                    Properties.Settings.Default.Save();
                    FileVersionInfo verInfo = FileVersionInfo.GetVersionInfo(Properties.Settings.Default.splint_exePath);
                    if(verInfo.FileVersion != null)
                    {
                        bool isOldVer = false;
                        foreach (UpdateFunction.SoftwareInfo info in UpdateFunc.CloudSoftwareTotal)
                        {
                            if (info.softwareID != (int)_softwareID.Splint)
                                continue;
                            else if (info.softwareVersion > new Version(verInfo.FileVersion))
                            {
                                isOldVer = true;
                                break;
                            }
                        }
                        if (isOldVer == false)
                        {
                            this.Dispatcher.Invoke((Action)(() =>
                            {
                                Handler_setSoftwareShow((int)_softwareID.Splint, (int)_softwareStatus.Installed, 0);
                                string snackStr = TranslationSource.Instance["Install"] + " " + OrderManagerFunc.GetSoftwareName(_softwareID.Splint)
                                + " " + TranslationSource.Instance["Successfully"];
                                SnackBarShow(snackStr);
                                OrderManagerFunc.AutoDetectEXE((int)_classFrom.MainWindow);
                                isInstalling = false;
                                ChangeSoftwareFilter();
                                watcher = new FileSystemWatcher();
                            }));
                        }
                    }
                }
                else if (UpdateFunc.readyInstallSoftwareInfo.softwareID == (int)_softwareID.Guide && File.Exists(Properties.Settings.Default.guide_exePath + @"Bin\EZCAD.guide.exe") == true)
                {
                    Properties.Settings.Default.guide_exePath += @"Bin\EZCAD.guide.exe";
                    Properties.Settings.Default.Save();
                    FileVersionInfo verInfo = FileVersionInfo.GetVersionInfo(Properties.Settings.Default.guide_exePath);
                    if(verInfo.FileVersion != null)
                    {
                        bool isOldVer = false;
                        foreach (UpdateFunction.SoftwareInfo info in UpdateFunc.CloudSoftwareTotal)
                        {
                            if (info.softwareID != (int)_softwareID.Guide)
                                continue;
                            else if (info.softwareVersion > new Version(verInfo.FileVersion))
                            {
                                isOldVer = true;
                                break;
                            }
                        }
                        if (isOldVer == false)
                        {
                            this.Dispatcher.Invoke((Action)(() =>
                            {
                                Handler_setSoftwareShow((int)_softwareID.Guide, (int)_softwareStatus.Installed, 0);
                                string snackStr = TranslationSource.Instance["Install"] + " " + OrderManagerFunc.GetSoftwareName(_softwareID.Guide)
                                + " " + TranslationSource.Instance["Successfully"];
                                SnackBarShow(snackStr);
                                OrderManagerFunc.AutoDetectEXE((int)_classFrom.MainWindow);
                                isInstalling = false;
                                ChangeSoftwareFilter();
                                watcher = new FileSystemWatcher();
                            }));
                        }
                    }
                }

                if (Path.GetExtension(exeName).ToLower() == ".exe")
                {
                    Console.WriteLine(exeName);
                    if (exeName.IndexOf("design.exe") != -1 || exeName.IndexOf("dentdesign.exe") != -1 || exeName.IndexOf("ezcad.exe") != -1 || exeName.IndexOf("implantplanning.exe") != -1 || exeName.IndexOf("orthoanalysis.exe") != -1
                    || exeName.IndexOf("tray.exe") != -1 || exeName.IndexOf("splint.exe") != -1 || exeName.IndexOf("guide.exe") != -1)
                {
                    DialogBeforeDownload.SetPropertiesSoftwarePath(UpdateFunc.readyInstallSoftwareInfo.softwareID, e.FullPath);
                    haveEXE = true;

                        if(dirSize >= LimitSize)
                        {
                            this.Dispatcher.Invoke((Action)(() =>
                            {
                                haveEXE = false;
                                watcher = new FileSystemWatcher();
                                Thread.Sleep(2000);//2秒緩衝
                                Handler_setSoftwareShow(UpdateFunc.readyInstallSoftwareInfo.softwareID, (int)_softwareStatus.Installed, 0);
                                string snackStr = TranslationSource.Instance["Install"] + " " + OrderManagerFunc.GetSoftwareName(UpdateFunc.readyInstallSoftwareInfo.softwareID)
                                + " " + TranslationSource.Instance["Successfully"];
                                SnackBarShow(snackStr);
                                OrderManagerFunc.AutoDetectEXE((int)_classFrom.MainWindow);
                                isInstalling = false;
                                ChangeSoftwareFilter();
                                return;
                            }));
                        }
                    }
                }

                if (dirSize >= LimitSize && haveEXE == true)
                {
                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        haveEXE = false;
                        watcher = new FileSystemWatcher();
                        Thread.Sleep(2000);//2秒緩衝
                        Handler_setSoftwareShow(UpdateFunc.readyInstallSoftwareInfo.softwareID, (int)_softwareStatus.Installed, 0);
                        string snackStr = TranslationSource.Instance["Install"] + " " + OrderManagerFunc.GetSoftwareName(UpdateFunc.readyInstallSoftwareInfo.softwareID) 
                        + " " + TranslationSource.Instance["Successfully"];
                        SnackBarShow(snackStr);
                        OrderManagerFunc.AutoDetectEXE((int)_classFrom.MainWindow);
                        isInstalling = false;
                        ChangeSoftwareFilter();
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
                    string snackStr = TranslationSource.Instance["Uninstall"] + " " + OrderManagerFunc.GetSoftwareName(UpdateFunc.readyUninstallSoftwareInfo.softwareID)
                        + " " + TranslationSource.Instance["Successfully"];
                    SnackBarShow(snackStr);
                    OrderManagerFunc.AutoDetectEXE((int)_classFrom.MainWindow);
                    //System.Threading.Thread.Sleep(1000);
                    Handler_setSoftwareShow(UpdateFunc.readyUninstallSoftwareInfo.softwareID, (int)_softwareStatus.NotInstall, 0);
                    ChangeSoftwareFilter();
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
            //TODO:還在測試，目前先關閉Case Watcher
            return;

            /*if (Directory.Exists(Path) == false)
                return;

            Watcher = new FileSystemWatcher
            {
                Path = Path,
                //設定所要監控的變更類型
                NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,

                //設定是否監控子資料夾
                IncludeSubdirectories = true,

                //設定是否啟動元件，此部分必須要設定為 true，不然事件是不會被觸發的
                EnableRaisingEvents = true
            };

            //設定觸發事件
            Watcher.Created += new FileSystemEventHandler(Watcher_ProjectChanged);
            Watcher.Deleted += new FileSystemEventHandler(Watcher_ProjectChanged);*/
        }

        private void Watcher_ProjectChanged(object sender, FileSystemEventArgs e)
        {
            if (Path.GetExtension(e.Name).ToLower() != ".xml" && Path.GetExtension(e.Name).ToLower() != "")
                return;

            this.Dispatcher.Invoke((Action)(() =>
            {
                try
                {
                    if ((Properties.Settings.Default.LastSoftwareFilter == (int)_softwareID.EZCAD) && (e.FullPath.Replace(e.Name, "") == Properties.OrderManagerProps.Default.cad_projectDirectory))
                        ProjHandle.LoadEZCADProj();
                    else if ((Properties.Settings.Default.LastSoftwareFilter == (int)_softwareID.Implant) && (e.FullPath.Replace(e.Name, "") == Properties.OrderManagerProps.Default.implant_projectDirectory))
                        ProjHandle.LoadImplantProjV2();
                    else if ((Properties.Settings.Default.LastSoftwareFilter == (int)_softwareID.Ortho) && (e.FullPath.Replace(e.Name, "") == Properties.OrderManagerProps.Default.ortho_projectDirectory))
                        ProjHandle.LoadOrthoProj();
                    else if ((Properties.Settings.Default.LastSoftwareFilter == (int)_softwareID.Tray) && (e.FullPath.Replace(e.Name, "") == Properties.OrderManagerProps.Default.tray_projectDirectory))
                        ProjHandle.LoadTrayProj();
                    else if ((Properties.Settings.Default.LastSoftwareFilter == (int)_softwareID.Splint) && (e.FullPath.Replace(e.Name, "") == Properties.OrderManagerProps.Default.splint_projectDirectory))
                        ProjHandle.LoadSplintProj();
                }
                catch (Exception ex)
                {
                    log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "Watcher_ProjectCreated_exception", ex.Message);
                }
            }));
        }
#endregion

#region WindowFrame
        private void KeyUp_MainWindow(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)
                ChooseToLoadProj();
        }
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
                            Properties.Settings.Default.LastSoftwareFilter = -1;
                            Properties.Settings.Default.AirdentalAcc = "";
                            Properties.Settings.Default.AirdentalCookie = "";
                            Properties.Settings.Default.Save();

                            Properties.OrderManagerProps.Default.cad_projectDirectory = "";
                            Properties.OrderManagerProps.Default.implant_projectDirectory = "";
                            Properties.OrderManagerProps.Default.ortho_projectDirectory = "";
                            Properties.OrderManagerProps.Default.tray_projectDirectory = "";
                            Properties.OrderManagerProps.Default.splint_projectDirectory = "";
                            Properties.OrderManagerProps.Default.DateFilter = (int)_DateFilter.All;
                            Properties.OrderManagerProps.Default.PatientNameFilter = "";
                            Properties.OrderManagerProps.Default.CaseNameFilter = "";
                            Properties.OrderManagerProps.Default.mostsoftwareDisk = "";
                            Properties.OrderManagerProps.Default.systemDisk = "";
                            Properties.OrderManagerProps.Default.AirD_uid = "";
                            Properties.OrderManagerProps.Default.AirD_CAD_Dir = "";
                            Properties.OrderManagerProps.Default.AirD_Implant_Dir = "";
                            Properties.OrderManagerProps.Default.AirD_Ortho_Dir = "";

                            Handler_setSoftwareShow((int)_softwareID.EZCAD, (int)_softwareStatus.NotInstall, 0.0);
                            Handler_setSoftwareShow((int)_softwareID.Implant, (int)_softwareStatus.NotInstall, 0.0);
                            Handler_setSoftwareShow((int)_softwareID.Ortho, (int)_softwareStatus.NotInstall, 0.0);
                            Handler_setSoftwareShow((int)_softwareID.Tray, (int)_softwareStatus.NotInstall, 0.0);
                            Handler_setSoftwareShow((int)_softwareID.Splint, (int)_softwareStatus.NotInstall, 0.0);
                            Handler_setSoftwareShow((int)_softwareID.Guide, (int)_softwareStatus.NotInstall, 0.0);
                            ChangeSoftwareFilter();

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
                                if(childCase is OrderManagerNew.Local_UserControls.Order_orthoBase)
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
            var mouseWasDownOnUserDetail = e.Source as Local_UserControls.AirdentalUserDetail;
            var mouseWasDownOnomFunction_User = e.Source as Button;
            if (loginStatus == true)
            {
                if (mouseWasDownOnomFunction_User != null && mouseWasDownOnomFunction_User.Name == "omFunction_User")
                    return;

                if (mouseWasDownOnUserDetail == null)
                    UserDetailshow(false);
            }
        }
        private void LoginSuccess(string[] UserDetail)
        {
            usercontrolUserDetail.Usergroup = UserDetail[(int)_AirD_LoginDetail.USERGROUP];
            usercontrolUserDetail.UserMail = UserDetail[(int)_AirD_LoginDetail.EMAIL];
            usercontrolUserDetail.UserName = UserDetail[(int)_AirD_LoginDetail.USERNAME];
            Properties.OrderManagerProps.Default.AirDentalAPI = AirDentalProjHandle.APIPortal;
            usercontrolUserDetail.SetUserPic(AirDentalProjHandle.APIPortal + @"v2/user/avatar/" + Properties.OrderManagerProps.Default.AirD_uid);
            AirDentalProjHandle.CheckAirDentalDirExist();
            loginStatus = true;
            SnackBarShow(TranslationSource.Instance["Hello"] + usercontrolUserDetail.UserName);
            tabitem_Cloud.ToolTip = null;
            tabitem_Cloud.IsEnabled = true;
            tabcontrol_Main.SelectedItem = tabitem_Cloud;
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

        private void SetBlur(bool isBlur)
        {
            switch(isBlur)
            {
                case true:
                    {
                        //主視窗羽化
                        var blur = new BlurEffect();
                        this.Effect = blur;
                        break;
                    }
                case false:
                    {
                        //主視窗還原
                        this.Effect = null;
                        this.OpacityMask = null;
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
        private void Click_ProjectsRefresh(object sender, RoutedEventArgs e)
        {
            ChooseToLoadProj();
        }
        private void Click_FunctionTable_Setting(object sender, RoutedEventArgs e)
        {
            GoToSetting(-1);
        }
        private void Click_FunctionTable_User(object sender, RoutedEventArgs e)
        {
            if (AirDentalProjHandle.Airdental.CheckServerStatus(AirDentalProjHandle.APIPortal) != true)
            {
                SnackBarShow(TranslationSource.Instance["NetworkError"]);
                log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "Click_FunctionTable_User", "Connection error");
                return;
            }

            if (loginStatus == false)
            {
                var blur = new BlurEffect();
                this.Effect = blur;
                UserLogin DialogLogin = new UserLogin
                {
                    Airdental_main = AirDentalProjHandle.Airdental,
                    Owner = this
                };
                var dialogResult = DialogLogin.ShowDialog();
                if (dialogResult == true)
                {
                    LoginSuccess(DialogLogin.UserDetail);
                    ChooseToLoadProj();
                }
                this.Effect = null;
                this.OpacityMask = null;
            }
            else
            {
                if (showUserDetail == false)
                {
                    UserDetailshow(true);
                }
                else
                    UserDetailshow(false);
            }
        }
        private void Click_UserDetail_Logout(object sender, RoutedEventArgs e)
        {
            UserDetailshow(false);
            AirDentalProjHandle.UserLogout();
            StackPanel_Cloud.Children.Clear();
            loginStatus = false;

            tabcontrol_Main.SelectedItem = tabitem_Local;
            tabitem_Cloud.ToolTip = TranslationSource.Instance["PleaseLoginFirst"];
            tabitem_Cloud.IsEnabled = false;
        }
        /// <summary>
        /// 設定SofttwareTable的PopupBox事件
        /// </summary>
        private void Click_SoftwareTable(object sender, RoutedEventArgs e)
        {
            if (sender is Button)
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
                            Handler_setSoftwareShow((int)_softwareID.EZCAD, (int)_softwareStatus.Updating, 0);
                            for (int i = 0; i < UpdateFunc.CloudSoftwareTotal.Count; i++)
                            {
                                if (UpdateFunc.CloudSoftwareTotal[i].softwareID == (int)_softwareID.EZCAD)
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
                            OrderManagerFunc.RunCommandLine(Properties.Settings.Default.cad_exePath, "");
                            break;
                        }
                    case "cad_webIntro":
                        {
                            if (Properties.Settings.Default.sysLanguage == "zh-TW")
                                OrderManagerFunc.RunCommandLine("https://www.inteware.com.tw/wp-content/uploads/2020/07/EZCAD-%E8%BB%9F%E9%AB%94%E4%BD%BF%E7%94%A8%E6%89%8B%E5%86%8A-V2.1.20325.pdf", "");
                            else
                                OrderManagerFunc.RunCommandLine("https://www.inteware.com.tw/wp-content/uploads/2020/07/EZCAD-User-Guide-V2.1_20325.pdf", "");
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
                            Inteware_Messagebox Msg = new Inteware_Messagebox();
                            Msg.ShowMessage(TranslationSource.Instance["AreyousureUninstall"] + "EZCAD?", TranslationSource.Instance["Uninstall"], MessageBoxButton.YesNo, MessageBoxImage.Warning);
                            if (Msg.ReturnClickWhitchButton == (int)Inteware_Messagebox._ReturnButtonName.YES)
                            {
                                if (Path.GetExtension(Properties.Settings.Default.cad_exePath) == ".exe")
                                {
                                    string uninstallPath = Path.GetDirectoryName(Properties.Settings.Default.cad_exePath) + @"\Uninstall.lnk";
                                    if (File.Exists(uninstallPath) == true)
                                    {
                                        Handler_setSoftwareShow((int)_softwareID.EZCAD, (int)_softwareStatus.Uninstalling, 0);
                                        OrderManagerFunc.RunCommandLine(uninstallPath, "/quiet", true);
                                    }
                                    else
                                    {
                                        Msg.ShowMessage(TranslationSource.Instance["CannotFindUninstall"]);
                                    }
                                }
                            }
                            break;
                        }
                    case "cad_relNote":
                        {
                            ShowReleaseNote(_softwareID.EZCAD);
                            break;
                        }
                    #endregion
                    #region Implant
                    case "implant_update":
                        {
                            Handler_setSoftwareShow((int)_softwareID.Implant, (int)_softwareStatus.Updating, 0);
                            for (int i = 0; i < UpdateFunc.CloudSoftwareTotal.Count; i++)
                            {
                                if (UpdateFunc.CloudSoftwareTotal[i].softwareID == (int)_softwareID.Implant)
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
                    case "implant_open":
                        {
                            OrderManagerFunc.RunCommandLine(Properties.Settings.Default.implant_exePath, "");
                            break;
                        }
                    case "implant_create":
                        {
                            SetBlur(true);
                            V2Implant.ImplantV2NewOrder w1 = new V2Implant.ImplantV2NewOrder
                            {
                                Owner = this,
                                ShowActivated = true,
                                m_ImplantRoot = Properties.OrderManagerProps.Default.implant_projectDirectory,
                                Selected_folder_path = Properties.OrderManagerProps.Default.implant_projectDirectory
                            };
                            w1.NewOrderStatusUpdated += OrderEventHandlerFunction_StatusUpdated;
                            w1.ShowDialog();
                            SetBlur(false);
                            break;
                        }
                    case "implant_webIntro":
                        {
                            if (Properties.Settings.Default.sysLanguage == "zh-TW")
                                OrderManagerFunc.RunCommandLine("https://www.inteware.com.tw/wp-content/uploads/2020/07/ImplantPlanning_User-Guide-V2.1_20214.pdf", "");
                            else
                                OrderManagerFunc.RunCommandLine("https://www.inteware.com.tw/wp-content/uploads/2020/07/ImplantPlanning_User-Guide-V2.1_20214.pdf", "");
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
                            Inteware_Messagebox Msg = new Inteware_Messagebox();
                            Msg.ShowMessage(TranslationSource.Instance["AreyousureUninstall"] + "ImplantPlanning?", TranslationSource.Instance["Uninstall"], MessageBoxButton.YesNo, MessageBoxImage.Warning);
                            if (Msg.ReturnClickWhitchButton == (int)Inteware_Messagebox._ReturnButtonName.YES)
                            {
                                if (Path.GetExtension(Properties.Settings.Default.implant_exePath) == ".exe")
                                {
                                    string uninstallPath = Path.GetDirectoryName(Properties.Settings.Default.implant_exePath) + @"\Uninstall.lnk";
                                    if (File.Exists(uninstallPath) == true)
                                    {
                                        Handler_setSoftwareShow((int)_softwareID.Implant, (int)_softwareStatus.Uninstalling, 0);
                                        OrderManagerFunc.RunCommandLine(uninstallPath, "/quiet", true);
                                    }
                                    else
                                    {
                                        Msg.ShowMessage(TranslationSource.Instance["CannotFindUninstall"]);
                                    }
                                }
                            }
                            break;
                        }
                    case "implant_relNote":
                        {
                            ShowReleaseNote(_softwareID.Implant);
                            break;
                        }
                    #endregion
                    #region Ortho
                    case "ortho_update":
                        {
                            Handler_setSoftwareShow((int)_softwareID.Ortho, (int)_softwareStatus.Updating, 0);
                            for (int i = 0; i < UpdateFunc.CloudSoftwareTotal.Count; i++)
                            {
                                if (UpdateFunc.CloudSoftwareTotal[i].softwareID == (int)_softwareID.Ortho)
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
                            OrderManagerFunc.RunCommandLine(Properties.Settings.Default.ortho_exePath, "");
                            break;
                        }
                    case "ortho_webIntro":
                        {
                            if (Properties.Settings.Default.sysLanguage == "zh-TW")
                                OrderManagerFunc.RunCommandLine("https://www.inteware.com.tw/wp-content/uploads/2020/09/OrthoAnalysis_軟體使用手册-v3.1_20508.pdf", "");
                            else
                                OrderManagerFunc.RunCommandLine("https://www.inteware.com.tw/wp-content/uploads/2020/09/OrthoAnalysis_User-Guide-v3.1_20409.pdf", "");
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
                            Inteware_Messagebox Msg = new Inteware_Messagebox();
                            Msg.ShowMessage(TranslationSource.Instance["AreyousureUninstall"] + "OrthoAnalysis?", TranslationSource.Instance["Uninstall"], MessageBoxButton.YesNo, MessageBoxImage.Warning);
                            if (Msg.ReturnClickWhitchButton == (int)Inteware_Messagebox._ReturnButtonName.YES)
                            {
                                if (Path.GetExtension(Properties.Settings.Default.ortho_exePath) == ".exe")
                                {
                                    string uninstallPath = Path.GetDirectoryName(Properties.Settings.Default.ortho_exePath) + @"\Uninstall.lnk";
                                    if (File.Exists(uninstallPath) == true)
                                    {
                                        Handler_setSoftwareShow((int)_softwareID.Ortho, (int)_softwareStatus.Uninstalling, 0);
                                        OrderManagerFunc.RunCommandLine(uninstallPath, "/quiet", true);
                                    }
                                    else
                                    {
                                        Msg.ShowMessage(TranslationSource.Instance["CannotFindUninstall"]);
                                    }
                                }
                            }
                            break;
                        }
                    case "ortho_relNote":
                        {
                            ShowReleaseNote(_softwareID.Ortho);
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
                            OrderManagerFunc.RunCommandLine(Properties.Settings.Default.tray_exePath, "");
                            break;
                        }
                    case "tray_webIntro":
                        {
                            if (Properties.Settings.Default.sysLanguage == "zh-TW")
                                OrderManagerFunc.RunCommandLine("https://www.inteware.com.tw/wp-content/uploads/2020/09/EZCAD-Tray_軟體使用手冊-V1.0_20218.pdf", "");
                            else
                                OrderManagerFunc.RunCommandLine("https://www.inteware.com.tw/wp-content/uploads/2020/09/EZCAD-Tray_User-Guide-V1.0_20218.pdf", "");
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
                            Inteware_Messagebox Msg = new Inteware_Messagebox();
                            Msg.ShowMessage(TranslationSource.Instance["AreyousureUninstall"] + "EZCAD.tray?", TranslationSource.Instance["Uninstall"], MessageBoxButton.YesNo, MessageBoxImage.Warning);
                            if (Msg.ReturnClickWhitchButton == (int)Inteware_Messagebox._ReturnButtonName.YES)
                            {
                                if (Path.GetExtension(Properties.Settings.Default.tray_exePath) == ".exe")
                                {
                                    string uninstallPath = Path.GetDirectoryName(Properties.Settings.Default.tray_exePath) + @"\Uninstall.lnk";
                                    if (File.Exists(uninstallPath) == true)
                                    {
                                        Handler_setSoftwareShow((int)_softwareID.Tray, (int)_softwareStatus.Uninstalling, 0);
                                        OrderManagerFunc.RunCommandLine(uninstallPath, "/quiet", true);
                                    }
                                    else
                                    {
                                        Msg.ShowMessage(TranslationSource.Instance["CannotFindUninstall"]);
                                    }
                                }
                            }
                            break;
                        }
                    case "tray_relNote":
                        {
                            ShowReleaseNote(_softwareID.Tray);
                            break;
                        }
                    #endregion
                    #region Splint
                    case "splint_update":
                        {
                            Handler_setSoftwareShow((int)_softwareID.Splint, (int)_softwareStatus.Updating, 0);
                            for (int i = 0; i < UpdateFunc.CloudSoftwareTotal.Count; i++)
                            {
                                if (UpdateFunc.CloudSoftwareTotal[i].softwareID == (int)_softwareID.Splint)
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
                            OrderManagerFunc.RunCommandLine(Properties.Settings.Default.splint_exePath, "");
                            break;
                        }
                    case "splint_webIntro":
                        {
                            if (Properties.Settings.Default.sysLanguage == "zh-TW")
                                OrderManagerFunc.RunCommandLine("https://www.inteware.com.tw/wp-content/uploads/2020/09/EZCAD-Splint_軟體使用手冊-V1.0_20218.pdf", "");
                            else
                                OrderManagerFunc.RunCommandLine("https://www.inteware.com.tw/wp-content/uploads/2020/09/EZCAD-Splint_User-Guide-V1.0_20218.pdf", "");
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
                            Inteware_Messagebox Msg = new Inteware_Messagebox();
                            Msg.ShowMessage(TranslationSource.Instance["AreyousureUninstall"] + "EZCAD.splint?", TranslationSource.Instance["Uninstall"], MessageBoxButton.YesNo, MessageBoxImage.Warning);
                            if (Msg.ReturnClickWhitchButton == (int)Inteware_Messagebox._ReturnButtonName.YES)
                            {
                                if (Path.GetExtension(Properties.Settings.Default.splint_exePath) == ".exe")
                                {
                                    string uninstallPath = Path.GetDirectoryName(Properties.Settings.Default.splint_exePath) + @"\Uninstall.lnk";
                                    if (File.Exists(uninstallPath) == true)
                                    {
                                        Handler_setSoftwareShow((int)_softwareID.Splint, (int)_softwareStatus.Uninstalling, 0);
                                        OrderManagerFunc.RunCommandLine(uninstallPath, "/quiet", true);
                                    }
                                    else
                                    {
                                        Msg.ShowMessage(TranslationSource.Instance["CannotFindUninstall"]);
                                    }
                                }
                            }
                            break;
                        }
                    case "splint_relNote":
                        {
                            ShowReleaseNote(_softwareID.Splint);
                            break;
                        }
                    #endregion
                    #region Guide
                    case "guide_update":
                        {
                            Handler_setSoftwareShow((int)_softwareID.Guide, (int)_softwareStatus.Updating, 0);
                            for (int i = 0; i < UpdateFunc.CloudSoftwareTotal.Count; i++)
                            {
                                if (UpdateFunc.CloudSoftwareTotal[i].softwareID == (int)_softwareID.Guide)
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
                    case "guide_selectPath":
                        {
                            GoToSetting((int)_softwareID.Guide);
                            break;
                        }
                    case "guide_download":
                        {
                            for (int i = 0; i < UpdateFunc.CloudSoftwareTotal.Count; i++)
                            {
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
                            Inteware_Messagebox Msg = new Inteware_Messagebox();
                            Msg.ShowMessage(TranslationSource.Instance["AreyousureUninstall"] + "EZCAD.guide?", TranslationSource.Instance["Uninstall"], MessageBoxButton.YesNo, MessageBoxImage.Warning);
                            if (Msg.ReturnClickWhitchButton == (int)Inteware_Messagebox._ReturnButtonName.YES)
                            {
                                if (Path.GetExtension(Properties.Settings.Default.guide_exePath) == ".exe")
                                {
                                    string uninstallPath = Path.GetDirectoryName(Properties.Settings.Default.guide_exePath) + @"\Uninstall.lnk";
                                    if (File.Exists(uninstallPath) == true)
                                    {
                                        Handler_setSoftwareShow((int)_softwareID.Guide, (int)_softwareStatus.Uninstalling, 0);
                                        OrderManagerFunc.RunCommandLine(uninstallPath, "/quiet", true);
                                    }
                                    else
                                    {
                                        Msg.ShowMessage(TranslationSource.Instance["CannotFindUninstall"]);
                                    }
                                }
                            }
                            break;
                        }
                    case "guide_relNote":
                        {
                            ShowReleaseNote(_softwareID.Guide);
                            break;
                        }
                    #endregion
                }
            }
        }

        private void ShowReleaseNote(_softwareID software)
        {
            SetBlur(true);
            ReleaseNote relNote = new ReleaseNote();
            relNote.SetCurrentSoftware(software);
            relNote.Owner = this;
            relNote.ShowDialog();
            SetBlur(false);
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
                                    cad_demo.Visibility = Visibility.Collapsed;
                                    cad_troubleShooting.Visibility = Visibility.Collapsed;
                                    cad_buyLic.Visibility = Visibility.Collapsed;

                                    popupbox_EZCAD.IsEnabled = true;
                                    cad_selectPath.Visibility = Visibility.Collapsed;
                                    cad_download.Visibility = Visibility.Collapsed;
                                    cad_open.Visibility = Visibility.Visible;
                                    cad_unInstall.Visibility = Visibility.Visible;
                                    SoftwareFilterCAD.IsEnabled = true;
                                    cad_update.Foreground = this.FindResource("Common_DarkBrown") as SolidColorBrush;
                                    cad_update.Content = TranslationSource.Instance["LatestVer"];
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
                                    implant_open.Visibility = Visibility.Collapsed;
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
                                    implant_demo.Visibility = Visibility.Collapsed;
                                    implant_troubleShooting.Visibility = Visibility.Collapsed;
                                    implant_buyLic.Visibility = Visibility.Collapsed;
                                    
                                    popupbox_Implant.IsEnabled = true;
                                    implant_selectPath.Visibility = Visibility.Collapsed;
                                    implant_download.Visibility = Visibility.Collapsed;
                                    implant_open.Visibility = Visibility.Visible;
                                    implant_create.Visibility = Visibility.Visible;
                                    implant_unInstall.Visibility = Visibility.Visible;
                                    SoftwareFilterImplant.IsEnabled = true;
                                    implant_update.Foreground = this.FindResource("Common_DarkBrown") as SolidColorBrush;
                                    implant_update.Content = TranslationSource.Instance["LatestVer"];
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
                                    ortho_demo.Visibility = Visibility.Collapsed;
                                    ortho_troubleShooting.Visibility = Visibility.Collapsed;
                                    ortho_buyLic.Visibility = Visibility.Collapsed;

                                    popupbox_Ortho.IsEnabled = true;
                                    ortho_selectPath.Visibility = Visibility.Collapsed;
                                    ortho_download.Visibility = Visibility.Collapsed;
                                    ortho_open.Visibility = Visibility.Visible;
                                    ortho_unInstall.Visibility = Visibility.Visible;
                                    SoftwareFilterOrtho.IsEnabled = true;
                                    ortho_update.Foreground = this.FindResource("Common_DarkBrown") as SolidColorBrush;
                                    ortho_update.Content = TranslationSource.Instance["LatestVer"];
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
                                    tray_demo.Visibility = Visibility.Collapsed;
                                    tray_troubleShooting.Visibility = Visibility.Collapsed;
                                    tray_buyLic.Visibility = Visibility.Collapsed;

                                    popupbox_Tray.IsEnabled = true;
                                    tray_selectPath.Visibility = Visibility.Collapsed;
                                    tray_download.Visibility = Visibility.Collapsed;
                                    tray_open.Visibility = Visibility.Visible;
                                    tray_unInstall.Visibility = Visibility.Visible;
                                    SoftwareFilterTray.IsEnabled = true;
                                    tray_update.Foreground = this.FindResource("Common_DarkBrown") as SolidColorBrush;
                                    tray_update.Visibility = Visibility.Visible;
                                    tray_update.Content = TranslationSource.Instance["LatestVer"];
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
                                    splint_demo.Visibility = Visibility.Collapsed;
                                    splint_troubleShooting.Visibility = Visibility.Collapsed;
                                    splint_buyLic.Visibility = Visibility.Collapsed;

                                    popupbox_Splint.IsEnabled = true;
                                    splint_selectPath.Visibility = Visibility.Collapsed;
                                    splint_download.Visibility = Visibility.Collapsed;
                                    splint_open.Visibility = Visibility.Visible;
                                    splint_unInstall.Visibility = Visibility.Visible;
                                    SoftwareFilterSplint.IsEnabled = true;
                                    splint_update.Foreground = this.FindResource("Common_DarkBrown") as SolidColorBrush;
                                    splint_update.Content = TranslationSource.Instance["LatestVer"];
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
                                    guide_demo.Visibility = Visibility.Collapsed;
                                    guide_troubleShooting.Visibility = Visibility.Collapsed;
                                    guide_buyLic.Visibility = Visibility.Collapsed;

                                    popupbox_Guide.IsEnabled = true;
                                    guide_selectPath.Visibility = Visibility.Collapsed;
                                    guide_download.Visibility = Visibility.Collapsed;
                                    guide_open.Visibility = Visibility.Visible;
                                    guide_unInstall.Visibility = Visibility.Visible;
                                    guide_update.Foreground = this.FindResource("Common_DarkBrown") as SolidColorBrush;
                                    guide_update.Content = TranslationSource.Instance["LatestVer"];
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
            switch (currentProgress)
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
        }
        /// <summary>
        /// 設定各軟體顯示的版本號
        /// </summary>
        /// <param name="softwareID">(軟體ID) EZCAD、Implant、Ortho、Tray、Splint</param>
        /// <param name="SoftwareStatus">設定開啟軟體或更新軟體Button</param>
        /// <param name="SoftwareVersion">版本號</param>
        /// <returns></returns>
        private void Handler_setSoftwareVersion(int softwareID, int SoftwareStatus, string SoftwareVersion)
        {
            switch(SoftwareStatus)
            {
                case (int)_softwareStatus.Installed:    //開啟軟體button
                    {
                        switch(softwareID)
                        {
                            case (int)_softwareID.EZCAD:
                                {
                                    cad_open.Content = TranslationSource.Instance["Open_EZCAD"] + "(" + SoftwareVersion + ")";
                                    break;
                                }
                            case (int)_softwareID.Implant:
                                {
                                    implant_open.Content = TranslationSource.Instance["Open_Implant"] + "(" + SoftwareVersion + ")";
                                    break;
                                }
                            case (int)_softwareID.Ortho:
                                {
                                    ortho_open.Content = TranslationSource.Instance["Open_Ortho"] + "(" + SoftwareVersion + ")";
                                    break;
                                }
                            case (int)_softwareID.Tray:
                                {
                                    tray_open.Content = TranslationSource.Instance["Open_Tray"] + "(" + SoftwareVersion + ")";
                                    break;
                                }
                            case (int)_softwareID.Splint:
                                {
                                    splint_open.Content = TranslationSource.Instance["Open_Splint"] + "(" + SoftwareVersion + ")";
                                    break;
                                }
                            case (int)_softwareID.Guide:
                                {
                                    guide_open.Content = TranslationSource.Instance["Open_Guide"] + "(" + SoftwareVersion + ")";
                                    break;
                                }
                        }
                        break;
                    }
                case (int)_softwareStatus.Updating: //更新軟體button
                    {
                        switch (softwareID)
                        {
                            case (int)_softwareID.EZCAD:
                                {
                                    if(((string)cad_update.Content).IndexOf(")") == -1)
                                        cad_update.Content = TranslationSource.Instance["Updating"];
                                    break;
                                }
                            case (int)_softwareID.Implant:
                                {
                                    if (((string)implant_update.Content).IndexOf(")") == -1)
                                        implant_update.Content = TranslationSource.Instance["Updating"];
                                    break;
                                }
                            case (int)_softwareID.Ortho:
                                {
                                    if (((string)ortho_update.Content).IndexOf(")") == -1)
                                        ortho_update.Content = TranslationSource.Instance["Updating"];
                                    break;
                                }
                            case (int)_softwareID.Tray:
                                {
                                    if (((string)tray_update.Content).IndexOf(")") == -1)
                                        tray_update.Content = TranslationSource.Instance["Updating"];
                                    break;
                                }
                            case (int)_softwareID.Splint:
                                {
                                    if (((string)splint_update.Content).IndexOf(")") == -1)
                                        splint_update.Content = TranslationSource.Instance["Updating"];
                                    break;
                                }
                            case (int)_softwareID.Guide:
                                {
                                    if (((string)guide_update.Content).IndexOf(")") == -1)
                                        guide_update.Content = TranslationSource.Instance["Updating"];
                                    break;
                                }
                        }
                        break;
                    }
            }
        }
        /// <summary>
        /// 設定各單機軟體更新Button的isEnable狀態
        /// </summary>
        /// <param name="SoftwareID">參考_softwareID</param>
        /// <param name="canUpdate">isEnable開關</param>
        /// <param name="SoftwareVersion">最新軟體版本號</param>
        private void Handler_SetSoftwareUpdateButtonStatus(int SoftwareID,bool canUpdate, string SoftwareVersion)
        {
            switch(SoftwareID)
            {
                case (int)_softwareID.EZCAD:
                    {
                        cad_update.IsEnabled = canUpdate;
                        if (cad_update.IsEnabled == true)
                        {
                            updateimage_EZCAD.Visibility = Visibility.Visible;
                            cad_update.FontWeight = FontWeights.Bold;
                            cad_update.Foreground = Brushes.Red;
                            cad_update.Content = TranslationSource.Instance["SoftwareUpdate"] + "(" + SoftwareVersion + ")";
                        }   
                        else
                        {
                            updateimage_EZCAD.Visibility = Visibility.Hidden;
                            cad_update.FontWeight = FontWeights.Normal;
                            cad_update.Content = TranslationSource.Instance["LatestVer"];
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
                            implant_update.Content = TranslationSource.Instance["SoftwareUpdate"] + "(" + SoftwareVersion + ")";
                        }   
                        else
                        {
                            updateimage_Implant.Visibility = Visibility.Hidden;
                            implant_update.FontWeight = FontWeights.Normal;
                            implant_update.Content = TranslationSource.Instance["LatestVer"];
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
                            ortho_update.Content = TranslationSource.Instance["SoftwareUpdate"] + "(" + SoftwareVersion + ")";
                        }   
                        else
                        {
                            updateimage_Ortho.Visibility = Visibility.Hidden;
                            ortho_update.FontWeight = FontWeights.Normal;
                            ortho_update.Content = TranslationSource.Instance["LatestVer"];
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
                            tray_update.Content = TranslationSource.Instance["SoftwareUpdate"] + "(" + SoftwareVersion + ")";
                        }
                        else
                        {
                            updateimage_Tray.Visibility = Visibility.Hidden;
                            tray_update.FontWeight = FontWeights.Normal;
                            tray_update.Content = TranslationSource.Instance["LatestVer"];
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
                            splint_update.Content = TranslationSource.Instance["LatestVer"];
                            splint_update.Content = TranslationSource.Instance["SoftwareUpdate"] + "(" + SoftwareVersion + ")";
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
                            guide_update.Content = TranslationSource.Instance["LatestVer"];
                            guide_update.Content = TranslationSource.Instance["SoftwareUpdate"] + "(" + SoftwareVersion + ")";
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
        public void OrderEventHandlerFunction_StatusUpdated(object sender, EventArgs e)
        {
            if (sender is V2Implant.ImplantV2NewOrder wnewoder)
            {
                string dirPath = Properties.OrderManagerProps.Default.implant_projectDirectory + wnewoder.m_order_num.Text;
                string Args = "\"readct\" \"" + dirPath + "\"";
                OrderManagerFunc.RunCommandLine(Properties.Settings.Default.implant_exePath, Args);
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

            if(enable == false)
            {
                cad_update.IsEnabled = enable;
                implant_update.IsEnabled = enable;
                ortho_update.IsEnabled = enable;
                tray_update.IsEnabled = enable;
                splint_update.IsEnabled = enable;
                guide_update.IsEnabled = enable;
            }
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
                SnackBarShow(TranslationSource.Instance["StartDownloading"]);
                UpdateFunc.StartDownloadSoftware();
            }
            else
            {
                SnackBarShow(TranslationSource.Instance["CannotGetexePath"]);
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
            log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "Handler_ShowBeforeDownload()", "Start");
            if (DialogBeforeDownload.SetInformation() == true)
            {
                log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "DialogBeforeDownload.SetInformation()", "result is true");
                SetBlur(true);

                DialogBeforeDownload.Owner = this;
                DialogBeforeDownload.ShowActivated = true;
                DialogBeforeDownload.ShowDialog();
                if(DialogBeforeDownload.DialogResult == true)
                {
                    SnackBarShow(TranslationSource.Instance["StartDownloading"]);
                    DownloadStart = true;
                }
                else
                    SetAllSoftwareTableDownloadisEnable(true);

                SetBlur(false);
            }

            if (DownloadStart == true)
                UpdateFunc.StartDownloadSoftware();
        }
        private void GoToSetting(int softwareID)
        {
            SetBlur(true);
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
                OrderManagerFunc.DoubleCheckEXEexist(true);
                usercontrolUserDetail.RefreshData();    //usergroup是多國語系

                ChooseToLoadProj();
                if (SoftwareFilterCAD.IsChecked == false && SoftwareFilterImplant.IsChecked == false &&
                SoftwareFilterOrtho.IsChecked == false && SoftwareFilterTray.IsChecked == false &&
                SoftwareFilterSplint.IsChecked == false)
                {
                    ChangeSoftwareFilter();
                }
                if(tabitem_Cloud.IsEnabled == false)
                    tabitem_Cloud.ToolTip = TranslationSource.Instance["PleaseLoginFirst"];
                log.RecordConfigLog("Click_FunctionTable_Setting()", "Config changed");
                ChangeSoftwareFilter();
            }
            SetBlur(false);
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

        private void MouseUp_progressbar(object sender, MouseButtonEventArgs e)
        {
            if (sender is ProgressBar pbar)
            {
                if (isInstalling == false)
                    return;

                switch (pbar.Name)
                {
                    case "progressbar_EZCAD_Installing":
                        {
                            if (File.Exists(Properties.Settings.Default.cad_exePath + @"Bin\EZCAD.exe") == true)
                            {
                                Properties.Settings.Default.cad_exePath += @"Bin\EZCAD.exe";
                                Properties.Settings.Default.Save();
                                FileVersionInfo verInfo = FileVersionInfo.GetVersionInfo(Properties.Settings.Default.cad_exePath);
                                bool isOldVer = false;
                                foreach (UpdateFunction.SoftwareInfo info in UpdateFunc.CloudSoftwareTotal)
                                {
                                    if (info.softwareID != (int)_softwareID.EZCAD)
                                        continue;
                                    else if (info.softwareVersion < new Version(verInfo.FileVersion))
                                    {
                                        isOldVer = true;
                                        break;
                                    }
                                }
                                if (isOldVer == false)
                                {
                                    Handler_setSoftwareShow((int)_softwareID.EZCAD, (int)_softwareStatus.Installed, 0);
                                    string snackStr = TranslationSource.Instance["Install"] + " " + OrderManagerFunc.GetSoftwareName(_softwareID.EZCAD)
                                    + " " + TranslationSource.Instance["Successfully"];
                                    SnackBarShow(snackStr);
                                    OrderManagerFunc.AutoDetectEXE((int)_classFrom.MainWindow);
                                    isInstalling = false;
                                    ChangeSoftwareFilter();
                                }
                            }
                            break;
                        }
                    case "progressbar_Implant_Installing":
                        {
                            if (File.Exists(Properties.Settings.Default.implant_exePath + @"ImplantPlanning.exe") == true)
                            {
                                Properties.Settings.Default.implant_exePath += @"ImplantPlanning.exe";
                                Properties.Settings.Default.Save();
                                FileVersionInfo verInfo = FileVersionInfo.GetVersionInfo(Properties.Settings.Default.implant_exePath);
                                bool isOldVer = false;
                                foreach (UpdateFunction.SoftwareInfo info in UpdateFunc.CloudSoftwareTotal)
                                {
                                    if (info.softwareID != (int)_softwareID.Implant)
                                        continue;
                                    else if (info.softwareVersion < new Version(verInfo.FileVersion))
                                    {
                                        isOldVer = true;
                                        break;
                                    }
                                }
                                if (isOldVer == false)
                                {
                                    Handler_setSoftwareShow((int)_softwareID.Implant, (int)_softwareStatus.Installed, 0);
                                    string snackStr = TranslationSource.Instance["Install"] + " " + OrderManagerFunc.GetSoftwareName(_softwareID.Implant)
                                    + " " + TranslationSource.Instance["Successfully"];
                                    SnackBarShow(snackStr);
                                    OrderManagerFunc.AutoDetectEXE((int)_classFrom.MainWindow);
                                    isInstalling = false;
                                    ChangeSoftwareFilter();
                                }
                            }
                            break;
                        }
                    case "progressbar_Ortho_Installing":
                        {
                            if (File.Exists(Properties.Settings.Default.ortho_exePath + @"OrthoAnalysis.exe") == true)
                            {
                                Properties.Settings.Default.ortho_exePath += @"OrthoAnalysis.exe";
                                Properties.Settings.Default.Save();
                                FileVersionInfo verInfo = FileVersionInfo.GetVersionInfo(Properties.Settings.Default.ortho_exePath);
                                bool isOldVer = false;
                                foreach (UpdateFunction.SoftwareInfo info in UpdateFunc.CloudSoftwareTotal)
                                {
                                    if (info.softwareID != (int)_softwareID.Ortho)
                                        continue;
                                    else if (info.softwareVersion < new Version(verInfo.FileVersion))
                                    {
                                        isOldVer = true;
                                        break;
                                    }
                                }
                                if (isOldVer == false)
                                {
                                    Handler_setSoftwareShow((int)_softwareID.Ortho, (int)_softwareStatus.Installed, 0);
                                    string snackStr = TranslationSource.Instance["Install"] + " " + OrderManagerFunc.GetSoftwareName(_softwareID.Ortho)
                                    + " " + TranslationSource.Instance["Successfully"];
                                    SnackBarShow(snackStr);
                                    OrderManagerFunc.AutoDetectEXE((int)_classFrom.MainWindow);
                                    isInstalling = false;
                                    ChangeSoftwareFilter();
                                }
                            }
                            break;
                        }
                    case "progressbar_Tray_Installing":
                        {
                            if (File.Exists(Properties.Settings.Default.tray_exePath + @"Bin\EZCAD.tray.exe") == true)
                            {
                                Properties.Settings.Default.tray_exePath += @"Bin\EZCAD.tray.exe";
                                Properties.Settings.Default.Save();
                                FileVersionInfo verInfo = FileVersionInfo.GetVersionInfo(Properties.Settings.Default.tray_exePath);
                                bool isOldVer = false;
                                foreach (UpdateFunction.SoftwareInfo info in UpdateFunc.CloudSoftwareTotal)
                                {
                                    if (info.softwareID != (int)_softwareID.Tray)
                                        continue;
                                    else if (info.softwareVersion < new Version(verInfo.FileVersion))
                                    {
                                        isOldVer = true;
                                        break;
                                    }
                                }
                                if (isOldVer == false)
                                {
                                    Handler_setSoftwareShow((int)_softwareID.Tray, (int)_softwareStatus.Installed, 0);
                                    string snackStr = TranslationSource.Instance["Install"] + " " + OrderManagerFunc.GetSoftwareName(_softwareID.Tray)
                                    + " " + TranslationSource.Instance["Successfully"];
                                    SnackBarShow(snackStr);
                                    OrderManagerFunc.AutoDetectEXE((int)_classFrom.MainWindow);
                                    isInstalling = false;
                                    ChangeSoftwareFilter();
                                }
                            }
                            break;
                        }
                    case "progressbar_Splint_Installing":
                        {
                            if (File.Exists(Properties.Settings.Default.splint_exePath + @"Bin\EZCAD.splint.exe") == true)
                            {
                                Properties.Settings.Default.splint_exePath += @"Bin\EZCAD.splint.exe";
                                Properties.Settings.Default.Save();
                                FileVersionInfo verInfo = FileVersionInfo.GetVersionInfo(Properties.Settings.Default.splint_exePath);
                                bool isOldVer = false;
                                foreach (UpdateFunction.SoftwareInfo info in UpdateFunc.CloudSoftwareTotal)
                                {
                                    if (info.softwareID != (int)_softwareID.Splint)
                                        continue;
                                    else if (info.softwareVersion < new Version(verInfo.FileVersion))
                                    {
                                        isOldVer = true;
                                        break;
                                    }
                                }
                                if (isOldVer == false)
                                {
                                    Handler_setSoftwareShow((int)_softwareID.Splint, (int)_softwareStatus.Installed, 0);
                                    string snackStr = TranslationSource.Instance["Install"] + " " + OrderManagerFunc.GetSoftwareName(_softwareID.Splint)
                                    + " " + TranslationSource.Instance["Successfully"];
                                    SnackBarShow(snackStr);
                                    OrderManagerFunc.AutoDetectEXE((int)_classFrom.MainWindow);
                                    isInstalling = false;
                                    ChangeSoftwareFilter();
                                }
                            }
                            break;
                        }
                    case "progressbar_Guide_Installing":
                        {
                            if (File.Exists(Properties.Settings.Default.guide_exePath + @"Bin\EZCAD.guide.exe") == true)
                            {
                                Properties.Settings.Default.guide_exePath += @"Bin\EZCAD.guide.exe";
                                Properties.Settings.Default.Save();
                                FileVersionInfo verInfo = FileVersionInfo.GetVersionInfo(Properties.Settings.Default.guide_exePath);
                                bool isOldVer = false;
                                foreach (UpdateFunction.SoftwareInfo info in UpdateFunc.CloudSoftwareTotal)
                                {
                                    if (info.softwareID != (int)_softwareID.Guide)
                                        continue;
                                    else if (info.softwareVersion < new Version(verInfo.FileVersion))
                                    {
                                        isOldVer = true;
                                        break;
                                    }
                                }
                                if (isOldVer == false)
                                {
                                    Handler_setSoftwareShow((int)_softwareID.Guide, (int)_softwareStatus.Installed, 0);
                                    string snackStr = TranslationSource.Instance["Install"] + " " + OrderManagerFunc.GetSoftwareName(_softwareID.Guide)
                                    + " " + TranslationSource.Instance["Successfully"];
                                    SnackBarShow(snackStr);
                                    OrderManagerFunc.AutoDetectEXE((int)_classFrom.MainWindow);
                                    isInstalling = false;
                                    ChangeSoftwareFilter();
                                }
                            }
                            break;
                        }
                }
            }
            else if (sender is Rectangle rect)
            {
                if (isInstalling == false)
                    return;

                switch (rect.Name)
                {
                    case "mask2_EZCAD_Installing":
                        {
                            if (File.Exists(Properties.Settings.Default.cad_exePath + @"Bin\EZCAD.exe") == true)
                            {
                                Properties.Settings.Default.cad_exePath += @"Bin\EZCAD.exe";
                                Properties.Settings.Default.Save();
                                FileVersionInfo verInfo = FileVersionInfo.GetVersionInfo(Properties.Settings.Default.cad_exePath);
                                bool isOldVer = false;
                                foreach (UpdateFunction.SoftwareInfo info in UpdateFunc.CloudSoftwareTotal)
                                {
                                    if (info.softwareID != (int)_softwareID.EZCAD)
                                        continue;
                                    else if (info.softwareVersion < new Version(verInfo.FileVersion))
                                    {
                                        isOldVer = true;
                                        break;
                                    }
                                }
                                if (isOldVer == false)
                                {
                                    Handler_setSoftwareShow((int)_softwareID.EZCAD, (int)_softwareStatus.Installed, 0);
                                    string snackStr = TranslationSource.Instance["Install"] + " " + OrderManagerFunc.GetSoftwareName(_softwareID.EZCAD)
                                    + " " + TranslationSource.Instance["Successfully"];
                                    SnackBarShow(snackStr);
                                    OrderManagerFunc.AutoDetectEXE((int)_classFrom.MainWindow);
                                    isInstalling = false;
                                    ChangeSoftwareFilter();
                                }
                            }
                            break;
                        }
                    case "mask2_Implant_Installing":
                        {
                            if (File.Exists(Properties.Settings.Default.implant_exePath + @"ImplantPlanning.exe") == true)
                            {
                                Properties.Settings.Default.implant_exePath += @"ImplantPlanning.exe";
                                Properties.Settings.Default.Save();
                                FileVersionInfo verInfo = FileVersionInfo.GetVersionInfo(Properties.Settings.Default.implant_exePath);
                                bool isOldVer = false;
                                foreach (UpdateFunction.SoftwareInfo info in UpdateFunc.CloudSoftwareTotal)
                                {
                                    if (info.softwareID != (int)_softwareID.Implant)
                                        continue;
                                    else if (info.softwareVersion < new Version(verInfo.FileVersion))
                                    {
                                        isOldVer = true;
                                        break;
                                    }
                                }
                                if (isOldVer == false)
                                {
                                    Handler_setSoftwareShow((int)_softwareID.Implant, (int)_softwareStatus.Installed, 0);
                                    string snackStr = TranslationSource.Instance["Install"] + " " + OrderManagerFunc.GetSoftwareName(_softwareID.Implant)
                                    + " " + TranslationSource.Instance["Successfully"];
                                    SnackBarShow(snackStr);
                                    OrderManagerFunc.AutoDetectEXE((int)_classFrom.MainWindow);
                                    isInstalling = false;
                                    ChangeSoftwareFilter();
                                }
                            }
                            break;
                        }
                    case "mask2_Ortho_Installing":
                        {
                            if (File.Exists(Properties.Settings.Default.ortho_exePath + @"OrthoAnalysis.exe") == true)
                            {
                                Properties.Settings.Default.ortho_exePath += @"OrthoAnalysis.exe";
                                Properties.Settings.Default.Save();
                                FileVersionInfo verInfo = FileVersionInfo.GetVersionInfo(Properties.Settings.Default.ortho_exePath);
                                bool isOldVer = false;
                                foreach (UpdateFunction.SoftwareInfo info in UpdateFunc.CloudSoftwareTotal)
                                {
                                    if (info.softwareID != (int)_softwareID.Ortho)
                                        continue;
                                    else if (info.softwareVersion < new Version(verInfo.FileVersion))
                                    {
                                        isOldVer = true;
                                        break;
                                    }
                                }
                                if (isOldVer == false)
                                {
                                    Handler_setSoftwareShow((int)_softwareID.Ortho, (int)_softwareStatus.Installed, 0);
                                    string snackStr = TranslationSource.Instance["Install"] + " " + OrderManagerFunc.GetSoftwareName(_softwareID.Ortho)
                                    + " " + TranslationSource.Instance["Successfully"];
                                    SnackBarShow(snackStr);
                                    OrderManagerFunc.AutoDetectEXE((int)_classFrom.MainWindow);
                                    isInstalling = false;
                                    ChangeSoftwareFilter();
                                }
                            }
                            break;
                        }
                    case "mask2_Tray_Installing":
                        {
                            if (File.Exists(Properties.Settings.Default.tray_exePath + @"Bin\EZCAD.tray.exe") == true)
                            {
                                Properties.Settings.Default.tray_exePath += @"Bin\EZCAD.tray.exe";
                                Properties.Settings.Default.Save();
                                FileVersionInfo verInfo = FileVersionInfo.GetVersionInfo(Properties.Settings.Default.tray_exePath);
                                bool isOldVer = false;
                                foreach (UpdateFunction.SoftwareInfo info in UpdateFunc.CloudSoftwareTotal)
                                {
                                    if (info.softwareID != (int)_softwareID.Tray)
                                        continue;
                                    else if (info.softwareVersion < new Version(verInfo.FileVersion))
                                    {
                                        isOldVer = true;
                                        break;
                                    }
                                }
                                if (isOldVer == false)
                                {
                                    Handler_setSoftwareShow((int)_softwareID.Tray, (int)_softwareStatus.Installed, 0);
                                    string snackStr = TranslationSource.Instance["Install"] + " " + OrderManagerFunc.GetSoftwareName(_softwareID.Tray)
                                    + " " + TranslationSource.Instance["Successfully"];
                                    SnackBarShow(snackStr);
                                    OrderManagerFunc.AutoDetectEXE((int)_classFrom.MainWindow);
                                    isInstalling = false;
                                    ChangeSoftwareFilter();
                                }
                            }
                            break;
                        }
                    case "mask2_Splint_Installing":
                        {
                            if (File.Exists(Properties.Settings.Default.splint_exePath + @"Bin\EZCAD.splint.exe") == true)
                            {
                                Properties.Settings.Default.splint_exePath += @"Bin\EZCAD.splint.exe";
                                Properties.Settings.Default.Save();
                                FileVersionInfo verInfo = FileVersionInfo.GetVersionInfo(Properties.Settings.Default.splint_exePath);
                                bool isOldVer = false;
                                foreach (UpdateFunction.SoftwareInfo info in UpdateFunc.CloudSoftwareTotal)
                                {
                                    if (info.softwareID != (int)_softwareID.Splint)
                                        continue;
                                    else if (info.softwareVersion < new Version(verInfo.FileVersion))
                                    {
                                        isOldVer = true;
                                        break;
                                    }
                                }
                                if (isOldVer == false)
                                {
                                    Handler_setSoftwareShow((int)_softwareID.Splint, (int)_softwareStatus.Installed, 0);
                                    string snackStr = TranslationSource.Instance["Install"] + " " + OrderManagerFunc.GetSoftwareName(_softwareID.Splint)
                                    + " " + TranslationSource.Instance["Successfully"];
                                    SnackBarShow(snackStr);
                                    OrderManagerFunc.AutoDetectEXE((int)_classFrom.MainWindow);
                                    isInstalling = false;
                                    ChangeSoftwareFilter();
                                }
                            }
                            break;
                        }
                    case "mask2_Guide_Installing":
                        {
                            if (File.Exists(Properties.Settings.Default.guide_exePath + @"Bin\EZCAD.guide.exe") == true)
                            {
                                Properties.Settings.Default.guide_exePath += @"Bin\EZCAD.guide.exe";
                                Properties.Settings.Default.Save();
                                FileVersionInfo verInfo = FileVersionInfo.GetVersionInfo(Properties.Settings.Default.guide_exePath);
                                bool isOldVer = false;
                                foreach (UpdateFunction.SoftwareInfo info in UpdateFunc.CloudSoftwareTotal)
                                {
                                    if (info.softwareID != (int)_softwareID.Guide)
                                        continue;
                                    else if (info.softwareVersion < new Version(verInfo.FileVersion))
                                    {
                                        isOldVer = true;
                                        break;
                                    }
                                }
                                if (isOldVer == false)
                                {
                                    Handler_setSoftwareShow((int)_softwareID.Guide, (int)_softwareStatus.Installed, 0);
                                    string snackStr = TranslationSource.Instance["Install"] + " " + OrderManagerFunc.GetSoftwareName(_softwareID.Guide)
                                    + " " + TranslationSource.Instance["Successfully"];
                                    SnackBarShow(snackStr);
                                    OrderManagerFunc.AutoDetectEXE((int)_classFrom.MainWindow);
                                    isInstalling = false;
                                    ChangeSoftwareFilter();
                                }
                            }
                            break;
                        }
                }
            }
        }
        private void Handler_SetOMUpate()
        {
            updateimage_Setting.Visibility = Visibility.Visible;
            SnackBarShow(TranslationSource.Instance["omCanUpdate"]);
        }
#endregion

#region FilterTable事件
        /// <summary>
        /// F5重新整理專案
        /// </summary>
        private void ChooseToLoadProj()
        {
            //本地端
            if (SoftwareFilterCAD.IsChecked == true)
                ProjHandle.LoadEZCADProj();
            else if (SoftwareFilterImplant.IsChecked == true)
                ProjHandle.LoadImplantProjV2();
            else if (SoftwareFilterOrtho.IsChecked == true)
                ProjHandle.LoadOrthoProj();
            else if (SoftwareFilterTray.IsChecked == true)
                ProjHandle.LoadTrayProj();
            else if (SoftwareFilterSplint.IsChecked == true)
                ProjHandle.LoadSplintProj();
            else
                StackPanel_Local.Children.Clear();

            //AirDental端
            if (SoftwareFilterCAD.IsChecked == true && loginStatus == true)
                AirDentalProjHandle.ReceiveCADProjects();
            else if (SoftwareFilterImplant.IsChecked == true && loginStatus == true)
                AirDentalProjHandle.ReceiveImplantProjects();
            else if (SoftwareFilterOrtho.IsChecked == true && loginStatus == true)
                AirDentalProjHandle.ReceiveOrthoProjects();
            else
                StackPanel_Cloud.Children.Clear();
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
                StackPanel_Cloud.Children.Clear();
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
                    case "DateFilterTW":
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
                            Properties.Settings.Default.LastSoftwareFilter = (int)_softwareID.EZCAD;
                            ProjHandle.LoadEZCADProj();
                            if (loginStatus == true)
                                AirDentalProjHandle.ReceiveCADProjects();
                            break;
                        }
                    case "SoftwareFilterImplant":
                        {
                            Properties.Settings.Default.LastSoftwareFilter = (int)_softwareID.Implant;
                            ProjHandle.LoadImplantProjV2();
                            if (loginStatus == true)
                                AirDentalProjHandle.ReceiveImplantProjects();
                            break;
                        }
                    case "SoftwareFilterOrtho":
                        {
                            Properties.Settings.Default.LastSoftwareFilter = (int)_softwareID.Ortho;
                            ProjHandle.LoadOrthoProj();
                            if (loginStatus == true)
                                AirDentalProjHandle.ReceiveOrthoProjects();
                            break;
                        }
                    case "SoftwareFilterTray":
                        {
                            Properties.Settings.Default.LastSoftwareFilter = (int)_softwareID.Tray;
                            ProjHandle.LoadTrayProj();
                            Handler_SetCaseShow_Airdental((int)_softwareID.Tray);
                            break;
                        }
                    case "SoftwareFilterSplint":
                        {
                            Properties.Settings.Default.LastSoftwareFilter = (int)_softwareID.Splint;
                            ProjHandle.LoadSplintProj();
                            Handler_SetCaseShow_Airdental((int)_softwareID.Splint);
                            break;
                        }
                }
                Properties.Settings.Default.Save();
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
        /// <summary>
        /// 由程式判斷並切換顯示哪個軟體Case(目前用到只有Loaded和刪除軟體時)
        /// </summary>
        private void ChangeSoftwareFilter()
        {
            if (SoftwareFilterCAD.IsEnabled == false && SoftwareFilterImplant.IsEnabled == false && SoftwareFilterOrtho.IsEnabled == false && SoftwareFilterTray.IsEnabled == false && SoftwareFilterSplint.IsEnabled == false)
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
#endregion

#region CaseTable_Local事件
        /// <summary>
        /// 顯示Case
        /// </summary>
        /// <param name="SoftwareID"></param>
        public void Handler_SetCaseShow(int SoftwareID)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                StackPanel_Local.Children.Clear();
                StackPanel_Detail.Children.Clear();
                switch (SoftwareID)
                {
                    case (int)_softwareID.EZCAD:
                        {
                            if (ProjHandle.Caselist_EZCAD != null && ProjHandle.Caselist_EZCAD.Count > 0)
                            {
                                int countIndex = 0;
                                foreach (CadInformation cadInfo in ProjHandle.Caselist_EZCAD)
                                {
                                    Local_UserControls.Order_cadBase Order_CAD = new Local_UserControls.Order_cadBase();
                                    Order_CAD.SetBaseProjectShow += CaseHandler_EZCAD_showSingleProject;
                                    Order_CAD.SetCaseInfo(cadInfo, countIndex);
                                    StackPanel_Local.Children.Add(Order_CAD);
                                    countIndex++;
                                }
                                Watcher_CaseProject(new FileSystemWatcher(), Properties.OrderManagerProps.Default.cad_projectDirectory);
                            }
                            else
                                StackPanel_Local.Children.Add(new Local_UserControls.NoResult());
                            break;
                        }
                    case (int)_softwareID.Implant:
                        {
                            int countIndex = 0;
                            if (ProjHandle.Caselist_ImplantOuterCase != null && ProjHandle.Caselist_ImplantOuterCase.Count > 0)
                            {
                                foreach (ImplantOuterInformation implantInfo in ProjHandle.Caselist_ImplantOuterCase)
                                {
                                    Local_UserControls.Order_implantBase Order_Implant = new Local_UserControls.Order_implantBase();
                                    Order_Implant.SetBaseProjectShow += CaseHandler_Implant_showSingleProject;
                                    Order_Implant.SetSmallProjectDetailShow += CaseHandler_Implant_showDetail;
                                    Order_Implant.SetCaseInfo(implantInfo, countIndex);
                                    StackPanel_Local.Children.Add(Order_Implant);
                                    countIndex++;
                                }
                                Watcher_CaseProject(new FileSystemWatcher(), Properties.OrderManagerProps.Default.implant_projectDirectory);
                            }
                            else
                                StackPanel_Local.Children.Add(new Local_UserControls.NoResult());
                            break;
                        }
                    case (int)_softwareID.Ortho:
                        {
                            int countIndex = 0;
                            if (ProjHandle.Caselist_OrthoOuterCase != null && ProjHandle.Caselist_OrthoOuterCase.Count > 0)
                            {
                                foreach (OrthoOuterInformation orthoInfo in ProjHandle.Caselist_OrthoOuterCase)
                                {
                                    Local_UserControls.Order_orthoBase Order_Ortho = new Local_UserControls.Order_orthoBase();
                                    Order_Ortho.SetBaseProjectShow += CaseHandler_Ortho_showSingleProject;
                                    Order_Ortho.SetSmallProjectDetailShow += CaseHandler_Ortho_showDetail;
                                    Order_Ortho.SetCaseInfo(orthoInfo, countIndex);
                                    StackPanel_Local.Children.Add(Order_Ortho);
                                    countIndex++;
                                }
                                Watcher_CaseProject(new FileSystemWatcher(), Properties.OrderManagerProps.Default.ortho_projectDirectory);
                            }
                            else
                                StackPanel_Local.Children.Add(new Local_UserControls.NoResult());
                            break;
                        }
                    case (int)_softwareID.Tray:
                        {
                            int countIndex = 0;
                            if (ProjHandle.Caselist_Tray != null && ProjHandle.Caselist_Tray.Count > 0)
                            {
                                foreach (TrayInformation trayInfo in ProjHandle.Caselist_Tray)
                                {
                                    Local_UserControls.Order_tsBase Order_Tray = new Local_UserControls.Order_tsBase();
                                    Order_Tray.SetBaseProjectShow += CaseHandler_TraySplint_showSingleProject;
                                    Order_Tray.SetTrayCaseInfo(trayInfo, countIndex);
                                    StackPanel_Local.Children.Add(Order_Tray);
                                    countIndex++;
                                }
                                Watcher_CaseProject(new FileSystemWatcher(), Properties.OrderManagerProps.Default.tray_projectDirectory);
                            }
                            else
                                StackPanel_Local.Children.Add(new Local_UserControls.NoResult());
                            break;
                        }
                    case (int)_softwareID.Splint:
                        {
                            int countIndex = 0;
                            if (ProjHandle.Caselist_Splint != null && ProjHandle.Caselist_Splint.Count > 0)
                            {
                                foreach (SplintInformation splintInfo in ProjHandle.Caselist_Splint)
                                {
                                    Local_UserControls.Order_tsBase Order_Splint = new Local_UserControls.Order_tsBase();
                                    Order_Splint.SetBaseProjectShow += CaseHandler_TraySplint_showSingleProject;
                                    Order_Splint.SetSplintCaseInfo(splintInfo, countIndex);
                                    StackPanel_Local.Children.Add(Order_Splint);
                                    countIndex++;
                                }
                                Watcher_CaseProject(new FileSystemWatcher(), Properties.OrderManagerProps.Default.splint_projectDirectory);
                            }
                            else
                                StackPanel_Local.Children.Add(new Local_UserControls.NoResult());
                            break;
                        }
                }
            }));
        }
        private void CaseHandler_EZCAD_showSingleProject(int projectIndex)
        {
            StackPanel_Detail.Children.Clear();
            if (((Local_UserControls.Order_cadBase)StackPanel_Local.Children[projectIndex]).IsFocusCase == false)
            {
                Local_UserControls.Detail_cad detail_cad = new Local_UserControls.Detail_cad();
                detail_cad.SetDetailInfo(ProjHandle.Caselist_EZCAD[projectIndex]);
                StackPanel_Detail.Children.Add(detail_cad);
            }
            for (int i = 0; i < StackPanel_Local.Children.Count; i++)
            {
                if (i == projectIndex)
                    continue;

                ((Local_UserControls.Order_cadBase)StackPanel_Local.Children[i]).SetCaseFocusStatus(false);
            }
        }
        private void CaseHandler_Implant_showSingleProject(int projectIndex)
        {
            StackPanel_Detail.Children.Clear();
            for (int i = 0; i < StackPanel_Local.Children.Count; i++)
            {
                if (i == projectIndex)
                    continue;

                ((Local_UserControls.Order_implantBase)StackPanel_Local.Children[i]).SetCaseFocusStatus(false);
            }
        }
        private void CaseHandler_Ortho_showSingleProject(int projectIndex)
        {
            StackPanel_Detail.Children.Clear();
            for (int i = 0; i < StackPanel_Local.Children.Count; i++)
            {
                if (i == projectIndex)
                    continue;

                ((Local_UserControls.Order_orthoBase)StackPanel_Local.Children[i]).SetCaseFocusStatus(false);
            }
        }
        private void CaseHandler_TraySplint_showSingleProject(int projectIndex)
        {
            StackPanel_Detail.Children.Clear();
            if (((Local_UserControls.Order_tsBase)StackPanel_Local.Children[projectIndex]).IsFocusCase == false)
            {
                if(((Local_UserControls.Order_tsBase)StackPanel_Local.Children[projectIndex]).trayInfo != null)
                {
                    Local_UserControls.Detail_traysplint detail_tray = new Local_UserControls.Detail_traysplint();
                    detail_tray.SetTrayDetailInfo(ProjHandle.Caselist_Tray[projectIndex]);
                    StackPanel_Detail.Children.Add(detail_tray);
                }
                else
                {
                    Local_UserControls.Detail_traysplint detail_splint = new Local_UserControls.Detail_traysplint();
                    detail_splint.SetSplintDetailInfo(ProjHandle.Caselist_Splint[projectIndex]);
                    StackPanel_Detail.Children.Add(detail_splint);
                }
            }
            for (int i = 0; i < StackPanel_Local.Children.Count; i++)
            {
                if (i == projectIndex)
                    continue;

                ((Local_UserControls.Order_tsBase)StackPanel_Local.Children[i]).SetCaseFocusStatus(false);
            }
        }
        private void CaseHandler_Implant_showDetail(int BaseCaseIndex, int SmallCaseIndex)
        {
            StackPanel_Detail.Children.Clear();
            if (StackPanel_Local.Children[BaseCaseIndex] is Local_UserControls.Order_implantBase)
            {
                if (((Local_UserControls.Order_implantBase)StackPanel_Local.Children[BaseCaseIndex]).stackpanel_Implant.Children[SmallCaseIndex + 1] is Local_UserControls.Order_ImplantSmallcase tmpImplnatSmall)
                {
                    if (tmpImplnatSmall.IsFocusSmallCase == false)
                    {
                        Local_UserControls.Detail_implantV2 detail_implant = new Local_UserControls.Detail_implantV2();
                        detail_implant.SetDetailInfo(((Local_UserControls.Order_implantBase)StackPanel_Local.Children[BaseCaseIndex]).implantInfo, tmpImplnatSmall.implantsmallcaseInfo);
                        StackPanel_Detail.Children.Add(detail_implant);
                    }
                }
            }
        }
        private void CaseHandler_Ortho_showDetail(int BaseCaseIndex, int SmallCaseIndex)
        {
            StackPanel_Detail.Children.Clear();
            if(StackPanel_Local.Children[BaseCaseIndex] is Local_UserControls.Order_orthoBase)
            {
                if (((Local_UserControls.Order_orthoBase)StackPanel_Local.Children[BaseCaseIndex]).stackpanel_Ortho.Children[SmallCaseIndex + 1] is Local_UserControls.Order_orthoSmallcase tmpOrthoSmall)
                {
                    if (tmpOrthoSmall.IsFocusSmallCase == false)
                    {
                        Local_UserControls.Detail_ortho detail_ortho = new Local_UserControls.Detail_ortho();
                        detail_ortho.SetDetailInfo(tmpOrthoSmall.orthosmallcaseInfo);
                        StackPanel_Detail.Children.Add(detail_ortho);
                    }
                }
            }
        }
#endregion

#region CaseTable_Airdental事件
        public void Handler_SetCaseShow_Airdental(int SoftwareID)
        {
            StackPanel_Cloud.Children.Clear();
            StackPanel_Detail.Children.Clear();
            switch(SoftwareID)
            {
                case (int)_softwareID.EZCAD:
                    {
                        if (AirDentalProjHandle.Projectlist_CAD != null && AirDentalProjHandle.Projectlist_CAD.Count > 0)
                        {
                            foreach (AirDental_UserControls.AirD_cadBase cadProject in AirDentalProjHandle.Projectlist_CAD)
                            {
                                StackPanel_Cloud.Children.Add(cadProject);
                            }
                        }
                        else
                            StackPanel_Cloud.Children.Add(new Local_UserControls.NoResult());
                        break;
                    }
                case (int)_softwareID.Implant:
                    {
                        if (AirDentalProjHandle.Projectlist_Implant != null && AirDentalProjHandle.Projectlist_Implant.Count > 0)
                        {
                            foreach (AirDental_UserControls.AirD_implantBase implantProject in AirDentalProjHandle.Projectlist_Implant)
                            {
                                StackPanel_Cloud.Children.Add(implantProject);
                            }
                        }
                        else
                            StackPanel_Cloud.Children.Add(new Local_UserControls.NoResult());
                        break;
                    }
                case (int)_softwareID.Ortho:
                    {
                        if (AirDentalProjHandle.Projectlist_Ortho != null && AirDentalProjHandle.Projectlist_Ortho.Count > 0)
                        {
                            foreach (AirDental_UserControls.AirD_orthoBase orthoProject in AirDentalProjHandle.Projectlist_Ortho)
                            {
                                StackPanel_Cloud.Children.Add(orthoProject);
                            }
                        }
                        else
                            StackPanel_Cloud.Children.Add(new Local_UserControls.NoResult());
                        break;
                    }
                case (int)_softwareID.Tray:
                    {
                        StackPanel_Cloud.Children.Add(new Local_UserControls.NoResult());
                        break;
                    }
                case (int)_softwareID.Splint:
                    {
                        StackPanel_Cloud.Children.Add(new Local_UserControls.NoResult());
                        break;
                    }
            }
        }
        private void CloudCaseHandler_Ortho_showSingleProject(int projectIndex)
        {
            StackPanel_Detail.Children.Clear();
            for (int i = 0; i < StackPanel_Cloud.Children.Count; i++)
            {
                if (i == projectIndex)
                    continue;

                ((AirDental_UserControls.AirD_orthoBase)StackPanel_Cloud.Children[i]).SetCaseFocusStatus(false);
            }
        }
        private void CloudCaseHandler_Implant_showSingleProject(int projectIndex)
        {
            StackPanel_Detail.Children.Clear();
            for (int i = 0; i < StackPanel_Cloud.Children.Count; i++)
            {
                if (i == projectIndex)
                    continue;

                ((AirDental_UserControls.AirD_implantBase)StackPanel_Cloud.Children[i]).SetCaseFocusStatus(false);
            }
        }
        private void CloudCaseHandler_CAD_showSingleProject(int projectIndex)
        {
            StackPanel_Detail.Children.Clear();
            for (int i = 0; i < StackPanel_Cloud.Children.Count; i++)
            {
                if (i == projectIndex)
                    continue;

                ((AirDental_UserControls.AirD_cadBase)StackPanel_Cloud.Children[i]).SetCaseFocusStatus(false);
            }
        }
        private void CloudCaseHandler_Ortho_showDetail(int BaseCaseIndex, int SmallCaseIndex)
        {
            StackPanel_Detail.Children.Clear();
            /*if (StackPanel_Local.Children[BaseCaseIndex] is Local_UserControls.Order_orthoBase)
            {
                if (((Local_UserControls.Order_orthoBase)StackPanel_Local.Children[BaseCaseIndex]).stackpanel_Ortho.Children[SmallCaseIndex + 1] is Local_UserControls.Order_orthoSmallcase tmpOrthoSmall)
                {
                    if (tmpOrthoSmall.IsFocusSmallCase == false)
                    {
                        Local_UserControls.Detail_ortho detail_ortho = new Local_UserControls.Detail_ortho();
                        detail_ortho.SetDetailInfo(tmpOrthoSmall.orthosmallcaseInfo);
                        StackPanel_Detail.Children.Add(detail_ortho);
                    }
                }
            }*/
        }
        private void CloudCaseHandler_Implant_showDetail(int BaseCaseIndex, int SmallCaseIndex)
        {
            StackPanel_Detail.Children.Clear();
            /*if (StackPanel_Local.Children[BaseCaseIndex] is Local_UserControls.Order_orthoBase)
            {
                if (((Local_UserControls.Order_orthoBase)StackPanel_Local.Children[BaseCaseIndex]).stackpanel_Ortho.Children[SmallCaseIndex + 1] is Local_UserControls.Order_orthoSmallcase tmpOrthoSmall)
                {
                    if (tmpOrthoSmall.IsFocusSmallCase == false)
                    {
                        Local_UserControls.Detail_ortho detail_ortho = new Local_UserControls.Detail_ortho();
                        detail_ortho.SetDetailInfo(tmpOrthoSmall.orthosmallcaseInfo);
                        StackPanel_Detail.Children.Add(detail_ortho);
                    }
                }
            }*/
        }
        private void CloudCaseHandler_CAD_showDetail(int BaseCaseIndex, int SmallCaseIndex)
        {
            StackPanel_Detail.Children.Clear();
            /*if (StackPanel_Local.Children[BaseCaseIndex] is Local_UserControls.Order_orthoBase)
            {
                if (((Local_UserControls.Order_orthoBase)StackPanel_Local.Children[BaseCaseIndex]).stackpanel_Ortho.Children[SmallCaseIndex + 1] is Local_UserControls.Order_orthoSmallcase tmpOrthoSmall)
                {
                    if (tmpOrthoSmall.IsFocusSmallCase == false)
                    {
                        Local_UserControls.Detail_ortho detail_ortho = new Local_UserControls.Detail_ortho();
                        detail_ortho.SetDetailInfo(tmpOrthoSmall.orthosmallcaseInfo);
                        StackPanel_Detail.Children.Add(detail_ortho);
                    }
                }
            }*/
        }
        #endregion
        
    }
}
