using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EsirDriver.Modeli.esir
{
    public class EsirErrorResopnse
    {
        public string message { get; set; }
        public List<ModelState> modelState { get; set; }
    }
    public class ModelState
    {
        public List<string> errors { get; set; }
        public string property { get; set; }
    }
}
