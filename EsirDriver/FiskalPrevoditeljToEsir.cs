using EsirDriver.Modeli;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;


namespace EsirDriver
{
    public class FiskalPrevoditeljToEsir : EsirDriverEngin
    {
        public event EventHandler<PorukaFiskalnogPrintera>? MessageReceived;

        private PeriodicTimer? _timer;
        private Modeli.EsirConfigModel esirSettings { get; set; }= new EsirConfigModel();
        private EsirConfigModel esirConfig { get; set; }
        private Modeli.PrevoditeljSettingModel _prevoditeljSettings { get; set; }

        private bool _jeliPrviStart { get; set; }  
        public FiskalPrevoditeljToEsir(EsirConfigModel esirConfigModel,PrevoditeljSettingModel prevoditeljSettingModel) : base(esirConfigModel)
        {
            this.esirConfig = esirConfigModel;
            this._prevoditeljSettings = prevoditeljSettingModel;
            if (_prevoditeljSettings.Enabled)
            {
                Start();
            }
        }

        


        private async Task RunPeriodicTaskAsync()
        {
            try
            {
                while (await _timer.WaitForNextTickAsync() && _prevoditeljSettings.Enabled )
                {
                    // Simulate some asynchronous processing
                    await Task.Delay(1000);

                    // Create a message
                    var message = new PorukaFiskalnogPrintera
                    {
                        Poruka = $"Radim normal",
                        IsError = false,
                        MozeNastaviti = true,
                        NeAutorizovan = false,
                        NemaBezbednsniElement = false,
                        TraziPin = false,
                        LogLevel= Microsoft.Extensions.Logging.LogLevel.Information
                    };

                    // Raise the MessageReceived event
                    OnMessageReceived(message);
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
            // Check if the timer is already running
            if (_timer != null)
            {
                if (_prevoditeljSettings.Enabled)
                {
                    return; 
                }

            }
            if (_timer == null)
                _timer = new PeriodicTimer(TimeSpan.FromMilliseconds(_prevoditeljSettings.ReadFolderEvryMiliSec));

            else
                _timer.Period=TimeSpan.FromMilliseconds(_prevoditeljSettings.ReadFolderEvryMiliSec);

            // Start the timer
            _prevoditeljSettings.Enabled = true;
            
            _ = RunPeriodicTaskAsync();

            OnMessageReceived(new PorukaFiskalnogPrintera() { Poruka = "Serivs upaljen", LogLevel= Microsoft.Extensions.Logging.LogLevel.Information });

        }

        public void Stop()
        {
            // Stop the timer
            if (_timer !=null)
                _timer.Period = Timeout.InfiniteTimeSpan;
            
            _timer = null;
            _prevoditeljSettings.Enabled = false;
        }


        protected virtual void OnMessageReceived(PorukaFiskalnogPrintera poruka)
        {
            MessageReceived?.Invoke(this, poruka);
        }





    }
}
