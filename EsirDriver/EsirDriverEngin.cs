using EsirDriver.Modeli;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            _httpClient = new HttpClient() { BaseAddress = new Uri(esirConfigModel?.webserverAddress ?? "http://127.0.0.1:3566/")};
        }

        public async Task<PorukaFiskalnogPrintera> Konfigurisi(EsirConfigModel esirConfigModel)
        {
            this._esirConfig = esirConfigModel;


            _httpClient = new HttpClient() { BaseAddress = new Uri(esirConfigModel?.webserverAddress ?? "http://127.0.0.1:3566/") };

            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_esirConfig.apiKey}");

            var dostupan = await ProvjeriDostupan();

            return new PorukaFiskalnogPrintera() { IsError = false };
            
        }
        
        private async Task<(bool dostupan,string poruka)> ProvjeriDostupan()
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "/api/attention/");
                var response = await _httpClient.SendAsync(request);

                response.EnsureSuccessStatusCode();
                return new(true, "");

            }
            catch (Exception ex)
            {
                return new(false, $"Sistemska greša");
            }

        }
       

        




    }
}
