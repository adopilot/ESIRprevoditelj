using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EsirDriver.Modeli
{
    public class PrevoditeljSettingModel
    {
        public bool KoristiSe { get; set; } = false;
        public bool Upaljen { get; set; } = false;
        public EsirConfigModel EsirConfig { get; set; } = new EsirConfigModel() { EsirSettingsModel = new EsirSettingsModel() };


    }
}
