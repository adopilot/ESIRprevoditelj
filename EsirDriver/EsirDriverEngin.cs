using EsirDriver.Modeli;
using EsirDriver.Modeli.esir;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EsirDriver
{
    public class EsirDriverEngin
    {
        private EsirConfigModel _esirConfig;
        HttpClient _httpClient;
        public EsirDriverEngin(EsirConfigModel esirConfigModel)
        {
            _esirConfig = esirConfigModel;
            _httpClient = new HttpClient() { BaseAddress = new Uri(esirConfigModel?.webserverAddress ?? "http://127.0.0.1:3566/") };
        }

        public async Task<PorukaFiskalnogPrintera> Konfigurisi(EsirConfigModel esirConfigModel)
        {
            this._esirConfig = esirConfigModel;


            _httpClient = new HttpClient() { BaseAddress = new Uri(esirConfigModel?.webserverAddress ?? "http://127.0.0.1:3566/") };

            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_esirConfig.apiKey}");

            var dostupan = await ProvjeriDostupan();

            if (dostupan.IsError)
                return dostupan;



            return new PorukaFiskalnogPrintera() { IsError = false };

        }

        public async Task<PorukaFiskalnogPrintera> StatusFiskalnogPrintera()
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "/api/settings");
                var response = await _httpClient.SendAsync(request);
                string res = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                   Modeli.esir.EsirCurrentSettingsModel esirCurrent = JsonSerializer.Deserialize<EsirCurrentSettingsModel>(res);

                }
                else
                
            }
            catch (Exception ex)
            {
                return new PorukaFiskalnogPrintera { IsError = true,  };
            }
        }

        public async Task<PorukaFiskalnogPrintera> ProvjeriDostupan()
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "/api/attention/");
                var response = await _httpClient.SendAsync(request);
                
                if (response.IsSuccessStatusCode)
                    return new PorukaFiskalnogPrintera() { IsError = false, LogLevel = Microsoft.Extensions.Logging.LogLevel.Information, MozeNastaviti = true, Poruka = "Sistem je aktivan" };
                else
                {
                    var poruka = await response.Content.ReadAsStringAsync();
                    return new PorukaFiskalnogPrintera() { IsError = true, LogLevel = Microsoft.Extensions.Logging.LogLevel.Error, MozeNastaviti = false, Poruka = $"Sistem nije aktivan: {poruka}" };

                }


            }
            catch (HttpRequestException ex)
            {
                return new PorukaFiskalnogPrintera() { Poruka = $"Sistem nije aktivan sa http greškom: {ex.StatusCode} {ex.Message} ", IsError = true, MozeNastaviti = false, LogLevel = Microsoft.Extensions.Logging.LogLevel.Error };
            }
            catch (Exception ex)
            {
                return new PorukaFiskalnogPrintera() { Poruka = $"Sistem nije aktivan sa sistemom greškom:  {ex.Message} ", IsError = true, MozeNastaviti = false, LogLevel = Microsoft.Extensions.Logging.LogLevel.Error };

            }

        }
    
       

        




    }
}
