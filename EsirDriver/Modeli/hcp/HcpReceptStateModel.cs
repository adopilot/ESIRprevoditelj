using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace EsirDriver.Modeli.hcp
{
    [XmlRoot("RECEIPT_STATE")]
    public class HcpReceptStateModel
    {

        [XmlAttribute("AMOUNT")]
        public decimal Amount { get; set; }

        [XmlAttribute("DIFFERENCE")]
        public decimal Difference { get; set; }

        [XmlAttribute("RECEIPT_NUMBER")]
        public string ReceiptNumber { get; set; }

        [XmlAttribute("REFOUND_RECEIPT_NUMBER")]
        public string RefoundReceiptNumber { get; set; }

        [XmlAttribute("RECEIPT_TO_REFUND")]
        public int ReceiptToRefund { get; set; }

        [XmlAttribute("NUM_PAY")]
        public int NumPay { get; set; }

        [XmlAttribute("NUM_PLU")]
        public int NumPlu { get; set; }

        [XmlAttribute("CLIENT")]
        public int Client { get; set; }

        [XmlAttribute("CASHIER")]
        public int Cashier { get; set; }

        [XmlAttribute("FISCAL_DAY_STARTED")]
        public bool FiscalDayStarted { get; set; }

        [XmlAttribute("FISCAL_RECEIPT_STARTED")]
        public bool FiscalReceiptStarted { get; set; }

        [XmlAttribute("REFOUND_MODE")]
        public bool RefoundMode { get; set; }

        [XmlElement("PAY")]
        public List<Pay> Pays { get; set; }
    }

    public class Pay
    {
        [XmlAttribute("AMOUNT")]
        public decimal Amount { get; set; }
    }

}

