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
    class LogRecorder
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
        /// <param name="Block"> 區塊</param>
        /// <param name="logMessage"> 詳細資訊</param>
        /// <param name="w">log檔路徑</param>
        /// <returns></returns>
        public void RecordLog(string Row,string Block, string logMessage)
        {
            using (StreamWriter w = File.AppendText("OrderManager.log"))
            {
                string str = "row_" + Row + " " + Block;
                Log(str, logMessage, w);
            }

            /*using (StreamReader r = File.OpenText("OrderManager.log"))
            {
                DumpLog(r);
            }*/
        }

        public void RecordLogContinue(string Row, string Block, string logMessage)
        {
            using (StreamWriter w = File.AppendText("OrderManager.log"))
            {
                string str = "row_" + Row + " " + Block;
                shortLog(str, logMessage, w);
            }
        }

        public void RecordConfigLog(string Row, string Block, string logMessage)
        {
            using (StreamWriter w = File.AppendText("OrderManager.log"))
            {
                string str = "row_" + Row + " " + Block;
                ConfigLog(str, logMessage, w);
            }
        }

        public void RecordConfigLog(string Block, string logMessage)
        {
            using (StreamWriter w = File.AppendText("OrderManager.log"))
            {
                ConfigLog(Block, logMessage, w);
            }
        }

        private void ConfigLog(string Block, string logMessage, TextWriter w)
        {
            w.Write("\r\nLog Entry : ");
            w.WriteLine($"{DateTime.Now.ToLongDateString()} {DateTime.Now.ToLongTimeString()}");
            w.WriteLine($"{Block}:{logMessage}");
            w.WriteLine("-------------------------------");
            w.WriteLine($"path_EZCAD:{Properties.Settings.Default.path_EZCAD}");
            w.WriteLine($"path_Implant:{Properties.Settings.Default.path_Implant}");
            w.WriteLine($"path_Ortho:{Properties.Settings.Default.path_Ortho}");
            w.WriteLine($"path_Tray:{Properties.Settings.Default.path_Tray}");
            w.WriteLine($"path_Splint:{Properties.Settings.Default.path_Splint}");
            w.WriteLine($"path_Guide:{Properties.Settings.Default.path_Guide}");
            w.WriteLine($"UserLanguage:{Properties.Settings.Default.sysLanguage}");
        }

        /// <summary>
        /// 寫入log資訊
        /// </summary>
        /// <param name="Block"> 區塊</param>
        /// <param name="logMessage"> 詳細資訊</param>
        /// <param name="w">log檔路徑</param>
        /// <returns></returns>
        private void Log(string Block, string logMessage, TextWriter w)
        {
            w.Write("\r\nLog Entry : ");
            w.WriteLine($"{DateTime.Now.ToLongDateString()} {DateTime.Now.ToLongTimeString()}");
            w.WriteLine("-------------------------------");
            w.WriteLine($"{Block}:{logMessage}");
        }

        /// <summary>
        /// 寫入log資訊(去除前面空行)
        /// </summary>
        /// <param name="Block"> 區塊</param>
        /// <param name="logMessage"> 詳細資訊</param>
        /// <param name="w">log檔路徑</param>
        /// <returns></returns>
        private void shortLog(string Block, string logMessage, TextWriter w)
        {
            w.WriteLine("");
            w.WriteLine($"{Block}:{logMessage}");
        }

        /// <summary>
        /// 把log資料顯示在Console
        /// </summary>
        /// <param name="r">log檔路徑</param>
        /// <returns></returns>
        private void DumpLog(StreamReader r)
        {
            string line;
            while ((line = r.ReadLine()) != null)
            {
                Console.WriteLine(line);
            }
        }
    }
}
