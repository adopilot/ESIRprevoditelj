using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EsirDriver.Modeli
{
    public class PrevoditeljSettingModel
    {
        
        public bool Enabled { get; set; }
        public string PathInputFiles { get; set; }
        public string PathOutputFiles { get; set; }
        public int ReadFolderEvryMiliSec { get; set; } = 3000;
        public bool AutomaticallyCloseRecept { get; set; } = true;

        public string EncodingName { get; set; } = "UTF-8";
        public PrevodimoKomandePrintera KomandePrintera { get; set; }= PrevodimoKomandePrintera.HcpFBiH;

    }

    public enum PrevodimoKomandePrintera
    {
        HcpFBiH =0,
        GalebFisLink = 1,
        MikroElektornikaFLink =2
    }
}
