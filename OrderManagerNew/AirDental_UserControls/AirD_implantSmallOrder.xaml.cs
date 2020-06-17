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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OrderManagerNew.AirDental_UserControls
{
    /// <summary>
    /// AirD_implantSmallOrder.xaml 的互動邏輯
    /// </summary>
    public partial class AirD_implantSmallOrder : UserControl
    {
        //委派到Order_orthoBase.xaml.cs裡面的SmallCaseHandler()
        public delegate void implantOrderEventHandler(int projectIndex);
        public event implantOrderEventHandler SetOrderCaseShow;

        public bool IsFocusSmallCase;
        Dll_Airdental.Main._implantOrder OrderInfo;
        int ItemIndex;

        public AirD_implantSmallOrder()
        {
            InitializeComponent();
        }

        public void SetOrderInfo(Dll_Airdental.Main._implantOrder Import, int Index)
        {
            OrderInfo = Import;
            if (Import._stageKey.IndexOf("guide_") == 0)
                Import._stageKey = Import._stageKey.Remove(0, 6);
            label_ProjectName.Content = TranslationSource.Instance[Import._group] + " " + TranslationSource.Instance[Import._actionKey] + TranslationSource.Instance[Import._stageKey];
            label_ProjectName.ToolTip = Import._date.DateTime.ToLongDateString() + Import._date.DateTime.ToLongTimeString();
            ItemIndex = Index;
        }

        private void Click_ButtonEvent(object sender, RoutedEventArgs e)
        {

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
                SetOrderCaseShow(ItemIndex);
                if (IsFocusSmallCase == false)
                {
                    SetCaseFocusStatus(true);
                }
                else
                {
                    SetCaseFocusStatus(false);
                }
            }
        }
    }
}
