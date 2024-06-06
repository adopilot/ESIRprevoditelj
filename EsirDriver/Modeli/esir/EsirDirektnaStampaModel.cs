using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EsirDriver.Modeli.esir
{
    public class EsirDirektnaStampaModel
    {
        public List<string> textLines { get; set; } = new List<string>();
        public string imagePngBase64 { get; set; }
        public string rawBase64 { get; set; }
    }
}
