using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace OrderManagerNew.V2Implant
{
    /// <summary>
    /// Interaction logic for UCToothTypeBase.xaml
    /// </summary>
    public partial class UCToothTypeBase : UserControl
    {
        public UCToothTypeBase()
        {
            InitializeComponent();

            ProductTypeIdx = 0;
            SendClickEvent = false;
        }

        public int ProductTypeIdx;

        public bool SendClickEvent { set; get; }

        public void SetImage(String path)
        {
            ToothTypeImage.BeginInit();
            ToothTypeImage.Source = new BitmapImage(new Uri(path, UriKind.RelativeOrAbsolute));
            ToothTypeImage.EndInit();
        }

        private void Base_MouseEnter(object sender, MouseEventArgs e)
        {

        }

        private void Base_MouseLeave(object sender, MouseEventArgs e)
        {

        }

        public event RoutedEventHandler ToothTypeBaseClick;

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(SendClickEvent)
            {
                ToothTypeBaseClick?.Invoke(this, null);
            }
        }
    }
}
