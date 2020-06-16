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
    /// AirD_orthoSmallOrder.xaml 的互動邏輯
    /// </summary>
    public partial class AirD_orthoSmallOrder : UserControl
    {
        //委派到Order_orthoBase.xaml.cs裡面的SmallCaseHandler()
        public delegate void orthoOrderEventHandler(int projectIndex);
        public event orthoOrderEventHandler SetOrderCaseShow;

        public bool IsFocusSmallCase;
        Dll_Airdental.Main._orthoOrder OrderInfo;
        int ItemIndex;

        public AirD_orthoSmallOrder()
        {
            InitializeComponent();
            ItemIndex = -1;
        }

        public void SetOrderInfo(Dll_Airdental.Main._orthoOrder Import, int Index)
        {
            OrderInfo = Import;
            label_ProjectName.Content = TranslationSource.Instance[Import._group] + Import._action + Import._stage;
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
