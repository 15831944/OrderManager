using System;
using System.Collections.Generic;
using System.IO;
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
using ImplantOuterInformation = OrderManagerNew.UserControls.Order_implantBase.ImplantOuterInformation;
using ImplantSmallCaseInformation = OrderManagerNew.UserControls.Order_ImplantSmallcase.ImplantSmallCaseInformation;

namespace OrderManagerNew.UserControls
{
    /// <summary>
    /// Detail_implant.xaml 的互動邏輯
    /// </summary>
    public partial class Detail_implantV2 : UserControl
    {
        private ImplantOuterInformation implantOuterInfo;
        private ImplantSmallCaseInformation implantInfo;

        public Detail_implantV2()
        {
            InitializeComponent();
            implantInfo = new ImplantSmallCaseInformation();
            image_toothJPG.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// 設定Detail顯示資訊
        /// </summary>
        /// <param name="importOuter">要匯入的ImplantOuterInformation</param>
        /// <param name="Import">要匯入的ImplantSmallInformation</param>
        public void SetDetailInfo(ImplantOuterInformation importOuter, ImplantSmallCaseInformation Import)
        {
            implantOuterInfo = importOuter;
            implantInfo = Import;

            textbox_Order.Text = implantOuterInfo.OrderNumber;
            textbox_Patient.Text = implantOuterInfo.PatientName;
            textbox_Gender.Text = implantOuterInfo.Gender ? TranslationSource.Instance["Male"] : TranslationSource.Instance["Female"];
            if (implantOuterInfo.PatientBirth != new DateTime())
            {
                int patientAge = DateTime.Today.Year - implantOuterInfo.PatientBirth.Year;
                textbox_Age.Text = patientAge.ToString();
            }
            textbox_Clinic.Text = implantOuterInfo.Clinic;
            textbox_SurgicalGT.Text = implantOuterInfo.SurgicalGuide;
            textbox_SurgicalKit.Text = implantOuterInfo.Surgicalkit;

            string JpgPath = importOuter.XmlfilePath.Remove(importOuter.XmlfilePath.Length - 4) + ".jpg";
            if (File.Exists(JpgPath) == true)
            {
                image_toothJPG.BeginInit();
                image_toothJPG.Source = new BitmapImage(new Uri(JpgPath, UriKind.RelativeOrAbsolute));
                image_toothJPG.EndInit();
                image_toothJPG.Visibility = Visibility.Visible;
            }
            else
            {
                image_toothJPG.Visibility = Visibility.Hidden;
            }

            if (File.Exists(implantInfo.GuideModelPath) != true)
                button_openDir.IsEnabled = false;
            else
                button_openDir.IsEnabled = true;

            if (File.Exists(implantInfo.PDFpath) != true)
                button_openPDF.IsEnabled = false;
            else
                button_openPDF.IsEnabled = true;
        }

        private void Click_systemButton(object sender, RoutedEventArgs e)
        {
            if (sender is Button)
            {
                OrderManagerFunctions omFunc = new OrderManagerFunctions();
                switch (((Button)sender).Name)
                {
                    case "button_loadImp":
                        {
                            omFunc.RunCommandLine(Properties.Settings.Default.implant_exePath, "\"readdii\" \"" + implantInfo.ImplantTiiPath + "\"");
                            break;
                        }
                    case "button_loadGuide":
                        {
                            string gmlFile = implantInfo.GuideCaseDir + implantInfo.OrderName + "-Guide.gml";
                            string lmgFile = implantInfo.GuideCaseDir + implantInfo.OrderName + ".lmg";

                            if (File.Exists(gmlFile) == true)//有gml就先讀
                                omFunc.RunCommandLine(Properties.Settings.Default.guide_exePath, "guiderpd \"" + gmlFile + "\"");
                            else if (File.Exists(lmgFile) == true)//沒有gml再讀lmg
                                omFunc.RunCommandLine(Properties.Settings.Default.guide_exePath, "guide \"" + lmgFile + "\"");
                            break;
                        }
                    case "button_openDir":
                        {
                            omFunc.RunCommandLine(Properties.OrderManagerProps.Default.systemDisk + @"Windows\explorer.exe", "\"" + System.IO.Path.GetDirectoryName(implantInfo.GuideModelPath) + @"\" + "\"");
                            break;
                        }
                    case "button_openPDF":
                        {
                            omFunc.RunCommandLine("\"" + implantInfo.PDFpath + "\"", "");
                            break;
                        }
                }
            }
        }
    }
}
