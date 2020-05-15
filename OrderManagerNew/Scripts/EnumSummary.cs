﻿using System;
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
        Installing,
        Installed,
        Uninstalling,
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

    public enum _diskUnit : int
    {
        KB = 0,
        MB,
        GB,
        TB
    }

    public enum _watcherCommand : int
    {
        Installing = 0,
        Delete,
    }

    public enum _classFrom : int
    {
        MainWindow = 0,
        Setting,
    }

    public enum _langSupport : int
    {
        English = 0,
        zhTW,
    }
}