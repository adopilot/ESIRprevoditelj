using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EsirDriver.Modeli.esir
{
    public class InvoiceResponseModel
    {

            public string address { get; set; }
            public string businessName { get; set; }
            public string district { get; set; }
            public string encryptedInternalData { get; set; }
            public string invoiceCounter { get; set; }
            public string invoiceCounterExtension { get; set; }
            public string invoiceImageHtml { get; set; }
            public string invoiceImagePdfBase64 { get; set; }
            public string invoiceImagePngBase64 { get; set; }
            public string invoiceNumber { get; set; }
            public string journal { get; set; }
            public string locationName { get; set; }
            public string messages { get; set; }
            public string mrc { get; set; }
            public string requestedBy { get; set; }
            public DateTime sdcDateTime { get; set; }
            public string signature { get; set; }
            public string signedBy { get; set; }
            public int taxGroupRevision { get; set; }
            public List<TaxItem> taxItems { get; set; }
            public string tin { get; set; }
            public double totalAmount { get; set; }
            public int totalCounter { get; set; }
            public int transactionTypeCounter { get; set; }
            public string verificationQRCode { get; set; }
            public string verificationUrl { get; set; }
        }

        public class TaxItem
        {
            public double amount { get; set; }
            public string categoryName { get; set; }
            public int categoryType { get; set; }
            public string label { get; set; }
            public double rate { get; set; }
        }

}
