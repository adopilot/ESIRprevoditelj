using EsirDriver.Modeli;
using EsirDriver.Modeli.esir;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EsirDriver
{
    public class EsirDriverEngin
    {
        private EsirConfigModel _esirConfig;
        HttpClient _httpClient;
        public EsirDriverEngin(EsirConfigModel esirConfigModel)
        {
            _esirConfig = esirConfigModel;
            _httpClient = new HttpClient() { BaseAddress = new Uri(esirConfigModel?.webserverAddress ?? "http://127.0.0.1:3566/") };
        }

        public async Task<PorukaFiskalnogPrintera> Konfigurisi(EsirConfigModel esirConfigModel)
        {
            this._esirConfig = esirConfigModel;

            _httpClient = new HttpClient() { BaseAddress = new Uri(esirConfigModel?.webserverAddress ?? "http://127.0.0.1:3566/") };
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_esirConfig.apiKey}");


            return new PorukaFiskalnogPrintera() { LogLevel= LogLevel.Information, MozeNastaviti= true, Poruka="Sve je ok" };

            var dostupan = await ProvjeriDostupan();

            //if (dostupan.IsError)
               // return dostupan;

            var statusFp1 = await StatusFiskalnogPrintera();

            if (statusFp1.Poruka== "ESIR nema isrpavan sigurnosti elemnt")
            {
                
            }
            else if (statusFp1.Poruka== "ESIR nema ispravan pin")
            {

            }
         

            return statusFp1;



        }

        internal EsirStatusCodeModel GetStatusFromCode(int code)
        {
            switch (code)
            {
                case 0:
                    return new EsirStatusCodeModel() { Code = code, Info = "Sve OK", Opis = "Komanda je izvršena bez upozorenja ili grešaka", LogLevel = Microsoft.Extensions.Logging.LogLevel.Information };
                case 100:
                    return new EsirStatusCodeModel() { Code = code, Info = "PIN je OK", Opis = "Ovaj kod ukazuje da je pruženi PIN kod ispravan", LogLevel = Microsoft.Extensions.Logging.LogLevel.Information };
                case 210:
                    return new EsirStatusCodeModel() { Code = code, Info = "Internet dostupan", Opis = "Internet veza je dostupna (opcionalno)", LogLevel = Microsoft.Extensions.Logging.LogLevel.Information };
                case 220:
                    return new EsirStatusCodeModel() { Code = code, Info = "Internet nedostupan", Opis = "Internet veza nije dostupna (opcionalno)", LogLevel = Microsoft.Extensions.Logging.LogLevel.Information };
                case 1100:
                    return new EsirStatusCodeModel() { Code = code, Info = "Skladište 90% popunjeno", Opis = "Skladište koje se koristi za pohranu paketa za reviziju je 90% popunjeno. Vrijeme je za izvršenje revizije.", LogLevel = Microsoft.Extensions.Logging.LogLevel.Warning };
                case 1300:
                    return new EsirStatusCodeModel() { Code = code, Info = "Smart kartica nije prisutna", Opis = "Sigurnosna kartica nije umetnuta u čitač pametnih kartica E-SDC", LogLevel = Microsoft.Extensions.Logging.LogLevel.Warning };
                case 1400:
                    return new EsirStatusCodeModel() { Code = code, Info = "Potrebna je revizija", Opis = "Ukupni iznos prodaje i povrata dostigao je 75% SE limita. Vrijeme je za izvršenje revizije.", LogLevel = Microsoft.Extensions.Logging.LogLevel.Warning };
                case 1500:
                    return new EsirStatusCodeModel() { Code = code, Info = "Potreban je PIN kod", Opis = "Pokazuje da POS mora pružiti PIN kod", LogLevel = Microsoft.Extensions.Logging.LogLevel.Warning };
                case 1999:
                    return new EsirStatusCodeModel() { Code = code, Info = "Nedefinisano upozorenje", Opis = "Nešto nije u redu, ali specifično upozorenje nije definisano za tu situaciju. Proizvođač može koristiti specifične kodove proizvođača da opiše upozorenje u više detalja.", LogLevel = Microsoft.Extensions.Logging.LogLevel.Warning };
                case 2100:
                    return new EsirStatusCodeModel() { Code = code, Info = "PIN nije OK", Opis = "PIN kod poslan od strane POS-a nije validan", LogLevel = Microsoft.Extensions.Logging.LogLevel.Error };
                case 2110:
                    return new EsirStatusCodeModel() { Code = code, Info = "Kartica je zaključana", Opis = "Broj dozvoljenih unosa PIN-a je premašen. Kartica je zaključana za upotrebu", LogLevel = Microsoft.Extensions.Logging.LogLevel.Error };
                case 2210:
                    return new EsirStatusCodeModel() { Code = code, Info = "SE je zaključan", Opis = "Sigurnosni element je zaključan. Nema dodatnih faktura koje se mogu potpisati prije nego što se revizija završi", LogLevel = Microsoft.Extensions.Logging.LogLevel.Error };
                case 2220:
                    return new EsirStatusCodeModel() { Code = code, Info = "Komunikacija sa SE je neuspješna", Opis = "E-SDC ne može da se poveže sa aplikacijom Sigurnosnog Elementa", LogLevel = Microsoft.Extensions.Logging.LogLevel.Error };
                case 2230:
                    return new EsirStatusCodeModel() { Code = code, Info = "Neslaganje protokola SE", Opis = "Sigurnosni Element ne podržava traženu verziju protokola (rezervisano za kasniju upotrebu)", LogLevel = Microsoft.Extensions.Logging.LogLevel.Error };
                case 2310:
                    return new EsirStatusCodeModel() { Code = code, Info = "Neispravne poreske oznake", Opis = "Poreske oznake poslane od strane POS-a nisu definisane", LogLevel = Microsoft.Extensions.Logging.LogLevel.Error };
                case 2400:
                    return new EsirStatusCodeModel() { Code = code, Info = "Nije konfigurisano", Opis = "SDC uređaj nije potpuno konfigurisan za potpisivanje fakture (npr. nedostaju stope poreza ili URL za verifikaciju itd.)", LogLevel = Microsoft.Extensions.Logging.LogLevel.Error };
                case 2800:
                    return new EsirStatusCodeModel() { Code = code, Info = "Polje je obavezno", Opis = "Polje je obavezno (nedostaje obavezno polje zahtjeva za fakturu)", LogLevel = Microsoft.Extensions.Logging.LogLevel.Error };
                case 2801:
                    return new EsirStatusCodeModel() { Code = code, Info = "Vrijednost polja je preduga", Opis = "Dužina vrijednosti polja je duža od očekivane", LogLevel = Microsoft.Extensions.Logging.LogLevel.Error };
                case 2802:
                    return new EsirStatusCodeModel() { Code = code, Info = "Vrijednost polja je prekratka", Opis = "Dužina vrijednosti polja je kraća od očekivane", LogLevel = Microsoft.Extensions.Logging.LogLevel.Error };
                case 2803:
                    return new EsirStatusCodeModel() { Code = code, Info = "Neispravna dužina polja", Opis = "Dužina vrijednosti polja je kraća ili duža od očekivane", LogLevel = Microsoft.Extensions.Logging.LogLevel.Error };
                case 2804:
                    return new EsirStatusCodeModel() { Code = code, Info = "Vrijednost polja je van opsega", Opis = "Vrijednost polja je van očekivanog opsega", LogLevel = Microsoft.Extensions.Logging.LogLevel.Error };
                case 2805:
                    return new EsirStatusCodeModel() { Code = code, Info = "Neispravna vrijednost polja", Opis = "Polje sadrži neispravnu vrijednost", LogLevel = Microsoft.Extensions.Logging.LogLevel.Error };
                case 2806:
                    return new EsirStatusCodeModel() { Code = code, Info = "Neispravan format podataka", Opis = "Format podataka je neispravan", LogLevel = Microsoft.Extensions.Logging.LogLevel.Error };
                case 2807:
                    return new EsirStatusCodeModel() { Code = code, Info = "Lista je prekratka", Opis = "Lista stavki ili lista poreskih oznaka u zahtjevu za fakturu ne sadrži barem jedan element (stavku/oznaku)", LogLevel = Microsoft.Extensions.Logging.LogLevel.Error };
                case 2808:
                    return new EsirStatusCodeModel() { Code = code, Info = "Lista je preduga", Opis = "Lista stavki ili lista poreskih oznaka u zahtjevu za fakturu prelazi maksimalni dozvoljeni broj elemenata (stavki/oznaka) ili veličinu bajta. Maksimalne vrijednosti zavise od kapaciteta SDC-a za obradu zahtjeva za fakturu i mogu biti specifične za proizvođača.", LogLevel = Microsoft.Extensions.Logging.LogLevel.Error };
                default:
                    return new EsirStatusCodeModel() { Code = code, Info = "Nedefinisano", Opis = "Nedefinisani kod statusa", LogLevel = Microsoft.Extensions.Logging.LogLevel.Warning };
            }
            

        }





        public async Task<PorukaFiskalnogPrintera> StatusFiskalnogPrintera()
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "/api/status");
                var response = await _httpClient.SendAsync(request);
                string res = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                   Modeli.esir.EsirStatusModel esirCurrent = JsonSerializer.Deserialize<EsirStatusModel>(res);

                    if (esirCurrent == null)
                        return new PorukaFiskalnogPrintera() { IsError = true, LogLevel = LogLevel.Error, MozeNastaviti = false, Poruka = "ESIR status ima vrijednost null ne možemo nastaviti" };

                    List<EsirStatusCodeModel> statusiUOdgovoru = new List<EsirStatusCodeModel>();
                    foreach (var status in  esirCurrent.gsc)
                    {
                        var sts =  GetStatusFromCode(int.Parse(status??"0"));
                        if (sts.LogLevel == LogLevel.Error)
                            return new PorukaFiskalnogPrintera() { IsError = true, LogLevel = LogLevel.Error, MozeNastaviti = false, Poruka = $"ESIR je u grešci: {sts.Info} - {sts.Opis}" };

                    }

                    if (esirCurrent?.gsc?.Contains("1300") ?? false)
                        return new PorukaFiskalnogPrintera() { IsError = true, MozeNastaviti = false, LogLevel = LogLevel.Error, Poruka = "ESIR nema isrpavan sigurnosti elemnt" };

                    if (esirCurrent?.gsc?.Contains("1500") ?? false)
                        return new PorukaFiskalnogPrintera() { IsError = true, MozeNastaviti = false, LogLevel = LogLevel.Error, Poruka = "ESIR nema ispravan pin" };




                   return new PorukaFiskalnogPrintera() {  IsError=false, LogLevel= Microsoft.Extensions.Logging.LogLevel.Information, MozeNastaviti = true , Poruka="Fiskalni printer je spreman"};
                }
                else
                {                    
                    return new PorukaFiskalnogPrintera() { IsError = true, LogLevel = Microsoft.Extensions.Logging.LogLevel.Error, MozeNastaviti = false, Poruka = $"Nismo dobili ispravan staus ESIR-a sa http kodom {response.StatusCode} odgovor ESIR-a: {res}" };   
                }
                
            }
            catch (Exception ex)
            {
                return new PorukaFiskalnogPrintera { IsError = true,  };
            }
        }

        public async Task<PorukaFiskalnogPrintera> ProvjeriDostupan()
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "/api/attention/");
                var response = await _httpClient.SendAsync(request);
                
                if (response.IsSuccessStatusCode)
                    return new PorukaFiskalnogPrintera() { IsError = false, LogLevel = Microsoft.Extensions.Logging.LogLevel.Information, MozeNastaviti = true, Poruka = "Sistem je aktivan" };
                else
                {
                    var poruka = await response.Content.ReadAsStringAsync();
                    return new PorukaFiskalnogPrintera() { IsError = true, LogLevel = Microsoft.Extensions.Logging.LogLevel.Error, MozeNastaviti = false, Poruka = $"Sistem nije aktivan: {poruka}" };

                }


            }
            catch (HttpRequestException ex)
            {
                return new PorukaFiskalnogPrintera() { Poruka = $"ESIR sistem nije aktivan sa http greškom: {ex.StatusCode} {ex.Message} ", IsError = true, MozeNastaviti = false, LogLevel = Microsoft.Extensions.Logging.LogLevel.Error };
            }
            catch (Exception ex)
            {
                return new PorukaFiskalnogPrintera() { Poruka = $"ESIR sistem nije aktivan sa sistemom greškom:  {ex.Message} ", IsError = true, MozeNastaviti = false, LogLevel = Microsoft.Extensions.Logging.LogLevel.Error };

            }

        }
    
       

        




    }
}
