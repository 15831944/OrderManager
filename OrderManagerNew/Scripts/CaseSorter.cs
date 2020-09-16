using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CadInformation = OrderManagerNew.Local_UserControls.Order_cadBase.CadInformation;
using TrayInformation = OrderManagerNew.Local_UserControls.Order_tsBase.TrayInformation;
using SplintInformation = OrderManagerNew.Local_UserControls.Order_tsBase.SplintInformation;
using ImplantOuterInformation = OrderManagerNew.Local_UserControls.Order_implantBase.ImplantOuterInformation;
using OrthoOuterInformation = OrderManagerNew.Local_UserControls.Order_orthoBase.OrthoOuterInformation;

//快速排序演算法: https://dotblogs.com.tw/kennyshu/2009/10/24/11270

namespace OrderManagerNew
{
    class CaseSorter
    {
        #region EZCAD
        private void Filter_EZCAD(List<CadInformation> array, ref int arrayLastIndex)
        {
            for(int i=0; i<array.Count; i++)
            {
                if(Properties.OrderManagerProps.Default.PatientNameFilter != "")
                {
                    if(array[i].PatientName.ToLower().IndexOf(Properties.OrderManagerProps.Default.PatientNameFilter.ToLower()) == -1)
                    {
                        array.RemoveAt(i);
                        --i;
                        continue;
                    }
                }
                else if(Properties.OrderManagerProps.Default.CaseNameFilter != "")
                {
                    if (array[i].OrderID.ToLower().IndexOf(Properties.OrderManagerProps.Default.CaseNameFilter.ToLower()) == -1)
                    {
                        array.RemoveAt(i);
                        --i;
                        continue;
                    }
                }

                switch (Properties.OrderManagerProps.Default.DateFilter)
                {
                    case (int)_DateFilter.Today:
                        {
                            if (array[i].CreateDate.ToLongDateString() != DateTime.Today.ToLongDateString())
                            {
                                array.RemoveAt(i);
                                --i;
                            }   
                            break;
                        }
                    case (int)_DateFilter.ThisWeek:
                        {
                            if (array[i].CreateDate < DateTime.Today.AddDays(-7))
                            {
                                array.RemoveAt(i);
                                --i;
                            }   
                            break;
                        }
                    case (int)_DateFilter.LastTwoWeek:
                        {
                            if (array[i].CreateDate < DateTime.Today.AddDays(-14))
                            {
                                array.RemoveAt(i);
                                --i;
                            }   
                            break;
                        }
                }
            }
            arrayLastIndex = array.Count - 1;
        }
        private void Swap_EZCADCase(List<CadInformation> array, int left, int right)
        {
            CadInformation tmpCase = array[left];
            array[left] = array[right];
            array[right] = tmpCase;
        }
        /// <summary>
        /// EZCAD專案排序(依創建日期)
        /// </summary>
        /// <param name="array">專案陣列</param>
        /// <param name="left">輸入0</param>
        /// <param name="right">輸入array.count()-1</param>
        /// <param name="FirstIn">輸入true</param>
        public void Sort_EZCAD(List<CadInformation> array, int left, int right, bool FirstIn)
        {
            if (FirstIn == true)
                Filter_EZCAD(array, ref right);

            if (left < right)
            {
                int i = left - 1;   //left margin
                int j = right + 1;  //right margin
                CadInformation axle = array[(left + right) / 2];  //axle

                while (true)
                {
                    while (array[++i].CreateDate > axle.CreateDate) ;
                    while (array[--j].CreateDate < axle.CreateDate) ;
                    if (i >= j)
                        break;

                    Swap_EZCADCase(array, i, j);
                }

                Sort_EZCAD(array, left, (i-1), false);
                Sort_EZCAD(array, (j+1), right, false);
            }
        }
        #endregion

        #region Implant
        private void Filter_Implant(List<ImplantOuterInformation> array, ref int arrayLastIndex)
        {
            for (int i = 0; i < array.Count; i++)
            {
                if (Properties.OrderManagerProps.Default.PatientNameFilter != "")
                {
                    if (array[i].PatientName.ToLower().IndexOf(Properties.OrderManagerProps.Default.PatientNameFilter.ToLower()) == -1)
                    {
                        array.RemoveAt(i);
                        --i;
                        continue;
                    }
                }
                else if (Properties.OrderManagerProps.Default.CaseNameFilter != "")
                {
                    if (array[i].OrderNumber.ToLower().IndexOf(Properties.OrderManagerProps.Default.CaseNameFilter.ToLower()) == -1)
                    {
                        array.RemoveAt(i);
                        --i;
                        continue;
                    }
                }

                switch (Properties.OrderManagerProps.Default.DateFilter)
                {
                    case (int)_DateFilter.Today:
                        {
                            if (array[i].CreateDate.ToLongDateString() != DateTime.Today.ToLongDateString())
                            {
                                array.RemoveAt(i);
                                --i;
                            }
                            break;
                        }
                    case (int)_DateFilter.ThisWeek:
                        {
                            if (array[i].CreateDate < DateTime.Today.AddDays(-7))
                            {
                                array.RemoveAt(i);
                                --i;
                            }
                            break;
                        }
                    case (int)_DateFilter.LastTwoWeek:
                        {
                            if (array[i].CreateDate < DateTime.Today.AddDays(-14))
                            {
                                array.RemoveAt(i);
                                --i;
                            }
                            break;
                        }
                }
            }
            arrayLastIndex = array.Count - 1;
        }
        private void Swap_ImplantCase(List<ImplantOuterInformation> array, int left, int right)
        {
            ImplantOuterInformation tmpCase = array[left];
            array[left] = array[right];
            array[right] = tmpCase;
        }
        /// <summary>
        /// PrintIn ImplantPlanning專案排序(依創建日期)
        /// </summary>
        /// <param name="array">專案陣列</param>
        /// <param name="left">輸入0</param>
        /// <param name="right">輸入array.count()-1</param>
        /// <param name="FirstIn">輸入true</param>
        public void Sort_Implant(List<ImplantOuterInformation> array, int left, int right, bool FirstIn)
        {
            if (FirstIn == true)
                Filter_Implant(array, ref right);

            if (left < right)
            {
                int i = left - 1;   //left margin
                int j = right + 1;  //right margin
                ImplantOuterInformation axle = array[(left + right) / 2];  //axle

                while (true)
                {
                    while (array[++i].CreateDate > axle.CreateDate) ;
                    while (array[--j].CreateDate < axle.CreateDate) ;
                    if (i >= j)
                        break;

                    Swap_ImplantCase(array, i, j);
                }

                Sort_Implant(array, left, (i-1), false);
                Sort_Implant(array, (j+1), right, false);
            }
        }
        #endregion

        #region Ortho
        private void Filter_Ortho(List<OrthoOuterInformation> array, ref int arrayLastIndex)
        {
            for (int i = 0; i < array.Count; i++)
            {
                if (Properties.OrderManagerProps.Default.PatientNameFilter != "")
                {
                    if (array[i].PatientName.ToLower().IndexOf(Properties.OrderManagerProps.Default.PatientNameFilter.ToLower()) == -1)
                    {
                        array.RemoveAt(i);
                        --i;
                        continue;
                    }
                }
                else if (Properties.OrderManagerProps.Default.CaseNameFilter != "")
                {
                    if (array[i].PatientID.ToLower().IndexOf(Properties.OrderManagerProps.Default.CaseNameFilter.ToLower()) == -1)
                    {
                        array.RemoveAt(i);
                        --i;
                        continue;
                    }
                }

                switch (Properties.OrderManagerProps.Default.DateFilter)
                {
                    case (int)_DateFilter.Today:
                        {
                            if (array[i].CreateDate.ToLongDateString() != DateTime.Today.ToLongDateString())
                            {
                                array.RemoveAt(i);
                                --i;
                            }
                            break;
                        }
                    case (int)_DateFilter.ThisWeek:
                        {
                            if (array[i].CreateDate < DateTime.Today.AddDays(-7))
                            {
                                array.RemoveAt(i);
                                --i;
                            }
                            break;
                        }
                    case (int)_DateFilter.LastTwoWeek:
                        {
                            if (array[i].CreateDate < DateTime.Today.AddDays(-14))
                            {
                                array.RemoveAt(i);
                                --i;
                            }
                            break;
                        }
                }
            }
            arrayLastIndex = array.Count - 1;
        }
        private void Swap_OrthoCase(List<OrthoOuterInformation> array, int left, int right)
        {
            OrthoOuterInformation tmpCase = array[left];
            array[left] = array[right];
            array[right] = tmpCase;
        }
        /// <summary>
        /// Ortho專案排序(依創建日期)
        /// </summary>
        /// <param name="array">專案陣列</param>
        /// <param name="left">輸入0</param>
        /// <param name="right">輸入array.count()-1</param>
        /// <param name="FirstIn">輸入true</param>
        public void Sort_Ortho(List<OrthoOuterInformation> array, int left, int right, bool FirstIn)
        {
            if (FirstIn == true)
                Filter_Ortho(array, ref right);

            if (left < right)
            {
                int i = left - 1;   //left margin
                int j = right + 1;  //right margin
                OrthoOuterInformation axle = array[(left + right) / 2];  //axle

                while (true)
                {
                    while (array[++i].CreateDate > axle.CreateDate) ;
                    while (array[--j].CreateDate < axle.CreateDate) ;
                    if (i >= j)
                        break;

                    Swap_OrthoCase(array, i, j);
                }

                Sort_Ortho(array, left, (i-1), false);
                Sort_Ortho(array, (j+1), right, false);
            }
        }
        #endregion

        #region Tray
        private void Filter_Tray(List<TrayInformation> array, ref int arrayLastIndex)
        {
            for (int i = 0; i < array.Count; i++)
            {
                if (Properties.OrderManagerProps.Default.CaseNameFilter != "")
                {
                    if (array[i].OrderID.ToLower().IndexOf(Properties.OrderManagerProps.Default.CaseNameFilter.ToLower()) == -1)
                    {
                        array.RemoveAt(i);
                        --i;
                        continue;
                    }
                }

                switch (Properties.OrderManagerProps.Default.DateFilter)
                {
                    case (int)_DateFilter.Today:
                        {
                            if (array[i].CreateDate.ToLongDateString() != DateTime.Today.ToLongDateString())
                            {
                                array.RemoveAt(i);
                                --i;
                            }
                            break;
                        }
                    case (int)_DateFilter.ThisWeek:
                        {
                            if (array[i].CreateDate < DateTime.Today.AddDays(-7))
                            {
                                array.RemoveAt(i);
                                --i;
                            }
                            break;
                        }
                    case (int)_DateFilter.LastTwoWeek:
                        {
                            if (array[i].CreateDate < DateTime.Today.AddDays(-14))
                            {
                                array.RemoveAt(i);
                                --i;
                            }
                            break;
                        }
                }
            }
            arrayLastIndex = array.Count - 1;
        }
        private void Swap_TrayCase(List<TrayInformation> array, int left, int right)
        {
            TrayInformation tmpCase = array[left];
            array[left] = array[right];
            array[right] = tmpCase;
        }
        /// <summary>
        /// Tray專案排序(依創建日期)
        /// </summary>
        /// <param name="array">專案陣列</param>
        /// <param name="left">輸入0</param>
        /// <param name="right">輸入array.count()-1</param>
        /// <param name="FirstIn">輸入true</param>
        public void Sort_Tray(List<TrayInformation> array, int left, int right, bool FirstIn)
        {
            if (FirstIn == true)
                Filter_Tray(array, ref right);

            if (left < right)
            {
                int i = left - 1;   //left margin
                int j = right + 1;  //right margin
                TrayInformation axle = array[(left + right) / 2];  //axle

                while (true)
                {
                    while (array[++i].CreateDate > axle.CreateDate) ;
                    while (array[--j].CreateDate < axle.CreateDate) ;
                    if (i >= j)
                        break;

                    Swap_TrayCase(array, i, j);
                }

                Sort_Tray(array, left, (i-1), false);
                Sort_Tray(array, (j+1), right, false);
            }
        }
        #endregion

        #region Splint
        private void Filter_Splint(List<SplintInformation> array, ref int arrayLastIndex)
        {
            for (int i = 0; i < array.Count; i++)
            {
                if (Properties.OrderManagerProps.Default.CaseNameFilter != "")
                {
                    if (array[i].OrderID.ToLower().IndexOf(Properties.OrderManagerProps.Default.CaseNameFilter.ToLower()) == -1)
                    {
                        array.RemoveAt(i);
                        --i;
                        continue;
                    }
                }

                switch (Properties.OrderManagerProps.Default.DateFilter)
                {
                    case (int)_DateFilter.Today:
                        {
                            if (array[i].CreateDate.ToLongDateString() != DateTime.Today.ToLongDateString())
                            {
                                array.RemoveAt(i);
                                --i;
                            }
                            break;
                        }
                    case (int)_DateFilter.ThisWeek:
                        {
                            if (array[i].CreateDate < DateTime.Today.AddDays(-7))
                            {
                                array.RemoveAt(i);
                                --i;
                            }
                            break;
                        }
                    case (int)_DateFilter.LastTwoWeek:
                        {
                            if (array[i].CreateDate < DateTime.Today.AddDays(-14))
                            {
                                array.RemoveAt(i);
                                --i;
                            }
                            break;
                        }
                }
            }
            arrayLastIndex = array.Count - 1;
        }
        private void Swap_SplintCase(List<SplintInformation> array, int left, int right)
        {
            SplintInformation tmpCase = array[left];
            array[left] = array[right];
            array[right] = tmpCase;
        }
        /// <summary>
        /// Splint專案排序(依創建日期)
        /// </summary>
        /// <param name="array">專案陣列</param>
        /// <param name="left">輸入0</param>
        /// <param name="right">輸入array.count()-1</param>
        /// <param name="FirstIn">輸入true</param>
        public void Sort_Splint(List<SplintInformation> array, int left, int right, bool FirstIn)
        {
            if (FirstIn == true)
                Filter_Splint(array, ref right);

            if (left < right)
            {
                int i = left - 1;   //left margin
                int j = right + 1;  //right margin
                SplintInformation axle = array[(left + right) / 2];  //axle

                while (true)
                {
                    while (array[++i].CreateDate > axle.CreateDate) ;
                    while (array[--j].CreateDate < axle.CreateDate) ;
                    if (i >= j)
                        break;

                    Swap_SplintCase(array, i, j);
                }

                Sort_Splint(array, left, (i-1), false);
                Sort_Splint(array, (j+1), right, false);
            }
        }
        #endregion
    }
}
