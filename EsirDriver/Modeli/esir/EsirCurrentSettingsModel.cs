using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EsirDriver.Modeli.esir
{
    internal class EsirCurrentSettingsModel
    {
        public List<int> allowedPaymentTypes { get; set; }
        public string apiKey { get; set; }
        public string applicationLanguage { get; set; }
        public bool authorizeLocalClients { get; set; }
        public bool authorizeRemoteClients { get; set; }
        public List<object> availableDisplayDevices { get; set; }
        public List<string> availableEftPosDevices { get; set; }
        public List<string> availableEftPosProtocols { get; set; }
        public List<string> availablePrinterTypes { get; set; }
        public List<string> availablePrinters { get; set; }
        public List<object> availableScaleDevices { get; set; }
        public List<string> availableScaleProtocols { get; set; }
        public object customTabName { get; set; }
        public object customTabUrl { get; set; }
        public object displayDeviceName { get; set; }
        public object displayDeviceRs232BaudRate { get; set; }
        public object displayDeviceRs232DataBits { get; set; }
        public object displayDeviceRs232HardwareFlowControl { get; set; }
        public object displayDeviceRs232Parity { get; set; }
        public object displayDeviceRs232StopBits { get; set; }
        public bool displayEnabled { get; set; }
        public object displayHandler { get; set; }
        public object displayProtocol { get; set; }
        public object displayTextCodePage { get; set; }
        public object displayTextCols { get; set; }
        public object displayTextRows { get; set; }
        public object eFakturaApiKey { get; set; }
        public object eFakturaCompanyAddress { get; set; }
        public object eFakturaCompanyBankAccount { get; set; }
        public object eFakturaCompanyCity { get; set; }
        public object eFakturaCompanyEMail { get; set; }
        public object eFakturaCompanyName { get; set; }
        public object eFakturaCompanyPhone { get; set; }
        public object eFakturaCompanyRegistrationId { get; set; }
        public object eFakturaCompanyTaxId { get; set; }
        public bool eFakturaTest { get; set; }
        public object eftPosCredentials { get; set; }
        public string eftPosDeviceName { get; set; }
        public object eftPosDeviceRs232BaudRate { get; set; }
        public object eftPosDeviceRs232DataBits { get; set; }
        public object eftPosDeviceRs232HardwareFlowControl { get; set; }
        public object eftPosDeviceRs232Parity { get; set; }
        public object eftPosDeviceRs232StopBits { get; set; }
        public string eftPosProtocol { get; set; }
        public bool issueCopyOnRefund { get; set; }
        public string language { get; set; }
        public List<string> languages { get; set; }
        public Lpfr lpfr { get; set; }
        public string lpfrUrl { get; set; }
        public object paperHeight { get; set; }
        public object paperMargin { get; set; }
        public object paperWidth { get; set; }
        public object posName { get; set; }
        public object printerDpi { get; set; }
        public object printerName { get; set; }
        public string printerType { get; set; }
        public object qrCodeSize { get; set; }
        public object receiptCustomCommandBegin { get; set; }
        public object receiptCustomCommandEnd { get; set; }
        public string receiptCutPaper { get; set; }
        public object receiptDiscountText { get; set; }
        public int receiptFeedLinesBegin { get; set; }
        public int receiptFeedLinesEnd { get; set; }
        public int receiptFontSizeLarge { get; set; }
        public int receiptFontSizeNormal { get; set; }
        public object receiptFooterImage { get; set; }
        public object receiptFooterTextLines { get; set; }
        public object receiptHeaderImage { get; set; }
        public object receiptHeaderTextLines { get; set; }
        public string receiptLayout { get; set; }
        public double receiptLetterSpacingCondensed { get; set; }
        public int receiptLetterSpacingNormal { get; set; }
        public string receiptOpenCashDrawer { get; set; }
        public object receiptSplitMaxHeight { get; set; }
        public int receiptWidth { get; set; }
        public int receiptsDelay { get; set; }
        public bool runUi { get; set; }
        public object scaleDeviceName { get; set; }
        public int scaleDeviceRs232BaudRate { get; set; }
        public int scaleDeviceRs232DataBits { get; set; }
        public int scaleDeviceRs232HardwareFlowControl { get; set; }
        public int scaleDeviceRs232Parity { get; set; }
        public int scaleDeviceRs232StopBits { get; set; }
        public object scaleProtocol { get; set; }
        public bool syncReceipts { get; set; }
        public object vpfrCertificateAddress { get; set; }
        public object vpfrCertificateBusinessName { get; set; }
        public object vpfrCertificateCity { get; set; }
        public object vpfrCertificateCountry { get; set; }
        public object vpfrCertificateSerialNumber { get; set; }
        public object vpfrCertificateShopName { get; set; }
        public object vpfrClientCertificateBase64 { get; set; }
        public bool vpfrEnabled { get; set; }
        public object vpfrPac { get; set; }
    }
    internal class Lpfr
    {
        public string apiKey { get; set; }
        public bool authorizeLocalClients { get; set; }
        public bool authorizeRemoteClients { get; set; }
        public List<string> availableSmartCardReaders { get; set; }
        public bool canHaveMultipleSmartCardReaders { get; set; }
        public string externalStorageFolder { get; set; }
        public List<string> languages { get; set; }
        public string password { get; set; }
        public string smartCardReaderName { get; set; }
        public string username { get; set; }
    }

}
