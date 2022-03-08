using DiskoAIO.DiskoTasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiskoAIO
{
    public interface IDiskoTask
    {
        string Type { get; }
        Progress progress { get; set; }
        //IGroup accountGroup { get; set; }
        public string accountGroupName { get; set; }
        ProxyGroup proxyGroup { get; set; }
        string Account { get; }
        string Proxy { get; }
        int delay { get; set; }
        bool Running { get; set; }
        bool Paused { get; }
        void Start();
        void Stop();
        void Pause();
        void Resume();
    }
    public struct Progress
    {
        public int total_tokens { get; set; }
        public int completed_tokens { get; set; }
        public override string ToString()
        {
            return completed_tokens.ToString() + '/' + total_tokens.ToString();
        }
        public Progress(int _total_tokens)
        {
            total_tokens = _total_tokens;
            completed_tokens = 0;
        }
        public void Fill()
        {
            completed_tokens = total_tokens;
        }
        public void Add(int amount)
        {
            if (completed_tokens + amount > total_tokens)
            {
                Fill();
            }
            else
            {
                completed_tokens += amount;
            }
        }
    }
}
