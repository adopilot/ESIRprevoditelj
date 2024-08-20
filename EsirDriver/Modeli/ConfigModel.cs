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
    public class ConfigModel
    {
        public EsirConfigModel EsirConfigModel { get; set; }
        public PrevoditeljSettingModel PrevoditeljSettingModel { get; set; }
    }
}
