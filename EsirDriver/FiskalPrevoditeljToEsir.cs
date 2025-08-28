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
        private EsirConfigModel _esirConfig { get; set; }
        private Modeli.PrevoditeljSettingModel _prevoditeljSettings { get; set; } = new PrevoditeljSettingModel() { Enabled=false };
        private IFiskalniPrevoditelj _fiskalniPrevoditelj;
        private string _stateInfoMsg { get; set; } = "";
        private bool _stateIsError { get; set; }

        private bool _preplacenNaEventePrevoditelja = false;
        







        public EsirDriverEngin _esir { get; set; }
        
        public FiskalPrevoditeljToEsir(EsirConfigModel esirConfigModel,PrevoditeljSettingModel prevoditeljSettingModel) 
        {
            _esir = new EsirDriverEngin();
            _esir.PorukaEvent += _esir_PorukaEvent;

           

            _ = Konfigurisi(esirConfigModel, prevoditeljSettingModel);
            _ = RunPeriodicTaskAsync();
        }

        private void _esir_PorukaEvent(object? sender, PorukaFiskalnogPrintera e)
        {

            OnMessageReceived(e);
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


                PorukaFiskalnogPrintera porukaFiskalnogPrintera = new PorukaFiskalnogPrintera() { IsError = true, LogLevel = LogLevel.Debug, Poruka = ex.Message };
                OnMessageReceived(porukaFiskalnogPrintera);
            }
        }

        public async Task<PorukaFiskalnogPrintera> Konfigurisi(EsirConfigModel esirConfigModel, PrevoditeljSettingModel prevoditeljSettingModel)
        {
            try
            {
                _esirConfig = esirConfigModel;
                var tempDoesItNeedToWork = prevoditeljSettingModel.Enabled;
                Stop();
                _timer.Period = Timeout.InfiniteTimeSpan;
                
                
                _prevoditeljSettings = prevoditeljSettingModel;

             
                var configMsg = await _esir.Konfigurisi(this._esirConfig);
                if (!(configMsg?.MozeNastaviti ?? false))
                    {
                        _prevoditeljSettings.Enabled = false;
                        _stateInfoMsg = $"Nismo uspijeli konfigusati ESIR poruka:  {(configMsg?.Poruka ?? "null")}";
                        _stateIsError = true;
                        //OnMessageReceived( configMsg ?? new PorukaFiskalnogPrintera() { MozeNastaviti = false, IsError = true, LogLevel = LogLevel.Critical, Poruka = "Nisam uspijeo konfigursati  esir" });
                        return configMsg ?? new PorukaFiskalnogPrintera() { MozeNastaviti = false, IsError = true, LogLevel = LogLevel.Critical, Poruka = "Nisam uspijeo konfigursati  esir" };
                    }

                switch (prevoditeljSettingModel.KomandePrintera)
                {
                    case PrevodimoKomandePrintera.HcpFBiH:
                         Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                        try
                        {
                            var encoding = Encoding.GetEncoding(prevoditeljSettingModel?.EncodingName ?? "windows-1250");
                        }
                        catch (Exception ex)
                        {
                           var encodingSex = new PorukaFiskalnogPrintera() { IsError=true, LogLevel= LogLevel.Warning, MozeNastaviti= false, Poruka=$"Postavke za enkoing xml fajlova nisu ispravne greška {ex.Message}" };
                            OnMessageReceived(encodingSex);
                            return encodingSex;
                        }
                        _fiskalniPrevoditelj = new HCPPrevoditelj(_esir,_prevoditeljSettings);
                        break;
                    default:
                        _stateInfoMsg = $"Tip fiskalnog printera {prevoditeljSettingModel.KomandePrintera} još nisu podržane";
                        _stateIsError = true;
                        var poruka = new PorukaFiskalnogPrintera() { IsError = true, LogLevel = LogLevel.Critical, Poruka = _stateInfoMsg };
                        OnMessageReceived(poruka);
                        return poruka;
                }
                var gtinSetInfo = await _esir.SetReceiptPrintGtin(false);
                OnMessageReceived(gtinSetInfo);



                if (tempDoesItNeedToWork)
                {
                    Start();
                }
                var porukaOk = new PorukaFiskalnogPrintera() { LogLevel = LogLevel.Information, MozeNastaviti = true, IsError = false, Poruka = "Serivs je konfigursan" };
                OnMessageReceived(porukaOk);
                return porukaOk;
               
                
            }
            catch (Exception ex)
            {
                _prevoditeljSettings.Enabled = false;
                _stateInfoMsg = $"Greška prilikom konfiguracije servisa FiskalPrevoditelj {ex.Message}";
                _stateIsError = true;
                var erroMsg = new PorukaFiskalnogPrintera() { 
                    LogLevel = LogLevel.Error, 
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
            if (_fiskalniPrevoditelj == null)
            {
                OnMessageReceived(new PorukaFiskalnogPrintera() { Poruka = "Servis prevođenja nije konfigursan i nemže biti pokrenut", IsError=true, MozeNastaviti=false, LogLevel = LogLevel.Error });
                return;
            }

            _prevoditeljSettings.Enabled = true;
            if (!_preplacenNaEventePrevoditelja)
            {
                _fiskalniPrevoditelj.PorukaEvent += _fiskalniPrevoditelj_PorukaEvent;
                _preplacenNaEventePrevoditelja = true;
            }
           


            _timer.Period = TimeSpan.FromMilliseconds(_prevoditeljSettings.ReadFolderEvryMiliSec);
            OnMessageReceived(new PorukaFiskalnogPrintera() { Poruka = "Pokrećem serivs", LogLevel= LogLevel.Information, MozeNastaviti=true, IsError=false });

        }

        public void Stop()
        {
            _prevoditeljSettings.Enabled = false;
            _timer.Period = Timeout.InfiniteTimeSpan;

            if (_fiskalniPrevoditelj !=null && _fiskalniPrevoditelj_PorukaEvent != null)
            {
                OnMessageReceived(new PorukaFiskalnogPrintera() { Poruka = "Serivs Ugašen", LogLevel = LogLevel.Information });
                _fiskalniPrevoditelj.PorukaEvent -= _fiskalniPrevoditelj_PorukaEvent;
            }
            _preplacenNaEventePrevoditelja = false;

        }

        protected virtual void OnMessageReceived(PorukaFiskalnogPrintera poruka)
        {
            MessageReceived?.Invoke(this, poruka);
        }

        

        public PrevoditeljSettingModel GetPrevoditeljConfig()
        {
            return _prevoditeljSettings;
        }

    }
}
