using System.Text;

namespace PrevoditeljKonzola
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Upalio sam se!");

            //var prevo new EsirDriver.Modeli.EsirSettingsModel() { apiKey = "adoa" }
            var esirSettings = new EsirDriver.Modeli.EsirConfigModel();
            var prevoditeljSettings = new EsirDriver.Modeli.PrevoditeljSettingModel() {
                AutomaticallyCloseRecept = true,
                KomandePrintera = EsirDriver.Modeli.PrevodimoKomandePrintera.HcpFBiH,
                ReadFolderEvryMiliSec = 500,
                Enabled = true, PathInputFiles = "C:\\HCP\\TO_FP",
                PathOutputFiles = "C:\\HCP\\FROM_FP",
                EncodingName = "windows-1250"
            };
            EsirDriver.FiskalPrevoditeljToEsir servis = new EsirDriver.FiskalPrevoditeljToEsir(esirSettings,prevoditeljSettings);

            servis.MessageReceived += Servis_MessageReceived;

            pocetak:
            Console.WriteLine($"Odaberite opicju:" +
                $"\nZa pokretanje servisa prisnite 1" +
                $"\nZa stoprianje servisa pritsnite 2" +
                $"\nZa konfiguraciju servisa prisnite 3" +
                $"\nDebug kreiraj cmd.OK HCP datokeu sitni 4" +
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
                    await servis.Konfigurisi(esirSettings, prevoditeljSettings);
                    break;

                    case "4":
                        await KreateCmdOkFile(prevoditeljSettings.PathInputFiles);
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
            Console.WriteLine($"{DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")} {(e.IsError?"Greška je nakva":"")} poruika: {e.Poruka} {e.LogLevel}");
        }
    }
}

