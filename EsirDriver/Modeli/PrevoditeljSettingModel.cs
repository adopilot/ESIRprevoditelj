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
        public bool AutomaticallyCloseRecept { get; set; }

    }
}
