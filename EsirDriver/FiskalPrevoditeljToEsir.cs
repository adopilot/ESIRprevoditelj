using EsirDriver.Modeli;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace EsirDriver
{
    public class FiskalPrevoditeljToEsir : EsirDriverEngin
    {
        public event EventHandler<PorukaFiskalnogPrintera> MessageReceived;

        private readonly PeriodicTimer _timer;
        private Modeli.EsirSettingsModel esirSettings { get; set; }
        private EsirConfigModel esirConfig { get; set; }
        private Modeli.PrevoditeljSettingModel prevoditeljSettings { get; set; }
        public FiskalPrevoditeljToEsir(EsirConfigModel esirConfigModel,PrevoditeljSettingModel prevoditeljSettingModel) : base(esirConfigModel)
        {
            this.esirConfig = esirConfigModel;
            this.prevoditeljSettings = prevoditeljSettingModel;

            
                // Initialize your service
                _timer = new PeriodicTimer(TimeSpan.FromMilliseconds(100));
                _ = RunPeriodicTaskAsync();
            
             
              

        }

        protected virtual void OnMessageReceived(PorukaFiskalnogPrintera poruka)
        {
            MessageReceived?.Invoke(this, poruka);
        }

        private async Task RunPeriodicTaskAsync()
        {
            while (await _timer.WaitForNextTickAsync())
            {
                // Simulate some asynchronous processing
                await Task.Delay(1000);

                // Create a message
                var message = new PorukaFiskalnogPrintera
                {
                    Poruka = "Message from timer",
                    IsError = false,
                    MozeNastaviti = true,
                    NeAutorizovan = false,
                    NemaBezbednsniElement = false,
                    TraziPin = false
                };

                // Raise the MessageReceived event
                OnMessageReceived(message);
            }
        }






    }
}
