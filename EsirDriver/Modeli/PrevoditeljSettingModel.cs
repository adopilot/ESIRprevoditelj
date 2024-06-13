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
        public string EncodingName { get; set; } = "windows-1250";
        public PrevodimoKomandePrintera KomandePrintera { get; set; }= PrevodimoKomandePrintera.HcpFBiH;
        public string PodrazumjevanaPoreskaStopa { get; set; } = "F";
        public string DefSklSifra { get; set; } = "000";
    }

    public enum PrevodimoKomandePrintera
    {
        HcpFBiH =0,
        GalebFisLink = 1,
        MikroElektornikaFLink =2
    }
}
