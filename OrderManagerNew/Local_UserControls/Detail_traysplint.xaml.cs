using System.Windows;
using System.Windows.Controls;
using TrayInformation = OrderManagerNew.Local_UserControls.Order_tsBase.TrayInformation;
using SplintInformation = OrderManagerNew.Local_UserControls.Order_tsBase.SplintInformation;

namespace OrderManagerNew.Local_UserControls
{
    /// <summary>
    /// Detail_traysplint.xaml 的互動邏輯
    /// </summary>
    public partial class Detail_traysplint : UserControl
    {
        private TrayInformation TrayInfo;
        private SplintInformation SplintInfo;

        public Detail_traysplint()
        {
            InitializeComponent();
            TrayInfo = null;
            SplintInfo = null;
            textbox_Order.Text = "";
            textbox_DesignStep.Text = "";
            textbox_Brand.Text = "";
            textbox_CreateDate.Text = "";
            textbox_ModifyDate.Text = "";
        }

        public void SetTrayDetailInfo(TrayInformation Import)
        {
            TrayInfo = Import;
            textbox_Order.Text = TrayInfo.OrderID;
            textbox_DesignStep.Text = TrayInfo.DesignStepString;
            textbox_Brand.Text = TrayInfo.Brand;
            textbox_CreateDate.Text = TrayInfo.CreateDate.ToLongDateString() + " " + TrayInfo.CreateDate.ToLongTimeString();
            textbox_ModifyDate.Text = TrayInfo.ModifyDate.ToLongDateString() + " " + TrayInfo.ModifyDate.ToLongTimeString();
        }

        public void SetSplintDetailInfo(SplintInformation Import)
        {
            SplintInfo = Import;
            textbox_Order.Text = SplintInfo.OrderID;
            textbox_DesignStep.Text = SplintInfo.DesignStepString;
            textbox_Brand.Text = SplintInfo.Brand;
            textbox_CreateDate.Text = SplintInfo.CreateDate.ToString(System.Globalization.CultureInfo.CreateSpecificCulture("en-US"));
            textbox_ModifyDate.Text = SplintInfo.ModifyDate.ToString(System.Globalization.CultureInfo.CreateSpecificCulture("en-US"));
        }

        private void Click_systemButton(object sender, RoutedEventArgs e)
        {
            if (sender is Button)
            {
                switch (((Button)sender).Name)
                {
                    case "button_loadProj":
                        {
                            if (TrayInfo != null && System.IO.File.Exists(TrayInfo.CaseXmlPath) == true)
                            {
                                OrderManagerFunctions omFunc = new OrderManagerFunctions();
                                omFunc.RunCommandLine(Properties.Settings.Default.tray_exePath, "-guiderpd \"" + TrayInfo.CaseXmlPath + "\"");
                            }
                            else if (SplintInfo != null && System.IO.File.Exists(SplintInfo.CaseXmlPath) == true)
                            {
                                OrderManagerFunctions omFunc = new OrderManagerFunctions();
                                omFunc.RunCommandLine(Properties.Settings.Default.splint_exePath, "-guiderpd \"" + SplintInfo.CaseXmlPath + "\"");
                            }
                            break;
                        }
                    case "button_openDir":
                        {
                            if (TrayInfo != null && System.IO.Directory.Exists(TrayInfo.CaseDirectoryPath) == true)
                            {
                                OrderManagerFunctions omFunc = new OrderManagerFunctions();
                                omFunc.RunCommandLine(Properties.OrderManagerProps.Default.systemDisk + @"Windows\explorer.exe", "\"" + TrayInfo.CaseDirectoryPath + "\"");
                            }
                            else if (SplintInfo != null && System.IO.Directory.Exists(SplintInfo.CaseDirectoryPath) == true)
                            {
                                OrderManagerFunctions omFunc = new OrderManagerFunctions();
                                omFunc.RunCommandLine(Properties.OrderManagerProps.Default.systemDisk + @"Windows\explorer.exe", "\"" + SplintInfo.CaseDirectoryPath + "\"");
                            }
                            break;
                        }
                }
            }
        }
    }
}
