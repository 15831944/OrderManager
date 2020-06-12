using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Xml.Linq;
using CadInformation = OrderManagerNew.UserControls.Order_cadBase.CadInformation;

namespace OrderManagerNew.UserControls
{
    /// <summary>
    /// Detail_cad.xaml 的互動邏輯
    /// </summary>
    public partial class Detail_cad : UserControl
    {
        private CadInformation CADInfo;
        private string[] FDI = new string[32];
        private bool ToothSystem = true;    //isFDI

        public Detail_cad()
        {
            InitializeComponent();

            CADInfo = new CadInformation();
            textbox_Order.Text = "";
            textbox_Client.Text = "";
            textbox_Patient.Text = "";
            textbox_Technician.Text = "";
            textbox_Note.Text = "";
            image_toothJPG.Source = null;
            textbox_toothProductInfo.Text = "";
            ToothSystem = true;

            FDI[0] = "T18"; FDI[1] = "T17";
            FDI[2] = "T16"; FDI[3] = "T15";
            FDI[4] = "T14"; FDI[5] = "T13";
            FDI[6] = "T12"; FDI[7] = "T11";

            FDI[8] = "T21"; FDI[9] = "T22";
            FDI[10] = "T23"; FDI[11] = "T24";
            FDI[12] = "T25"; FDI[13] = "T26";
            FDI[14] = "T27"; FDI[15] = "T28";

            FDI[16] = "T38"; FDI[17] = "T37";
            FDI[18] = "T36"; FDI[19] = "T35";
            FDI[20] = "T34"; FDI[21] = "T33";
            FDI[22] = "T32"; FDI[23] = "T31";

            FDI[24] = "T41"; FDI[25] = "T42";
            FDI[26] = "T43"; FDI[27] = "T44";
            FDI[28] = "T45"; FDI[29] = "T46";
            FDI[30] = "T47"; FDI[31] = "T48";
        }

        private string GetFDIToothIndexString(int idx)
        {
            return FDI[idx];
        }

        private string GetToothProductString(string str)
        {
            try
            {
                return OrderManagerNew.TranslationSource.Instance[str];
            }
            catch(Exception ex)
            {
                LogRecorder Log = new LogRecorder();
                Log.RecordLog(new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString(), "Detail_cad.xaml.cs GetToothProductString()_exception", ex.Message);
                //MessageBox.Show(ex.Message);//TODO log要記下來
                return "UnKnow";
            }
        }

        /// <summary>
        /// 讀取ToothProduct
        /// </summary>
        private void ReadToothProductDetail()
        {
            XDocument xmlDoc = XDocument.Load(CADInfo.CaseXmlPath);
            //判斷是否為Material的XML
            if (xmlDoc.Element("OrderExport") == null)
            {
                return;
            }

            textbox_toothProductInfo.Text = "";

            XElement ele = xmlDoc.Element("OrderExport").Element("Teeth");
            var query = from c in ele.Descendants("Tooth") select c;
            foreach (var item in query)
            {
                string ToothNumber = item.Element("Number").Value;
                string ToothProduct = item.Element("ProductType").Value;
                textbox_toothProductInfo.Text += string.Format("{0}: {1}\n", (ToothSystem == true) ? GetFDIToothIndexString(Convert.ToInt32(ToothNumber) - 1) : ToothNumber, GetToothProductString(ToothProduct));
            }
        }

        /// <summary>
        /// 設定Detail顯示資訊
        /// </summary>
        /// <param name="Import">要匯入的CadInformation</param>
        public void SetDetailInfo(CadInformation Import)
        {
            CADInfo = Import;
            textbox_Order.Text = CADInfo.OrderID.TrimStart('-');
            textbox_Client.Text = CADInfo.Client;
            textbox_Patient.Text = CADInfo.PatientName;
            textbox_Technician.Text = CADInfo.Technician;
            textbox_Note.Text = CADInfo.Note;
            string imgPath = CADInfo.CaseXmlPath.Replace(".xml", ".jpg");
            try
            {
                image_toothJPG.BeginInit();
                image_toothJPG.Source = new BitmapImage(new Uri(imgPath, UriKind.RelativeOrAbsolute));
                image_toothJPG.EndInit();
            }
            catch
            {

            }
            try
            {
                ReadToothProductDetail();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);//TODO Log記錄
            }
        }

        private void Click_systemButton(object sender, RoutedEventArgs e)
        {
            if(sender is Button)
            {
                switch(((Button)sender).Name)
                {
                    case "button_loadProj":
                        {
                            OrderManagerFunctions omFunc = new OrderManagerFunctions();
                            omFunc.RunCommandLine(Properties.Settings.Default.cad_exePath, "-openrpd \"" + CADInfo.CaseXmlPath + "\"");
                            break;
                        }
                    case "button_openDir":
                        {
                            OrderManagerFunctions omFunc = new OrderManagerFunctions();
                            omFunc.RunCommandLine(Properties.OrderManagerProps.Default.systemDisk + @"Windows\explorer.exe", "\"" + CADInfo.CaseDirectoryPath + @"\" + "\"");
                            break;
                        }
                }
            }
        }
    }
}
