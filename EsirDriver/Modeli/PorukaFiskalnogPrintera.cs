using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace EsirDriver.Modeli
{
    public class PorukaFiskalnogPrintera
    {
        public string Poruka { get; set; }
        public bool IsError { get; set; }
        public  bool MozeNastaviti { get; set; }
        public bool NeAutorizovan { get; set; }
        public bool NemaBezbednsniElement { get; set; }
        public bool TraziPin { get; set; }
        public LogLevel LogLevel { get; set; } = LogLevel.Warning;

        
    }

    

}
