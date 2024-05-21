using EsirDriver.Modeli;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EsirDriver.Modeli.hcp;
using System.Xml;
using System.Reflection.PortableExecutable;
using System.Reflection.Metadata;
using System.IO;
using System.Globalization;

namespace EsirDriver.Implmentacije
{
    internal class HCPPrevoditelj : IFiskalniPrevoditelj
    {
        private readonly EsirDriverEngin _esir;
        private readonly PrevoditeljSettingModel _prevoditeljSettingModel;
        private List<HcpFooterRowModel> _footerRows = new List<HcpFooterRowModel>();
        private List<HcpClientRowModel> _clients = new List<HcpClientRowModel>();
        private HcpClientRowModel _client;
        private readonly Encoding encoding;
        private string _lastFiscalNumber = "0";

        public event EventHandler<PorukaFiskalnogPrintera> PorukaEvent;


        internal HCPPrevoditelj(EsirDriverEngin esir, PrevoditeljSettingModel prevoditeljSettingModel)
        {
            this._esir = esir;
            this._prevoditeljSettingModel = prevoditeljSettingModel ?? new PrevoditeljSettingModel();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            try
            {
                encoding = Encoding.GetEncoding(prevoditeljSettingModel?.EncodingName ?? "UTF-8");
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


            foreach (XmlNode node in root.ChildNodes)
            {
                if (node.Name == "DATA")
                {
                    cmdTxt = node?.Attributes?["CMD"]?.Value?? "NEPOZNATA KOMANDA";
                }
            }
            
            switch (cmdTxt)
            {
                case "RECEIPT_STATE":
                    await ObradiCmdReceptState(fileFullPath);
                break;
                case "SET_CLIENT":
                    //await OdradiCmdSetClinet(fileFullPath);
                    await OdgovoriIObrisi(fileFullPath, false, $"Obrađijem CMD datokeuk sa komandom {cmdTxt}");
                    break;
                default:
                    await OdgovoriIObrisi(fileFullPath, false, $"Obrađijem CMD datokeuk sa komandom {cmdTxt}");
                break;
            }



            return new PorukaFiskalnogPrintera() { IsError = false, LogLevel = LogLevel.Debug, MozeNastaviti = true, Poruka = $"Odrali smo cmd datkeu {cmdTxt}" };
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
                            HcpArtOnRnModel hcpArtOnRnModel = new HcpArtOnRnModel();
                            hcpArtOnRnModel.Brc = brc;
                            int vat = 1;
                            var vatS = node?.Attributes?["VAT"]?.Value ?? "1";
                            int.TryParse(vatS, out vat);
                            hcpArtOnRnModel.Vat = vat;

                            int mes = 0;
                            var mesS = node?.Attributes?["MES"]?.Value ?? "1";
                            int.TryParse(mesS, out mes);
                            hcpArtOnRnModel.Mes = mes;

                            hcpArtOnRnModel.Dsc = node?.Attributes?["DSC"]?.Value ?? "Nepoznat arikal";

                            string prcS = (node?.Attributes?["PRC"]?.Value ?? "0").Replace(",", ".");
                            decimal prc = 0;

                            decimal.TryParse(prcS, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out prc);

                            hcpArtOnRnModel.Prc = prc;

                            string amnS = (node?.Attributes?["AMN"]?.Value ?? "0").Replace(",", ".");
                            decimal amn = 0;
                            decimal.TryParse(amnS, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out amn);

                            hcpArtOnRnModel.Amn = amn;

                            string dsValueS = (node?.Attributes?["DS_VALUE"]?.Value ?? "0").Replace(",", ".");
                            decimal ds = 0;
                            decimal.TryParse(dsValueS, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out ds);
                            hcpArtOnRnModel.DsValue = ds;

                            string discauntS = node?.Attributes?["DISCOUNT"]?.Value ?? "false";
                            bool discaunt = false;
                            bool.TryParse(discauntS, out discaunt);
                            hcpArtOnRnModel.Discount = discaunt;
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
                
                foreach(var red in stavke)
                {
                    Console.WriteLine($"Stavka računa na kolicina {red.Amn.ToString("#.###")} cijena {red.Prc.ToString("#.###")} naziv {red.Dsc}");
                }
                foreach (var red in payStavke)
                {
                    Console.WriteLine($"Plaćanmje iznos {red.Amn.ToString("#.###")} vrsta {red.Pay}");
                }



                if (_prevoditeljSettingModel.AutomaticallyCloseRecept)
                {

                    //Sada štampamo račun 
                }
                else
                {
                    //tempiramo račun ovo nećemo implentirati do daljenjg
                }
                File.Copy(fileFullPath, Path.Combine("C:\\HCP\\to_fp\\tring_temp", fileName),true);
                await OdgovoriIObrisi(fileFullPath, false, "Oštaman rn ");
                return new PorukaFiskalnogPrintera() { Poruka = $"Oradio sam rcp {fileName} datoku", LogLevel = LogLevel.Debug, MozeNastaviti = true };
            }
            catch (Exception ex)
            {
                
                await OdgovoriIObrisi(fileFullPath, true, $"1 - \nGreska kod stampanja raruna ex {ex.Message}");
                return new PorukaFiskalnogPrintera() { Poruka = $"Greška u RCP-u {fileName} datokoom {ex.Message}", LogLevel = LogLevel.Debug, MozeNastaviti = true };
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
                        string ibk = node?.Attributes?["IBK"]?.Value?? "0000000000000";
                        string name = node?.Attributes?["NAME"]?.Value??"Kupac";
                        string address = node?.Attributes?["ADDRESS"]?.Value??"Adrsa";
                        string town = node?.Attributes?["TOWN"]?.Value ?? "Grad";
                        _clients.Add(new HcpClientRowModel() { Address=address, Ibk=ibk, Name=name, Town=town });
                    }
                }
                foreach (var row in _clients)
                {
                    Console.WriteLine("Klinet: " + row.Name);
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
                
                foreach (XmlNode node in root.ChildNodes)
                {
                    if (node.Name == "DATA")
                    {
                        // Get the TEXT attribute value
                        string text = node?.Attributes?["TXT"]?.Value ??"";

                        if (!string.IsNullOrEmpty(text))
                            nefiskalniTekst += $"{text}{Environment.NewLine}";

                    }
                }

                //TODO: Send nefisalni tekst prema esiru;
                PorukaEvent.Invoke(this, new PorukaFiskalnogPrintera()
                {
                    LogLevel = LogLevel.Trace,
                    Poruka= "Nefiskalni tekst je: \n" + nefiskalniTekst,
                     IsError=false,
                     MozeNastaviti = true
                });
                await OdgovoriIObrisi(fileFullPath, false, "");
                return new PorukaFiskalnogPrintera() { LogLevel = LogLevel.Debug, MozeNastaviti = true, Poruka = "Obradili smo nefiskalni tekst datoku" };

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


        }
        private async Task ObradiCmdReceptState(string fileFullPath)
        {
            try
            {

                XmlDocument xmlDoc = new XmlDocument();
                XmlDeclaration xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", encoding.ToString(), "yes");
                XmlElement root = xmlDoc.CreateElement("RECEIPT_STATE");
                root.SetAttribute("AMOUNT", "-1");
                root.SetAttribute("DIFFERENCE", "-1");

                //TODO: Ovo ritam učitava nazad ostalo mislim da je nevažno pošalji ali prvo stpasi state
                root.SetAttribute("RECEIPT_NUMBER", $"{_lastFiscalNumber}");
                root.SetAttribute("REFOUND_RECEIPT_NUMBER", "0");
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
    }
}
