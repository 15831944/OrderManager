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

            Visual C++ 2008 SP1 MFC Security Update Redistributable Package (x86):
            {9BE518E6-ECC6-35A9-88E4-87755C07200F}

            Visual C++ 2010 SP1 Redistributable Package (x86):
            {F0C3E5D1-1ADE-321E-8167-68EF0DE699A5}

            Visual C++ 2013 Redistributable (x86):
            {35B83883-40FA-423C-AE73-2AFF7E1EA820}

            Visual C++ 2015 Redistributable (x86):
            {462F63A8-6347-4894-A1B3-DBFE3A4C981D}

            用法:
            if (MsiQueryProductState("{710F4C1C-CC18-4C49-8CBF-51240C89A1A2}") == INSTALLSTATE.INSTALLSTATE_DEFAULT)
                return true;//代表有裝
        **/

        enum _RedistPackID : int
        {
            VC2005 = 0,
            VC2008,
            VC2010,
            VC2013,
            VC2015,
        }
        
        class RedistributablePackage
        {
            class RedistributableInfo
            {
                public string RedistributableName { get; set; }
                public string RedistributableGUID { get; set; }

                public RedistributableInfo()
                {
                    RedistributableName = "";
                    RedistributableGUID = "";
                }
            }

            List<RedistributableInfo> redistPack { get; set; }
            public RedistributablePackage()
            {
                redistPack = new List<RedistributableInfo>();
                RedistributableInfo VCPack = new RedistributableInfo();
                VCPack.RedistributableName = "Visual C++ 2005 SP1 MFC Security Update Redistributable Package(x86)";
                VCPack.RedistributableGUID = "{710F4C1C-CC18-4C49-8CBF-51240C89A1A2}";
                redistPack.Add(VCPack);
                VCPack.RedistributableName = "Visual C++ 2008 SP1 MFC Security Update Redistributable Package (x86)";
                VCPack.RedistributableGUID = "{9BE518E6-ECC6-35A9-88E4-87755C07200F}";
                redistPack.Add(VCPack);
                VCPack.RedistributableName = "Visual C++ 2010 SP1 Redistributable Package (x86)";
                VCPack.RedistributableGUID = "{F0C3E5D1-1ADE-321E-8167-68EF0DE699A5}";
                redistPack.Add(VCPack);
                VCPack.RedistributableName = "Visual C++ 2013 Redistributable (x86)";
                VCPack.RedistributableGUID = "{35B83883-40FA-423C-AE73-2AFF7E1EA820}";
                redistPack.Add(VCPack);
                VCPack.RedistributableName = "Visual C++ 2015 Redistributable (x86)";
                VCPack.RedistributableGUID = "{462F63A8-6347-4894-A1B3-DBFE3A4C981D}";
                redistPack.Add(VCPack);
            }

            public bool checkHaveInstallVC(int RedistPackID)
            {
                if (MsiQueryProductState(redistPack[RedistPackID].RedistributableGUID) == INSTALLSTATE.INSTALLSTATE_DEFAULT)
                    return true;

                return false;
            }

            public string GetVCname(int RedistPackID)
            {
                return redistPack[RedistPackID].RedistributableName.ToString();
            }
        }

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

        public bool checkSoftwareVC(int softwareID)
        {
            RedistributablePackage redistPackage = new RedistributablePackage();

            if (softwareID == (int)_softwareID.EZCAD)
            {
                //EZCAD:vc2013
            }

            if (softwareID == (int)_softwareID.Implant)
            {
                //Implant:vc2005 vc2010
                if (redistPackage.checkHaveInstallVC((int)_RedistPackID.VC2005) == false || redistPackage.checkHaveInstallVC((int)_RedistPackID.VC2010) == false)
                {
                    string ErrMessage = "您缺少運行庫:\n";
                    if (redistPackage.checkHaveInstallVC((int)_RedistPackID.VC2005) == false)
                        ErrMessage += redistPackage.GetVCname((int)_RedistPackID.VC2005) + "/n";
                    if (redistPackage.checkHaveInstallVC((int)_RedistPackID.VC2010) == false)
                        ErrMessage += redistPackage.GetVCname((int)_RedistPackID.VC2010) + "/n";
                    MessageBox.Show(ErrMessage);
                }
                else
                    return true;
            }

            if (softwareID == (int)_softwareID.Ortho)
            {
                //Ortho: vc2008 vc2010 vc2013 vc2015
            }

            if (softwareID == (int)_softwareID.Tray)
            {
                //Tray:vc2013
            }

            if (softwareID == (int)_softwareID.Splint)
            {
                //Splint:vc2013
            }

            if (softwareID == (int)_softwareID.Guide)
            {
                //Guide:vc2013
            }
            return false;
        }
    }
}
