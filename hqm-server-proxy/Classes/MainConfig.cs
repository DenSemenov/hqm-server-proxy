using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hqm_server_proxy.Classes
{
    public class MainConfig
    {
        public string Name { get; set; }
        public List<Config> Servers { get; set; } = new List<Config>();
    }
}
