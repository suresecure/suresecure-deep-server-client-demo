using System;
using System.IO;

namespace VerifySerivce
{
    internal class Logger
    {
        public static void AddLog(Type type, string methodName, string msg)
        {
            try
            {
                string directory = System.Environment.CurrentDirectory + "\\Logs\\";
                if (!Directory.Exists(directory))
                {
                    DirectoryInfo direInfo = Directory.CreateDirectory(directory);
                }
                string path = directory + DateTime.Now.ToString("yyyyMMdd") + "VerifySerivce.log";
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
                    sw.WriteLine(DateTime.Now.ToString() + "  【ClassName:】" + className + "   【MethodName:】" + methodName + "    【异常信息:】" + msg);
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
