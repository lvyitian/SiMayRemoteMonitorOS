using System;
using System.Collections.Generic;
using System.Text;

namespace SuperWebSocket
{
    public class WebSocketConsoleWrite
    {
        public WebSocketConsoleWriteType WriteType { get; set; }
        public WebSocketConsoleWriteLevel WriteLevel { get; set; }

        private void WriteToConsole(string Type, string Text, DateTime DateTime, string Source, string Trace)
        {
            if (string.IsNullOrEmpty(Trace))
                Console.WriteLine(String.Format("类型:{0} 信息：{1} 时间：{2} 来源：{3} ", Type, Text, DateTime.ToString(), Source));
            else
                Console.WriteLine(String.Format("类型:{0} 信息：{1} 时间：{2} 来源：{3} 跟踪:{4}", Type, Text, DateTime.ToString(), Source, Trace));
        }

        private void WriteToTextFile(string Type, string Text, DateTime DateTime, string Source, string Trace)
        {           
            string YearPath = DateTime.Now.ToString("yyyy");
            string MonthPath = DateTime.Now.ToString("yyyyMM");
            string DatePath = DateTime.Now.ToString("yyyyMMdd");

            try
            {
                if (!System.IO.Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "//log"))
                    System.IO.Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "//log");

                if (!System.IO.Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "//log//" + YearPath))
                    System.IO.Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "//log//" + YearPath);

                if (!System.IO.Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "//log//" + YearPath + "//" + MonthPath))
                    System.IO.Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "//log//" + YearPath + "//" + MonthPath);

                string txtPath = AppDomain.CurrentDomain.BaseDirectory + "//log//" + YearPath + "//" + MonthPath + "//" + DatePath + ".txt";
                if (!System.IO.File.Exists(txtPath))
                {
                    using (System.IO.FileStream fs = System.IO.File.Create(txtPath))
                    {
                        using (System.IO.StreamWriter sw = new System.IO.StreamWriter(fs, System.Text.Encoding.UTF8))
                        {
                            sw.WriteLine("配置文件创建于：{0} By {1}", DateTime.Now.ToString("yyyy年MM月dd日hh时mm分ss秒"), "WebSocketConsoleWrite");
                            sw.WriteLine("-------------------------------------------------------------");
                            sw.Flush();
                        }
                        fs.Close();
                    }
                    WriteToTextFile(Type, Text, DateTime, Source, Trace);
                }
                else
                {
                    using (System.IO.StreamWriter sw = System.IO.File.AppendText(txtPath))
                    {                        
                        sw.WriteLine("{0}：{1} 位置：{2} 时间：{3}", Type, Text, Source, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                        if (!string.IsNullOrEmpty(Trace))
                        {
                            sw.WriteLine("跟踪：{0}", Trace);
                        }
                        sw.Flush();
                    }
                }
            }
            catch (Exception ex)
            {
                
            }             
        }

        public void Message(string Type, string Text, DateTime DateTime, string Source, string Trace)
        {
            if (Type == "release")
                return;

            switch (WriteType)
            {
                case WebSocketConsoleWriteType.console:
                    WriteToConsole(Type, Text, DateTime, Source, Trace);
                    break;
                case WebSocketConsoleWriteType.textfile:
                    WriteToTextFile(Type, Text, DateTime, Source, Trace);
                    break;
            }
        }

        public string StringFromBytes(byte[] data)
        {
            string result = string.Empty;

#if DEBUG

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sb.Append(((int)data[i]).ToString("000"));
            }
            result = sb.ToString();          

#endif

            return result;
        }

    }
}
