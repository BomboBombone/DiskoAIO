using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DiskoAIO
{
    class Debug
    {
        public static void Log(string input)
        {
            int tries = 0;
            while (tries < 3)
            {
                try
                {
                    if(!Directory.Exists(App.strWorkPath + "\\logs"))
                    {
                        Directory.CreateDirectory(App.strWorkPath + "\\logs");
                    }
                    if(!File.Exists(App.strWorkPath + "\\logs\\log.txt"))
                    {
                        File.Create(App.strWorkPath + "\\logs\\log.txt");
                    }
                    using (StreamWriter w = File.AppendText(App.strWorkPath + "\\logs\\log.txt"))
                    {
                        w.WriteLine(input + " - " + DateTime.Now.ToString());
                    }
                    break;
                }
                catch (Exception ex)
                {
                    tries++;
                    Thread.Sleep(1000);
                }
            }
        }
    }
}
