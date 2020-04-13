using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderManagerNew
{
    public enum _softwareStatus : int
    {
        NotInstall = 0,
        Downloading,
        Installed,
        Cloud
    }

    public enum _softwareID : int
    {
        EZCAD = 0,
        Implant,
        Ortho,
        Tray,
        Splint,
        Guide = 5
    }

    public enum _softwareLic : int
    {
        License = 0,
        Dongle = 1
    }
}
