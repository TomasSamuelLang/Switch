using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApplication1
{
    class Rule
    {
        public int ID { get; set; }
        public int Port { get; set; }
        public string Direction { get; set; }
        public string sdMac { get; set; }
        public string mac { get; set; }
        public string sdIP { get; set; }
        public string ip { get; set; }
        public bool ICMPReq { get; set; }
        public bool TCP { get; set; }
        public bool UDP { get; set; }
        public bool HTTPS { get; set; }
        public bool HTTPD { get; set; }
        public bool ARP { get; set; }
        public bool ICMPRep { get; set; }
    }
}
