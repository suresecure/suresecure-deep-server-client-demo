using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace RestfulTest
{
    public class Logger
    {
        static string appName = Assembly.GetExecutingAssembly().GetName().Name;
        public static void AddLog(Type type, string methodName, string msg, Exception exp = null)
        {
            try
            {
                StringBuilder errBuf = new StringBuilder("异常描述:" + msg);
                if (exp != null)
                {
                    if (!string.IsNullOrWhiteSpace(msg))
                        errBuf.Append(",");
                    errBuf.Append(string.Format("{0}   \r\n 详细信息:{1}", exp.Message, exp.StackTrace));
                }
                string directory = System.Environment.CurrentDirectory + "\\Logs\\";
                if (!Directory.Exists(directory))
                {
                    DirectoryInfo direInfo = Directory.CreateDirectory(directory);
                }
                string path = directory + DateTime.Now.ToString("yyyyMMdd") + appName + ".txt";
                if (!File.Exists(path))
                {
                    using (FileStream fs = File.Create(path))
                    {
                        fs.Close();
                        fs.Dispose();
                    }
                }
                using (StreamWriter sw = new StreamWriter(path, true))
                {
                    string className = type.ToString();
                    sw.WriteLine(DateTime.Now.ToString() + "  ClassName:" + className + "   MethodName:" + methodName + "    Output:" + errBuf.ToString());
                    sw.Close();
                    sw.Dispose();
                }
            }
            catch
            {
            }
        }
    }
}
