using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Fiscal
{
    public class PrevoditeljSettingModel
    {
        public bool Upaljen { get; set; }
        public string EsirEndPoint { get; set; } = "http://127.0.0.1:3566";
        public string ApiKey { get; set; } = "0123456789abcdef0123456789abcdef";
        public string Pin { get; set; }
        public string MyProperty { get; set; }

    }
}
