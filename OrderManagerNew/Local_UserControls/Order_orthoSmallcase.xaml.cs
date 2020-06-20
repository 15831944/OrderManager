using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace OrderManagerNew.Local_UserControls
{
    /// <summary>
    /// Order_orthoSmallcase.xaml 的互動邏輯
    /// </summary>
    public partial class Order_orthoSmallcase : UserControl
    {
        //委派到Order_orthoBase.xaml.cs裡面的SmallCaseHandler()
        public delegate void orthoSmallCaseEventHandler(int projectIndex);
        public event orthoSmallCaseEventHandler SetsmallCaseShow;
        public OrthoSmallCaseInformation orthosmallcaseInfo;
        public bool IsFocusSmallCase;
        private int ItemIndex;

        public class OrthoSmallCaseInformation
        {
            public string SmallCaseXmlPath { get; set; }
            public int WorkflowStep { get; set; }
            public string CreateDate { get; set; }
            public string Describe { get; set; }
            public DateTime ModifyTime { get; set; }

            public string ProductTypeString { get; set; }
            public string Name { get; set; }
            public string OrderID { get; set; }
            public string Gender { get; set; }
            public string Age { get; set; }
            public string Clinic { get; set; }
            public string Dentist { get; set; }

            public OrthoSmallCaseInformation()
            {
                SmallCaseXmlPath = "";
                WorkflowStep = -1;
                CreateDate = "";
                Describe = "";
                ModifyTime = new DateTime();

                ProductTypeString = "";
                Name = "";
                OrderID = "";
                Gender = "";
                Age = "";
                Clinic = "";
                Dentist = "";
            }
        }

        public Order_orthoSmallcase()
        {
            InitializeComponent();
            label_ProjectName.Content = "";
            button_LoadOrthoProject.IsEnabled = false;
            IsFocusSmallCase = false;
            ItemIndex = -1;
        }

        public void SetOrthoSmallCaseInfo(OrthoSmallCaseInformation Import, int Index)
        {
            orthosmallcaseInfo = Import;

            if (File.Exists(orthosmallcaseInfo.SmallCaseXmlPath) == true && File.Exists(Properties.Settings.Default.ortho_exePath) == true)
                button_LoadOrthoProject.IsEnabled = true;
            else
                button_LoadOrthoProject.IsEnabled = false;

            label_ProjectName.Content = orthosmallcaseInfo.CreateDate;
            ItemIndex = Index;
        }

        private void Click_ButtonEvent(object sender, RoutedEventArgs e)
        {
            OrderManagerFunctions omFunc = new OrderManagerFunctions();
            omFunc.RunCommandLine(Properties.Settings.Default.ortho_exePath, "-rp \"" + orthosmallcaseInfo.SmallCaseXmlPath + "\"");
        }

        /// <summary>
        /// 設定Case的Focus狀態
        /// </summary>
        /// <param name="isFocused">是否要Focus</param>
        public void SetCaseFocusStatus(bool isFocused)
        {
            switch (isFocused)
            {
                case true:
                    {
                        background_orthoSmallcase.Fill = this.FindResource("background_FocusedSmallCase") as SolidColorBrush;
                        background_orthoSmallcase.Stroke = this.FindResource("borderbrush_FocusedSmallCase") as SolidColorBrush;
                        IsFocusSmallCase = true;
                        break;
                    }
                case false:
                    {
                        background_orthoSmallcase.Fill = this.FindResource("background_SmallCase") as SolidColorBrush;
                        background_orthoSmallcase.Stroke = null;
                        IsFocusSmallCase = false;
                        break;
                    }
            }
        }

        private void PMDown_StackPanelMain(object sender, MouseButtonEventArgs e)
        {
            if (e.Source is Button)
            {
                Click_ButtonEvent(e.Source, e);
            }
            else
            {
                SetsmallCaseShow(ItemIndex);
                if (IsFocusSmallCase == false)
                {
                    SetCaseFocusStatus(true);
                }
                else
                {
                    SetCaseFocusStatus(false);
                }
            }
            e.Handled = true;
        }
    }
}
