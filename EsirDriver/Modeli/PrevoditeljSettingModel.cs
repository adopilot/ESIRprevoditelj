using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EsirDriver.Modeli
{

    [RequiresUnreferencedCode("Necessary because of RangeAttribute usage")]
    public class PrevoditeljSettingModel
    {
        
        public bool Enabled { get; set; }
        public string PathInputFiles { get; set; }
        public string PathOutputFiles { get; set; }
        public int ReadFolderEvryMiliSec { get; set; } = 1000;
        public bool AutomaticallyCloseRecept { get; set; } = true;
        public string EncodingName { get; set; } = "windows-1250";
        public PrevodimoKomandePrintera KomandePrintera { get; set; }= PrevodimoKomandePrintera.HcpFBiH;
        [Required(ErrorMessage = "Poreska stopa je obavzna")]
        [MinLength(1, ErrorMessage = "Samoj jedna poreska stopa je dozvoljnja")]
        [StringLength(1, ErrorMessage = "Samo jedna poreka stopa je dozvoljena ")]
        public string PodrazumjevanaPoreskaStopa { get; set; } = "Е";
        public string DefSklSifra { get; set; } = "000";
    }

    public enum PrevodimoKomandePrintera
    {
        HcpFBiH =0,
        GalebFisLink = 1,
        MikroElektornikaFLink =2
    }
}
