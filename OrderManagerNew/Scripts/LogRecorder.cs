using System;
using System.IO; //StreamWriter要用
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//Source: https://docs.microsoft.com/zh-tw/dotnet/standard/io/how-to-open-and-append-to-a-log-file
namespace OrderManagerNew
{
    /// <summary>
    /// 日誌檔讀寫
    /// </summary>
    public class LogRecorder
    {
        public LogRecorder()
        {
            if(File.Exists("OrderManager.log"))
            {
                FileStream fs = new FileStream("OrderManager.log", FileMode.Open, FileAccess.Read);
                if (fs.Length > Math.Pow(2, 20) * 100)  //超過100M就刪掉重建新的log檔
                {
                    fs.Close();
                    File.Delete("OrderManager.log");
                }
                fs.Close();
            }
        }

        /// <summary>
        /// 寫入log資訊
        /// </summary>
        /// <param name="Row">第幾行--> new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString()</param>
        /// <param name="Block"> 區塊</param>
        /// <param name="logMessage"> 詳細資訊</param>
        /// <returns></returns>
        public void RecordLog(string Row,string Block, string logMessage)
        {
            if (Properties.Settings.Default.FullRecord == false)
                return;

            using (StreamWriter w = File.AppendText("OrderManager.log"))
            {
                string str = "row_" + Row + " " + Block;
                Log(str, logMessage, w);
            }
        }

        /// <summary>
        /// 寫入log資訊(去除前面空行)
        /// </summary>
        /// <param name="Row"> 第幾行</param>
        /// <param name="Block"> 區塊</param>
        /// <param name="logMessage"> 詳細資訊</param>
        /// <returns></returns>
        public void RecordLogContinue(string Row, string Block, string logMessage)
        {
            if (Properties.Settings.Default.FullRecord == false)
                return;

            using (StreamWriter w = File.AppendText("OrderManager.log"))
            {
                string str = "row_" + Row + " " + Block;
                ShortLog(str, logMessage, w);
            }
        }

        /// <summary>
        /// 寫入log資訊(設定檔)
        /// </summary>
        /// <param name="Block"> 區塊</param>
        /// <param name="logMessage"> 詳細資訊</param>
        /// <returns></returns>
        public void RecordConfigLog(string Block, string logMessage)
        {
            if (Properties.Settings.Default.FullRecord == false)
                return;

            using (StreamWriter w = File.AppendText("OrderManager.log"))
            {
                ConfigLog(Block, logMessage, w);
            }
        }

        /// <summary>
        /// log分段
        /// </summary>
        public void RecordLogSaperate()
        {
            if (Properties.Settings.Default.FullRecord == false)
                return;

            using (StreamWriter w = File.AppendText("OrderManager.log"))
            {
                SeprateLog(w);
            }
        }

        /// <summary>
        /// 記錄客戶Config資訊
        /// </summary>
        /// <param name="Block"></param>
        /// <param name="logMessage"></param>
        /// <param name="w"></param>
        private void ConfigLog(string Block, string logMessage, TextWriter w)
        {
            if (Properties.Settings.Default.FullRecord == false)
                return;

            w.Write("\r\nLog Entry : ");
            w.WriteLine($"{DateTime.Now.ToLongDateString()} {DateTime.Now.ToLongTimeString()}");
            w.WriteLine($"{Block}:{logMessage}");
            w.WriteLine("-------------------------------");
            w.WriteLine($"EXEpath_EZCAD:{Properties.Settings.Default.cad_exePath}");
            w.WriteLine($"EXEpath_Implant:{Properties.Settings.Default.implant_exePath}");
            w.WriteLine($"EXEpath_Ortho:{Properties.Settings.Default.ortho_exePath}");
            w.WriteLine($"EXEpath_Tray:{Properties.Settings.Default.tray_exePath}");
            w.WriteLine($"EXEpath_Splint:{Properties.Settings.Default.splint_exePath}");
            w.WriteLine($"EXEpath_Guide:{Properties.Settings.Default.guide_exePath}");
            w.WriteLine($"caseDir_EZCAD:{Properties.OrderManagerProps.Default.cad_projectDirectory}");
            w.WriteLine($"caseDir_Implant:{Properties.OrderManagerProps.Default.implant_projectDirectory}");
            w.WriteLine($"caseDir_Ortho:{Properties.OrderManagerProps.Default.ortho_projectDirectory}");
            w.WriteLine($"caseDir_Tray:{Properties.OrderManagerProps.Default.tray_projectDirectory}");
            w.WriteLine($"caseDir_Splint:{Properties.OrderManagerProps.Default.splint_projectDirectory}");
            w.WriteLine($"SystemDisk:{Properties.OrderManagerProps.Default.systemDisk}");
            w.WriteLine($"MostSoftwareDisk:{Properties.OrderManagerProps.Default.mostsoftwareDisk}");
            w.WriteLine($"DownloadFolder:{Properties.Settings.Default.DownloadFolder}");
            w.WriteLine($"PingTime:{Properties.Settings.Default.PingTime}");
            w.WriteLine($"UserLanguage:{Properties.Settings.Default.sysLanguage}");
        }
        
        private void SeprateLog( TextWriter w)
        {
            if (Properties.Settings.Default.FullRecord == false)
                return;

            w.WriteLine("-------------------------------");
            w.WriteLine("");
        }
        
        private void Log(string Block, string logMessage, TextWriter w)
        {
            w.Write("\r\nLog Entry : ");
            w.WriteLine($"{DateTime.Now.ToLongDateString()} {DateTime.Now.ToLongTimeString()}");
            w.WriteLine("-------------------------------");
            w.WriteLine($"{Block}:{logMessage}");
        }

        
        private void ShortLog(string Block, string logMessage, TextWriter w)
        {
            w.WriteLine($"{Block}:{logMessage}");
        }
    }
}
