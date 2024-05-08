using EsirDriver.Modeli;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EsirDriver.Implmentacije
{
    internal interface IFiskalniPrevoditelj
    {
        event EventHandler<PorukaFiskalnogPrintera> PorukaEvent;
        Task<PorukaFiskalnogPrintera> SatTik();

    }
}
