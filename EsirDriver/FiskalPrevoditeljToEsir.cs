using EsirDriver.Implmentacije;
using EsirDriver.Modeli;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;


namespace EsirDriver
{
    public class FiskalPrevoditeljToEsir 
    {
        public event EventHandler<PorukaFiskalnogPrintera>? MessageReceived;

        private PeriodicTimer _timer;
        private Modeli.EsirConfigModel _esirSettings { get; set; }= new EsirConfigModel();
        private EsirConfigModel _esirConfig { get; set; }
        private Modeli.PrevoditeljSettingModel _prevoditeljSettings { get; set; }
        private IFiskalniPrevoditelj _fiskalniPrevoditelj;
        private string _stateInfoMsg { get; set; } = "";
        private bool _stateIsError { get; set; }

        
        





        private EsirDriverEngin _esir;
        private bool _jeliPrviStart { get; set; }  
        public FiskalPrevoditeljToEsir(EsirConfigModel esirConfigModel,PrevoditeljSettingModel prevoditeljSettingModel) 
        {
            try
            {

                _esir = new EsirDriverEngin(esirConfigModel);

                switch (prevoditeljSettingModel.KomandePrintera)
                {
                    case PrevodimoKomandePrintera.TringFbih:
                        _fiskalniPrevoditelj = new TringPrevoditelj(_esir);
                        break;
                    default:
                        throw new Exception($"Tip fiskalnog printera {prevoditeljSettingModel.KomandePrintera} još nisu podržane");
                }


                _esirConfig = esirConfigModel;
                _prevoditeljSettings = prevoditeljSettingModel;
                if (_prevoditeljSettings.Enabled)
                {
                    _timer = new PeriodicTimer(TimeSpan.FromMilliseconds(_prevoditeljSettings?.ReadFolderEvryMiliSec??3000));
                }
                else
                {
                    _timer = new PeriodicTimer(Timeout.InfiniteTimeSpan);
                }
                _ = RunPeriodicTaskAsync();
            }
            catch (Exception ex)
            {
                prevoditeljSettingModel.Enabled = false;
                _stateInfoMsg = $"Greška u kostrukotu servisa FiskalPrevoditelj {ex.Message}";
                _stateIsError = true;
            }
        }

        


        private async Task RunPeriodicTaskAsync()
        {
            try
            {
                if (_jeliPrviStart)
                {
                    var aktivanSam =await _esir.ProvjeriDostupan();

                }


                while ( await _timer.WaitForNextTickAsync()  && _prevoditeljSettings.Enabled )
                {
                    // Simulate some asynchronous processing
                    var poruka = await _fiskalniPrevoditelj.SatTik();

                    if (!(poruka?.MozeNastaviti ?? false))
                    {
                        Stop();
                        OnMessageReceived(new PorukaFiskalnogPrintera() { IsError = true, Poruka = "Gasim servis jer je tako rekao  tring prvoditelj" });
                    }

                    OnMessageReceived(poruka);
                }
            }
            catch (Exception ex)
            {
                PorukaFiskalnogPrintera porukaFiskalnogPrintera = new PorukaFiskalnogPrintera() { IsError = true, LogLevel = Microsoft.Extensions.Logging.LogLevel.Error, Poruka = ex.Message };
                OnMessageReceived(porukaFiskalnogPrintera);
            }
        }

        public void Start()
        {
          
            _prevoditeljSettings.Enabled = true;
            _timer.Period = TimeSpan.FromMilliseconds(_prevoditeljSettings.ReadFolderEvryMiliSec);
            OnMessageReceived(new PorukaFiskalnogPrintera() { Poruka = "Serivs upaljen", LogLevel= Microsoft.Extensions.Logging.LogLevel.Information });

        }

        public void Stop()
        {
            _prevoditeljSettings.Enabled = false;
            _timer.Period = Timeout.InfiniteTimeSpan;
            OnMessageReceived(new PorukaFiskalnogPrintera() { Poruka = "Serivs Ugašen", LogLevel = Microsoft.Extensions.Logging.LogLevel.Information });
        }

        public PorukaFiskalnogPrintera GetState()
        {
            return new PorukaFiskalnogPrintera() { LogLevel= Microsoft.Extensions.Logging.LogLevel.Information, MozeNastaviti=, Poruka=prevoditeljStateInfo}
        }

        protected virtual void OnMessageReceived(PorukaFiskalnogPrintera poruka)
        {
            MessageReceived?.Invoke(this, poruka);
        }





    }
}
