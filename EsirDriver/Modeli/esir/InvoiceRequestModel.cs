using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EsirDriver.Modeli.esir
{
    public enum InvoiceType
    {
        Normal, Proforma, Copy, Training, Advance
    }
    public enum TranscationType
    {
        Sale, Refund
    }
    public enum PaymentTypes
    {
        Cash, Card, Check, WireTransfer, Voucher, MobileMoney, Other

    }
    public class PaymentModel
    {
        public decimal amount { get; set; }
        public PaymentTypes paymentType { get; set; }
    }

    public class ItemModel
    {
        public string name { get; set; }
        public string gtin { get; set; }
        public List<string> labels { get; set; } = new List<string>();
        public decimal unitPrice { get; set; }
        public decimal quantity { get; set; }
        public decimal totalAmount { get; set; }
        public decimal discount { get; set; }
        public decimal discountAmount { get; set; }

    }



    public class InvoiceModel
    {
        /// <summary>
        /// Base64 kodirana slika koja se štampa na početku računa
        /// </summary>
        public string receiptHeaderImage { get; set; }
        /// <summary>
        /// Base64 kodirana slika koja se štampa na kraju računa
        /// </summary>
        public string receiptFooterImage { get; set; }
        public List<string> receiptHeaderTextLines { get; set; } = new List<string>();
        public List<string> receiptFooterTextLines { get; set; } = new List<string>();

        public InvoiceRequest invoiceRequest { get; set; }

    }



    public class InvoiceRequest
    {

        public InvoiceType invoiceType { get; set; } = InvoiceType.Normal;
        public TranscationType transactionType { get; set; }= TranscationType.Sale;
        public List<PaymentModel> payment {  get; set; } = new List<PaymentModel>();
        public List<ItemModel> items { get; set; } = new List<ItemModel>();
        public string cashier { get; set; }
        /// <summary>
        /// identifikator kupca
        /// </summary>
        public string buyerId { get; set; }
        /// <summary>
        /// opciono polje kupca
        /// </summary>
        public string buyerCostCenterId { get; set; }
        /// <summary>
        /// broj referentnog dokumenta (broj računa na koji se referencira ovaj račun)
        /// </summary>
        public string referentDocumentNumber { get; set; }
        /// <summary>
        /// vreme kada je izdat referentni dokument (vreme kada je fiskalizovan račun na koji se referencira ovaj račun)
        /// </summary>
        public DateTime? referentDocumentDT { get; set; }

        public bool print { get; set; } = true;
        public string email { get; set; }
        public bool renderReceiptImage { get; set; }

        public DateTime? dateAndTimeOfIssue { get; set; }
        /// <summary>
        /// iznos avansne uplate, kod automatskog izdavanja konačnog računa.
        /// </summary>
        public decimal? advancePaid { get; set; }
        /// <summary>
        /// iznos poreza obračunatog na avansne uplate, kod automatskog izdavanja konačnog računa.
        /// </summary>
        public decimal? advanceTax { get; set; }

        public string printerName { get; set; }







    }
}
