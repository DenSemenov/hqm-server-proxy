using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace hqm_server_proxy.Classes
{
    public class UdpProxyClient
    {
        public UdpClient? Socket { get; set; }
        public IPEndPoint Client { get; set; }
    }
}
