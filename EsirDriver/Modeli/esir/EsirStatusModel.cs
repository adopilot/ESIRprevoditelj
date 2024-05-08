using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EsirDriver.Modeli.esir
{
    internal class EsirStatusModel
    {
        public List<AllTaxRate> allTaxRates { get; set; }
        public CurrentTaxRates currentTaxRates { get; set; }
        public string deviceSerialNumber { get; set; }
        public List<string> gsc { get; set; }
        public string hardwareVersion { get; set; }
        public string lastInvoiceNumber { get; set; }
        public string make { get; set; }
        public string model { get; set; }
        public List<object> mssc { get; set; }
        public string protocolVersion { get; set; }
        public DateTime sdcDateTime { get; set; }
        public string softwareVersion { get; set; }
        public List<string> supportedLanguages { get; set; }
    }
    internal class AllTaxRate
    {
        public int groupId { get; set; }
        public List<TaxCategory> taxCategories { get; set; }
        public DateTime validFrom { get; set; }
    }

    internal class CurrentTaxRates
    {
        public int groupId { get; set; }
        public List<TaxCategory> taxCategories { get; set; }
        public DateTime validFrom { get; set; }
    }
    internal class TaxCategory
    {
        public int categoryType { get; set; }
        public string name { get; set; }
        public int orderId { get; set; }
        public List<TaxRate> taxRates { get; set; }
    }

    internal class TaxRate
    {
        public string label { get; set; }
        public int rate { get; set; }
    }

}
