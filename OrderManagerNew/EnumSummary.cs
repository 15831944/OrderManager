using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderManagerNew
{
    public enum _softwareTableStatus : int
    {
        NotInstall = 0,
        Downloading,
        Installed
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
}
