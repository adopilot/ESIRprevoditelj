using EsirDriver.Modeli.esir;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EsirDriver.Modeli
{
    [RequiresUnreferencedCode("Necessary because of RangeAttribute usage")]
    public class EsirConfigModel
    {
        public bool authorizeLocalClients { get; set; }
        public bool authorizeRemoteClients { get; set; }
        public string apiKey { get; set; }
        public string webserverAddress { get; set; }
        public int pin { get; set; }

        public int TimeoutInSec { get; set; } = 3;

        public InvoiceType OperationMode { get; set; } = InvoiceType.Training;


    }

}
