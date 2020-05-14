using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
//c#檢查是否安裝了相應vc++運行庫: https://www.cnblogs.com/yidanda888/p/11987411.html

namespace OrderManagerNew
{
    public class QueryProductState
    {
        /*Visual C++ 2005 Redistributable Package(x86)
        { A49F249F-0C91-497F-86DF-B2585E8E76B7}

        Visual C++ 2005 Redistributable Package(x64)
        { 6E8E85E8-CE4B-4FF5-91F7-04999C9FAE6A}

        Visual C++ 2005 Redistributable Package(ia64)
        { 03ED71EA-F531-4927-AABD-1C31BCE8E187}

        Visual C++ 2005 SP1 Redistributable Package(x86)
        { 7299052B-02A4-4627-81F2-1818DA5D550D}

        Visual C++ 2005 SP1 Redistributable Package(x64)
        { 071C9B48-7C32-4621-A0AC-3F809523288F}

        Visual C++ 2005 SP1 Redistributable Package(ia64)
        { 0F8FB34E-675E-42ED-850B-29D98C2ECE08}

        Visual C++ 2008 Redistributable Package(x86)
        { FF66E9F6-83E7-3A3E-AF14-8DE9A809A6A4}

        Visual C++ 2008 Redistributable Package(x64)
        { 350AA351-21FA-3270-8B7A-835434E766AD}

        Visual C++ 2008 Redistributable Package(ia64)
        { 2B547B43-DB50-3139-9EBE-37D419E0F5FA}

        Visual C++ 2008 SP1 Redistributable Package(x86)
        { 9A25302D-30C0-39D9-BD6F-21E6EC160475}

        Visual C++ 2008 SP1 Redistributable Package(x86 9.0.30729.6161)
        { 9BE518E6-ECC6-35A9-88E4-87755C07200F}

        Visual C++ 2008 SP1 Redistributable Package(x64)
        { 8220EEFE-38CD-377E-8595-13398D740ACE}

        Visual C++ 2008 SP1 Redistributable Package(ia64)
        { 5827ECE1-AEB0-328E-B813-6FC68622C1F9}

        Visual C++ 2010 Redistributable Package(x86)
        { 196BB40D-1578-3D01-B289-BEFC77A11A1E}

        Visual C++ 2010 Redistributable Package(x64)
        { DA5E371C-6333-3D8A-93A4-6FD5B20BCC6E}

        Visual C++ 2010 Redistributable Package(ia64)
        { C1A35166-4301-38E9-BA67-02823AD72A1B}

        Visual C++ 2010 SP1 Redistributable Package(x86 10.0.40219)
        { F0C3E5D1-1ADE-321E-8167-68EF0DE699A5}

        Visual C++ 2010 SP1 Redistributable Package(x64)
        { 1D8E6291-B0D5-35EC-8441-6616F567A0F7}

        Visual C++ 2010 SP1 Redistributable Package(ia64)
        { 88C73C1C-2DE5-3B01-AFB8-B46EF4AB41CD}

        Visual C++ 2013 Redistributable Package(x86 12.0.21005)
        { 13A4EE12-23EA-3371-91EE-EFB36DDFFF3E}
        and {F8CFEB22-A2E7-3971-9EDA-4B11EDEFC185}

        Visual C++ 2013 Redistributable Package(x64)
        { 929FBD26-9020-399B-9A7A-751D61F0B942}
        and {A749D8E6-B613-3BE3-8F5F-045C84EBA29B}*/

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
            if(softwareID == (int)_softwareID.Implant)
            {
                var a = MsiQueryProductState("{DA5E371C-6333-3D8A-93A4-6FD5B20BCC6E}");
                var b = MsiQueryProductState("{F0C3E5D1-1ADE-321E-8167-68EF0DE699A5}");
                var c = MsiQueryProductState("{1D8E6291-B0D5-35EC-8441-6616F567A0F7}");

                //Visual C++ 2010 Redistributable Package(x86)
                if (MsiQueryProductState("{196BB40D-1578-3D01-B289-BEFC77A11A1E}") == INSTALLSTATE.INSTALLSTATE_DEFAULT)
                {
                    //Visual C++ 2005 SP1 Redistributable Package(x86)
                    if (MsiQueryProductState("{7299052B-02A4-4627-81F2-1818DA5D550D}") == INSTALLSTATE.INSTALLSTATE_DEFAULT)
                        return true;
                }
            }
            return false;
            //Visual C++ 2013 Redistributable Package (x86 12.0.21005)
            //{13A4EE12-23EA-3371-91EE-EFB36DDFFF3E} and {F8CFEB22-A2E7-3971-9EDA-4B11EDEFC185}
            //Visual C++ 2013 Redistributable Package (x64)
            //{929FBD26-9020-399B-9A7A-751D61F0B942} and {A749D8E6-B613-3BE3-8F5F-045C84EBA29B}
            //INSTALLSTATE state = MsiQueryProductState("{13A4EE12-23EA-3371-91EE-EFB36DDFFF3E}");
            /*if (MsiQueryProductState("{13A4EE12-23EA-3371-91EE-EFB36DDFFF3E}") == INSTALLSTATE.INSTALLSTATE_DEFAULT)
                return true;
            else if (MsiQueryProductState("{F8CFEB22-A2E7-3971-9EDA-4B11EDEFC185}") == INSTALLSTATE.INSTALLSTATE_DEFAULT)
                return true;
            else if (MsiQueryProductState("{929FBD26-9020-399B-9A7A-751D61F0B942}") == INSTALLSTATE.INSTALLSTATE_DEFAULT)
                return true;
            else if (MsiQueryProductState("{A749D8E6-B613-3BE3-8F5F-045C84EBA29B}") == INSTALLSTATE.INSTALLSTATE_DEFAULT)
                return true;
            else
                return false;*/
        }
    }
}
