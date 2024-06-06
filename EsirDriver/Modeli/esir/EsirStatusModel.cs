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
        public bool auditRequired { get; set; }
        public CurrentTaxRates currentTaxRates { get; set; }
        public string deviceSerialNumber { get; set; }
        public List<string> gsc { get; set; }
        public string hardwareVersion { get; set; }
        public bool isPinRequired { get; set; }
        public string lastInvoiceNumber { get; set; }
        public string make { get; set; }
        public string model { get; set; }
        public List<object> mssc { get; set; }
        public string protocolVersion { get; set; }
        public DateTime sdcDateTime { get; set; }
        public string secureElementVersion { get; set; }
        public string softwareVersion { get; set; }
        public List<string> supportedLanguages { get; set; }
        public string taxCoreApi { get; set; }
        public string uid { get; set; }
    }
    public class AllTaxRate
    {
        public int groupId { get; set; }
        public List<TaxCategory> taxCategories { get; set; }
        public DateTime validFrom { get; set; }
    }

    public class CurrentTaxRates
    {
        public int groupId { get; set; }
        public List<TaxCategory> taxCategories { get; set; }
        public DateTime validFrom { get; set; }
    }
    public class TaxCategory
    {
        public int categoryType { get; set; }
        public string name { get; set; }
        public int orderId { get; set; }
        public List<TaxRate> taxRates { get; set; }
    }

    public class TaxRate
    {
        public string label { get; set; }
        public double rate { get; set; }
    }
}
