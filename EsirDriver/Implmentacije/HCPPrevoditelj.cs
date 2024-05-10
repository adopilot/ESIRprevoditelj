using EsirDriver.Modeli;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using EsirDriver.Modeli.hcp;
using System.Xml;
using System.Reflection.PortableExecutable;

namespace EsirDriver.Implmentacije
{
    internal class HCPPrevoditelj : IFiskalniPrevoditelj
    {
        private readonly EsirDriverEngin _esir;
        private readonly PrevoditeljSettingModel _prevoditeljSettingModel;
        private List<HcpFooterRowModel> _footerRows = new List<HcpFooterRowModel>();
        private List<HcpClientRowModel> _clients = new List<HcpClientRowModel>();
        private readonly Encoding encoding;


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

            if (!files.Any()) { return new PorukaFiskalnogPrintera() { MozeNastaviti = true, LogLevel = LogLevel.Debug, Poruka = "Folder mi je prazan" }; }

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
                    PorukaEvent.Invoke(this, new PorukaFiskalnogPrintera() { LogLevel = LogLevel.Debug, Poruka = "Imam cmd file i obrađujem ostale datoeke ", MozeNastaviti = true });
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
            }

            //deleteCmdOkFile();

            //PorukaEvent?.Invoke(this, new PorukaFiskalnogPrintera() { IsError = false, MozeNastaviti = true, LogLevel = LogLevel.Debug, Poruka = "Ovo je resendani event" });            
            return new PorukaFiskalnogPrintera() { LogLevel = Microsoft.Extensions.Logging.LogLevel.Debug, MozeNastaviti = true, Poruka = "Odradio sam klik sata" };
        }

        private async Task<PorukaFiskalnogPrintera> OdradiCmdDatoteku(string fileFullPath)
        {
            await OdgovoriIObrisi(fileFullPath, false, "To je to");
            return new PorukaFiskalnogPrintera() { IsError = false, LogLevel = LogLevel.Debug, MozeNastaviti = true, Poruka = "Odardli smo cmd datoku" };
        }

        private async Task<PorukaFiskalnogPrintera> OdradiRCPDaoteku(string fileFullPath)
        {
            var fileName = Path.GetFileNameWithoutExtension(fileFullPath);
            string exMsg = string.Empty;
            int greskaId = 0;
            string opisGreske = "Sve je ok";
            try
            {
                if (_prevoditeljSettingModel.AutomaticallyCloseRecept)
                {

                    //Sada štampamo račun 
                }
                else
                {
                    //tempiramo račun ovo nećemo implentirati do daljenjg
                }
                //throw new Exception("RCP exception žćčđš ŽĆČĐŠ ");
                await OdgovoriIObrisi(fileFullPath, false, "Oštaman rn ");
                return new PorukaFiskalnogPrintera() { Poruka = $"Oradio sam rcp {fileName} datoku", LogLevel = LogLevel.Debug, MozeNastaviti = true };
            }
            catch (Exception ex)
            {
                exMsg = ex.Message;
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
                        string ibk = node.Attributes["IBK"].Value;
                        string name = node.Attributes["IBK"].Value;
                        string address = node.Attributes["IBK"].Value;
                        string town = node.Attributes["IBK"].Value;
                        _clients.Add(new HcpClientRowModel() { Address=address, Ibk=ibk, Name=name, Town=town });
                    }
                }
                foreach (var row in _clients)
                {
                    Console.WriteLine("Klinet: " + row.Name);
                }
                await OdgovoriIObrisi(fileFullPath, false, "");
                return new PorukaFiskalnogPrintera() { LogLevel = LogLevel.Debug, MozeNastaviti = true, Poruka = "Obradili smo datoteku" };
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

                _footerRows = new List<HcpFooterRowModel>();

                // Iterate through each DATA element
                foreach (XmlNode node in root.ChildNodes)
                {
                    if (node.Name == "DATA")
                    {
                        // Get the TEXT attribute value
                        string text = node.Attributes["TEXT"]?.Value??"";

                        bool bold = false;
                        bool.TryParse(node.Attributes["BOLD"]?.Value?? "false", out bold);
                        _footerRows.Add(new HcpFooterRowModel { Data = text, Bold = bold });
                    }
                }

                //TODO: Prebaci u trace model
                foreach (var row in _footerRows)
                {
                    Console.WriteLine("Data: " + row.Data);
                }
                await OdgovoriIObrisi(fileFullPath, false, "");
                return new PorukaFiskalnogPrintera() { LogLevel = LogLevel.Debug, MozeNastaviti = true, Poruka = "Obradili smo datoteku" };
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
                        string text = node?.Attributes["TXT"]?.Value ??"";

                        if (!string.IsNullOrEmpty(text))
                            nefiskalniTekst += $"{text}{Environment.NewLine}";

                    }
                }

                //TODO: Send nefisalni tekst prema esiru;
                Console.WriteLine("Nefiskalni tekst je: \n" + nefiskalniTekst);
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
            File.Delete(fileFullPath);
            deleteCmdOkFile();

            

            var fileNameNoExt =  Path.GetFileNameWithoutExtension(fileFullPath);

            string filePath = Path.Combine(_prevoditeljSettingModel.PathOutputFiles, $"{fileNameNoExt}.{(greskaLiJe ? "ERR" : "OK")}");


            using (StreamWriter writer = new StreamWriter(filePath, false, encoding))
            {
                await writer.WriteAsync(content??"");
            }

        }
    }
}
