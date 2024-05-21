using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EsirDriver.Modeli.esir
{
    internal class EsirStatusCodeModel
    {
        public int Code { get; set; }
        public string Info { get; set; }
        public string Opis { get; set; }
        public LogLevel LogLevel { get; set; }
    }

}
