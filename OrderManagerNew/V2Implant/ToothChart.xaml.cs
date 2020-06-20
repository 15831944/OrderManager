using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace OrderManagerNew.V2Implant
{
	/// <summary>
	/// Interaction logic for ToothChart.xaml
	/// </summary>
	public partial class ToothChart : UserControl
	{
		public ToothChart()
		{
			InitializeComponent();

			IsShowOnly = false;
			StartImageIndex = -1;

			PressCtrl = false;
			PressShift = false;

			for (int i = 0; i < 32; i++)
			{
				ProductTypeIdx[i] = 0;
				ToothTypeIndex[i] = 0;
				ConnectorIdx[i] = false;
				ToothSelectIdx[i] = false;
			}

			ActiveProductType = -1;
		}

		public static string NormalPath = "ToothImg\\None\\";
		public static string MissPath = "ToothImg\\Pontic\\";
		public static string NeighborPath = "ToothImg\\Neighbor\\";
		public static string CopinglPath = "ToothImg\\Coping\\";
		public static string TypePath = "ToothImg\\";

		public bool IsShowOnly { set; get; }
		public int StartImageIndex { set; get; }
		public int EndImageIndex { set; get; }

		//記錄內部的產品類型
		public int[] ProductTypeIdx = new int[32];

		//記錄目前選取的齒位
		public bool[] ToothSelectIdx = new bool[32];

		//記錄內部的Connector flag
		public bool[] ConnectorIdx = new bool[32];

		//記錄內部的牙齒類型//0: non, 1:coping, 2: pontic, 3:neighbor
		public int[] ToothTypeIndex = new int[32];

		//記錄外部的作動產品類型
		public int ActiveProductType { set; get; }
		public bool PressCtrl { set; get; }
		public bool PressShift { set; get; }

		//取得牙齒圖示檔案名稱
		public string GetToothImgString(int idx)
		{
			if (idx == 0) return "T18.png";
			else if (idx == 1) return "T17.png";
			else if (idx == 2) return "T16.png";
			else if (idx == 3) return "T15.png";
			else if (idx == 4) return "T14.png";
			else if (idx == 5) return "T13.png";
			else if (idx == 6) return "T12.png";
			else if (idx == 7) return "T11.png";
			else if (idx == 8) return "T21.png";
			else if (idx == 9) return "T22.png";
			else if (idx == 10) return "T23.png";
			else if (idx == 11) return "T24.png";
			else if (idx == 12) return "T25.png";
			else if (idx == 13) return "T26.png";
			else if (idx == 14) return "T27.png";
			else if (idx == 15) return "T28.png";
			else if (idx == 16) return "T38.png";
			else if (idx == 17) return "T37.png";
			else if (idx == 18) return "T36.png";
			else if (idx == 19) return "T35.png";
			else if (idx == 20) return "T34.png";
			else if (idx == 21) return "T33.png";
			else if (idx == 22) return "T32.png";
			else if (idx == 23) return "T31.png";
			else if (idx == 24) return "T41.png";
			else if (idx == 25) return "T42.png";
			else if (idx == 26) return "T43.png";
			else if (idx == 27) return "T44.png";
			else if (idx == 28) return "T45.png";
			else if (idx == 29) return "T46.png";
			else if (idx == 30) return "T47.png";
			else if (idx == 31) return "T48.png";
			else return "";
		}

		public string GetToothTypeImgString(int idx)
		{
			if ((int)ToothType.ToothTypeList.IMPLANT == idx)
				return "icon_implant.png";
			else
				return "ttype_no.png";
		}

		//return tooth type index, mean 0, 1, 2, 3
		public int GetToothTypeIndex(int idx)
		{

			switch (idx)
			{
				case (int)ToothType.ToothTypeList.NON_TOOTH:
					return 0;
				//break;
				case (int)ToothType.ToothTypeList.OFFSET_COPING:
				case (int)ToothType.ToothTypeList.ANATOMIC_COPING:
				case (int)ToothType.ToothTypeList.ANATOMIC_CROWN:
				case (int)ToothType.ToothTypeList.VENEER:
				case (int)ToothType.ToothTypeList.ABUTMENT:
				case (int)ToothType.ToothTypeList.ABUTMENT_OFFSET_COPING:
				case (int)ToothType.ToothTypeList.ABUTMENT_ANATOMIC_COPING:
				case (int)ToothType.ToothTypeList.ABUTMENT_ANATOMIC_CROWN:
				case (int)ToothType.ToothTypeList.ABUTMENT_TEMP:
				case (int)ToothType.ToothTypeList.SRIA:
				case (int)ToothType.ToothTypeList.SRIA_CROWN:
					return 1;
				//break;
				case (int)ToothType.ToothTypeList.OFFSET_PONTIC:
				case (int)ToothType.ToothTypeList.REDUCED_PONTIC:
				case (int)ToothType.ToothTypeList.ANATOMIC_PONTIC:
					return 2;
				//break;
				case (int)ToothType.ToothTypeList.NEIGHBOR:
					return 3;
				//break;
				default:
					break;
			}

			return -1;
		}

		//取得牙齒圖示路徑
		public string GetStatusString(int idx)
		{
			if (idx == 0) return NormalPath;
			else if (idx == 1) return CopinglPath;
			else if (idx == 2) return MissPath;
			else return NeighborPath;
		}

		//設定牙齒圖示
		public void UpdateToothChart(int[] toothidx)
		{
			string imgpath = "";

			for (int i = 0; i < 32; i++)
			{
				imgpath = GetStatusString(toothidx[i]) + GetToothImgString(i);
				Image img = GetToothImage(i);
				img.BeginInit();
				img.Source = new BitmapImage(new Uri(imgpath, UriKind.RelativeOrAbsolute));
				img.EndInit();
			}
		}

		//設計connector圖案的style
		public void UpdateConnector(Boolean[] toothidx)
		{
			for (int i = 0; i < 31; i++)
			{
				if (i == 15)
					continue;

				Ellipse e = GetConnectorImage(i);
				if (toothidx[i] == true)
					e.Style = FindResource("ToothConnect") as Style;
				else
					e.Style = FindResource("DisToothConnect") as Style;
			}

			DrawConnectorLine(toothidx);
		}

		//設定牙齒產品
		public void UpdateToothType(int[] toothidx)
		{
			string imgpath = "";

			for (int i = 0; i < 32; i++)
			{
				imgpath = TypePath + GetToothTypeImgString(toothidx[i]);
				Image img = GetToothTypeImage(i);
				ProductTypeIdx[i] = toothidx[i];
				ToothTypeIndex[i] = GetToothTypeIndex(toothidx[i]);
				img.BeginInit();
				img.Source = new BitmapImage(new Uri(imgpath, UriKind.RelativeOrAbsolute));
				img.EndInit();
			}
		}

		public void SetToothType(int toothidx, int toothTypeidx)
		{
			string imgpath = "";
			imgpath = TypePath + GetToothTypeImgString(toothTypeidx);
			Image img = GetToothTypeImage(toothidx);
			ProductTypeIdx[toothidx] = toothTypeidx;
			ToothTypeIndex[toothidx] = GetToothTypeIndex(toothTypeidx);

			//設定取消或設定鄰牙要把connector取消
			if (ProductTypeIdx[toothidx] == (int)ToothType.ToothTypeList.NON_TOOTH ||
				ProductTypeIdx[toothidx] == (int)ToothType.ToothTypeList.NEIGHBOR ||
				ProductTypeIdx[toothidx] == (int)ToothType.ToothTypeList.ABUTMENT ||
				ProductTypeIdx[toothidx] == (int)ToothType.ToothTypeList.SRIA ||
				ProductTypeIdx[toothidx] == (int)ToothType.ToothTypeList.SRIA_CROWN ||
				ProductTypeIdx[toothidx] == (int)ToothType.ToothTypeList.ABUTMENT_TEMP)
			{
				Ellipse es = GetConnectorImage(toothidx);
				if (es != null)//if i==15 es will be null
				{
					es.Tag = "0";
					es.Style = FindResource("UnSetToothConnect") as Style;
				}

				ConnectorIdx[toothidx] = false;

				if (toothidx != 0)
				{
					es = GetConnectorImage(toothidx - 1);
					es.Tag = "0";
					es.Style = FindResource("UnSetToothConnect") as Style;
					ConnectorIdx[toothidx - 1] = false;
				}
			}

			img.BeginInit();
			img.Source = new BitmapImage(new Uri(imgpath, UriKind.RelativeOrAbsolute));
			img.EndInit();
		}

		public int GetToothType(string toothName)
		{
			return ProductTypeIdx[GetToothImageIndex(toothName)];
		}

		public void SetConnectorAllUnset()
		{
			for (int i = 0; i < 31; i++)
			{
				ConnectorIdx[i] = false;

				if (i == 15)
					continue;

				Ellipse e = GetConnectorImage(i);
				e.Tag = "0";
				e.Style = FindResource("UnSetToothConnect") as Style;
			}

			DrawConnectorLine(ConnectorIdx);
		}

		public Ellipse GetConnectorImage(int index)
		{
			if (index == 0) return C01;
			else if (index == 1) return C02;
			else if (index == 2) return C03;
			else if (index == 3) return C04;
			else if (index == 4) return C05;
			else if (index == 5) return C06;
			else if (index == 6) return C07;
			else if (index == 7) return C08;
			else if (index == 8) return C09;
			else if (index == 9) return C10;
			else if (index == 10) return C11;
			else if (index == 11) return C12;
			else if (index == 12) return C13;
			else if (index == 13) return C14;
			else if (index == 14) return C15;
			// else if (index == 15) return tcT28;
			else if (index == 16) return C17;
			else if (index == 17) return C18;
			else if (index == 18) return C19;
			else if (index == 19) return C20;
			else if (index == 20) return C21;
			else if (index == 21) return C22;
			else if (index == 22) return C23;
			else if (index == 23) return C24;
			else if (index == 24) return C25;
			else if (index == 25) return C26;
			else if (index == 26) return C27;
			else if (index == 27) return C28;
			else if (index == 28) return C29;
			else if (index == 29) return C30;
			else if (index == 30) return C31;
			//else if (index == 31) return tcT48;

			return null;
		}

		public int GetConnectorImageIndex(Ellipse e)
		{
			if (e.Name == "C01") return 0;
			else if (e.Name == "C02") return 1;
			else if (e.Name == "C03") return 2;
			else if (e.Name == "C04") return 3;
			else if (e.Name == "C05") return 4;
			else if (e.Name == "C06") return 5;
			else if (e.Name == "C07") return 6;
			else if (e.Name == "C08") return 7;
			else if (e.Name == "C09") return 8;
			else if (e.Name == "C10") return 9;
			else if (e.Name == "C11") return 10;
			else if (e.Name == "C12") return 11;
			else if (e.Name == "C13") return 12;
			else if (e.Name == "C14") return 13;
			else if (e.Name == "C15") return 14;

			else if (e.Name == "C17") return 16;
			else if (e.Name == "C18") return 17;
			else if (e.Name == "C19") return 18;
			else if (e.Name == "C20") return 19;
			else if (e.Name == "C21") return 20;
			else if (e.Name == "C22") return 21;
			else if (e.Name == "C23") return 22;
			else if (e.Name == "C24") return 23;
			else if (e.Name == "C25") return 24;
			else if (e.Name == "C26") return 25;
			else if (e.Name == "C27") return 26;
			else if (e.Name == "C28") return 27;
			else if (e.Name == "C29") return 28;
			else if (e.Name == "C30") return 29;
			else if (e.Name == "C31") return 30;

			return -1;
		}

		public int GetToothImageIndex(String name)
		{
			int index = -1;

			if (name == "tcT18") index = 0;
			else if (name == "tcT17") index = 1;
			else if (name == "tcT16") index = 2;
			else if (name == "tcT15") index = 3;
			else if (name == "tcT14") index = 4;
			else if (name == "tcT13") index = 5;
			else if (name == "tcT12") index = 6;
			else if (name == "tcT11") index = 7;

			else if (name == "tcT21") index = 8;
			else if (name == "tcT22") index = 9;
			else if (name == "tcT23") index = 10;
			else if (name == "tcT24") index = 11;
			else if (name == "tcT25") index = 12;
			else if (name == "tcT26") index = 13;
			else if (name == "tcT27") index = 14;
			else if (name == "tcT28") index = 15;

			else if (name == "tcT38") index = 16;
			else if (name == "tcT37") index = 17;
			else if (name == "tcT36") index = 18;
			else if (name == "tcT35") index = 19;
			else if (name == "tcT34") index = 20;
			else if (name == "tcT33") index = 21;
			else if (name == "tcT32") index = 22;
			else if (name == "tcT31") index = 23;

			else if (name == "tcT41") index = 24;
			else if (name == "tcT42") index = 25;
			else if (name == "tcT43") index = 26;
			else if (name == "tcT44") index = 27;
			else if (name == "tcT45") index = 28;
			else if (name == "tcT46") index = 29;
			else if (name == "tcT47") index = 30;
			else if (name == "tcT48") index = 31;

			return index;

		}

		public Image GetToothImage(int index)
		{
			if (index == 0) return tcT18;
			else if (index == 1) return tcT17;
			else if (index == 2) return tcT16;
			else if (index == 3) return tcT15;
			else if (index == 4) return tcT14;
			else if (index == 5) return tcT13;
			else if (index == 6) return tcT12;
			else if (index == 7) return tcT11;
			else if (index == 8) return tcT21;
			else if (index == 9) return tcT22;
			else if (index == 10) return tcT23;
			else if (index == 11) return tcT24;
			else if (index == 12) return tcT25;
			else if (index == 13) return tcT26;
			else if (index == 14) return tcT27;
			else if (index == 15) return tcT28;
			else if (index == 16) return tcT38;
			else if (index == 17) return tcT37;
			else if (index == 18) return tcT36;
			else if (index == 19) return tcT35;
			else if (index == 20) return tcT34;
			else if (index == 21) return tcT33;
			else if (index == 22) return tcT32;
			else if (index == 23) return tcT31;
			else if (index == 24) return tcT41;
			else if (index == 25) return tcT42;
			else if (index == 26) return tcT43;
			else if (index == 27) return tcT44;
			else if (index == 28) return tcT45;
			else if (index == 29) return tcT46;
			else if (index == 30) return tcT47;
			else if (index == 31) return tcT48;

			return null;
		}

		public Image GetToothTypeImage(int index)
		{
			if (index == 0) return T18_Type;
			else if (index == 1) return T17_Type;
			else if (index == 2) return T16_Type;
			else if (index == 3) return T15_Type;
			else if (index == 4) return T14_Type;
			else if (index == 5) return T13_Type;
			else if (index == 6) return T12_Type;
			else if (index == 7) return T11_Type;
			else if (index == 8) return T21_Type;
			else if (index == 9) return T22_Type;
			else if (index == 10) return T23_Type;
			else if (index == 11) return T24_Type;
			else if (index == 12) return T25_Type;
			else if (index == 13) return T26_Type;
			else if (index == 14) return T27_Type;
			else if (index == 15) return T28_Type;
			else if (index == 16) return T38_Type;
			else if (index == 17) return T37_Type;
			else if (index == 18) return T36_Type;
			else if (index == 19) return T35_Type;
			else if (index == 20) return T34_Type;
			else if (index == 21) return T33_Type;
			else if (index == 22) return T32_Type;
			else if (index == 23) return T31_Type;
			else if (index == 24) return T41_Type;
			else if (index == 25) return T42_Type;
			else if (index == 26) return T43_Type;
			else if (index == 27) return T44_Type;
			else if (index == 28) return T45_Type;
			else if (index == 29) return T46_Type;
			else if (index == 30) return T47_Type;
			else if (index == 31) return T48_Type;

			return null;
		}

		public event RoutedEventHandler ImageClick;

		private void ToothImg_MouseLBtnDown(object sender, MouseButtonEventArgs e)
		{
			if (IsShowOnly) return;


			if (!(sender is Image img))
			{
				//reset tooth select index
				for (int i = 0; i < 32; i++)
				{
					Image sI = GetToothImage(i);
					sI.Tag = "0";
					sI.Style = FindResource("DisToothSelect") as Style;

					ToothSelectIdx[i] = false;
				}

				return;
			}

			PressShift = false;
			if (System.Windows.Forms.Control.ModifierKeys == System.Windows.Forms.Keys.Shift)
				PressShift = true;

			PressCtrl = false;
			if (System.Windows.Forms.Control.ModifierKeys == System.Windows.Forms.Keys.Control)
				PressCtrl = true;

			int tag = Convert.ToInt32(img.Tag);

			//set select
			ToothSelectIdx[GetToothImageIndex(img.Name)] = true;

			//單選
			if (PressShift == false)
			{
				StartImageIndex = GetToothImageIndex(img.Name);

				//Ctrl複選
				if (PressCtrl)
				{
					if (tag == 1)
					{
						img.Tag = "0";
						img.Style = FindResource("DisToothSelect") as Style;

						StartImageIndex = -1;
					}
					else
					{
						img.Tag = "1";
						img.Style = FindResource("ToothSelect") as Style;

						StartImageIndex = GetToothImageIndex(img.Name);
						ToothSelectIdx[StartImageIndex] = true;
					}
				}
				else
				{
					for (int i = 0; i < 32; i++)
					{
						if (i == StartImageIndex)
						{
							img.Tag = "1";
							img.Style = FindResource("ToothSelect") as Style;

							StartImageIndex = GetToothImageIndex(img.Name);
							ToothSelectIdx[i] = true;
						}
						else
						{
							Image sI = GetToothImage(i);
							sI.Tag = "0";
							sI.Style = FindResource("DisToothSelect") as Style;
							ToothSelectIdx[i] = false;
						}
					}

					//send click event to winNewOrder class
					ImageClick?.Invoke(sender, new RoutedEventArgs());
				}


			}
			else//shift連選
			{
				if (StartImageIndex != -1)
				{
					int sImageIndex = StartImageIndex;
					int eImageIndex = GetToothImageIndex(img.Name);

					//save select end index
					EndImageIndex = eImageIndex;

					if (eImageIndex < sImageIndex)
					{
						int tmp = sImageIndex;
						sImageIndex = eImageIndex;
						eImageIndex = tmp;
					}

					if (sImageIndex < 15 && eImageIndex > 15)
						return;

					for (int i = 0; i < 32; i++)
					{
						if (i < sImageIndex || i > eImageIndex)
						{
							Image sI = GetToothImage(i);
							sI.Tag = "0";
							sI.Style = FindResource("DisToothSelect") as Style;
						}
						else
						{
							Image sI = GetToothImage(i);
							sI.Tag = "1";
							sI.Style = FindResource("ToothSelect") as Style;

							ToothSelectIdx[i] = true;
						}

					}
				}
			}
		}

		private void ToothImg_MouseEnter(object sender, MouseEventArgs e)
		{
			if (IsShowOnly) return;

			//             Image img = sender as Image;
			//             String imgName = img.Name;
			//             Storyboard sb = (Storyboard)TryFindResource("OnMouseEnter");
			//             foreach (var animation in sb.Children)
			//             {
			//                 Storyboard.SetTargetName(animation, img.Name);
			//                 Storyboard.SetTarget(animation, img);
			//             }            
			//             sb.Begin();
		}

		private void ToothImg_MouseLeave(object sender, MouseEventArgs e)
		{
			if (IsShowOnly) return;

			//             Image img = sender as Image;
			//             String imgName = img.Name;
			//             Storyboard sb = (Storyboard)TryFindResource("OnMouseLeave");
			//             foreach (var animation in sb.Children)
			//             {
			//                 Storyboard.SetTargetName(animation, img.Name);
			//                 Storyboard.SetTarget(animation, img);
			//             }
			//             sb.Begin();
		}

		public void SetAllImageUnSelectStatus()
		{
			for (int i = 0; i < 32; i++)
			{
				Image sI = GetToothImage(i);
				sI.Tag = "0";
				sI.Style = FindResource("DisToothSelect") as Style;
				ProductTypeIdx[i] = 0;
				ToothTypeIndex[i] = 0;
				ToothSelectIdx[i] = false;

				String imgpath = GetStatusString(ToothTypeIndex[i]) + GetToothImgString(i);
				sI.BeginInit();
				sI.Source = new BitmapImage(new Uri(imgpath, UriKind.RelativeOrAbsolute));
				sI.EndInit();
			}
		}

		public void SetToothStatus(Image img/*, bool isSelect*/)
		{
			/* if (isSelect)
			 {
				 //img.Tag = "1";
				 //img.Style = FindResource("ToothSelect") as Style;
				 //ProductTypeIdx[GetToothImageIndex(img.Name)] = 0;

				 int index = GetToothImageIndex(img.Name);
				 String imgpath = GetStatusString(ToothTypeIndex[index]) + GetToothImgString(index);
				 img.BeginInit();
				 img.Source = new BitmapImage(new Uri(imgpath, UriKind.RelativeOrAbsolute));
				 img.EndInit();
			 }
			 else
			 {
				 //img.Tag = "0";
				 //img.Style = FindResource("DisToothSelect") as Style;
				 int index = GetToothImageIndex(img.Name);
				 ProductTypeIdx[index] = 0;
				 ToothTypeIndex[index] = 0;

				 String imgpath = GetStatusString(ToothTypeIndex[index]) + GetToothImgString(index);
				 img.BeginInit();
				 img.Source = new BitmapImage(new Uri(imgpath, UriKind.RelativeOrAbsolute));
				 img.EndInit();
			 }*/

			int index = GetToothImageIndex(img.Name);
			String imgpath = GetStatusString(ToothTypeIndex[index]) + GetToothImgString(index);
			img.BeginInit();
			img.Source = new BitmapImage(new Uri(imgpath, UriKind.RelativeOrAbsolute));
			img.EndInit();
		}

		public void SetAllConnectorToNon()
		{
			for (int i = 0; i < 31; i++)
			{
				if (i == 15)
					continue;

				Ellipse e = GetConnectorImage(i);
				e.Style = FindResource("DisToothConnect") as Style;
			}
		}

		public void SetAllImageTypeToNon()
		{
			for (int i = 0; i < 32; i++)
			{
				ProductTypeIdx[i] = 0;
				ToothTypeIndex[i] = 0;
				Image img = GetToothTypeImage(i);
				string imgpath = TypePath + GetToothTypeImgString(0);
				img.BeginInit();
				img.Source = new BitmapImage(new Uri(imgpath, UriKind.RelativeOrAbsolute));
				img.EndInit();
			}
		}

		private void Connector_MouseLBtnDown(object sender, MouseButtonEventArgs e)
		{
			if (IsShowOnly)
				return;

			Ellipse img = sender as Ellipse;

			int conIdx = GetConnectorImageIndex(img);
			if (ToothTypeIndex[conIdx] == 0 ||
				ToothTypeIndex[conIdx] == 3 ||
				ToothTypeIndex[conIdx + 1] == 0 ||
				ToothTypeIndex[conIdx + 1] == 3)
				return;

			int tag = Convert.ToInt32(img.Tag);
			if (tag == 1)
			{
				img.Tag = "0";
				img.Style = FindResource("UnSetToothConnect") as Style;
				ConnectorIdx[conIdx] = false;
			}
			else
			{
				img.Tag = "1";
				img.Style = FindResource("SetToothConnect") as Style;
				ConnectorIdx[conIdx] = true;
			}

			DrawConnectorLine(ConnectorIdx);
		}

		public void SetConnectorStatus(int idx, bool status)
		{
			if (status)
			{
				Ellipse es = GetConnectorImage(idx);
				es.Tag = "1";
				es.Style = FindResource("SetToothConnect") as Style;
				ConnectorIdx[idx] = true;
			}
			else
			{
				Ellipse es = GetConnectorImage(idx);
				es.Tag = "0";
				es.Style = FindResource("UnSetToothConnect") as Style;
				ConnectorIdx[idx] = false;
			}
		}

		public void ClearSelection()
		{
			SetAllImageUnSelectStatus();
			SetAllImageTypeToNon();
			SetConnectorAllUnset();

			StartImageIndex = -1;

			return;
		}

		public void DrawConnectorLine(Boolean[] toothidx)
		{
			canvas_connector.Children.Clear();

			for (int i = 0; i < 15; i++)
			{
				if (toothidx[i] && toothidx[i + 1])
				{
					Ellipse e1 = GetConnectorImage(i);
					Ellipse e2 = GetConnectorImage(i + 1);

					Line myLine = new Line
					{
						Stroke = System.Windows.Media.Brushes.Orange,
						X1 = Canvas.GetLeft(e1) + 5,
						X2 = Canvas.GetLeft(e2) + 5,
						Y1 = Canvas.GetTop(e1) + 5,
						Y2 = Canvas.GetTop(e2) + 5,
						StrokeThickness = 6
					};
					canvas_connector.Children.Add(myLine);
				}
			}

			for (int i = 16; i < 31; i++)
			{
				if (toothidx[i] && toothidx[i + 1])
				{
					Ellipse e1 = GetConnectorImage(i);
					Ellipse e2 = GetConnectorImage(i + 1);

					Line myLine = new Line
					{
						Stroke = System.Windows.Media.Brushes.Orange,
						X1 = Canvas.GetLeft(e1) + 5,
						X2 = Canvas.GetLeft(e2) + 5,
						Y1 = Canvas.GetTop(e1) + 5,
						Y2 = Canvas.GetTop(e2) + 5,
						StrokeThickness = 6
					};
					canvas_connector.Children.Add(myLine);
				}
			}
		}
	}
}
