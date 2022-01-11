using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiskoAIO
{
    interface DiskoTask
    {
        TaskType type { get; set; }
        Progress progress { get; set; }
        AccountGroup accountGroup { get; set; }
        ProxyGroup proxyGroup { get; set; }
    }
    public struct Progress
    {
        public int total_tokens { get; set; }
        public int completed_tokens { get; set; }
        public override string ToString()
        {
            return total_tokens.ToString() + '/' + completed_tokens.ToString();
        }
        public Progress(int _total_tokens)
        {
            total_tokens = _total_tokens;
            completed_tokens = 0;
        }
    }
}
