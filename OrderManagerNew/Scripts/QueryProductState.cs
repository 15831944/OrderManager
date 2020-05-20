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
            {13A4EE12-23EA-3371-91EE-EFB36DDFFF3E}
            {35B83883-40FA-423C-AE73-2AFF7E1EA820}
            {DD1E9BDE-2AD6-4E92-8C07-7D4723EAB8B8}
            {F65DB027-AFF3-4070-886A-0D87064AABB1}

            Visual C++ 2015 Redistributable (x86):
            {462F63A8-6347-4894-A1B3-DBFE3A4C981D}
            {74d0e5db-b326-4dae-a6b2-445b9de1836e}
            {BE960C1C-7BAD-3DE6-8B1A-2616FE532845}
            {A2563E55-3BEC-3828-8D67-E5E8B9E8B675}

            Microsoft Visual C++ 2015-2019 Redistributable (x86):
            {65e650ff-30be-469d-b63a-418d71ea1765}

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
            VC2013_2,
            VC2013_3,
            VC2013_4,
            VC2015,
            VC2015_2,
            VC2015_3,
            VC2015_4,
            VC2015TO2019,
        }

        enum INSTALLSTATE
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

            List<RedistributableInfo> RedistPack { get; set; }
            public RedistributablePackage()
            {
                /*RedistPack = new List<RedistributableInfo>();
                RedistributableInfo VCPack = new RedistributableInfo();
                VCPack.RedistributableName = "Visual C++ 2005 SP1 MFC Security Update Redistributable Package(x86)";
                VCPack.RedistributableGUID = "{710F4C1C-CC18-4C49-8CBF-51240C89A1A2}";
                RedistPack.Add(VCPack);
                VCPack = new RedistributableInfo();
                VCPack.RedistributableName = "Visual C++ 2008 SP1 MFC Security Update Redistributable Package (x86)";
                VCPack.RedistributableGUID = "{9BE518E6-ECC6-35A9-88E4-87755C07200F}";
                RedistPack.Add(VCPack);
                VCPack = new RedistributableInfo();
                VCPack.RedistributableName = "Visual C++ 2010 SP1 Redistributable Package (x86)";
                VCPack.RedistributableGUID = "{F0C3E5D1-1ADE-321E-8167-68EF0DE699A5}";
                RedistPack.Add(VCPack);
                VCPack = new RedistributableInfo();
                VCPack.RedistributableName = "Visual C++ 2013 Redistributable Update 5 (x86)";
                VCPack.RedistributableGUID = "{35B83883-40FA-423C-AE73-2AFF7E1EA820}";
                RedistPack.Add(VCPack);
                VCPack = new RedistributableInfo();
                VCPack.RedistributableName = "Visual C++ 2013 Redistributable Update 5 (x86)";
                VCPack.RedistributableGUID = "{13A4EE12-23EA-3371-91EE-EFB36DDFFF3E}";
                RedistPack.Add(VCPack);
                VCPack = new RedistributableInfo();
                VCPack.RedistributableName = "Visual C++ 2013 Redistributable Update 5 (x86)";
                VCPack.RedistributableGUID = "{DD1E9BDE-2AD6-4E92-8C07-7D4723EAB8B8}";
                RedistPack.Add(VCPack);
                VCPack = new RedistributableInfo();
                VCPack.RedistributableName = "Visual C++ 2013 Redistributable Update 5 (x86)";
                VCPack.RedistributableGUID = "{F65DB027-AFF3-4070-886A-0D87064AABB1}";
                RedistPack.Add(VCPack);
                VCPack = new RedistributableInfo();
                VCPack.RedistributableName = "Visual C++ 2015 Redistributable Update 3 (x86)";
                VCPack.RedistributableGUID = "{462F63A8-6347-4894-A1B3-DBFE3A4C981D}";
                RedistPack.Add(VCPack);
                VCPack = new RedistributableInfo();
                VCPack.RedistributableName = "Visual C++ 2015 Redistributable Update 3 (x86)";
                VCPack.RedistributableGUID = "{F899BAD3-98ED-308E-A905-56B5338963FF}";
                RedistPack.Add(VCPack);
                VCPack = new RedistributableInfo();
                VCPack.RedistributableName = "Visual C++ 2015 Redistributable Update 3 (x86)";
                VCPack.RedistributableGUID = "{BE960C1C-7BAD-3DE6-8B1A-2616FE532845}";
                RedistPack.Add(VCPack);
                VCPack = new RedistributableInfo();
                VCPack.RedistributableName = "Visual C++ 2015 Redistributable Update 3 (x86)";
                VCPack.RedistributableGUID = "{A2563E55-3BEC-3828-8D67-E5E8B9E8B675}";
                RedistPack.Add(VCPack);
                VCPack = new RedistributableInfo();
                VCPack.RedistributableName = "Microsoft Visual C++ 2015-2019 Redistributable (x86)";
                VCPack.RedistributableGUID = "{65E650FF-30BE-469D-B63A-418D71EA1765}";
                RedistPack.Add(VCPack);*/
            }

            public bool CheckHaveInstallVC(int RedistPackID)
            {


                if (MsiQueryProductState(RedistPack[RedistPackID].RedistributableGUID) == INSTALLSTATE.INSTALLSTATE_DEFAULT)
                    return true;

                return false;
            }

            public string GetVCname(int RedistPackID)
            {
                return RedistPack[RedistPackID].RedistributableName.ToString();
            }
        }

        [DllImport("msi.dll")]
        private static extern INSTALLSTATE MsiQueryProductState(string product);
        static INSTALLSTATE GetProcuct(string product)
        {
            INSTALLSTATE state = MsiQueryProductState(product);
            return state;
        }

        public bool CheckSoftwareVC(int softwareID)
        {
            return true;//TODO之後要再修這個

            /*RedistributablePackage redistPackage = new RedistributablePackage();

            if (softwareID == (int)_softwareID.EZCAD)
            {
                //EZCAD:vc2013
                if (redistPackage.CheckHaveInstallVC((int)_RedistPackID.VC2013) == false && redistPackage.CheckHaveInstallVC((int)_RedistPackID.VC2013_2) == false
                    && redistPackage.CheckHaveInstallVC((int)_RedistPackID.VC2013_3) == false && redistPackage.CheckHaveInstallVC((int)_RedistPackID.VC2013_4) == false)
                {
                    string ErrMessage = "您缺少運行庫:\n";//TODO多國語系
                    if (redistPackage.CheckHaveInstallVC((int)_RedistPackID.VC2013) == false)
                        ErrMessage += redistPackage.GetVCname((int)_RedistPackID.VC2013) + "\n";
                    MessageBox.Show(ErrMessage);
                }
                else
                    return true;
            }

            if (softwareID == (int)_softwareID.Implant)
            {
                //Implant:vc2005 vc2010
                if (redistPackage.CheckHaveInstallVC((int)_RedistPackID.VC2005) == false || redistPackage.CheckHaveInstallVC((int)_RedistPackID.VC2010) == false)
                {
                    string ErrMessage = "您缺少運行庫:\n";
                    if (redistPackage.CheckHaveInstallVC((int)_RedistPackID.VC2005) == false)
                        ErrMessage += redistPackage.GetVCname((int)_RedistPackID.VC2005) + "\n";
                    if (redistPackage.CheckHaveInstallVC((int)_RedistPackID.VC2010) == false)
                        ErrMessage += redistPackage.GetVCname((int)_RedistPackID.VC2010) + "\n";
                    MessageBox.Show(ErrMessage);
                }
                else
                    return true;
            }

            if (softwareID == (int)_softwareID.Ortho)
            {
                //Ortho: vc2008 vc2010 vc2013 vc2015
                if (redistPackage.CheckHaveInstallVC((int)_RedistPackID.VC2008) == false || redistPackage.CheckHaveInstallVC((int)_RedistPackID.VC2010) == false ||
                    (redistPackage.CheckHaveInstallVC((int)_RedistPackID.VC2013) == false && redistPackage.CheckHaveInstallVC((int)_RedistPackID.VC2013_2) == false) ||
                    (redistPackage.CheckHaveInstallVC((int)_RedistPackID.VC2015) == false && redistPackage.CheckHaveInstallVC((int)_RedistPackID.VC2015_2) == false
                        && redistPackage.CheckHaveInstallVC((int)_RedistPackID.VC2015_3) == false && redistPackage.CheckHaveInstallVC((int)_RedistPackID.VC2015_4) == false
                        && redistPackage.CheckHaveInstallVC((int)_RedistPackID.VC2015TO2019) == false))
                {
                    string ErrMessage = "您缺少運行庫:\n";
                    if (redistPackage.CheckHaveInstallVC((int)_RedistPackID.VC2008) == false)
                        ErrMessage += redistPackage.GetVCname((int)_RedistPackID.VC2008) + "\n";
                    if (redistPackage.CheckHaveInstallVC((int)_RedistPackID.VC2010) == false)
                        ErrMessage += redistPackage.GetVCname((int)_RedistPackID.VC2010) + "\n";
                    if (redistPackage.CheckHaveInstallVC((int)_RedistPackID.VC2013) == false && redistPackage.CheckHaveInstallVC((int)_RedistPackID.VC2013_2) == false)
                        ErrMessage += redistPackage.GetVCname((int)_RedistPackID.VC2013) + "\n";
                    if (redistPackage.CheckHaveInstallVC((int)_RedistPackID.VC2015) == false && redistPackage.CheckHaveInstallVC((int)_RedistPackID.VC2015_2) == false
                        && redistPackage.CheckHaveInstallVC((int)_RedistPackID.VC2015_3) == false && redistPackage.CheckHaveInstallVC((int)_RedistPackID.VC2015_4) == false
                        && redistPackage.CheckHaveInstallVC((int)_RedistPackID.VC2015TO2019) == false)
                        ErrMessage += redistPackage.GetVCname((int)_RedistPackID.VC2015) + "\n";
                    MessageBox.Show(ErrMessage);
                }
                else
                    return true;
            }

            if (softwareID == (int)_softwareID.Tray)
            {
                //Tray:vc2013
                if (redistPackage.CheckHaveInstallVC((int)_RedistPackID.VC2013) == false)
                {
                    string ErrMessage = "您缺少運行庫:\n";
                    if (redistPackage.CheckHaveInstallVC((int)_RedistPackID.VC2013) == false)
                        ErrMessage += redistPackage.GetVCname((int)_RedistPackID.VC2013) + "\n";
                    MessageBox.Show(ErrMessage);
                }
                else
                    return true;
            }

            if (softwareID == (int)_softwareID.Splint)
            {
                //Splint:vc2013
                if (redistPackage.CheckHaveInstallVC((int)_RedistPackID.VC2013) == false)
                {
                    string ErrMessage = "您缺少運行庫:\n";
                    if (redistPackage.CheckHaveInstallVC((int)_RedistPackID.VC2013) == false)
                        ErrMessage += redistPackage.GetVCname((int)_RedistPackID.VC2013) + "\n";
                    MessageBox.Show(ErrMessage);
                }
                else
                    return true;
            }

            if (softwareID == (int)_softwareID.Guide)
            {
                //Guide:vc2013
                if (redistPackage.CheckHaveInstallVC((int)_RedistPackID.VC2013) == false)
                {
                    string ErrMessage = "您缺少運行庫:\n";
                    if (redistPackage.CheckHaveInstallVC((int)_RedistPackID.VC2013) == false)
                        ErrMessage += redistPackage.GetVCname((int)_RedistPackID.VC2013) + "\n";
                    MessageBox.Show(ErrMessage);
                }
                else
                    return true;
            }
            return false;*/
        }
    }
}
