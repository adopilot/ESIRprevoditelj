using EsirDriver.Implmentacije;
using EsirDriver.Modeli;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;


namespace EsirDriver
{
    public class FiskalPrevoditeljToEsir 
    {
        public event EventHandler<PorukaFiskalnogPrintera>? MessageReceived;

        private PeriodicTimer _timer = new PeriodicTimer(Timeout.InfiniteTimeSpan);
        private EsirConfigModel _esirConfig { get; set; } = new EsirConfigModel();
        private Modeli.PrevoditeljSettingModel _prevoditeljSettings { get; set; } = new PrevoditeljSettingModel() { Enabled=false };
        private IFiskalniPrevoditelj _fiskalniPrevoditelj;
        private string _stateInfoMsg { get; set; } = "";
        private bool _stateIsError { get; set; }

        private bool _preplacenNaEventePrevoditelja = false;
        
        





        private EsirDriverEngin _esir;
        
        public FiskalPrevoditeljToEsir(EsirConfigModel esirConfigModel,PrevoditeljSettingModel prevoditeljSettingModel) 
        {
            _ = Konfigurisi(esirConfigModel, prevoditeljSettingModel);
            _ = RunPeriodicTaskAsync();
        }

        


        private async Task RunPeriodicTaskAsync()
        {
            try
            {
                while (await _timer.WaitForNextTickAsync() && _prevoditeljSettings.Enabled)
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
                _prevoditeljSettings.Enabled = false;
                _stateInfoMsg = $"Greška izvšavanju servisa: {ex.Message}";
                _stateIsError = true;


                PorukaFiskalnogPrintera porukaFiskalnogPrintera = new PorukaFiskalnogPrintera() { IsError = true, LogLevel = Microsoft.Extensions.Logging.LogLevel.Debug, Poruka = ex.Message };
                OnMessageReceived(porukaFiskalnogPrintera);
            }
        }

        public async Task<PorukaFiskalnogPrintera> Konfigurisi(EsirConfigModel esirConfigModel, PrevoditeljSettingModel prevoditeljSettingModel)
        {
            try
            {
                var tempDoesItNeedToWork = prevoditeljSettingModel.Enabled;
                Stop();
                _timer.Period = Timeout.InfiniteTimeSpan;
                
                _esir = new EsirDriverEngin(esirConfigModel);
                _prevoditeljSettings = prevoditeljSettingModel;

                var configMsg = await _esir.Konfigurisi(this._esirConfig);
                if (!(configMsg?.MozeNastaviti ?? false))
                    {
                        _prevoditeljSettings.Enabled = false;
                        _stateInfoMsg = $"Nismo uspijeli konfigusati ESIR poruka:  {(configMsg?.Poruka ?? "null")}";
                        _stateIsError = true;
                        OnMessageReceived( configMsg ?? new PorukaFiskalnogPrintera() { MozeNastaviti = false, IsError = true, LogLevel = Microsoft.Extensions.Logging.LogLevel.Critical, Poruka = "Nisam uspijeo konfigursati  esir" });
                        return configMsg ?? new PorukaFiskalnogPrintera() { MozeNastaviti = false, IsError = true, LogLevel = Microsoft.Extensions.Logging.LogLevel.Critical, Poruka = "Nisam uspijeo konfigursati  esir" };
                    }

                switch (prevoditeljSettingModel.KomandePrintera)
                {
                    case PrevodimoKomandePrintera.HcpFBiH:
                        _fiskalniPrevoditelj = new HCPPrevoditelj(_esir,_prevoditeljSettings);
                        break;
                    default:
                        _stateInfoMsg = $"Tip fiskalnog printera {prevoditeljSettingModel.KomandePrintera} još nisu podržane";
                        _stateIsError = true;
                        var poruka = new PorukaFiskalnogPrintera() { IsError = true, LogLevel = Microsoft.Extensions.Logging.LogLevel.Critical, Poruka = _stateInfoMsg };
                        OnMessageReceived(poruka);
                        return poruka;
                }

                


                if (tempDoesItNeedToWork)
                {
                    Start();
                }
                var porukaOk = new PorukaFiskalnogPrintera() { LogLevel = Microsoft.Extensions.Logging.LogLevel.Information, MozeNastaviti = true, IsError = false, Poruka = "Serivs je konfigursan" };
                OnMessageReceived(porukaOk);
                return porukaOk;
               
                
            }
            catch (Exception ex)
            {
                _prevoditeljSettings.Enabled = false;
                _stateInfoMsg = $"Greška prilikom konfiguracije servisa FiskalPrevoditelj {ex.Message}";
                _stateIsError = true;
                var erroMsg = new PorukaFiskalnogPrintera() { 
                    LogLevel = Microsoft.Extensions.Logging.LogLevel.Error, 
                    MozeNastaviti = false, IsError = true, 
                    Poruka = $"Greška prilikom konfiguracije servisa FiskalPrevoditelj {ex.Message}" ,
                };
                OnMessageReceived(erroMsg);
                return erroMsg;
            }
        }

        private void _fiskalniPrevoditelj_PorukaEvent(object? sender, PorukaFiskalnogPrintera e)
        {
            if (e != null)
                OnMessageReceived(e);
        }

        public void Start()
        {
            _prevoditeljSettings.Enabled = true;
            if (!_preplacenNaEventePrevoditelja)
            {
                _fiskalniPrevoditelj.PorukaEvent += _fiskalniPrevoditelj_PorukaEvent;
                _preplacenNaEventePrevoditelja = true;
            }
            _timer.Period = TimeSpan.FromMilliseconds(_prevoditeljSettings.ReadFolderEvryMiliSec);
            OnMessageReceived(new PorukaFiskalnogPrintera() { Poruka = "Pokrećem serivs", LogLevel= Microsoft.Extensions.Logging.LogLevel.Information });

        }

        public void Stop()
        {
            _prevoditeljSettings.Enabled = false;
            _timer.Period = Timeout.InfiniteTimeSpan;

            if (_fiskalniPrevoditelj !=null && _fiskalniPrevoditelj_PorukaEvent != null)
                _fiskalniPrevoditelj.PorukaEvent -= _fiskalniPrevoditelj_PorukaEvent;
            
            _preplacenNaEventePrevoditelja = false;
            

            OnMessageReceived(new PorukaFiskalnogPrintera() { Poruka = "Serivs Ugašen", LogLevel = Microsoft.Extensions.Logging.LogLevel.Information });
        }

        protected virtual void OnMessageReceived(PorukaFiskalnogPrintera poruka)
        {
            MessageReceived?.Invoke(this, poruka);
        }

        



    }
}
