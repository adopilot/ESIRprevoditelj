using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EsirDriver.Modeli.hcp
{
    internal class HcpArtOnRnModel
    {
        public string Brc { get; set; }
        public int Vat { get; set; }
        public int Mes { get; set; }
        public int Dep { get; set; }
        public string Dsc { get; set; }
        public decimal Prc { get; set; }
        public decimal Amn { get; set; }
        public decimal DsValue { get; set; }
        public bool Discount { get; set; }
    }
}
