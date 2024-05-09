using EsirDriver.Modeli;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace EsirDriver.Implmentacije
{
    internal class TringPrevoditelj : IFiskalniPrevoditelj
    {
        private readonly EsirDriverEngin _esir;
        private readonly PrevoditeljSettingModel _prevoditeljSettingModel;
        private bool isLocked { get; set; }

        internal TringPrevoditelj(EsirDriverEngin esir,PrevoditeljSettingModel prevoditeljSettingModel) 
        { 
            this._esir = esir; 
            this._prevoditeljSettingModel = prevoditeljSettingModel;
            this.isLocked = false;
        }

        public event EventHandler<PorukaFiskalnogPrintera> PorukaEvent;

        public async  Task<PorukaFiskalnogPrintera> SatTik()
        {
            if (!isLocked)
            {
                bool imamCmdOkFile = false;
                var files = Directory.GetFiles(_prevoditeljSettingModel.PathInputFiles)?.ToList()??new List<string>();

                foreach (var file in files) 
                {
                    if (Path.GetFileName(file) == "CMD.OK")
                    {
                        if (files.Count == 1)
                        {
                            File.Delete(file);
                            PorukaEvent.Invoke(this, new PorukaFiskalnogPrintera() { LogLevel = LogLevel.Debug, Poruka = "U folderu sam našao samo cmd.ok i brišem je " });
                        }
                        PorukaEvent.Invoke(this, new PorukaFiskalnogPrintera() { LogLevel = LogLevel.Debug, Poruka = "Imam cmd file i obrađujem ostale datoeke " });
                    }
                    
                }

            }

            
            
            await Task.Delay(300);

            

            //PorukaEvent?.Invoke(this, new PorukaFiskalnogPrintera() { IsError = false, MozeNastaviti = true, LogLevel = LogLevel.Debug, Poruka = "Ovo je resendani event" });            
            return new PorukaFiskalnogPrintera() { IsError = false, LogLevel= Microsoft.Extensions.Logging.LogLevel.Debug, MozeNastaviti=true, Poruka="Klik" };
        }
    }
}
