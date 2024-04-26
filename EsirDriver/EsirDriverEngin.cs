using EsirDriver.Modeli;
using Shared.Fiscal;
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
            _httpClient = new HttpClient() { BaseAddress = new Uri(esirConfigModel?.EsirSettingsModel.webserverAddress ?? "http://127.0.0.1:3566/")};
        }




    }
}
