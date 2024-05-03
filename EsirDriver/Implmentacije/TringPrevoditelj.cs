using EsirDriver.Modeli;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EsirDriver.Implmentacije
{
    internal class TringPrevoditelj : IFiskalniPrevoditelj
    {
        private readonly EsirDriverEngin _esir;
        internal TringPrevoditelj(EsirDriverEngin esir) 
        { 
            this._esir = esir; 
        }
        private bool isLocked { get; set; } 

        public async  Task<PorukaFiskalnogPrintera> SatTik()
        {
            await Task.Delay(199);
            return new PorukaFiskalnogPrintera() { IsError = false, LogLevel= Microsoft.Extensions.Logging.LogLevel.Information, MozeNastaviti=false, Poruka="Klik" };
            
        }
    }
}
