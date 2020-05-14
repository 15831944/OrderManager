using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//c#檢查是否安裝了相應vc++運行庫: https://www.cnblogs.com/yidanda888/p/11987411.html
//vc運行庫大全: http://blog.sina.com.cn/s/blog_3fed3a390102v4pe.html

namespace OrderManagerNew
{
    public class QueryProductState
    {
        /**
            Visual C++ 2005 SP1 MFC Security Update Redistributable Package(x86):
            {710F4C1C-CC18-4C49-8CBF-51240C89A1A2}

            Visual C++ 2010 SP1 Redistributable Package (x86):
            {F0C3E5D1-1ADE-321E-8167-68EF0DE699A5}
        **/

        public enum RedistributablePack : int
        {
            VC2005SP1MFC = 0,
            VC2010SP1,
        }

        string[] RedistributableName = { "Visual C++ 2005 SP1 MFC Security Update Redistributable Package (x86)", "Visual C++ 2010 SP1 Redistributable Package (x86)" };

        public enum INSTALLSTATE
        {
            INSTALLSTATE_NOTUSED = -7,  // component disabled
            INSTALLSTATE_BADCONFIG = -6,  // configuration data corrupt
            INSTALLSTATE_INCOMPLETE = -5,  // installation suspended or in progress
            INSTALLSTATE_SOURCEABSENT = -4,  // run from source, source is unavailable
            INSTALLSTATE_MOREDATA = -3,  // return buffer overflow
            INSTALLSTATE_INVALIDARG = -2,  // invalid function argument
            INSTALLSTATE_UNKNOWN = -1,  // unrecognized product or feature
            INSTALLSTATE_BROKEN = 0,  // broken
            INSTALLSTATE_ADVERTISED = 1,  // advertised feature
            INSTALLSTATE_REMOVED = 1,  // component being removed (action state, not settable)
            INSTALLSTATE_ABSENT = 2,  // uninstalled (or action state absent but clients remain)
            INSTALLSTATE_LOCAL = 3,  // installed on local drive
            INSTALLSTATE_SOURCE = 4,  // run from source, CD or net
            INSTALLSTATE_DEFAULT = 5,  // use default, local or source
        }

        [DllImport("msi.dll")]
        private static extern INSTALLSTATE MsiQueryProductState(string product);
        public static INSTALLSTATE GetProcuct(string product)
        {
            INSTALLSTATE state = MsiQueryProductState(product);
            return state;
        }

        public bool HaveInstallVc(int softwareID)
        {
            List<int> redistributableNeed = new List<int>();

            if(softwareID == (int)_softwareID.Implant)
            {
                //Visual C++ 2010 Redistributable Package(x86)
                if (MsiQueryProductState("{710F4C1C-CC18-4C49-8CBF-51240C89A1A2}") == INSTALLSTATE.INSTALLSTATE_DEFAULT && MsiQueryProductState("{F0C3E5D1-1ADE-321E-8167-68EF0DE699A5}") == INSTALLSTATE.INSTALLSTATE_DEFAULT)
                    return true;
                else
                {
                    if (MsiQueryProductState("{710F4C1C-CC18-4C49-8CBF-51240C89A1A2}") != INSTALLSTATE.INSTALLSTATE_DEFAULT && MsiQueryProductState("{F0C3E5D1-1ADE-321E-8167-68EF0DE699A5}") != INSTALLSTATE.INSTALLSTATE_DEFAULT)
                        MessageBox.Show("缺少以下運行庫:\n" + RedistributableName[0] + "\n" + RedistributableName[1]);
                    else
                    {
                        if (MsiQueryProductState("{710F4C1C-CC18-4C49-8CBF-51240C89A1A2}") != INSTALLSTATE.INSTALLSTATE_DEFAULT)
                            MessageBox.Show("缺少以下運行庫:\n" + RedistributableName[0]);
                        if (MsiQueryProductState("{F0C3E5D1-1ADE-321E-8167-68EF0DE699A5}") != INSTALLSTATE.INSTALLSTATE_DEFAULT)
                            MessageBox.Show("缺少以下運行庫:\n" + RedistributableName[1]);
                    }
                }
            }
            return false;
        }
    }
}
