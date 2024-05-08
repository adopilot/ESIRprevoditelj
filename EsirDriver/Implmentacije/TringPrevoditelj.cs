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
        private readonly PrevoditeljSettingModel _prevoditeljSettingModel;
        internal TringPrevoditelj(EsirDriverEngin esir,PrevoditeljSettingModel prevoditeljSettingModel) 
        { 
            this._esir = esir; 
            this._prevoditeljSettingModel = prevoditeljSettingModel;
        }
        private bool isLocked { get; set; }

        public event EventHandler<PorukaFiskalnogPrintera> PorukaEvent;

        public async  Task<PorukaFiskalnogPrintera> SatTik()
        {
            await Task.Delay(199);

            PorukaEvent?.Invoke(this, new PorukaFiskalnogPrintera() { IsError = false, MozeNastaviti = true, LogLevel = Microsoft.Extensions.Logging.LogLevel.Information, Poruka = "Ovo je resendani event" });            
            return new PorukaFiskalnogPrintera() { IsError = false, LogLevel= Microsoft.Extensions.Logging.LogLevel.Information, MozeNastaviti=true, Poruka="Klik" };
            
        }
    }
}
