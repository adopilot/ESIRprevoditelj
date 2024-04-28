namespace PrevoditeljKonzola
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Upalio sam se!");

            //var prevo new EsirDriver.Modeli.EsirSettingsModel() { apiKey = "adoa" }
            var esirSettings = new EsirDriver.Modeli.EsirConfigModel();
            var prevoditeljSettings = new EsirDriver.Modeli.PrevoditeljSettingModel() { ReadFolderEvryMiliSec=3000, Enabled = true,PathInputFiles="C:\\HCP\\TO_FP" , PathOutputFiles = "C:\\HCP\\FROM_FP" };
            EsirDriver.FiskalPrevoditeljToEsir servis = new EsirDriver.FiskalPrevoditeljToEsir(esirSettings,prevoditeljSettings);

            servis.MessageReceived += Servis_MessageReceived;

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
            Console.ReadKey();
        }

        private static void Servis_MessageReceived(object? sender, EsirDriver.Modeli.PorukaFiskalnogPrintera e)
        {
            Console.WriteLine($"{DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")} {(e.IsError?"Greška je nakva":"")} poruika: {e.Poruka} {e.LogLevel}");
        }
    }
}
