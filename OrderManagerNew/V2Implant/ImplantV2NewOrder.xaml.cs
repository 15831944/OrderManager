using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using Microsoft.Win32;
//using System.Windows.Forms;

namespace OrderManagerNew.V2Implant
{
	/// <summary>
	/// Interaction logic for winNewOrder.xaml
	/// </summary>
	public partial class ImplantV2NewOrder : Window
	{
		private Point startPos;
		System.Windows.Forms.Screen[] screens = System.Windows.Forms.Screen.AllScreens;

		int ListBox1_PreSelectIndex;
	  
		//記錄齒位資訊
		public ToothOrderInfo[] ToothData;

		public string m_ImplantRoot;

		public int ActiveToothType { get; set; }

		public List<int> Productlist{ get; set; }

		public bool[] connectorInfo = new bool[32];

		public class ImplantInforElement
		{
			public String CompanyName { get; set; }
			public List<String> SystemName { get; set; }
		}

		public List<ImplantInforElement> ImplantInforList;

		public bool IsUpperJaw_case = true;

		//5/15/2019 handtan//set default folder path
		public string Selected_folder_path;

		//10/08/2019 handtan//airdental串接
		public bool IsAirdentalCase = false;

		public bool haveXml;

		public ImplantV2NewOrder()
		{
			InitializeComponent();

			Productlist = new List<int>();
			ActiveToothType = -1;
			haveXml = true;
			ToothData = new ToothOrderInfo[32];
			for (int i = 0; i < 32; i++)
			{
				ToothData[i] = new ToothOrderInfo();
				connectorInfo[i] = false;
			}
			toothImg.SetConnectorAllUnset();

			SetDesignerGeneralProcuctType();

			ListBox1_PreSelectIndex = -1;
			
			LoadSurgicalKitXml();

			//SetImplantBrand();

			//-load implant icon
			if (true)
			{
				SetBridgeProductList();
				listbox2.Items.Clear();
				String imgstring = "", namestring = "";
				int toothtypeindex = 0;
				foreach (int id in Productlist)
				{
					GetToothTypeData(id, ref imgstring, ref namestring, ref toothtypeindex);
					UCToothTypeBase uc = new UCToothTypeBase();
					uc.SetImage(imgstring);
					uc.ToothTypeName.Content = namestring;
					uc.ProductTypeIdx = toothtypeindex;
					listbox2.Items.Add(uc);
				}

				//給預設order number
				DateTime localDate = DateTime.Now;
				localDate.Month.ToString();

				//-20200525 handtan//統一日期單位皆為二位數,如5月為05
				String m_month="";
				String m_day="";
				String m_hour="";
				String m_min="";
				String m_sec="";
				if (localDate.Month.ToString().Length == 1) m_month = "0" + localDate.Month.ToString();
				else                                        m_month = localDate.Month.ToString();

				if (localDate.Day.ToString().Length == 1)   m_day = "0" + localDate.Day.ToString();
				else                                        m_day = localDate.Day.ToString();

				if (localDate.Hour.ToString().Length == 1)  m_hour = "0" + localDate.Hour.ToString();
				else                                        m_hour = localDate.Hour.ToString();

				if (localDate.Minute.ToString().Length == 1)    m_min = "0" + localDate.Minute.ToString();
				else                                            m_min = localDate.Minute.ToString();

				if (localDate.Second.ToString().Length == 1)    m_sec = "0" + localDate.Second.ToString();
				else                                            m_sec = localDate.Second.ToString();

				//m_order_num.Text = localDate.Year.ToString() + localDate.Month.ToString() + localDate.Day.ToString() + localDate.Hour.ToString() + localDate.Minute.ToString() + localDate.Second.ToString();
				m_order_num.Text = localDate.Year.ToString() + m_month + m_day + m_hour + m_day + m_min + m_sec;

				//給預設生日

				//if (m_patient_birthday_airdental != "")
				//{
				//    m_patient_birthday.Text = m_patient_birthday_airdental;
				//}
				//else
				{
					m_patient_birthday.SelectedDate = localDate.Date;
				}


				//測試
				//m_implant_path.Text = @"C:\IntewareData\Implant\안녕하세요";

				LoadImplantInfor();

				//初始standard guide
				m_ct_path_check.IsChecked = true;
				m_upperjaw_path_check.IsChecked = true;

				m_lowerjaw_path.Opacity = 0.0;
				brolower.Opacity = 0.0;
				m_lowerjaw_path_clear.Opacity = 0.0;

				m_othermodel_path.Text = "";
				m_othermodel_path.Opacity = 0.0;
				broother.Opacity = 0.0;
				m_othermodel_path_check.IsChecked = false;
				m_othermodel_path_clear.Opacity = 0.0;
			}
			/*SaveXml();*/
		}

		public void GetToothTypeData(int index, ref String ImgString, ref String NameString, ref int ToothTypeIdx)
		{
			switch (index)
			{
				case (int)ToothType.ToothTypeList.NON_TOOTH:
					ImgString = "ToothImg\\icon_implant-delete.png";
					NameString = "Cancel";
					ToothTypeIdx = (int)ToothType.ToothTypeList.NON_TOOTH;
					break;
				case (int)ToothType.ToothTypeList.OFFSET_COPING:
					ImgString = "ToothImg\\ttype_co.png";
					NameString = "Offset Coping";
					ToothTypeIdx = (int)ToothType.ToothTypeList.OFFSET_COPING;
					break;
				case (int)ToothType.ToothTypeList.ANATOMIC_COPING:
					ImgString = "ToothImg\\ttype_co.png";
					NameString = "Anatomic Coping";
					ToothTypeIdx = (int)ToothType.ToothTypeList.ANATOMIC_COPING;
					break;
				case (int)ToothType.ToothTypeList.ANATOMIC_CROWN:
					ImgString = "ToothImg\\ttype_cr.png";
					NameString = "Anatomic Crown";
					ToothTypeIdx = (int)ToothType.ToothTypeList.ANATOMIC_CROWN;
					break;
				case (int)ToothType.ToothTypeList.OFFSET_PONTIC:
					ImgString = "ToothImg\\ttype_poco.png";
					NameString = "Core Pontic";
					ToothTypeIdx = (int)ToothType.ToothTypeList.OFFSET_PONTIC;
					break;
				case (int)ToothType.ToothTypeList.REDUCED_PONTIC:
					ImgString = "ToothImg\\ttype_poco.png";
					NameString = "Reduced Pontic";
					ToothTypeIdx = (int)ToothType.ToothTypeList.REDUCED_PONTIC;
					break;
				case (int)ToothType.ToothTypeList.ANATOMIC_PONTIC:
					ImgString = "ToothImg\\ttype_pocr.png";
					NameString = "Anatomic Pontic";
					ToothTypeIdx = (int)ToothType.ToothTypeList.ANATOMIC_PONTIC;
					break;
				case (int)ToothType.ToothTypeList.VENEER:
					ImgString = "ToothImg\\ttype_ve.png";
					NameString = "Veneer";
					ToothTypeIdx = (int)ToothType.ToothTypeList.VENEER;
					break;
				case (int)ToothType.ToothTypeList.INLAYONLAY:
					ImgString = "ToothImg\\ttype_in.png";
					NameString = "Inlay / Onlay";
					ToothTypeIdx = (int)ToothType.ToothTypeList.INLAYONLAY;
					break;
				case (int)ToothType.ToothTypeList.ABUTMENT:
					ImgString = "ToothImg\\ttype_ia.png";
					NameString = "Abutment";
					ToothTypeIdx = (int)ToothType.ToothTypeList.ABUTMENT;
					break;
				case (int)ToothType.ToothTypeList.ABUTMENT_OFFSET_COPING:
					ImgString = "ToothImg\\ttype_ia_co.png";
					NameString = "Abut. + Offset Coping";
					ToothTypeIdx = (int)ToothType.ToothTypeList.ABUTMENT_OFFSET_COPING;
					break;
				case (int)ToothType.ToothTypeList.ABUTMENT_ANATOMIC_COPING:
					ImgString = "ToothImg\\ttype_ia_co.png";
					NameString = "Abut. + Anatomic Coping";
					ToothTypeIdx = (int)ToothType.ToothTypeList.ABUTMENT_ANATOMIC_COPING;
					break;
				case (int)ToothType.ToothTypeList.ABUTMENT_ANATOMIC_CROWN:
					ImgString = "ToothImg\\ttype_ia_cr.png";
					NameString = "Abut. + Anatomic Crown";
					ToothTypeIdx = (int)ToothType.ToothTypeList.ABUTMENT_ANATOMIC_CROWN;
					break;
				case (int)ToothType.ToothTypeList.ABUTMENT_TEMP:
					ImgString = "ToothImg\\ttype_ia_cr.png";
					NameString = "Abut. + Temp Crown";
					ToothTypeIdx = (int)ToothType.ToothTypeList.ABUTMENT_TEMP;
					break;
				case (int)ToothType.ToothTypeList.SRIA:
					ImgString = "ToothImg\\ttype_sria.png";
					NameString = "Screw Retain Abutment";
					ToothTypeIdx = (int)ToothType.ToothTypeList.SRIA;
					break;
				case (int)ToothType.ToothTypeList.SRIA_CROWN:
					ImgString = "ToothImg\\ttype_sria_cr.png";
					NameString = "Screw Retain Abutment (Anatomic Crown)";
					ToothTypeIdx = (int)ToothType.ToothTypeList.SRIA_CROWN;
					break;
				case (int)ToothType.ToothTypeList.NEIGHBOR:
					ImgString = "ToothImg\\ttype_neighbor.png";
					NameString = "Neighbor Tooth";
					ToothTypeIdx = (int)ToothType.ToothTypeList.NEIGHBOR;
					break;
				case (int)ToothType.ToothTypeList.IMPLANT:
					ImgString = "ToothImg\\icon_implant.png";
					NameString = "Implant";
					ToothTypeIdx = (int)ToothType.ToothTypeList.IMPLANT;
					break;
				default:
					break;
			}
		}

		public void SetDesignerGeneralProcuctType()
		{
			UCToothTypeBase uc1 = new UCToothTypeBase();
			uc1.SetImage("ToothImg\\ttype_no.png");
			uc1.ToothTypeName.Content = "Bridge / Copings";
			uc1.ProductTypeIdx = 0;
			uc1.SendClickEvent = true;
			uc1.ToothTypeBaseClick += MyEventHandlerFunction_StatusUpdated;
			listbox1.Items.Add(uc1);

			UCToothTypeBase uc2 = new UCToothTypeBase();
			uc2.SetImage("ToothImg\\ttype_no.png");
			uc2.ToothTypeName.Content = "Custom Abutment";
			uc2.ProductTypeIdx = 0;
			uc2.SendClickEvent = true;
			uc2.ToothTypeBaseClick += MyEventHandlerFunction_StatusUpdated;
			listbox1.Items.Add(uc2);

			UCToothTypeBase uc3 = new UCToothTypeBase();
			uc3.SetImage("ToothImg\\ttype_no.png");
			uc3.ToothTypeName.Content = "Screw Retain Abutment";
			uc3.ProductTypeIdx = 0;
			uc3.SendClickEvent = true;
			uc3.ToothTypeBaseClick += MyEventHandlerFunction_StatusUpdated;
			listbox1.Items.Add(uc3);

			UCToothTypeBase uc4 = new UCToothTypeBase();
			uc4.SetImage("ToothImg\\ttype_no.png");
			uc4.ToothTypeName.Content = "Neighbor";
			uc4.ProductTypeIdx = 0;
			uc4.SendClickEvent = true;
			uc4.ToothTypeBaseClick += MyEventHandlerFunction_StatusUpdated;
			listbox1.Items.Add(uc4);

			UCToothTypeBase uc5 = new UCToothTypeBase();
			uc5.SetImage("ToothImg\\ttype_no.png");
			uc5.ToothTypeName.Content = "Cancel Select";
			uc5.ProductTypeIdx = 0;
			uc5.SendClickEvent = true;
			uc5.ToothTypeBaseClick += MyEventHandlerFunction_StatusUpdated;
			listbox1.Items.Add(uc5);

			UCToothTypeBase uc6 = new UCToothTypeBase();
			uc6.SetImage("ToothImg\\ttype_no.png");
			uc6.ToothTypeName.Content = "Clear";
			uc6.ProductTypeIdx = 0;
			uc6.SendClickEvent = true;
			uc6.ToothTypeBaseClick += MyEventHandlerFunction_StatusUpdated;
			listbox1.Items.Add(uc6);
		}

		public void SetImageClickEvent()
		{
			toothImg.ImageClick += MyEventHandlerFunction_StatusUpdated;
		}

		public void MyEventHandlerFunction_StatusUpdated(object sender, EventArgs e)
		{
			//點選list1要變動list2的物件
			if (sender is UCToothTypeBase uc)
			{
				Listbox1_SelectionChanged(listbox1, null);
				return;
			}

			if (sender is Image img)
			{
				int idx = toothImg.GetToothImageIndex(img.Name);

				//有產品設定
				if (ToothData[idx].FLAG)
				{
					SetProductDetail(ToothData[idx]);
				}
				else
				{
					listbox2.SelectedIndex = -1;
				}
			}
		}


		private void Listbox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			ListBox lb1 = sender as ListBox;
			
			int idx = lb1.SelectedIndex;
			if(idx == 0)
			{
				SetBridgeProductList();
				listbox2.Items.Clear();
				String imgstring = "", namestring = "";
				int toothtypeindex = 0;
				foreach (int id in Productlist)
				{
					GetToothTypeData(id, ref imgstring, ref namestring, ref toothtypeindex);
					UCToothTypeBase uc = new UCToothTypeBase();
					uc.SetImage(imgstring);
					uc.ToothTypeName.Content = namestring;
					uc.ProductTypeIdx = toothtypeindex;
					listbox2.Items.Add(uc);
				}
			}
			else if(idx == 1)
			{
				SetAbutmentProductList();
				listbox2.Items.Clear();
				String imgstring = "", namestring = "";
				int toothtypeindex = 0;
				foreach (int id in Productlist)
				{
					GetToothTypeData(id, ref imgstring, ref namestring, ref toothtypeindex);
					UCToothTypeBase uc = new UCToothTypeBase();
					uc.SetImage(imgstring);
					uc.ToothTypeName.Content = namestring;
					uc.ProductTypeIdx = toothtypeindex;
					listbox2.Items.Add(uc);
				}
			}
			else if(idx == 2)
			{
				SetSriaProductList();
				listbox2.Items.Clear();
				String imgstring = "", namestring = "";
				int toothtypeindex = 0;
				foreach (int id in Productlist)
				{
					GetToothTypeData(id, ref imgstring, ref namestring, ref toothtypeindex);
					UCToothTypeBase uc = new UCToothTypeBase();
					uc.SetImage(imgstring);
					uc.ToothTypeName.Content = namestring;
					uc.ProductTypeIdx = toothtypeindex;
					listbox2.Items.Add(uc);
				}
			}
			else if(idx==3)
			{
				listbox2.Items.Clear();
				ActiveToothType = (int)ToothType.ToothTypeList.NEIGHBOR;

				for (int i = 0; i < 32; i++)
				{
					if (toothImg.ToothSelectIdx[i])
					{
						toothImg.SetToothType(i, ActiveToothType);
						Image img = toothImg.GetToothImage(i);

						toothImg.SetToothStatus(img);

						ToothData[i].ResetData();
						ToothData[i].FLAG = true;
						ToothData[i].Product_idx = ActiveToothType;
						ToothData[i].Product = Enum.GetName(typeof(ToothType.ToothTypeList), ActiveToothType);
					}
				}

				toothImg.DrawConnectorLine(toothImg.ConnectorIdx);
			}
			else if(idx==4)
			{
				listbox2.Items.Clear();
				ActiveToothType = (int)ToothType.ToothTypeList.NON_TOOTH;

				for (int i = 0; i < 32; i++)
				{
					if (toothImg.ToothSelectIdx[i])
					{
						toothImg.SetToothType(i, ActiveToothType);
						Image img = toothImg.GetToothImage(i);

						toothImg.SetToothStatus(img);

						ToothData[i].ResetData();
					}
				}
			}
			else if (idx == 5)//clear
			{
				toothImg.ClearSelection();

				ActiveToothType = -1;

				listbox1.SelectedIndex = 0;
				listbox2.Items.Clear();
				listbox2.SelectedIndex = -1;

				for (int i = 0; i < 32; i++)
				{
					ToothData[i].ResetData();
				}
			}

			ListBox1_PreSelectIndex = idx;

		}

		public void SetBridgeProductList()
		{
			Productlist.Clear();
			Productlist.Add((int)ToothType.ToothTypeList.IMPLANT);
			Productlist.Add((int)ToothType.ToothTypeList.NON_TOOTH);
		}

		public void SetAbutmentProductList()
		{
			Productlist.Clear();
			Productlist.Add((int)ToothType.ToothTypeList.ABUTMENT);
			Productlist.Add((int)ToothType.ToothTypeList.ABUTMENT_OFFSET_COPING);
			Productlist.Add((int)ToothType.ToothTypeList.ABUTMENT_ANATOMIC_COPING);
			Productlist.Add((int)ToothType.ToothTypeList.ABUTMENT_ANATOMIC_CROWN);
			Productlist.Add((int)ToothType.ToothTypeList.ABUTMENT_TEMP);
		}

		public void SetSriaProductList()
		{
			Productlist.Clear();
			Productlist.Add((int)ToothType.ToothTypeList.SRIA);
			Productlist.Add((int)ToothType.ToothTypeList.SRIA_CROWN);
		}

		private void Listbox2_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			//MessageBox.Show("select change");
			if (listbox2.SelectedIndex!=-1)
			{
				UCToothTypeBase uc = listbox2.SelectedItem as UCToothTypeBase;
				ActiveToothType = uc.ProductTypeIdx;
				toothImg.ActiveProductType = ActiveToothType;

				for (int i = 0; i < 32; i++)
				{
					if (toothImg.ToothSelectIdx[i])
					{
						toothImg.SetToothType(i, ActiveToothType);
						Image img = toothImg.GetToothImage(i);

						toothImg.SetToothStatus(img);

						ToothData[i].ResetData();
						ToothData[i].FLAG = true;
						ToothData[i].Product_idx = ActiveToothType;
						ToothData[i].Product = Enum.GetName(typeof(ToothType.ToothTypeList), ActiveToothType);
					}
				}

				//set connector
				if (ActiveToothType != (int)(int)ToothType.ToothTypeList.ABUTMENT &&
					ActiveToothType != (int)(int)ToothType.ToothTypeList.ABUTMENT_TEMP &&
					ActiveToothType != (int)(int)ToothType.ToothTypeList.SRIA &&
					ActiveToothType != (int)(int)ToothType.ToothTypeList.SRIA_CROWN &&
					ActiveToothType != (int)(int)ToothType.ToothTypeList.NEIGHBOR &&
					ActiveToothType != (int)(int)ToothType.ToothTypeList.NON_TOOTH)
				{
					bool doConnector;
					doConnector = false;
					for (int i = 0; i < 31; i++)
					{
						if (toothImg.ToothSelectIdx[i])
						{
							if (toothImg.ToothSelectIdx[i + 1])
							{
								doConnector = true;
								break;
							}
						}
					}

					if(doConnector)
					{
						for (int i = 0; i < 31; i++)
						{
							if (toothImg.ToothSelectIdx[i])
							{
								if (toothImg.ToothSelectIdx[i + 1])
								{
									toothImg.SetConnectorStatus(i, true);
								}
							}
						}

						toothImg.DrawConnectorLine(toothImg.ConnectorIdx);
					}
				}
				else//clear connector status
				{
					for (int i = 0; i < 31; i++)
					{
						if (toothImg.ToothSelectIdx[i])
						{
							toothImg.SetConnectorStatus(i, false);
						}
					}
					toothImg.DrawConnectorLine(toothImg.ConnectorIdx);
				}

				//5/9/2019 handtan//植體無法重選bug修正
				listbox2.SelectedIndex = -1;

			}
			else
			{
				ActiveToothType = -1;
				toothImg.ActiveProductType = ActiveToothType;
			}
		}

		public void LoadMaterialXml_Ceramic()
		{
			XDocument xDoc;
			xDoc = XDocument.Load("..\\Material_Ceramic.xml");
			if (xDoc == null)
			{
                Inteware_Messagebox Msg = new Inteware_Messagebox();
                Msg.ShowMessage(TranslationSource.Instance["Loading"] + " Material_Ceramic.xml " + TranslationSource.Instance["Error"]);
				return;
			}

			var result = from q in xDoc.Descendants("Name")
						 select new
						 {
							 m_name = q.Value
						 };

			foreach (var item in result)
			{
				comboCeramic.Items.Add(item.m_name);
			}

			if (comboCeramic.Items.Count > 0)
				comboCeramic.SelectedIndex = 0;
		}

		public void LoadMaterialXml_Metal()
		{
			XDocument xDoc;
			xDoc = XDocument.Load("..\\Material_Metal.xml");
			if (xDoc == null)
			{
				//MessageBox.Show("Load Material_Metal.xml Error!");
				return;
			}
			var result = from q in xDoc.Descendants("Name")
						 select new
						 {
							 m_name = q.Value
						 };

			foreach (var item in result)
			{
				comboMetal.Items.Add(item.m_name);
			}

			if (comboMetal.Items.Count > 0)
				comboMetal.SelectedIndex = 0;
		}

		public void LoadMaterialXml_Other()
		{
			XDocument xDoc;
			xDoc = XDocument.Load("..\\Material_Other.xml");
			if (xDoc == null)
			{
				//MessageBox.Show("Load Material_Other.xml Error!");
				return;
			}
			var result = from q in xDoc.Descendants("Name")
						 select new
						 {
							 m_name = q.Value
						 };

			foreach (var item in result)
			{
				comboOther.Items.Add(item.m_name);
			}

			if (comboOther.Items.Count > 0)
				comboOther.SelectedIndex = 0;
		}

		public void LoadColorXml()
		{
			XDocument xDoc;
			xDoc = XDocument.Load("..\\ShadeColor.xml");
			if (xDoc == null)
			{
				//MessageBox.Show("Load ShadeColor.xml Error!");
				return;
			}
			var result = from q in xDoc.Descendants("Name")
						 select new
						 {
							 m_name = q.Value
						 };

			foreach (var item in result)
			{
				comboColor.Items.Add(item.m_name);
			}

			if (comboColor.Items.Count > 0)
				comboColor.SelectedIndex = 0;
		}

		public void LoadSurgicalKitXml()
		{
			XDocument xDoc;
			try
			{
				xDoc = XDocument.Load("V2Implant\\SurgicalKitInfor.xml");
			}
			catch(Exception ex)
			{
                Inteware_Messagebox Msg = new Inteware_Messagebox();
                Msg.ShowMessage(ex.Message);
                haveXml = false;
				return;
			}
			
			if (xDoc == null)
			{
                Inteware_Messagebox Msg = new Inteware_Messagebox();
                Msg.ShowMessage(TranslationSource.Instance["Loading"] + " SurgicalKitInfor.xml " + TranslationSource.Instance["Error"]);
				haveXml = false;
				return;
			}
			var result = from q in xDoc.Descendants("Name")
						 select new
						 {
							 m_name = q.Value
						 };

			foreach (var item in result)
			{
				comboSurgicalKit.Items.Add(item.m_name);
			}

			if (comboSurgicalKit.Items.Count > 0)
				comboSurgicalKit.SelectedIndex = 0;
		}
		
		public void LoadImplantInfor()
		{
			XDocument xDoc;
			try
			{
				xDoc = XDocument.Load("V2Implant\\ImplantInfor.xml");
			}
			catch(Exception ex)
			{
				Inteware_Messagebox Msg = new Inteware_Messagebox();
                Msg.ShowMessage(ex.Message);
				haveXml = false;
				return;
			}

			comboImplantBrand.Items.Clear();
			comboImplantSystem.Items.Clear();

			if (ImplantInforList == null)
			{
				ImplantInforList = new List<ImplantInforElement>();
			}
			else
			{
				ImplantInforList.Clear();
			}

			var brandstr = from item in xDoc.Element("ImplantInfor").Descendants("Brand")
						  select item;

			foreach (var item in brandstr)
			{
				ImplantInforElement ie = new ImplantInforElement();
		  
				string company_name = item.Descendants("Company").First().Value;
				comboImplantBrand.Items.Add(company_name);
				ie.CompanyName = company_name;

				var systemstr = from sutitem in item.Descendants("System")
								select sutitem;

				ie.SystemName = new List<String>();

				int index = 0;
				foreach (var sutitem in systemstr)
				{
					string system_name = sutitem.Value;

					if (index == 0) comboImplantSystem.Items.Add(system_name);

					ie.SystemName.Add(system_name);
					index++;
				}

				ImplantInforList.Add(ie);
			}

			if (comboImplantBrand.Items.Count > 0)
				comboImplantBrand.SelectedIndex = 0;

			if (comboImplantSystem.Items.Count > 0)
				comboImplantSystem.SelectedIndex = 0;
		}


		public event EventHandler NewOrderStatusUpdated;

		private void FunctionThatRaisesEvent()
		{
			//Null check makes sure the main page is attached to the event
			this.NewOrderStatusUpdated?.Invoke(this, new EventArgs());
		}

		public void SaveXml(string storepath)
		{
			//set group information
			List<GCase> grouplist = new List<GCase>();
			for (int i = 0; i < 32; i++)
			{
				if (ToothData[i].Product != "" && ToothData[i].Product!=Enum.GetName(typeof(ToothType.ToothTypeList), ToothType.ToothTypeList.NEIGHBOR))
				{
					if(toothImg.ConnectorIdx[i]==true)
					{
						for (int j = i+1; j < 32; j++)
						{
							if(toothImg.ConnectorIdx[j]==false)
							{
								GCase g = new GCase
								{
									c_start = i,
									c_end = j
								};
								i = j;
								grouplist.Add(g);
								break;
							}
						}
					}
					else
					{
						GCase g = new GCase
						{
							c_start = i,
							c_end = i
						};
						grouplist.Add(g);
					}
				}

			}


			var BIMLXdoc = new XDocument(
				 new XDeclaration("1.0", "utf-8", null));

			var xmlRoot = new XElement("ImplantOrderExport");

			var xmlOrderInfo = new XElement("OrderInfo");
			xmlOrderInfo.Add(new XElement("OrderNo", m_order_num.Text));
			xmlOrderInfo.Add(new XElement("PatientName", m_patient_name.Text));

			string tmpstring;
			//bool malecheck = m_patient_male.Checked;
			if (m_patient_male.IsChecked == true)
			{
				tmpstring = "Male";
			}
			else
			{
				tmpstring = "Female";
			}
			xmlOrderInfo.Add(new XElement("PatientGender", tmpstring));


			if (m_patient_birthday.SelectedDate == null)
			{
				DateTime nowd = DateTime.Now;
				tmpstring = nowd.Year.ToString() + "-" + nowd.Month.ToString() + "-" + nowd.Day.ToString();
			}
			else
			{
				DateTime dt = m_patient_birthday.SelectedDate.Value.Date;
				tmpstring = dt.Year.ToString() +"-"+ dt.Month.ToString() + "-" + dt.Day.ToString();
			}


			xmlOrderInfo.Add(new XElement("PatientBirthday", tmpstring));

			xmlOrderInfo.Add(new XElement("Clinic", m_lab_name.Text));
			//xmlOrderInfo.Add(new XElement("Note", m_note.Text));
			xmlOrderInfo.Add(new XElement("Note", m_instruction.Text));

			xmlRoot.Add(xmlOrderInfo);

			var xmlCaseInfo = new XElement("CaseInfo");

			if (m_goal_guide.IsChecked == true)  {tmpstring = "Guide";}
			else                                 {tmpstring = "ONE-DAY Implant";}
			xmlCaseInfo.Add(new XElement("SurgicalGoal", tmpstring));

			//1/21/2020 handtan//mi guide
			if (m_guide_standard.IsChecked == true) { tmpstring = "Standard"; }
			else if (m_guide_vacuum.IsChecked == true) { tmpstring = "Vacuum"; }
			else if (m_guide_denture.IsChecked == true) { tmpstring = "Denture"; }
			else { tmpstring = "MI"; }

			xmlCaseInfo.Add(new XElement("SurgicalGuide", tmpstring));

			if (m_surgery_flap.IsChecked == true) { tmpstring = "Flap"; }
			else if (m_surgery_mini.IsChecked == true) { tmpstring = "Flapless"; }
			else if (m_surgery_immediate.IsChecked == true) { tmpstring = "Immediate"; }
			else { tmpstring = "Undecided"; }
			xmlCaseInfo.Add(new XElement("SurgicalOption", tmpstring));

			tmpstring = comboSurgicalKit.Text;
			xmlCaseInfo.Add(new XElement("Surgicalkit", tmpstring));
			xmlRoot.Add(xmlCaseInfo);

			var xmlProjectPath = new XElement("ImageData");
			xmlProjectPath.Add(new XElement("CBCTPath", m_ct_path.Text));
			xmlProjectPath.Add(new XElement("JawPath", m_upperjaw_path.Text));
			xmlProjectPath.Add(new XElement("JawTrayPath", m_lowerjaw_path.Text));
			xmlProjectPath.Add(new XElement("DenturePath", m_othermodel_path.Text));
			xmlRoot.Add(xmlProjectPath);

			var xmlToothInfo = new XElement("ToothInfo");
			for (int i = 1; i <= 32; i++)
			{
				string a = string.Format("{0}", i);
				var xmlToothAttr = new XElement("Tooth", ToothData[i - 1].Product);
				xmlToothAttr.Add(new XAttribute("id", a));
				xmlToothInfo.Add(xmlToothAttr);

				if (xmlToothAttr.Value != "" && i > 16)
				{
					IsUpperJaw_case = false;
				}
			}
			xmlRoot.Add(xmlToothInfo);

			var xmlImplantInfo = new XElement("ImplantInfo");
			xmlImplantInfo.Add(new XElement("Brand",comboImplantBrand.Text));
			xmlImplantInfo.Add(new XElement("System", comboImplantSystem.Text));
			xmlRoot.Add(xmlImplantInfo);
			
			BIMLXdoc.Add(xmlRoot);
			string storefile;
			storefile = storepath +"\\"+ m_order_num.Text + ".xml";
			BIMLXdoc.Save(storefile);
		}

		public void SetProductDetail(ToothOrderInfo order)
		{
			int list1Idx, list2Idx;
			list1Idx = -1;
			list2Idx = -1;

			switch (order.Product_idx)
			{
				case (int)ToothType.ToothTypeList.NON_TOOTH:
					break;
				case (int)ToothType.ToothTypeList.OFFSET_COPING:
					list1Idx = 0;
					list2Idx = 0;
					break;
				case (int)ToothType.ToothTypeList.ANATOMIC_COPING:
					list1Idx = 0;
					list2Idx = 1;
					break;
				case (int)ToothType.ToothTypeList.ANATOMIC_CROWN:
					list1Idx = 0;
					list2Idx = 2;
					break;
				case (int)ToothType.ToothTypeList.OFFSET_PONTIC:
					list1Idx = 0;
					list2Idx = 3;
					break;
				case (int)ToothType.ToothTypeList.REDUCED_PONTIC:
					list1Idx = 0;
					list2Idx = 4;
					break;
				case (int)ToothType.ToothTypeList.ANATOMIC_PONTIC:
					list1Idx = 0;
					list2Idx = 5;
					break;
				case (int)ToothType.ToothTypeList.VENEER:
					list1Idx = 0;
					list2Idx = 6;
					break;
				case (int)ToothType.ToothTypeList.INLAYONLAY:
					list1Idx = 0;
					list2Idx = 7;
					break;
				case (int)ToothType.ToothTypeList.ABUTMENT:
					list1Idx = 1;
					list2Idx = 0;
					break;
				case (int)ToothType.ToothTypeList.ABUTMENT_OFFSET_COPING:
					list1Idx = 1;
					list2Idx = 1;
					break;
				case (int)ToothType.ToothTypeList.ABUTMENT_ANATOMIC_COPING:
					list1Idx = 1;
					list2Idx = 2;
					break;
				case (int)ToothType.ToothTypeList.ABUTMENT_ANATOMIC_CROWN:
					list1Idx = 1;
					list2Idx = 3;
					break;
				case (int)ToothType.ToothTypeList.ABUTMENT_TEMP:
					list1Idx = 1;
					list2Idx = 4;
					break;
				case (int)ToothType.ToothTypeList.SRIA:
					list1Idx = 2;
					list2Idx = 0;
					break;
				case (int)ToothType.ToothTypeList.SRIA_CROWN:
					list1Idx = 2;
					list2Idx = 1;
					break;
				case (int)ToothType.ToothTypeList.NEIGHBOR:
					list1Idx = 3;
					list2Idx = -1;
					break;
				default:
					list1Idx = -1;
					list2Idx = -1;
					break;
			}

			//Metarial
			if (order.Material_Type == "Ceramic")
			{
				Radio_Mat_Ceramic.IsChecked = true;

				int idx = GetComboxIndex(comboCeramic, order.Material_Name);
				if (idx == -1)
					comboCeramic.SelectedIndex = 0;
				else
					comboCeramic.SelectedIndex = idx;
			}
			else if (order.Material_Type == "Metal")
			{
				Radio_Mat_Metal.IsChecked = true;

				int idx = GetComboxIndex(comboMetal, order.Material_Name);
				if (idx == -1)
					comboMetal.SelectedIndex = 0;
				else
					comboMetal.SelectedIndex = idx;
			}
			else
			{
				Radio_Mat_Other.IsChecked = true;

				int idx = GetComboxIndex(comboOther, order.Material_Name);
				if (idx == -1)
					comboOther.SelectedIndex = 0;
				else
					comboOther.SelectedIndex = idx;
			}

			//color
			int cidx = GetComboxIndex(comboColor, order.Color);
			if (cidx == -1)
				comboColor.SelectedIndex = 0;
			else
				comboColor.SelectedIndex = cidx;

			listbox1.SelectedIndex = list1Idx;
			listbox2.SelectedIndex = list2Idx;

		}

		public int GetComboxIndex(ComboBox cb, string findstring)
		{
			int _count = 0;
			foreach (var item in cb.Items)
			{
				if (item.ToString() == findstring)
				{
					return _count;
				}
				_count++;
			}

			return -1;
		}

		public class ToothOrderInfo
		{
			public bool FLAG { set; get; }
			public int Product_idx { set; get; }
			public string Product { set; get; }
			public string Material_Type { set; get; }
			public string Material_Name { set; get; }
			public string Implant_Brand { set; get; }
			public string Implant_Type { set; get; }
			public string Implant_Screw { set; get; }
			public string Implant_Material { set; get; }
			public string Implant_Jig { set; get; }
			public string Color { set; get; }

			public ToothOrderInfo()
			{
				ResetData();
			}

			public void ResetData()
			{
				FLAG = false;
				Product_idx = 0;
				Product = "";
				Material_Type = "";
				Material_Name = "";
				Implant_Brand = "";
				Implant_Type = "";
				Implant_Screw = "";
				Implant_Material = "";
				Implant_Jig = "";
				Color = "";
			}
			
		}

		private void MaterialRadioBtnClick(object sender, RoutedEventArgs e)
		{
			if (ActiveToothType == (int)ToothType.ToothTypeList.NEIGHBOR || ActiveToothType == 0) 
				return;

			string MetrialType = "";
			string MetrialName = "";

			if (Radio_Mat_Ceramic.IsChecked == true)
			{
				MetrialType = "Ceramic";
				MetrialName = comboCeramic.SelectedItem.ToString();
			}              
			else if (Radio_Mat_Metal.IsChecked == true)
			{
				MetrialType = "Metal";
				MetrialName = comboMetal.SelectedItem.ToString();
			}             
			else
			{
				MetrialType = "Other";
				MetrialName = comboOther.SelectedItem.ToString();
			}

			for (int i = 0; i < 32; i++)
			{
				if (toothImg.ToothSelectIdx[i])
				{
					ToothData[i].Material_Type = MetrialType;
					ToothData[i].Material_Name = MetrialName;
				}
			}

		}

		private void SurgicalGuideRadioBtnClick(object sender, RoutedEventArgs e)
		{
			double trans_value = 1.0;

			if (IsAirdentalCase == false) m_ct_path.Text = "";
			m_ct_path.Opacity = trans_value;
			browct.Opacity = trans_value;
			m_ct_path_check.IsChecked = false;
			m_ct_path_clear.Opacity = trans_value;

			if (IsAirdentalCase == false) m_upperjaw_path.Text = "";
			m_upperjaw_path.Opacity = trans_value;
			broupper.Opacity = trans_value;
			m_upperjaw_path_check.IsChecked = false;
			m_upperjaw_path_clear.Opacity = trans_value;

			if (IsAirdentalCase == false) m_lowerjaw_path.Text = "";
			m_lowerjaw_path.Opacity = trans_value;
			brolower.Opacity = trans_value;
			m_lowerjaw_path_check.IsChecked = false;
			m_lowerjaw_path_clear.Opacity = trans_value;

			if (IsAirdentalCase == false) m_othermodel_path.Text = "";
			m_othermodel_path.Opacity = trans_value;
			broother.Opacity = trans_value;
			m_othermodel_path_check.IsChecked = false;
			m_othermodel_path_clear.Opacity = trans_value;

			listbox2.IsEnabled = true;
			label_brand.IsEnabled = true;
			label_system.IsEnabled = true;
			comboImplantBrand.IsEnabled = true;
			comboImplantSystem.IsEnabled = true;
			comboSurgicalKit.IsEnabled = true;
			toothImg.IsEnabled = true;

			trans_value = 0.0;
			if (m_guide_standard.IsChecked == true)
			{
				m_lowerjaw_path.Opacity = 0.0;
				brolower.Opacity = 0.0;
				m_lowerjaw_path_clear.Opacity = 0.0;


				m_ct_path_check.IsChecked = true;

				m_upperjaw_path_check.IsChecked = true;

				if (IsAirdentalCase == false) m_othermodel_path.Text = "";
				m_othermodel_path.Opacity = trans_value;
				broother.Opacity = trans_value;
				m_othermodel_path_check.IsChecked = false;
				m_othermodel_path_clear.Opacity = trans_value;
			}
			else if (m_guide_vacuum.IsChecked == true)
			{
				m_lowerjaw_path.Opacity = 0.0;
				brolower.Opacity = 0.0;
				m_lowerjaw_path_clear.Opacity = 0.0;

				m_ct_path_check.IsChecked = true;
				m_upperjaw_path_check.IsChecked = true;

				if (IsAirdentalCase == false) m_othermodel_path.Text = "";
				m_othermodel_path.Opacity = trans_value;
				broother.Opacity = trans_value;
				m_othermodel_path_check.IsChecked = false;
				m_othermodel_path_clear.Opacity = trans_value;
			}
			else if (m_guide_mi.IsChecked == true)
			{
				m_lowerjaw_path.Opacity = 0.0;
				brolower.Opacity = 0.0;
				m_lowerjaw_path_clear.Opacity = 0.0;

				m_ct_path_check.IsChecked = true;
				m_upperjaw_path_check.IsChecked = true;

				if (IsAirdentalCase == false) m_othermodel_path.Text = "";
				m_othermodel_path.Opacity = trans_value;
				broother.Opacity = trans_value;
				m_othermodel_path_check.IsChecked = false;
				m_othermodel_path_clear.Opacity = trans_value;

				listbox2.IsEnabled = false;
				label_brand.IsEnabled = false;
				label_system.IsEnabled = false;
				comboImplantBrand.IsEnabled = false;
				comboImplantSystem.IsEnabled = false;
				comboSurgicalKit.IsEnabled = false;
				toothImg.IsEnabled = false;
			}
			else//denture
			{
				m_lowerjaw_path_check.IsChecked = true;
				m_othermodel_path_check.IsChecked = true;

				if (IsAirdentalCase == false) m_ct_path.Text = "";
				browct.Opacity = trans_value;

				m_ct_path_check.IsChecked = true;
				m_upperjaw_path_check.IsChecked = true;

				if (IsAirdentalCase == false) m_upperjaw_path.Text = "";
				m_upperjaw_path.Opacity = trans_value;
				broupper.Opacity = trans_value;
				m_upperjaw_path_check.IsChecked = false;
				m_upperjaw_path_clear.Opacity = trans_value;
			}
		}

		//connector分組用結構
		public struct GCase
		{
			public int c_start;
			public int c_end;          
		}

		private void ComboCeramic_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (ActiveToothType == (int)ToothType.ToothTypeList.NEIGHBOR || ActiveToothType == 0) 
				return;

			for (int i = 0; i < 32; i++)
			{
				if (toothImg.ToothSelectIdx[i])
				{
					ToothData[i].Material_Name = comboCeramic.SelectedItem.ToString();
				}
			}
		}

		private void ComboMetal_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (ActiveToothType == (int)ToothType.ToothTypeList.NEIGHBOR || ActiveToothType == 0)
				return;

			for (int i = 0; i < 32; i++)
			{
				if (toothImg.ToothSelectIdx[i])
				{
					ToothData[i].Material_Name = comboMetal.SelectedItem.ToString();
				}
			}
		}

		private void ComboOther_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (ActiveToothType == (int)ToothType.ToothTypeList.NEIGHBOR || ActiveToothType == 0) 
				return;

			for (int i = 0; i < 32; i++)
			{
				if (toothImg.ToothSelectIdx[i])
				{
					ToothData[i].Material_Name = comboOther.SelectedItem.ToString();
				}
			}
		}

		private void ComboColor_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (ActiveToothType == (int)ToothType.ToothTypeList.NEIGHBOR || ActiveToothType == 0)
				return;

			for (int i = 0; i < 32; i++)
			{
				if (toothImg.ToothSelectIdx[i])
				{
					ToothData[i].Color = comboColor.SelectedItem.ToString();
				}
			}
		}

		private void BtnOK_Click(object sender, RoutedEventArgs e)
		{
			if (true)
			{
				//檢查
				if (m_ct_path.Text == "")
				{
                    Inteware_Messagebox Msg = new Inteware_Messagebox();
                    Msg.ShowMessage("C.B.C.T" + TranslationSource.Instance["DirNotSet"]);
					return;
				}

				Process_Neworder.Visibility = Visibility.Visible;
				Process_Neworder.Minimum = 1;
				Process_Neworder.Value = 1;
				Process_Neworder.Maximum = 100;
				Process_Neworder.SmallChange = 1;

				string targetdirectory = m_ImplantRoot + m_order_num.Text;
				string sourcedirectory = m_ct_path.Text;

				if (!Directory.Exists(targetdirectory))
				{
					Directory.CreateDirectory(targetdirectory);
				}

				//改先存,取得植牙齒位上或下顎
				SaveXml(targetdirectory);

				if (m_ct_path_check.IsChecked.GetValueOrDefault())
				{
					DirectoryInfo disource = new DirectoryInfo(sourcedirectory);
					DirectoryInfo ditarget = new DirectoryInfo(targetdirectory);

					FileInfo[] files = disource.GetFiles();
					Process_Neworder.Maximum = files.Length+4;
					
					//暫定
					//更新頻率
					int kk = 0;
					foreach (var file in Directory.GetFiles(sourcedirectory))
					{
						Process_Neworder.Value += 1;
						kk++;
						
						if (file.IndexOf(".dcm") > 0 &&
						   !(file.IndexOf("ZZProjectCT") > 0))
						{
							File.Copy(file, System.IO.Path.Combine(targetdirectory, System.IO.Path.GetFileName(file)));
							//11/21/2019 handtan//ordermanager新訂單進度列異常修正

							if (kk>=50)
							{
								Prograsee_update((int)Process_Neworder.Value);
								kk = 0;
							}

						}
					}

				}

				if (m_upperjaw_path_check.IsChecked.GetValueOrDefault() && m_upperjaw_path.Text !="")
				{
					string newfile = "";

					if (IsUpperJaw_case) newfile = targetdirectory + "\\" + "Upperjaw.stl";
					else                 newfile = targetdirectory + "\\" + "Lowerjaw.stl";
					
					File.Copy(m_upperjaw_path.Text, newfile,true);
					DirectoryInfo disource = new DirectoryInfo(sourcedirectory);  
				}

				//11/21/2019 handtan//ordermanager新訂單進度列異常修正
				//Process_Neworder.Value += 1;
				Prograsee_update((int)Process_Neworder.Value);

				if (m_lowerjaw_path_check.IsChecked.GetValueOrDefault() && m_lowerjaw_path.Text != "")
				{
					string newfile = "";

					if (IsUpperJaw_case) newfile = targetdirectory + "\\" + "Upperjaw_tray.stl";
					else                 newfile = targetdirectory + "\\" + "Lowerjaw_tray.stl";

					File.Copy(m_lowerjaw_path.Text, newfile, true);
					DirectoryInfo disource = new DirectoryInfo(sourcedirectory);
				}

				//11/21/2019 handtan//ordermanager新訂單進度列異常修正
				Prograsee_update((int)Process_Neworder.Value);

				if (m_othermodel_path_check.IsChecked.GetValueOrDefault() && m_othermodel_path.Text != "")
				{
					string newfile = targetdirectory + "\\" + "Denture.stl";
					File.Copy(m_othermodel_path.Text, newfile, true);
					DirectoryInfo disource = new DirectoryInfo(sourcedirectory);
				}

				//11/21/2019 handtan//ordermanager新訂單進度列異常修正
				Prograsee_update((int)Process_Neworder.Value);
				
				Process_Neworder.Visibility = Visibility.Hidden;
			}
			this.Close();
			FunctionThatRaisesEvent();
		}

		public void Prograsee_update(int new_value)
		{
			Dispatcher.Invoke(new Action<System.Windows.DependencyProperty,
								object>(Process_Neworder.SetValue),
								System.Windows.Threading.DispatcherPriority.Background,
								new object[] { ProgressBar.ValueProperty, Convert.ToDouble(new_value + 1) });
		}

		public void CopyAll(DirectoryInfo source, DirectoryInfo target)
		{
			if (source.FullName.ToLower() == target.FullName.ToLower())
			{
				return;
			}

			// Check if the target directory exists, if not, create it.
			if (Directory.Exists(target.FullName) == false)
			{
				Directory.CreateDirectory(target.FullName);
			}

			// Copy each file into it's new directory.
			foreach (FileInfo fi in source.GetFiles())
			{
				//Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);
				fi.CopyTo(System.IO.Path.Combine(target.ToString(), fi.Name), true);

				Process_Neworder.Value += 1;

				Prograsee_update((int)Process_Neworder.Value);
			}
		}

		private void BtnCancel_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

		public void SetImplantBrand()
		{
			String ImplantBrandFilePath = "..\\OrderData\\IA_ImplantBrane.dat";

			if (File.Exists(ImplantBrandFilePath) == false)
				return;

			StreamReader file = new StreamReader(ImplantBrandFilePath);

			while (file.Peek() !=-1)
			{
				comboImplantBrand.Items.Add(file.ReadLine());
			}

			file.Close();

			//set Scan body combobox
			comboImplantScanBody.Items.Add("TSP");
			comboImplantScanBody.Items.Add("5Axis");
			comboImplantScanBody.Items.Add("TDS");
		}

		private void ComboImplantBrand_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			string brandName = comboImplantBrand.SelectedItem.ToString();
			int brandIndex = comboImplantBrand.SelectedIndex;

			comboImplantModel.Items.Clear();
			comboImplantMaterial.Items.Clear();
			comboImplantScrew.Items.Clear();
			comboImplantSystem.Items.Clear();

			if (brandIndex != -1)
			{
				foreach (ImplantInforElement listbrand in ImplantInforList)
				{
					if (brandName == listbrand.CompanyName)
					{
						foreach (string listsystem in listbrand.SystemName)
						{
							comboImplantSystem.Items.Add(listsystem);
						}
					}
				}
			}
		}

		private void ComboImplantModel_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (comboImplantModel.Items.Count == 0) return;

			string brandName = comboImplantBrand.SelectedItem.ToString();
			string ModelName = comboImplantModel.SelectedItem.ToString();

			comboImplantMaterial.Items.Clear();
			comboImplantScrew.Items.Clear();

			string filename;
			filename = string.Format("..\\OrderData\\IA_Mat_{0}.dat", brandName);

			if (File.Exists(filename) == false)
				return;

			String lineText;
			string[] SplitText;
			StreamReader file = new StreamReader(filename);

			while (file.Peek() != -1)
			{
				lineText = file.ReadLine();
				SplitText = lineText.Split('_');
				if(SplitText[0]==ModelName)
					comboImplantMaterial.Items.Add(SplitText[1]);
			}

			file.Close();
		}

		private void ComboImplantMaterial_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (comboImplantMaterial.Items.Count == 0) return;

			string brandName = comboImplantBrand.SelectedItem.ToString();
			string ModelName = comboImplantModel.SelectedItem.ToString();
			string MatName = comboImplantMaterial.SelectedItem.ToString();

			comboImplantScrew.Items.Clear();

			string filename;
			filename = string.Format("..\\OrderData\\IA_Screw_DoOnly.dat");

			if (File.Exists(filename) == false)
				return;

			bool isDoOnly = false;
			String lineText;
			string[] SplitText;
			StreamReader file = new StreamReader(filename);
			while (file.Peek() != -1)
			{
				lineText = file.ReadLine();
				SplitText = lineText.Split('_');
				if (SplitText[0] == brandName && SplitText[1] == ModelName && SplitText[2] == ModelName)
				{
					comboImplantScrew.Items.Add(SplitText[3]);
					isDoOnly = true;
				}
			}
			file.Close();

			if(isDoOnly==false)
			{
				filename = string.Format("..\\OrderData\\IA_Screw_{0}.dat", MatName);

			   if (File.Exists(filename) == false)
				   return;

			   file = new StreamReader(filename);
			   while (file.Peek() != -1)
			   {
				   comboImplantScrew.Items.Add(file.ReadLine());
			   }
			   file.Close();
			}
		}

		private void ComboImolantScanBody_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (comboImplantModel.Items.Count == 0 ||
				comboImplantMaterial.Items.Count == 0 ||
				comboImplantScrew.Items.Count == 0)
				return;

			for (int i = 0; i < 32; i++)
			{
				if (toothImg.ToothSelectIdx[i])
				{
					if (toothImg.ProductTypeIdx[i] == (int)ToothType.ToothTypeList.ABUTMENT ||
						toothImg.ProductTypeIdx[i] == (int)ToothType.ToothTypeList.ABUTMENT_ANATOMIC_COPING ||
						toothImg.ProductTypeIdx[i] == (int)ToothType.ToothTypeList.ABUTMENT_ANATOMIC_CROWN ||
						toothImg.ProductTypeIdx[i] == (int)ToothType.ToothTypeList.ABUTMENT_TEMP ||
						toothImg.ProductTypeIdx[i] == (int)ToothType.ToothTypeList.SRIA ||
						toothImg.ProductTypeIdx[i] == (int)ToothType.ToothTypeList.SRIA_CROWN)
					{
						ToothData[i].Implant_Brand = comboImplantBrand.SelectedItem.ToString();
						ToothData[i].Implant_Type = comboImplantModel.SelectedItem.ToString();
						ToothData[i].Implant_Material = comboImplantMaterial.SelectedItem.ToString();
						ToothData[i].Implant_Screw = comboImplantScrew.SelectedItem.ToString();
						ToothData[i].Implant_Jig = comboImplantScanBody.SelectedItem.ToString();
					}
				}
			}
		}

		private void ComboImplantScrew_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (comboImplantScrew.SelectedIndex == -1 || comboImplantScanBody.SelectedIndex == -1)
				return;

			for (int i = 0; i < 32; i++)
			{
				if (toothImg.ToothSelectIdx[i])
				{
					if (toothImg.ProductTypeIdx[i] == (int)ToothType.ToothTypeList.ABUTMENT ||
						toothImg.ProductTypeIdx[i] == (int)ToothType.ToothTypeList.ABUTMENT_ANATOMIC_COPING ||
						toothImg.ProductTypeIdx[i] == (int)ToothType.ToothTypeList.ABUTMENT_ANATOMIC_CROWN ||
						toothImg.ProductTypeIdx[i] == (int)ToothType.ToothTypeList.ABUTMENT_TEMP ||
						toothImg.ProductTypeIdx[i] == (int)ToothType.ToothTypeList.SRIA ||
						toothImg.ProductTypeIdx[i] == (int)ToothType.ToothTypeList.SRIA_CROWN)
					{
						ToothData[i].Implant_Brand = comboImplantBrand.SelectedItem.ToString();
						ToothData[i].Implant_Type = comboImplantModel.SelectedItem.ToString();
						ToothData[i].Implant_Material = comboImplantMaterial.SelectedItem.ToString();
						ToothData[i].Implant_Screw = comboImplantScrew.SelectedItem.ToString();
						ToothData[i].Implant_Jig = comboImplantScanBody.SelectedItem.ToString();
					}
				}
			}
		}

		private void Listbox2_MouseDown(object sender, MouseButtonEventArgs e)
		{
			//MessageBox.Show("key down");
			if (listbox2.SelectedIndex != -1)
			{
				UCToothTypeBase uc = listbox2.SelectedItem as UCToothTypeBase;
				ActiveToothType = uc.ProductTypeIdx;
				toothImg.ActiveProductType = ActiveToothType;

				for (int i = 0; i < 32; i++)
				{
					if (toothImg.ToothSelectIdx[i])
					{
						toothImg.SetToothType(i, ActiveToothType);
						Image img = toothImg.GetToothImage(i);

						toothImg.SetToothStatus(img);

						ToothData[i].ResetData();
						ToothData[i].FLAG = true;
						ToothData[i].Product_idx = ActiveToothType;
						ToothData[i].Product = Enum.GetName(typeof(ToothType.ToothTypeList), ActiveToothType);

						if (ActiveToothType != (int)(int)ToothType.ToothTypeList.ABUTMENT &&
							ActiveToothType != (int)(int)ToothType.ToothTypeList.ABUTMENT_TEMP &&
							ActiveToothType != (int)(int)ToothType.ToothTypeList.SRIA &&
							ActiveToothType != (int)(int)ToothType.ToothTypeList.SRIA_CROWN)
						{
							if (Radio_Mat_Ceramic.IsChecked == true)
							{
								ToothData[i].Material_Type = "Ceramic";
								ToothData[i].Material_Name = comboCeramic.SelectedItem.ToString();
							}
							else if (Radio_Mat_Metal.IsChecked == true)
							{
								ToothData[i].Material_Type = "Metal";
								ToothData[i].Material_Name = comboMetal.SelectedItem.ToString();
							}
							else
							{
								ToothData[i].Material_Type = "Other";
								ToothData[i].Material_Name = comboOther.SelectedItem.ToString();
							}

							ToothData[i].Color = comboColor.SelectedItem.ToString();
						}
					}
				}

				//set connector
				if (ActiveToothType != (int)(int)ToothType.ToothTypeList.ABUTMENT &&
					ActiveToothType != (int)(int)ToothType.ToothTypeList.ABUTMENT_TEMP &&
					ActiveToothType != (int)(int)ToothType.ToothTypeList.SRIA &&
					ActiveToothType != (int)(int)ToothType.ToothTypeList.SRIA_CROWN &&
					ActiveToothType != (int)(int)ToothType.ToothTypeList.NEIGHBOR &&
					ActiveToothType != (int)(int)ToothType.ToothTypeList.NON_TOOTH)
				{
					bool doConnector;
					doConnector = false;
					for (int i = 0; i < 31; i++)
					{
						if (toothImg.ToothSelectIdx[i])
						{
							if (toothImg.ToothSelectIdx[i + 1])
							{
								doConnector = true;
								break;
							}
						}
					}

					if (doConnector)
					{
						for (int i = 0; i < 31; i++)
						{
							if (toothImg.ToothSelectIdx[i])
							{
								if (toothImg.ToothSelectIdx[i + 1])
								{
									toothImg.SetConnectorStatus(i, true);
								}
							}
						}

						toothImg.DrawConnectorLine(toothImg.ConnectorIdx);
					}
				}
				else//clear connector status
				{
					for (int i = 0; i < 31; i++)
					{
						if (toothImg.ToothSelectIdx[i])
						{
							toothImg.SetConnectorStatus(i, false);
						}
					}
					toothImg.DrawConnectorLine(toothImg.ConnectorIdx);
				}

			}
			else
			{
				ActiveToothType = -1;
				toothImg.ActiveProductType = ActiveToothType;
			}
		}

		private void Browct_Click(object sender, RoutedEventArgs e)
		{
			System.Windows.Forms.FolderBrowserDialog folderDialog = new System.Windows.Forms.FolderBrowserDialog
			{
				SelectedPath = Selected_folder_path
			};

			System.Windows.Forms.DialogResult result = folderDialog.ShowDialog();

			if (result == System.Windows.Forms.DialogResult.Cancel)
			{
				m_ct_path.Text = m_ct_path.Text;
			}
			else
			{
				m_ct_path.Text = folderDialog.SelectedPath.Trim();

				//5/15/2019 handtan//check the dcm file in selected folder
				if (Check_folder_dcm(m_ct_path.Text) == false)
				{
                    Inteware_Messagebox Msg = new Inteware_Messagebox();
                    Msg.ShowMessage(TranslationSource.Instance["NoCBCTInDir"]);
					m_ct_path.Text = "";
				}
				else
				{
					System.IO.DirectoryInfo directoryInfo = System.IO.Directory.GetParent(m_ct_path.Text);
					Selected_folder_path = directoryInfo.FullName;
				}
			}
		}

		public bool Check_folder_dcm(string targetDirectory)
		{
			DirectoryInfo subfolder = new DirectoryInfo(targetDirectory);
			FileInfo[] files = subfolder.GetFiles("*.dcm", SearchOption.TopDirectoryOnly);

			if (files.Length > 0) return true;
			else return false;
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog dialog = new OpenFileDialog
			{
				//dialog.InitialDirectory = "C:\\";
				InitialDirectory = Selected_folder_path,
				Filter = "Model file (*.stl)|*.stl"
			};
			if (dialog.ShowDialog() == true)
			{
				m_upperjaw_path.Text = dialog.FileName;
				Selected_folder_path = System.IO.Path.GetDirectoryName(dialog.FileName);
			}
		}

		private void Brolower_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog dialog = new OpenFileDialog
			{
				//dialog.InitialDirectory = "C:\\";
				InitialDirectory = Selected_folder_path,
				Filter = "Model file (*.stl)|*.stl"
			};
			if (dialog.ShowDialog() == true)
			{
				m_lowerjaw_path.Text = dialog.FileName;
				Selected_folder_path = System.IO.Path.GetDirectoryName(dialog.FileName);
			}
		}

		private void Broother_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog dialog = new OpenFileDialog
			{
				//dialog.InitialDirectory = "C:\\";
				InitialDirectory = Selected_folder_path,
				Filter = "Model file (*.stl)|*.stl"
			};
			if (dialog.ShowDialog() == true)
			{
				m_othermodel_path.Text = dialog.FileName;
				Selected_folder_path = System.IO.Path.GetDirectoryName(dialog.FileName);
			}
		}

		private void CheckBox_Checked(object sender, RoutedEventArgs e)
		{
			if (m_ct_path_check.IsChecked.HasValue && m_ct_path_check.IsChecked.Value)
			{
			   // m_ct_path_check.IsChecked = false;
			}

			m_ct_path.Opacity = 1.0;
			browct.Opacity = 1.0;
			m_ct_path_clear.Opacity = 1.0;
		}

		private void M_ct_path_check_Unchecked(object sender, RoutedEventArgs e)
		{
			m_ct_path.Opacity = 0.0;
			browct.Opacity = 0.0;
			m_ct_path_clear.Opacity = 0.0;
			if (IsAirdentalCase == false) m_ct_path.Text = "";
		}

		private void M_upperjaw_path_check_Checked(object sender, RoutedEventArgs e)
		{
			m_upperjaw_path.Opacity = 1.0;
			broupper.Opacity = 1.0;
			m_upperjaw_path_clear.Opacity = 1.0;
		}

		private void M_upperjaw_path_check_Unchecked(object sender, RoutedEventArgs e)
		{
			m_upperjaw_path.Opacity = 0.0;
			broupper.Opacity = 0.0;
			m_upperjaw_path_clear.Opacity = 0.0;
			if (IsAirdentalCase == false) m_upperjaw_path.Text = "";
		}

		private void M_lowerjaw_path_check_Checked(object sender, RoutedEventArgs e)
		{
			m_lowerjaw_path.Opacity = 1.0;
			brolower.Opacity = 1.0;
			m_lowerjaw_path_clear.Opacity = 1.0;
		}

		private void M_lowerjaw_path_check_Unchecked(object sender, RoutedEventArgs e)
		{
			m_lowerjaw_path.Opacity = 0.0;
			brolower.Opacity = 0.0;
			m_lowerjaw_path_clear.Opacity = 0.0;
			if (IsAirdentalCase == false) m_lowerjaw_path.Text = "";
		}

		private void M_othermodel_path_check_Checked(object sender, RoutedEventArgs e)
		{
			m_othermodel_path.Opacity = 1.0;
			broother.Opacity = 1.0;
			m_othermodel_path_clear.Opacity = 1.0;
		}

		private void M_othermodel_path_check_Unchecked(object sender, RoutedEventArgs e)
		{
			m_othermodel_path.Opacity = 0.0;
			broother.Opacity = 0.0;
			m_othermodel_path_clear.Opacity = 0.0;
			if (IsAirdentalCase == false) m_othermodel_path.Text = "";
		}

		private void Click_Test(object sender, RoutedEventArgs e)
		{
			DateTime dt = m_patient_birthday.SelectedDate.Value.Date;
			
			//MessageBox.Show(dt.Year.ToString() + dt.Month.ToString() + dt.Day.ToString());
		}

		private void M_ct_path_clear_Click(object sender, RoutedEventArgs e)
		{
			m_ct_path.Text = "";
		}

		private void M_upperjaw_path_clear_Click(object sender, RoutedEventArgs e)
		{
			m_upperjaw_path.Text = "";
		}

		private void M_lowerjaw_path_clear_Click(object sender, RoutedEventArgs e)
		{
			m_lowerjaw_path.Text = "";
		}

		private void M_othermodel_path_clear_Click(object sender, RoutedEventArgs e)
		{
			m_othermodel_path.Text = "";
		}

		private void M_guide_standard_Click(object sender, RoutedEventArgs e)
		{
			m_othermodel_path.Text = "";
			m_othermodel_path.Opacity = 0.5;
			broother.Opacity = 0.5;
			m_othermodel_path_check.IsChecked = false;
		}

		private void O(object sender, MouseEventArgs e)
		{
		   // Process_Neworder.Value++;
		}

		private void Process_Neworder_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			this.Process_Neworder.Value = e.NewValue;
		}
		//---title bar 設定
		private void Window_LocationChanged(object sender, EventArgs e)
		{
			int sum = 0;
			foreach (var item in screens)
			{
				sum += item.WorkingArea.Width;
				if (sum >= this.Left + this.Width / 2)
				{
					this.MaxHeight = item.WorkingArea.Height;
					break;
				}
			}
		}

		private void System_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Left)
			{

			}
			else if (e.ChangedButton == MouseButton.Right)
			{

			}
		}

		[DllImport("user32.dll")]
		static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);
		[DllImport("user32.dll")]
		static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
		[DllImport("user32.dll")]
		static extern int TrackPopupMenu(IntPtr hMenu, uint uFlags, int x, int y, int nReserved, IntPtr hWnd, IntPtr prcRect);

		private void System_MouseMove(object sender, MouseEventArgs e)
		{
			if (e.LeftButton == MouseButtonState.Pressed)
			{
				if (this.WindowState == WindowState.Maximized && Math.Abs(startPos.Y - e.GetPosition(null).Y) > 2)
				{
					var point = PointToScreen(e.GetPosition(null));

					this.WindowState = WindowState.Normal;

					this.Left = point.X - this.ActualWidth / 2;
					this.Top = point.Y - border.ActualHeight / 2;
				}
				DragMove();
			}
		}

		private void Maximize_Click(object sender, RoutedEventArgs e)
		{
			this.WindowState = (this.WindowState == WindowState.Normal) ? WindowState.Maximized : WindowState.Normal;
		}

		private void Close_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

		private void Mimimize_Click(object sender, RoutedEventArgs e)
		{
			this.WindowState = WindowState.Minimized;
		}

		private void Window_StateChanged(object sender, EventArgs e)
		{

		}

		public static byte[] GetJpgImage(UIElement source, int quality)
		{
			double scale = 1.0;
			double actualHeight = source.RenderSize.Height;
			double actualWidth = source.RenderSize.Width;

			double renderHeight = actualHeight * scale;
			double renderWidth = actualWidth * scale;

			RenderTargetBitmap renderTarget = new RenderTargetBitmap((int)renderWidth, (int)renderHeight, 96, 96, PixelFormats.Pbgra32);
			VisualBrush sourceBrush = new VisualBrush(source);

			DrawingVisual drawingVisual = new DrawingVisual();
			DrawingContext drawingContext = drawingVisual.RenderOpen();

			using (drawingContext)
			{
				drawingContext.PushTransform(new ScaleTransform(scale, scale));
				drawingContext.DrawRectangle(sourceBrush, null, new Rect(new Point(0, 0), new Point(actualWidth, actualHeight)));
			}
			renderTarget.Render(drawingVisual);

			JpegBitmapEncoder jpgEncoder = new JpegBitmapEncoder
			{
				QualityLevel = quality
			};
			jpgEncoder.Frames.Add(BitmapFrame.Create(renderTarget));

			Byte[] _imageArray;

			using (MemoryStream outputStream = new MemoryStream())
			{
				jpgEncoder.Save(outputStream);
				_imageArray = outputStream.ToArray();
			}

			return _imageArray;
		}

		private void LoadedNewImplantOrderV2(object sender, RoutedEventArgs e)
		{
			if (haveXml == false)
				DialogResult = false;
		}
	}
}
