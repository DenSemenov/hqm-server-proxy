using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace hqm_server_proxy.Classes
{
    public class Config
    {
        public int Port { get; set; }
        public string TargetIp { get; set; }
        public int TargetPort { get; set; }
    }
}
