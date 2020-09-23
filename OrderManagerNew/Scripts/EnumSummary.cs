using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderManagerNew
{
    /// <summary>
    /// 軟體安裝狀態
    /// </summary>
    public enum _softwareStatus : int
    {
        /// <summary>
        /// 未安裝
        /// </summary>
        NotInstall = 0,
        /// <summary>
        /// 下載中
        /// </summary>
        Downloading,
        /// <summary>
        /// 安裝中
        /// </summary>
        Installing,
        /// <summary>
        /// 已安裝
        /// </summary>
        Installed,
        /// <summary>
        /// 解除安裝中
        /// </summary>
        Uninstalling,
        /// <summary>
        /// 雲端
        /// </summary>
        Cloud,
        /// <summary>
        /// 更新中
        /// </summary>
        Updating
    }

    /// <summary>
    /// 軟體ID
    /// </summary>
    public enum _softwareID : int
    {
        EZCAD = 0,
        Implant,
        Ortho,
        Tray,
        Splint,
        Guide = 5,
        All
    }
    
    /// <summary>
    /// 電腦容量單位
    /// </summary>
    public enum _diskUnit : int
    {
        KB = 0,
        MB,
        GB,
        TB
    }

    /// <summary>
    /// 監看資料夾指令
    /// </summary>
    public enum _watcherCommand : int
    {
        /// <summary>
        /// 安裝
        /// </summary>
        Install = 0,
        /// <summary>
        /// 刪除
        /// </summary>
        Delete,
    }

    public enum _classFrom : int
    {
        MainWindow = 0,
        Setting,
    }

    /// <summary>
    /// 支援語系
    /// </summary>
    public enum _langSupport : int
    {
        /// <summary>
        /// 英文
        /// </summary>
        English = 0,
        /// <summary>
        /// 繁體中文
        /// </summary>
        zhTW,
    }
    
    /// <summary>
    /// 日期過濾
    /// </summary>
    public enum _DateFilter : int
    {
        All = 0,
        Today,
        ThisWeek,
        LastTwoWeek
    }
    /// <summary>
    /// 登入時傳回的字串
    /// </summary>
    public enum _AirD_LoginDetail : int
    {
        UID = 0,
        USERGROUP,
        EMAIL,
        USERNAME
    }

    public enum _ReceiveDataStatus : int
    {
        OK = 0,
        Error,
        Cancel
    }
}
