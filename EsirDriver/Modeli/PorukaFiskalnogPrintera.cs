using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;


namespace EsirDriver.Modeli
{
    public class PorukaFiskalnogPrintera
    {
        public string Poruka { get; set; }
        public bool IsError { get; set; }
        public bool MozeNastaviti { get; set; }
        public LogLevel LogLevel { get; set; } = LogLevel.Warning;
        
    }

    

}
