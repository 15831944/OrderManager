using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CadInformation = OrderManagerNew.UserControls.Order_cadBase.CadInformation;
using TrayInformation = OrderManagerNew.UserControls.Order_tsBase.TrayInformation;
using SplintInformation = OrderManagerNew.UserControls.Order_tsBase.SplintInformation;
using ImplantOuterInformation = OrderManagerNew.UserControls.Order_implantBase.ImplantOuterInformation;
using OrthoOuterInformation = OrderManagerNew.UserControls.Order_orthoBase.OrthoOuterInformation;

//快速排序演算法: https://dotblogs.com.tw/kennyshu/2009/10/24/11270

namespace OrderManagerNew
{
    class CaseSorter
    {
        #region EZCAD
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
        public void Sort_EZCAD(List<CadInformation> array, int left, int right)
        {
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

                Sort_EZCAD(array, left, i - 1);
                Sort_EZCAD(array, j + 1, right);
            }
        }
        #endregion

        #region Implant
        private void Swap_ImplantCase(List<ImplantOuterInformation> array, int left, int right)
        {
            ImplantOuterInformation tmpCase = array[left];
            array[left] = array[right];
            array[right] = tmpCase;
        }
        /// <summary>
        /// ImplantPlanning專案排序(依創建日期)
        /// </summary>
        /// <param name="array">專案陣列</param>
        /// <param name="left">輸入0</param>
        /// <param name="right">輸入array.count()-1</param>
        public void Sort_Implant(List<ImplantOuterInformation> array, int left, int right)
        {
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

                Sort_Implant(array, left, i - 1);
                Sort_Implant(array, j + 1, right);
            }
        }
        #endregion

        #region Ortho
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
        public void Sort_Ortho(List<OrthoOuterInformation> array, int left, int right)
        {
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

                Sort_Ortho(array, left, i - 1);
                Sort_Ortho(array, j + 1, right);
            }
        }
        #endregion

        #region Tray
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
        public void Sort_Tray(List<TrayInformation> array, int left, int right)
        {
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

                Sort_Tray(array, left, i - 1);
                Sort_Tray(array, j + 1, right);
            }
        }
        #endregion

        #region Splint
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
        public void Sort_Splint(List<SplintInformation> array, int left, int right)
        {
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

                Sort_Splint(array, left, i - 1);
                Sort_Splint(array, j + 1, right);
            }
        }
        #endregion
    }
}
