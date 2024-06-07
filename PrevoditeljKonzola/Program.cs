using EsirDriver.Modeli;
using System.Runtime.CompilerServices;
using System.Text;

namespace PrevoditeljKonzola
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Upalio sam se!");


            //var prevo new EsirDriver.Modeli.EsirSettingsModel() { apiKey = "adoa" }
            var esirSettings = new EsirDriver.Modeli.EsirConfigModel() 
            {
                apiKey = "5ead197a9fdf6600c82aff1a803a1625", 
                pin = 2011, 
                webserverAddress = "http://172.16.0.9:3566", 
                authorizeLocalClients = true, authorizeRemoteClients = true, 
                OperationMode = EsirDriver.Modeli.esir.InvoiceType.Normal, 
                TimeoutInSec = 20 };
            var prevoditeljSettings = new EsirDriver.Modeli.PrevoditeljSettingModel() {
                AutomaticallyCloseRecept = true,
                KomandePrintera = EsirDriver.Modeli.PrevodimoKomandePrintera.HcpFBiH,
                ReadFolderEvryMiliSec = 500,
                Enabled = true, 
                PathInputFiles = "C:\\HCP\\TO_FP",
                PathOutputFiles = "C:\\HCP\\FROM_FP",
                EncodingName = "windows-1250",
                 
            };
            EsirDriver.FiskalPrevoditeljToEsir servis = new EsirDriver.FiskalPrevoditeljToEsir(esirSettings,prevoditeljSettings);

            servis.MessageReceived += Servis_MessageReceived;

            pocetak:
            Console.WriteLine($"Odaberite opicju:" +
                $"\nZa pokretanje servisa prisnite 1" +
                $"\nZa stoprianje servisa pritsnite 2" +
                $"\nZa konfiguraciju servisa prisnite 3" +
                $"\nDebug kreiraj cmd.OK HCP datokeu sitni 4" +
                $"\nZa unos IBFU-a za stono kliknite 5" +
                $"\nZadnji račun request 6" +
                $"\nZa izlaz iz aplikcije prisnite 0");

            string opcija = Console.ReadLine()??"";

            switch (opcija)
            {
                case "1":
                    servis.Start();
                break;
                case "2":
                    servis.Stop(); 
                break;
                    case "3":
                    var poruka = await servis.Konfigurisi(esirSettings, prevoditeljSettings);
                    //Servis_MessageReceived(null, poruka);
                    break;

                    case "4":
                        await KreateCmdOkFile(prevoditeljSettings.PathInputFiles);
                    break;
                    case "5":
                    Console.WriteLine("Upište IBFU koji će se koristi kod stono (8 karaktrera dug)");
                    var ibfu = Console.ReadLine();
                    if ((ibfu ?? "").Trim().Length != 8)
                        Console.WriteLine("Ibfu mora sadžavati 8 karaktera");

                    
                    else
                        prevoditeljSettings.IbfmZaStorno = ibfu??"";
                        await servis.Konfigurisi(esirSettings,prevoditeljSettings);
                        Console.WriteLine($"Kod slijedeć stona IBFU će biti setovan na {ibfu}");
                        
                    break;
                case "6":
                    var response =await servis._esir.LastInvoice(EsirDriver.Modeli.esir.ReceiptLayoutType.Invoice, EsirDriver.Modeli.esir.ReceptImageFormat.Pdf, true);
                    Console.WriteLine($"Resšpmese {(response?.invoiceNumber??"nema broja fakture")}");
                    break;
                case "0":
                    servis.Stop();
                    Environment.Exit(0);
                    break;
                default:
                    goto pocetak;
            }
            goto pocetak;
            /*
            await Task.Delay(10000);
            servis.Start();
            Console.WriteLine("Upalio servis ponov");
            await Task.Delay(10000);
            servis.Stop();
            servis.Stop();
            Console.WriteLine("Ugsiao server");


            await Task.Delay(10000);
            servis.Start();
            Console.WriteLine("Upalio servis treci put");

            Console.WriteLine("Sisni bilo koje dugme da zavšima");
            */
            
        
        }
        private static async Task KreateCmdOkFile(string inputFolder)
        {
            string filePath = Path.Combine(inputFolder, "cmd.ok");
            using (StreamWriter writer = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                await writer.WriteAsync("");
            }
        }
        private static void Servis_MessageReceived(object? sender, EsirDriver.Modeli.PorukaFiskalnogPrintera e)
        {
            if (e.LogLevel >= LogLevel.Information)
            {
                Console.WriteLine($"{DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")} Lgl: {e.LogLevel} {(e.IsError ? "err!" : "")} Msg: \"{e.Poruka}\" {(e.MozeNastaviti?"; može nastaviti":"; ne može nastaviti")       } ");
            }
        }

    }
}

