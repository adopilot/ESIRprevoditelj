using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EsirDriver.Modeli
{    
    public class EsirConfigModel
    {
        public bool authorizeLocalClients { get; set; }
        public bool authorizeRemoteClients { get; set; }
        public string apiKey { get; set; }
        public string webserverAddress { get; set; }

    }

}
