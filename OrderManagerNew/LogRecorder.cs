using System;
using System.IO; //StreamWriter要用
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderManagerNew
{
    /// <summary>
    /// 日誌檔讀寫
    /// </summary>
    class LogRecorder
    {
        public LogRecorder()
        {
            //每次new LogRecorder()就刪掉之前舊的log檔
            if (File.Exists("OrderManager.log") == true)
                File.Delete("OrderManager.log");
        }

        public void Doit()
        {
            using (StreamWriter w = File.AppendText("OrderManager.log"))
            {
                Log("Test1", w);
                Log("Test2", w);
            }

            using (StreamReader r = File.OpenText("OrderManager.log"))
            {
                DumpLog(r);
            }
        }

        /// <summary>
        /// 寫入log
        /// </summary>
        /// <param name="logMessage"> 寫入log資訊</param>
        /// <param name="w">log檔路徑</param>
        /// <returns></returns>
        public void Log(string logMessage, TextWriter w)
        {
            w.Write("\r\nLog Entry : ");
            w.WriteLine($"{DateTime.Now.ToLongDateString()} {DateTime.Now.ToLongTimeString()}");
            w.WriteLine("  :");
            w.WriteLine($"  :{logMessage}");
            w.WriteLine("-------------------------------");
        }

        /// <summary>
        /// 把log資料顯示在Console
        /// </summary>
        /// <param name="r">log檔路徑</param>
        /// <returns></returns>
        public void DumpLog(StreamReader r)
        {
            string line;
            while ((line = r.ReadLine()) != null)
            {
                Console.WriteLine(line);
            }
        }
    }
}
