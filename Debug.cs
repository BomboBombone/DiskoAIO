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
