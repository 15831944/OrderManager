﻿using System;
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
using Path = System.IO.Path;

namespace OrderManagerNew.UserControls
{
    /// <summary>
    /// Order_ImplantSmallcase.xaml 的互動邏輯
    /// </summary>
    public partial class Order_ImplantSmallcase : UserControl
    {
        //委派到Order_implantBase.xaml.cs裡面的SmallCaseHandler()
        public delegate void implantSmallCaseEventHandler(int projectIndex);
        public event implantSmallCaseEventHandler SetsmallCaseShow;

        public ImplantSmallCaseInformation implantsmallcaseInfo;
        public bool IsFocusSmallCase;
        private int ItemIndex;

        public class ImplantSmallCaseInformation
        {
            public string OrderName { get; set; }
            public string ImplantTiiPath { get; set; }
            public string GuideModelPath { get; set; }
            public string GuideCaseDir { get; set; }
            public string PDFpath { get; set; }

            public ImplantSmallCaseInformation()
            {
                OrderName = "";
                ImplantTiiPath = "";
                GuideModelPath = "";
                GuideCaseDir = "";
                PDFpath = "";
            }
        }

        public Order_ImplantSmallcase()
        {
            InitializeComponent();
            IsFocusSmallCase = false;
            ItemIndex = -1;
        }

        public void SetImplantSmallCaseInfo(ImplantSmallCaseInformation Import, int Index)
        {
            implantsmallcaseInfo = Import;
            label_ProjectName.Content = implantsmallcaseInfo.OrderName;
            if (implantsmallcaseInfo.GuideModelPath == "")
            {
                button_GuideModelDir.IsEnabled = false;
                button_GuideModelDir.ToolTip = null;
            }
            else
            {
                button_GuideModelDir.IsEnabled = true;
                button_GuideModelDir.ToolTip = TranslationSource.Instance["GuideModel"];
            }
            ItemIndex = Index;
        }

        private void Click_ButtonEvent(object sender, RoutedEventArgs e)
        {
            OrderManagerFunctions omFunc = new OrderManagerFunctions();
            switch (((Button)sender).Name)
            {
                case "button_Implant":
                    {
                        omFunc.RunCommandLine(Properties.Settings.Default.implant_exePath, "\"readdii\" \"" + implantsmallcaseInfo.ImplantTiiPath + "\"");
                        break;
                    }
                case "button_Guide":
                    {
                        string gmlFile = implantsmallcaseInfo.GuideCaseDir + implantsmallcaseInfo.OrderName + "-Guide.gml";
                        string lmgFile = implantsmallcaseInfo.GuideCaseDir + implantsmallcaseInfo.OrderName + ".lmg";

                        if (File.Exists(gmlFile) == true)//有gml就先讀
                            omFunc.RunCommandLine(Properties.Settings.Default.guide_exePath, "guiderpd \"" + gmlFile + "\"");
                        else if (File.Exists(lmgFile) == true)//沒有gml再讀lmg
                            omFunc.RunCommandLine(Properties.Settings.Default.guide_exePath, "guide \"" + lmgFile + "\"");
                        break;
                    }
                case "button_GuideModelDir":
                    {
                        if(button_GuideModelDir.IsEnabled == true)
                            omFunc.RunCommandLine(Properties.OrderManagerProps.Default.systemDisk + @"Windows\explorer.exe", "\"" + Path.GetDirectoryName(implantsmallcaseInfo.GuideModelPath) + "\"");
                        break;
                    }
            }
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
                        background_implantSmallcase.Fill = this.FindResource("background_FocusedSmallCase") as SolidColorBrush;
                        background_implantSmallcase.Stroke = this.FindResource("borderbrush_FocusedSmallCase") as SolidColorBrush;
                        IsFocusSmallCase = true;
                        break;
                    }
                case false:
                    {
                        background_implantSmallcase.Fill = this.FindResource("background_SmallCase") as SolidColorBrush;
                        background_implantSmallcase.Stroke = null;
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
