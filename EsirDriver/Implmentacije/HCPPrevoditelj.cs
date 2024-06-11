using EsirDriver.Modeli;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EsirDriver.Modeli.hcp;
using System.Xml;
using System.IO;
using System.Globalization;
using EsirDriver.Modeli.esir;

using System.Xml.Serialization;

namespace EsirDriver.Implmentacije
{
    internal class HCPPrevoditelj : IFiskalniPrevoditelj
    {
        private readonly EsirDriverEngin _esir;
        private readonly PrevoditeljSettingModel _prevoditeljSettingModel;
        private List<HcpFooterRowModel> _footerRows = new List<HcpFooterRowModel>();
        private List<HcpClientRowModel> _clients = new List<HcpClientRowModel>();
        private readonly Encoding encoding;
        private string _lastFiscalNumber = "0";
        private string _clientSet = string.Empty;
        private string _refundSet = string.Empty;
        public event EventHandler<PorukaFiskalnogPrintera> PorukaEvent;


        internal HCPPrevoditelj(EsirDriverEngin esir, PrevoditeljSettingModel prevoditeljSettingModel)
        {
            this._esir = esir;
            this._prevoditeljSettingModel = prevoditeljSettingModel ?? new PrevoditeljSettingModel();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            try
            {
                encoding = Encoding.GetEncoding(prevoditeljSettingModel?.EncodingName ?? "windows-1250");

                var files = Directory.GetFiles(_prevoditeljSettingModel.PathInputFiles)?.ToList() ?? new List<string>();
                foreach (var file in files)
                {
                    File.Delete(file);
                };


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                encoding = Encoding.UTF8;
            }


            

        }


        public async Task<PorukaFiskalnogPrintera> SatTik()
        {
            bool imamCmdOkFile = false;
            var files = Directory.GetFiles(_prevoditeljSettingModel.PathInputFiles)?.ToList() ?? new List<string>();

            if (!files.Any()) { return new PorukaFiskalnogPrintera() { MozeNastaviti = true, LogLevel = LogLevel.Trace, Poruka = "Folder mi je prazan" }; }

            foreach (var file in files)
            {
                if (Path.GetFileName(file.ToLower()) == "cmd.ok")
                {

                    imamCmdOkFile = true;
                    if (files.Count == 1)
                    {
                        deleteCmdOkFile();
                        return new PorukaFiskalnogPrintera() { LogLevel = LogLevel.Debug, Poruka = "U folderu sam našao samo cmd.ok i brišem je ", MozeNastaviti = true };
                    }
                    PorukaEvent.Invoke(this, new PorukaFiskalnogPrintera() { LogLevel = LogLevel.Trace, Poruka = "Imam cmd file i obrađujem ostale datoeke ", MozeNastaviti = true });
                }

            }
            if (!imamCmdOkFile)
                return new PorukaFiskalnogPrintera() { Poruka = $"Čekamo cmd.ok datoeku da nasvimo obrađivati {files.Count} komandi ", LogLevel = LogLevel.Debug, MozeNastaviti = true };


            //Ako imamo cmd.ok komanud ond obrađujemo datoteke
            foreach (var fileFullPath in files)
            {
                var file = Path.GetFileName(fileFullPath)?.ToLower() ?? "sex.sex";
                if (file == "cmd.ok")
                    continue;

                else if (Path.GetExtension(file) != ".xml")
                {
                    File.Delete($"{file}");
                    PorukaEvent.Invoke(this, new PorukaFiskalnogPrintera() { LogLevel = LogLevel.Warning, MozeNastaviti = true, Poruka = $"Daotkea {file} nije podržana sistemom, samo čitamo xml datoeke" });
                    continue;
                }
                else if (file.StartsWith("rcp"))
                {
                    var rcpOdgovor = await OdradiRCPDaoteku(fileFullPath);
                    PorukaEvent.Invoke(this, rcpOdgovor);
                }
                else if (file == "footer.xml")
                {
                    var footerOdgovor = await OdradiFooterDatoteku(fileFullPath);
                    PorukaEvent.Invoke(this, footerOdgovor);
                }
                else if (file.StartsWith("txt"))
                {
                    var txtOdgovor = await OdradiTxtDatoteku(fileFullPath);
                    PorukaEvent.Invoke(this, txtOdgovor);
                }
                else if (file.StartsWith("cmd"))
                {
                    var cmdOdgovor = await OdradiCmdDatoteku(fileFullPath);
                    PorukaEvent.Invoke(this, cmdOdgovor);
                }
                else  if (file == "clients.xml")
                {
                    var clinetOdgovor = await OdradiClientsDatoteku(fileFullPath);
                    PorukaEvent.Invoke(this, clinetOdgovor);
                }
                else if (file.StartsWith("plu"))
                {
                    await OdgovoriIObrisi(fileFullPath, true, $"Plu komande nisu implenetriane");
                    return new PorukaFiskalnogPrintera() { IsError = true , MozeNastaviti= true, LogLevel= LogLevel.Error, Poruka=$"PLU komande nisu podržane prevoiteljem u ovoj ! Možda da se obratite na https://github.com/adopilot/ESIRprevoditelj" };
                }
                else
                {
                    await OdgovoriIObrisi(fileFullPath, true, $"Datoka {file} nije podržana prevoiteljem ");
                    return new PorukaFiskalnogPrintera() { IsError = true, MozeNastaviti = true, LogLevel = LogLevel.Error, Poruka = $"Prevoidtelj ne podržava datku {file}" };
                }
            }

            //deleteCmdOkFile();

            //PorukaEvent?.Invoke(this, new PorukaFiskalnogPrintera() { IsError = false, MozeNastaviti = true, LogLevel = LogLevel.Debug, Poruka = "Ovo je resendani event" });            
            return new PorukaFiskalnogPrintera() { LogLevel = LogLevel.Trace, MozeNastaviti = true, Poruka = "Odradio sam klik sata" };
        }

        private async Task<PorukaFiskalnogPrintera> OdradiCmdDatoteku(string fileFullPath)
        {
            string xmlContent;
            using (StreamReader sr = new StreamReader(fileFullPath, encoding))
            {
                xmlContent = await sr.ReadToEndAsync();
            }

            // Parse the XML content
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlContent);

            // Get the root element
            XmlElement root = doc.DocumentElement;


            string cmdTxt = string.Empty;
            string num = string.Empty;
            string value = string.Empty;

            foreach (XmlNode node in root.ChildNodes)
            {
                if (node.Name == "DATA")
                {
                    cmdTxt = node?.Attributes?["CMD"]?.Value?? "NEPOZNATA KOMANDA";
                    num = node?.Attributes?["NUM"]?.Value ?? string.Empty;
                    value = node?.Attributes?["VALUE"]?.Value?? string.Empty;
                }
            }
            
            switch (cmdTxt)
            {
                //Za ovo čekamo potvrdu upraljvanja novcem
                case "CASH_IN":
                case "CASH_OUT":
                    var status = await _esir.StatusFiskalnogPrintera();
                    await OdgovoriIObrisi(fileFullPath, !(status?.MozeNastaviti ?? false), status?.Poruka ?? "Nepoznata greška sa ESIR-om");
                    break;
                case "RECEIPT_STATE":
                    await ObradiCmdReceptState(fileFullPath);
                break;
                case "SET_CLIENT":
                    _clientSet = num;
                    await OdgovoriIObrisi(fileFullPath, false, $"Postavio sam klijenta za slijedći račun {num}");
                    break;
                case "REFUND_ON":
                    _refundSet = num;
                    await OdgovoriIObrisi(fileFullPath, false, $"Slijedeći račun je stono po broju {num}");
                    break;
                default:
                    await OdgovoriIObrisi(fileFullPath, false, $"Komanda {cmdTxt} nije implemntirana");
                break;
            }



            return new PorukaFiskalnogPrintera() { IsError = false, LogLevel = LogLevel.Debug, MozeNastaviti = true, Poruka = $"Odrali smo cmd datkeu {cmdTxt} {num}" };
        }

        public static  string ExtractBeforeSemicolon(string input)
        {
            if (input == null)
            {
                return null;
            }

            

            // Find the index of the semicolon
            int semicolonIndex = input.IndexOf(';');

            if (semicolonIndex <= 0)
            {
                return input;
            }
            

            // Extract the substring between the colon and the semicolon
            string result = input.Substring(0, semicolonIndex ).Trim();

           
           

            return result;
        }
        public  string ExtractAfterColon(string input)
        {
            if (input == null)
            {
                return null;
            }

            // Find the index of the colon
            int colonIndex = input.IndexOf(':');

            if (colonIndex <= 0)
            {
                return null;
            }

            // Extract the substring after the colon
            string result = input.Substring(colonIndex + 1).Trim();

            // Remove newline characters
            

            return result;
        }

       

        private async Task<PorukaFiskalnogPrintera> OdradiRCPDaoteku(string fileFullPath)
        {
            var fileName = Path.GetFileNameWithoutExtension(fileFullPath);

            try
            {
                string xmlContent;
                using (StreamReader sr = new StreamReader(fileFullPath, encoding))
                {
                    xmlContent = await sr.ReadToEndAsync();
                }

                // Parse the XML content
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xmlContent);

                // Get the root element
                XmlElement root = doc.DocumentElement;

                List<HcpArtOnRnModel> stavke = new List<HcpArtOnRnModel>();
                List<HcpPayOnRnModel> payStavke = new List<HcpPayOnRnModel>();

                InvoiceModel invoice = new InvoiceModel();
                InvoiceRequest invoiceRequest = new InvoiceRequest();






                NumberStyles numberStyles = NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign | NumberStyles.AllowTrailingSign | NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite ;

                int rbr = 0;
                List<int> arikliBezCijene = new List<int>();

                // Iterate through each DATA element
                foreach (XmlNode node in root.ChildNodes)
                {
                    if (node.Name == "DATA")
                    {

                        // Get the TEXT attribute value
                        string brc = node?.Attributes?["BCR"]?.Value ?? "";
                        string payS = node?.Attributes?["PAY"]?.Value ?? "";

                        //Ako imamo BRC atribut onda je to arikal red
                        if (!string.IsNullOrEmpty(brc))
                        {
                            rbr++;
                            HcpArtOnRnModel hcpArtOnRnModel = new HcpArtOnRnModel();
                            hcpArtOnRnModel.Brc = brc;
                            int vat = 1;
                            var vatS = node?.Attributes?["VAT"]?.Value ?? "0";
                            int.TryParse(vatS, out vat);
                            hcpArtOnRnModel.Vat = vat;

                            int mes = 0;
                            var mesS = node?.Attributes?["MES"]?.Value ?? "1";
                            int.TryParse(mesS, out mes);
                            hcpArtOnRnModel.Mes = mes;

                            hcpArtOnRnModel.Dsc = node?.Attributes?["DSC"]?.Value ?? "Nepoznat arikal";

                            string prcS = (node?.Attributes?["PRC"]?.Value ?? "0").Replace(",", ".");
                            decimal prc = 0;

                            decimal.TryParse(prcS, numberStyles, CultureInfo.InvariantCulture, out prc);

                            hcpArtOnRnModel.Prc = prc;

                            string amnS = (node?.Attributes?["AMN"]?.Value ?? "0").Replace(",", ".");
                            decimal amn = 0;
                            decimal.TryParse(amnS, numberStyles, CultureInfo.InvariantCulture, out amn);

                            hcpArtOnRnModel.Amn = amn;

                            string dsValueS = (node?.Attributes?["DS_VALUE"]?.Value ?? "0").Replace(",", ".");
                            decimal ds = 0;
                            decimal.TryParse(dsValueS, numberStyles, CultureInfo.InvariantCulture, out ds);
                            hcpArtOnRnModel.DsValue = ds;

                            string discauntS = node?.Attributes?["DISCOUNT"]?.Value ?? "false";
                            bool discaunt = false;
                            bool.TryParse(discauntS, out discaunt);
                            hcpArtOnRnModel.Discount = discaunt;

                            if (hcpArtOnRnModel.Amn == 0 && hcpArtOnRnModel.Prc == 0)
                            {
                                continue;
                            }
                            else if (hcpArtOnRnModel.Brc != "0" && hcpArtOnRnModel.Amn != 0 && hcpArtOnRnModel.Prc == 0)
                            {
                                arikliBezCijene.Add(rbr);
                            }
                            if (hcpArtOnRnModel.Amn < 0)
                            {
                                hcpArtOnRnModel.Amn = -1 * hcpArtOnRnModel.Amn;

                                invoiceRequest.transactionType = TranscationType.Refund;
                                invoiceRequest.referentDocumentDT = DateTime.Today;
                                invoiceRequest.referentDocumentNumber = $"0";
                            }

                            stavke.Add(hcpArtOnRnModel);
                        }
                        //Ako imamo PAY atribut onda je to py red
                        else if (!string.IsNullOrEmpty(payS))
                        {
                            int pay = 0;
                            int.TryParse(payS, out pay);

                            string amnS = (node?.Attributes?["AMN"]?.Value ?? "0").Replace(",", ".");
                            decimal amn = 0;
                            decimal.TryParse(amnS, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out amn);
                            HcpPayOnRnModel payRed = new HcpPayOnRnModel() { Amn= amn, Pay= pay };
                            payStavke.Add(payRed);
                        }
                    }

                        
                }

                //Ako smo u stavkama našli arikle bez 
                if (arikliBezCijene.Any())
                {
                    await OdgovoriIObrisi(fileFullPath, true, $"1 - \nStavke računa  {string.Join(',',arikliBezCijene)} neamj formiranu cijenu !   ");
                    return new PorukaFiskalnogPrintera() { IsError=true, Poruka = $"Stavke računa  {string.Join(',', arikliBezCijene)} nemaju formiranu cijenu !", LogLevel = LogLevel.Error, MozeNastaviti = true };
                }
                if (!stavke.Any())
                {
                    _refundSet = string.Empty;
                    _clientSet = string.Empty;
                    _footerRows = new List<HcpFooterRowModel>();
                    await OdgovoriIObrisi(fileFullPath, false, "Nemamo stavki samo payment onda je to close recept komanda koju ovdije namamo");
                    return new PorukaFiskalnogPrintera() { IsError = false, LogLevel = LogLevel.Warning, MozeNastaviti = true, Poruka = "Nemamo stavki samo payment onda je to close recept komanda koju ovdije namamo" };
                }

                string kasa =null, tip = null, broj = null, rn = null, rj = null;

                foreach (var red in _footerRows)
                {
                    if ((red?.Data??"").StartsWith("Blag:"))
                    {
                        var blagajnik = ExtractAfterColon(red?.Data??"");
                        if (!string.IsNullOrEmpty(blagajnik))
                            invoiceRequest.cashier = blagajnik;

                        continue;
                    }
                    else if ((red?.Data ?? "").StartsWith("Kasa:"))
                    {
                        kasa = ExtractAfterColon(red.Data);
                        
                    }
                    else if ((red?.Data ?? "").StartsWith("Rn:"))
                    {
                        rn = ExtractAfterColon(red.Data);
                    }
                    else if ((red?.Data ?? "").StartsWith("Br.dok:"))
                    {
                        broj = ExtractAfterColon(red.Data);

                    }
                    else if ((red?.Data ?? "").StartsWith("Broj:"))
                    {
                        broj = ExtractAfterColon(red.Data);
                        
                    }
                    else if ((red?.Data ?? "").StartsWith("Tip:"))
                    {
                        tip = ExtractAfterColon(red.Data);

                    }
                    else if ((red?.Data ?? "").StartsWith("RJ:"))
                    {
                        var arj = ExtractAfterColon(red.Data);
                        rj = ExtractBeforeSemicolon(arj);


                    }
                    invoice.receiptFooterTextLines.Add(red?.Data ?? "");
                    
                }
                string requestId = $"{(string.IsNullOrEmpty(rn) ? DateTime.Today.Year.ToString() + "X" : "")}{rj ?? _prevoditeljSettingModel.DefSklSifra}X{tip ?? kasa}X{((rn ?? broj) ?? "").Replace("-", "A")}";


                foreach (var red in stavke)
                {
                    //Ovo može a i ne mora kod nas nema smisla jer je ovo id kojeg niko neće vidjeti:
                    //gtin = (red?.Brc??"0").PadLeft(13,'0'),

                    invoiceRequest.items.Add(new ItemModel() { discount = 0, discountAmount = 0,  labels = new List<string>() { _prevoditeljSettingModel.PodrazumjevanaPoreskaStopa }, name=red.Dsc, quantity=red.Amn, unitPrice=red.Prc , totalAmount= red.Prc*red.Amn } );
                }
                foreach (var red in payStavke)
                {
                    //TODO: Maprianje hcp vrsta plaćanja sa njiv

                    PaymentTypes payTyp = PaymentTypes.Cash;

                    try
                    {
                        payTyp = (PaymentTypes)red.Pay;
                    }
                    catch (Exception ex) {
                        PorukaEvent.Invoke(this, new PorukaFiskalnogPrintera() { IsError = false, LogLevel = LogLevel.Warning, MozeNastaviti = true, Poruka = $"Pay type {red.Pay} se nije uspijeo castati u PaymetType esira {ex.Message}" }); 
                    }


                    invoiceRequest.payment.Add(new PaymentModel() { amount=red.Amn, paymentType= payTyp });
                }



                if (!string.IsNullOrEmpty(_refundSet))
                {
                    var uid = _esir.GetEsirUid();
                    invoiceRequest.transactionType = TranscationType.Refund;
                    invoiceRequest.referentDocumentDT = DateTime.Today;
                    invoiceRequest.referentDocumentNumber = $"{(string)_refundSet.Clone()}";
                    _refundSet = string.Empty;
                }

                if (!string.IsNullOrEmpty(_clientSet))
                {
                    invoiceRequest.buyerId = (((string)_clientSet.Clone())??"0").PadLeft(13,'0');
                    var client = _clients.Where(x => x.Ibk == _clientSet).FirstOrDefault();
                    if (client != null) {
                        invoice.receiptFooterTextLines.Add($"Kupac: {client.Name}");
                        invoice.receiptFooterTextLines.Add($"Kupac JIB: {client.Ibk}");
                        invoice.receiptFooterTextLines.Add($"Kupac adresa: {client.Address}");
                    }

                    _clientSet = string.Empty;
                }

                invoice.invoiceRequest = invoiceRequest;


                var ado = await _esir.OstampajRacun(invoice, requestId);

                

                /* logika zatvaranja računa ako budemo radili za nekoga 3ćeg
                if (_prevoditeljSettingModel.AutomaticallyCloseRecept)
                {
                 
                    //Sada štampamo račun 
                }
                else
                {
                    //tempiramo račun ovo nećemo implentirati do daljenjg
                }
                */
                //File.Copy(fileFullPath, Path.Combine("C:\\HCP\\to_fp\\tring_temp", fileName),true);
                await OdgovoriIObrisi(fileFullPath, false, "Oštaman rn ");
                return new PorukaFiskalnogPrintera() { Poruka = $"Oradio sam rcp {fileName} datoku", LogLevel = LogLevel.Debug, MozeNastaviti = true };
            }
            catch (Exception ex)
            {
                
                await OdgovoriIObrisi(fileFullPath, true, $"1 - \nGreska kod stampanja raruna ex {ex.Message}");
                return new PorukaFiskalnogPrintera() { Poruka = $"Greška u RCP-u {fileName} datotekom {ex.Message}", LogLevel = LogLevel.Error, MozeNastaviti = true };
            }


            
          

        }

        private void deleteCmdOkFile()
        {
            File.Delete(Path.Combine(_prevoditeljSettingModel.PathInputFiles, "cmd.ok"));
        }
     
        private async Task<PorukaFiskalnogPrintera> OdradiClientsDatoteku(string fileFullPath)
        {
            try
            {
                // Read the XML file asynchronously


                string xmlContent;
                using (StreamReader sr = new StreamReader(fileFullPath, encoding))
                {
                    xmlContent = await sr.ReadToEndAsync();
                }

                // Parse the XML content
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xmlContent);

                // Get the root element
                XmlElement root = doc.DocumentElement;
                
                foreach (XmlNode node in root.ChildNodes)
                {
                    if (node.Name == "DATA")
                    {
                        // Get the TEXT attribute value
                        string ibk = node?.Attributes?["IBK"]?.Value?? "";
                        string name = node?.Attributes?["NAME"]?.Value??"";
                        string address = node?.Attributes?["ADDRESS"]?.Value??"";
                        string town = node?.Attributes?["TOWN"]?.Value ?? "";
                        _clients.Add(new HcpClientRowModel() { Address=address, Ibk=ibk, Name=name, Town=town });
                    }
                }
                await OdgovoriIObrisi(fileFullPath, false, "");
                return new PorukaFiskalnogPrintera() { LogLevel = LogLevel.Debug, MozeNastaviti = true, Poruka = "Obradili smo clinets.xml datoteku" };
            }
            catch (Exception ex)
            {
                await OdgovoriIObrisi(fileFullPath, true, $"103-{ex.Message}");
                return new PorukaFiskalnogPrintera() { LogLevel = LogLevel.Error, MozeNastaviti = true, Poruka = $"Greška u čitanju clijent.xml komande {ex.Message}", IsError = true };
            }
        }

        private async Task<PorukaFiskalnogPrintera> OdradiFooterDatoteku(string fileFullPath)
        {
            try
            {
                
                _footerRows = new List<HcpFooterRowModel>();

                string xmlContent;
                using (StreamReader sr = new StreamReader(fileFullPath, encoding))
                {
                    xmlContent = await sr.ReadToEndAsync();
                }

                // Parse the XML content
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xmlContent);

                // Get the root element
                XmlElement root = doc.DocumentElement;

               

                // Iterate through each DATA element
                foreach (XmlNode node in root.ChildNodes)
                {
                    if (node.Name == "DATA")
                    {
                        // Get the TEXT attribute value
                        string text = node?.Attributes?["TEXT"]?.Value??"";

                        bool bold = false;
                        bool.TryParse(node?.Attributes?["BOLD"]?.Value ?? "false", out bold);
                        _footerRows.Add(new HcpFooterRowModel { Data = text, Bold = bold });
                    }
                }

                //TODO: Prebaci u trace model
                foreach (var row in _footerRows)
                {
                    Console.WriteLine("Imam Footer data: " + row.Data);
                }
                await OdgovoriIObrisi(fileFullPath, false, "");
                return new PorukaFiskalnogPrintera() { LogLevel = LogLevel.Debug, MozeNastaviti = true, Poruka = $"Učitali smo footer datoku sa {_footerRows.Count} redova" };
            }
            catch (Exception ex)
            {
                await OdgovoriIObrisi(fileFullPath,true,$"103-{ex.Message}");
                return new PorukaFiskalnogPrintera() { LogLevel = LogLevel.Error, MozeNastaviti = true, Poruka = $"Greška u čitanju footer komande {ex.Message}", IsError = true };
            }
        }
        private async Task<PorukaFiskalnogPrintera> OdradiTxtDatoteku(string fileFullPath)
        {
            try
            {
                string xmlContent;
                using (StreamReader sr = new StreamReader(fileFullPath, encoding))
                {
                    xmlContent = await sr.ReadToEndAsync();
                }
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xmlContent);

                if (doc==null  ||doc.DocumentElement ==null)
                    return new PorukaFiskalnogPrintera() { IsError=false, LogLevel = LogLevel.Warning, MozeNastaviti=true, Poruka="Nisam učitao txt comandu HCP doc mi je null" };

                
                XmlElement root = doc.DocumentElement;


                string nefiskalniTekst = string.Empty;

                EsirDirektnaStampaModel esirDirektnaStampaModel = new EsirDirektnaStampaModel();

                foreach (XmlNode node in root.ChildNodes)
                {
                    if (node.Name == "DATA")
                    {
                        // Get the TEXT attribute value
                        string text = node?.Attributes?["TXT"]?.Value ??"";

                        if (!string.IsNullOrEmpty(text))
                            esirDirektnaStampaModel.textLines.Add(text);

                    }
                }

                //TODO: Send nefisalni tekst prema esiru;
                PorukaEvent.Invoke(this, new PorukaFiskalnogPrintera()
                {
                    LogLevel = LogLevel.Debug,
                    Poruka= "Nefiskalni tekst je: \n" + nefiskalniTekst,
                     IsError=false,
                     MozeNastaviti = true
                });

                var reso = await _esir.DirektnaSampa(esirDirektnaStampaModel);

                await OdgovoriIObrisi(fileFullPath, reso?.IsError??true, reso?.Poruka??"103-Nepoznata porkua kod štampe");
                return reso;

            }
            catch (Exception ex)
            {
                await OdgovoriIObrisi(fileFullPath, true, $"103-Greška kod štampanja slbodnog texta na fp: {ex.Message}");
                return new PorukaFiskalnogPrintera() { LogLevel = LogLevel.Error, MozeNastaviti = true, Poruka = $"Greška u čitanju footer komande {ex.Message}", IsError = true };
            }
        }
        private async Task  OdgovoriIObrisi(string fileFullPath, bool greskaLiJe, string content)
        {
            
            

            var fileNameNoExt =  Path.GetFileNameWithoutExtension(fileFullPath);

            string filePath = Path.Combine(_prevoditeljSettingModel.PathOutputFiles, $"{fileNameNoExt}.{(greskaLiJe ? "ERR" : "OK")}");


            using (StreamWriter writer = new StreamWriter(filePath, false, encoding))
            {
                await writer.WriteAsync(content??"");
            }
            File.Delete(fileFullPath);
            deleteCmdOkFile();
            PorukaEvent.Invoke(this, new PorukaFiskalnogPrintera() { IsError = greskaLiJe, LogLevel = greskaLiJe ? LogLevel.Error : LogLevel.Debug, MozeNastaviti = true, Poruka = content??"Poruka" });


        }

        private async Task ObradiCmdReceptState(string fileFullPath)
        {
            try
            {
                var receptState = await _esir.LastInvoice(ReceiptLayoutType.Slip, ReceptImageFormat.Png, true);
   
                if (receptState==null)
                {
                    await OdgovoriIObrisi(fileFullPath, true, $"Esir mi nije dao recept state pa ni ja Vama ne mogu isit datu");
                }
                var brRacStat = receptState?.invoiceNumber ?? "0";
               var brRac = brRacStat.Length > 10 ? brRacStat.Substring(brRacStat.Length - 10) : brRacStat;

      


                // Create an instance of ReceiptState and populate it with data
                var receiptState = new HcpReceptStateModel
                {
                    Amount = (receptState?.totalAmount??0),
                    Difference = 0,
                    ReceiptNumber =  brRac,
                    RefoundReceiptNumber = brRac,
                    ReceiptToRefund = 0,
                    NumPay = 6,
                    NumPlu = 0,
                    Client = 0,
                    Cashier = 255,
                    FiscalDayStarted = false,
                    FiscalReceiptStarted = false,
                    RefoundMode = false,
                    Pays = new List<Pay>
            {
                new Pay { Amount =  receptState?.totalAmount??0 },
                new Pay { Amount = 0 },
                new Pay { Amount = 0 },
                new Pay { Amount = 0 },
                new Pay { Amount = 0 },
                new Pay { Amount = 0 }
            }
                };

                // Serialize the object to XML and write it to a file

                var filePath = Path.Combine(_prevoditeljSettingModel.PathOutputFiles, "bill_state.xml");

                if (File.Exists(filePath))
                    File.Delete(filePath);

                await using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
                {
                    await WriteXmlToStreamAsyncBillState(receiptState, fileStream);
                }

                await OdgovoriIObrisi(fileFullPath, false, "");
            }

            catch (Exception ex)
            {
                PorukaEvent.Invoke(this, new PorukaFiskalnogPrintera() { IsError = true, LogLevel = LogLevel.Error, MozeNastaviti = true, Poruka = $"Greška u odgovru recept state komande ex: {ex.Message}" });
                await OdgovoriIObrisi(fileFullPath, true, $"Greška u generisanju  odgovra recept_state {ex.Message} ");
            }

        }
                private async Task ObradiCmdReceptStateOldNeKorsite(string fileFullPath)
        {
            try
            {




                XmlDocument xmlDoc = new XmlDocument();
                XmlDeclaration xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", encoding.ToString(), "yes");
                XmlElement root = xmlDoc.CreateElement("RECEIPT_STATE");
                root.SetAttribute("AMOUNT", "-1");
                root.SetAttribute("DIFFERENCE", "-1");

                //TODO: Ovo ritam učitava nazad ostalo mislim da je nevažno pošalji ali prvo stpasi state
                root.SetAttribute("RECEIPT_NUMBER", $"ADMIR H 1");
                root.SetAttribute("REFOUND_RECEIPT_NUMBER", "ADMIR S 2");
                root.SetAttribute("RECEIPT_TO_REFUND", "0");
                root.SetAttribute("NUM_PAY", "6");
                root.SetAttribute("NUM_PLU", "0");
                root.SetAttribute("CLIENT", "0");
                root.SetAttribute("CASHIER", "255");
                root.SetAttribute("FISCAL_DAY_STARTED", "false");
                root.SetAttribute("FISCAL_RECEIPT_STARTED", "false");
                root.SetAttribute("REFOUND_MODE", "false");

                for (int i = 0; i < 6; i++)
                {
                    XmlElement payElement = xmlDoc.CreateElement("PAY");
                    payElement.SetAttribute("AMOUNT", "0");
                    root.AppendChild(payElement);
                }

                xmlDoc.AppendChild(root);

                var filePath = Path.Combine(_prevoditeljSettingModel.PathOutputFiles, "bill_state.xml");

                if (File.Exists(filePath))
                    File.Delete(filePath);

                using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
                {
                    await WriteXmlToStreamAsync(xmlDoc, fileStream);
                }

                await OdgovoriIObrisi(fileFullPath, false, "");
            }
            catch (Exception ex)
            {
                PorukaEvent.Invoke(this, new PorukaFiskalnogPrintera() { IsError = true , LogLevel= LogLevel.Error, MozeNastaviti=true, Poruka=$"Greška u odgovru recept state komande ex: {ex.Message}"});
                await OdgovoriIObrisi(fileFullPath, true, "Greška u generisanju  odgovra recept_state ");
            }

        }
        async Task WriteXmlToStreamAsync(XmlDocument xmlDoc, Stream stream)
        {
            using (XmlWriter writer = XmlWriter.Create(stream, new XmlWriterSettings { Async = true, Encoding = encoding }))
            {
                xmlDoc.WriteTo(writer);
                await writer.FlushAsync();
            }
        }

        private static async Task WriteXmlToStreamAsyncBillState(HcpReceptStateModel receiptState, Stream stream)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(HcpReceptStateModel));
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Async = true,
                Indent = true,
                Encoding = Encoding.UTF8
            };

            using (XmlWriter writer = XmlWriter.Create(stream, settings))
            {
                serializer.Serialize(writer, receiptState);
                await writer.FlushAsync();
            }
        }
    }
}
