﻿using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using ImplantOuterInformation = OrderManagerNew.Local_UserControls.Order_implantBase.ImplantOuterInformation;
using ImplantSmallCaseInformation = OrderManagerNew.Local_UserControls.Order_ImplantSmallcase.ImplantSmallCaseInformation;

namespace OrderManagerNew.Local_UserControls
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
            
            if (File.Exists(implantInfo.GuideModelPath) != true)
                button_openDir.IsEnabled = false;
            else
                button_openDir.IsEnabled = true;

            if (File.Exists(implantInfo.PDFpath) != true)
                button_openPDF.IsEnabled = false;
            else
                button_openPDF.IsEnabled = true;

            if (Properties.Settings.Default.guide_exePath == "")
                button_loadGuide.IsEnabled = false;

            textbox_toothProductInfo.Text = "";
            if(implantInfo.List_ImplantToothInfo != null && implantInfo.List_ImplantToothInfo.Count > 0)
            {
                //有植體資料
                foreach (var item in implantInfo.List_ImplantToothInfo)
                {
                    string ToothNumber = item.ToothID.ToString();
                    string ToothProduct = item.Implant_Company + "(" + item.Implant_System + ")";
                    textbox_toothProductInfo.Text += "Tooth_" + ToothNumber + ":" + ToothProduct + '\n';
                }
            }
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
