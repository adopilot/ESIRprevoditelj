using EsirDriver.JsonConverteri;
using EsirDriver.Modeli;
using EsirDriver.Modeli.esir;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;


namespace EsirDriver
{
    public class EsirDriverEngin
    {
        private EsirConfigModel _esirConfig;
        HttpClient _httpClient;

        public event EventHandler<PorukaFiskalnogPrintera> PorukaEvent;

        private readonly JsonSerializerOptions _jsonSerializerOptions;
        private Modeli.esir.EsirStatusModel esirCurrent { get; set; }

        private InvoiceResponseModel lastInvoiceResponseModel;


        public EsirDriverEngin()
        {
            _jsonSerializerOptions = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                Converters =
            {
                new JsonStringEnumConverter(),
                new DecimalRoundingConverter(),
                new DecimalNullRoundingConverter()
            }
            };


        }

        public string GetEsirUid()
        {
            return esirCurrent?.uid??"NEPOZNAT";
        }

        public async Task<PorukaFiskalnogPrintera> Konfigurisi(EsirConfigModel esirConfigModel)
        {
            try
            {

           

                this._esirConfig = esirConfigModel;

                Uri uri; ;
                if (!Uri.TryCreate(esirConfigModel?.webserverAddress, UriKind.Absolute, out uri))
                    return new PorukaFiskalnogPrintera() { IsError = true, MozeNastaviti = false, LogLevel = LogLevel.Error, Poruka = $"Adresa fiskalnog printera nije isrpavna" };


                _httpClient = new HttpClient() { BaseAddress = uri , Timeout= TimeSpan.FromSeconds(_esirConfig.TimeoutInSec)};
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_esirConfig.apiKey}");

                PorukaEvent.Invoke(this, new PorukaFiskalnogPrintera() { LogLevel = LogLevel.Information, MozeNastaviti = true, Poruka = $"Udesio sam http clienta na {uri}" });


                //deom mode
                //return new PorukaFiskalnogPrintera() { IsError = false, MozeNastaviti = true, LogLevel = LogLevel.Information, Poruka = $"Nazor da vidimo grešku" };


                
                    
                var statusFp1 = await StatusFiskalnogPrintera();
                if (!statusFp1.MozeNastaviti)
                {
                    _httpClient = null;
                }
                return statusFp1;
            }
            catch (Exception ex)
            {
                _httpClient = null;
                return new PorukaFiskalnogPrintera() { IsError=true, LogLevel = LogLevel.Error, MozeNastaviti=false, Poruka=$"Greška u konfigurisanju fp ex:{ex.Message}" };
            }


        }

        internal EsirStatusCodeModel GetStatusFromCode(int code)
        {
            switch (code)
            {
                case 0:
                    return new EsirStatusCodeModel() { Code = code, Info = "Sve OK", Opis = "Komanda je izvršena bez upozorenja ili grešaka", LogLevel = LogLevel.Information };
                case 100:
                    return new EsirStatusCodeModel() { Code = code, Info = "PIN je OK", Opis = "Ovaj kod ukazuje da je pruženi PIN kod ispravan", LogLevel = LogLevel.Information };
                case 210:
                    return new EsirStatusCodeModel() { Code = code, Info = "Internet dostupan", Opis = "Internet veza je dostupna (opcionalno)", LogLevel = LogLevel.Information };
                case 220:
                    return new EsirStatusCodeModel() { Code = code, Info = "Internet nedostupan", Opis = "Internet veza nije dostupna (opcionalno)", LogLevel = LogLevel.Information };
                case 1100:
                    return new EsirStatusCodeModel() { Code = code, Info = "Skladište 90% popunjeno", Opis = "Skladište koje se koristi za pohranu paketa za reviziju je 90% popunjeno. Vrijeme je za izvršenje revizije.", LogLevel = LogLevel.Warning };
                case 1300:
                    return new EsirStatusCodeModel() { Code = code, Info = "Smart kartica nije prisutna", Opis = "Sigurnosna kartica nije umetnuta u čitač pametnih kartica E-SDC", LogLevel = LogLevel.Warning };
                case 1400:
                    return new EsirStatusCodeModel() { Code = code, Info = "Potrebna je revizija", Opis = "Ukupni iznos prodaje i povrata dostigao je 75% SE limita. Vrijeme je za izvršenje revizije.", LogLevel = LogLevel.Warning };
                case 1500:
                    return new EsirStatusCodeModel() { Code = code, Info = "Potreban je PIN kod", Opis = "Pokazuje da POS mora pružiti PIN kod", LogLevel = LogLevel.Warning };
                case 1999:
                    return new EsirStatusCodeModel() { Code = code, Info = "Nedefinisano upozorenje", Opis = "Nešto nije u redu, ali specifično upozorenje nije definisano za tu situaciju. Proizvođač može koristiti specifične kodove proizvođača da opiše upozorenje u više detalja.", LogLevel = LogLevel.Error };
                case 2100:
                    return new EsirStatusCodeModel() { Code = code, Info = "PIN nije OK", Opis = "PIN kod poslan od strane POS-a nije validan", LogLevel = LogLevel.Error };
                case 2110:
                    return new EsirStatusCodeModel() { Code = code, Info = "Kartica je zaključana", Opis = "Broj dozvoljenih unosa PIN-a je premašen. Kartica je zaključana za upotrebu", LogLevel = LogLevel.Error };
                case 2210:
                    return new EsirStatusCodeModel() { Code = code, Info = "SE je zaključan", Opis = "Sigurnosni element je zaključan. Nema dodatnih faktura koje se mogu potpisati prije nego što se revizija završi", LogLevel = LogLevel.Error };
                case 2220:
                    return new EsirStatusCodeModel() { Code = code, Info = "Komunikacija sa SE je neuspješna", Opis = "E-SDC ne može da se poveže sa aplikacijom Sigurnosnog Elementa", LogLevel = LogLevel.Error };
                case 2230:
                    return new EsirStatusCodeModel() { Code = code, Info = "Neslaganje protokola SE", Opis = "Sigurnosni Element ne podržava traženu verziju protokola (rezervisano za kasniju upotrebu)", LogLevel = LogLevel.Error };
                case 2310:
                    return new EsirStatusCodeModel() { Code = code, Info = "Neispravne poreske oznake", Opis = "Poreske oznake poslane od strane POS-a nisu definisane", LogLevel = LogLevel.Error };
                case 2400:
                    return new EsirStatusCodeModel() { Code = code, Info = "Nije konfigurisano", Opis = "SDC uređaj nije potpuno konfigurisan za potpisivanje fakture (npr. nedostaju stope poreza ili URL za verifikaciju itd.)", LogLevel = LogLevel.Error };
                case 2800:
                    return new EsirStatusCodeModel() { Code = code, Info = "Polje je obavezno", Opis = "Polje je obavezno (nedostaje obavezno polje zahtjeva za fakturu)", LogLevel = LogLevel.Error };
                case 2801:
                    return new EsirStatusCodeModel() { Code = code, Info = "Vrijednost polja je preduga", Opis = "Dužina vrijednosti polja je duža od očekivane", LogLevel = LogLevel.Error };
                case 2802:
                    return new EsirStatusCodeModel() { Code = code, Info = "Vrijednost polja je prekratka", Opis = "Dužina vrijednosti polja je kraća od očekivane", LogLevel = LogLevel.Error };
                case 2803:
                    return new EsirStatusCodeModel() { Code = code, Info = "Neispravna dužina polja", Opis = "Dužina vrijednosti polja je kraća ili duža od očekivane", LogLevel = LogLevel.Error };
                case 2804:
                    return new EsirStatusCodeModel() { Code = code, Info = "Vrijednost polja je van opsega", Opis = "Vrijednost polja je van očekivanog opsega", LogLevel = LogLevel.Error };
                case 2805:
                    return new EsirStatusCodeModel() { Code = code, Info = "Neispravna vrijednost polja", Opis = "Polje sadrži neispravnu vrijednost", LogLevel = LogLevel.Error };
                case 2806:
                    return new EsirStatusCodeModel() { Code = code, Info = "Neispravan format podataka", Opis = "Format podataka je neispravan", LogLevel = LogLevel.Error };
                case 2807:
                    return new EsirStatusCodeModel() { Code = code, Info = "Lista je prekratka", Opis = "Lista stavki ili lista poreskih oznaka u zahtjevu za fakturu ne sadrži barem jedan element (stavku/oznaku)", LogLevel = LogLevel.Error };
                case 2808:
                    return new EsirStatusCodeModel() { Code = code, Info = "Lista je preduga", Opis = "Lista stavki ili lista poreskih oznaka u zahtjevu za fakturu prelazi maksimalni dozvoljeni broj elemenata (stavki/oznaka) ili veličinu bajta. Maksimalne vrijednosti zavise od kapaciteta SDC-a za obradu zahtjeva za fakturu i mogu biti specifične za proizvođača.", LogLevel = LogLevel.Error };
                default:
                    return new EsirStatusCodeModel() { Code = code, Info = "Nedefinisano", Opis = "Nedefinisani kod statusa", LogLevel = LogLevel.Warning };
            }
            

        }


        private PorukaFiskalnogPrintera ImamoLiConfig()
        {
            if (_httpClient == null || _esirConfig == null)
                return new PorukaFiskalnogPrintera() { IsError = true, LogLevel = LogLevel.Error, MozeNastaviti = false, Poruka = "Fiskalni printer nije konfigursan" };

            else return new PorukaFiskalnogPrintera() { MozeNastaviti = true };
        }
        public async Task<PorukaFiskalnogPrintera> DirektnaSampa(EsirDirektnaStampaModel model)
        {
            if (model == null)
                return new PorukaFiskalnogPrintera() { IsError = false, MozeNastaviti = true, LogLevel = LogLevel.Debug, Poruka = "Nemam šta da štamapm na esiru putem direktne štampe" };
            if (!ImamoLiConfig().MozeNastaviti)
            {
                return ImamoLiConfig();
            }
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, "/api/device/print/");



                var json = JsonSerializer.Serialize(model, _jsonSerializerOptions);

                var content = new StringContent(json, null, "application/json");
                request.Content = content;

                var response = await _httpClient.SendAsync(request);

                string res = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return new PorukaFiskalnogPrintera() { IsError = false, MozeNastaviti = true, LogLevel = LogLevel.Debug, Poruka = "Ostamapli smo ne fiskalni tekst" };
                }
                else
                {
                    try
                    {
                        EsirErrorResopnse esirErrorResopnse = JsonSerializer.Deserialize<EsirErrorResopnse>(res,_jsonSerializerOptions);
                        return new PorukaFiskalnogPrintera() { IsError = true, MozeNastaviti = true, LogLevel = LogLevel.Warning, Poruka = $"Nismo oštampali nefiskalni tekst sa greškom {esirErrorResopnse.message}" };
                    }
                    catch
                    {

                    }
                    
                        return new PorukaFiskalnogPrintera() { IsError = true, MozeNastaviti = true, LogLevel = LogLevel.Warning, Poruka = $"Nismo oštampali nefiskalni tekst sa greškom {res}" };
                }
        }
            catch (HttpRequestException ex)
            {
                return new PorukaFiskalnogPrintera() { IsError = true, MozeNastaviti = false, LogLevel = LogLevel.Error, Poruka = $"HTTP Greška kod štampanja nefiskalnog teksta {ex.Message}" };
            }
            catch (Exception ex)
            {
                return new PorukaFiskalnogPrintera() { IsError = true, MozeNastaviti = false, LogLevel = LogLevel.Error, Poruka = $" Greška kod štampanja nefiskalnog teksta {ex.Message}" };


            }


        }

        public async Task<InvoiceResponseModel> LastInvoice(ReceiptLayoutType receiptLayoutType, ReceptImageFormat receptImageFormat, bool includeHeaderAndFooter)
        {
            if (lastInvoiceResponseModel != null)
                return lastInvoiceResponseModel;


            try
            {
                var ado = this.ImamoLiConfig();
                if (ado.IsError)
                {
                    PorukaEvent.Invoke(this, ado);
                    return null;
                }

                var request = new HttpRequestMessage(HttpMethod.Get, $"/api/invoices/last?receiptLayout={receiptLayoutType.ToString()}&imageFormat={receptImageFormat.ToString()}&includeHeaderAndFooter={includeHeaderAndFooter.ToString()}");
                HttpResponseMessage response = await _httpClient.SendAsync(request);
                string res = await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode();

                var all = JsonSerializer.Deserialize<EsirInvoiceFromDbModel>(res,_jsonSerializerOptions);

                lastInvoiceResponseModel = all.invoiceResponse;
                return lastInvoiceResponseModel;



            }


            catch (Exception ex) {
                PorukaEvent.Invoke(this, new PorukaFiskalnogPrintera() { IsError = true, MozeNastaviti = true, LogLevel = LogLevel.Error, Poruka = $"Nismo dobili zadnji račun sa greškom {ex.Message}" });
                return null;
            }
          
            }

        private async Task<PorukaFiskalnogPrintera> unesitePin(int pin)
        {
            try
            {
                if (!ImamoLiConfig().MozeNastaviti)
                {
                    return ImamoLiConfig();
                }
                var request = new HttpRequestMessage(HttpMethod.Post, "/api/pin");
                request.Content = new StringContent(pin.ToString()); 
                var response = await _httpClient.SendAsync(request);
                var poruka = await response.Content.ReadAsStringAsync();

                switch (poruka)
                {
                    case "0100":
                    case "\"0100\"":
                        return new PorukaFiskalnogPrintera() { LogLevel = LogLevel.Information, MozeNastaviti = true, IsError = false, Poruka = "PIN je ispravno unet" };
                    case "1300":
                    case "\"1300\"":

                        return new PorukaFiskalnogPrintera() { LogLevel = LogLevel.Error, MozeNastaviti = false, IsError = true, Poruka = "Bezbednosni element nije prisutan" };
                    case "2800":
                    case "\"2800\"":
                        return new PorukaFiskalnogPrintera() { LogLevel = LogLevel.Error, MozeNastaviti = false, IsError = true, Poruka = "Pogrešan format PIN-a (očekivano 4 cifre)" };
                    case "2806":
                    case "\"2806\"":
                        return new PorukaFiskalnogPrintera() { LogLevel = LogLevel.Error, MozeNastaviti = false, IsError = true, Poruka = "Pogrešan format PIN-a (očekivano 4 cifre)" };
                    default:
                        int code = 0;
                        var codeStr = int.TryParse((poruka ?? "0").Replace("\"", ""),out code);
                        var info = GetStatusFromCode(code);
                        return new PorukaFiskalnogPrintera() { LogLevel = LogLevel.Error, MozeNastaviti = false, IsError = true, Poruka = $"Greška pirlikom unešanje pina  {info.Info}" };
                }

            }
            catch (Exception ex)
            {    
                    return new PorukaFiskalnogPrintera() { LogLevel = LogLevel.Error, MozeNastaviti = false, IsError = true, Poruka = $"Nismo unjeli pin sa greškom {ex.Message}" };
            }

        }
        public async Task<PorukaFiskalnogPrintera> StatusFiskalnogPrintera()
        {
            try
            {
                    if (!ImamoLiConfig().MozeNastaviti)
                    {
                        return ImamoLiConfig();
                    }

                var dostupan = await provjeriDostupan();
                PorukaEvent.Invoke(this, dostupan);

                if (dostupan.IsError)
                {
                    _httpClient = null;
                    return dostupan;
                }



                var request = new HttpRequestMessage(HttpMethod.Get, "/api/status");
                HttpResponseMessage response = await _httpClient.SendAsync(request);
                string res = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                   esirCurrent = null;
                   esirCurrent = JsonSerializer.Deserialize<EsirStatusModel>(res,_jsonSerializerOptions);

                    if (esirCurrent == null)
                        return new PorukaFiskalnogPrintera() { IsError = true, LogLevel = LogLevel.Error, MozeNastaviti = false, Poruka = "ESIR status ima vrijednost null ne možemo nastaviti" };

                    if (esirCurrent?.isPinRequired??false)
                    {
                        PorukaEvent.Invoke(this, new PorukaFiskalnogPrintera() { IsError = false, MozeNastaviti = true, LogLevel = LogLevel.Information, Poruka = "ESIR traži unos pina (pokušavamo)" });
                        var pinRespnse = await unesitePin(_esirConfig.pin);
                        PorukaEvent.Invoke(this, pinRespnse);
                        if (pinRespnse.IsError)
                            return pinRespnse;
                        else
                        {
                            var requestPin = new HttpRequestMessage(HttpMethod.Get, "/api/status");
                            HttpResponseMessage responsePin = await _httpClient.SendAsync(requestPin);
                            string resPin = await response.Content.ReadAsStringAsync();
                            esirCurrent = JsonSerializer.Deserialize<EsirStatusModel>(resPin);
                        }
                    }
                    foreach (var status in  esirCurrent?.gsc??new List<string>() { "2400"})
                    {
                        var sts =  GetStatusFromCode(int.Parse(status?? "2400"));
                        if (sts.LogLevel == LogLevel.Error)
                        {
                            var greska = new PorukaFiskalnogPrintera() { IsError = true, LogLevel = LogLevel.Error, MozeNastaviti = false, Poruka = $"ESIR je u grešci: {sts.Info} - {sts.Opis}" };
                            PorukaEvent.Invoke(this, greska);
                            return greska;
                        }
                        else
                        {
                            PorukaEvent.Invoke(this, new PorukaFiskalnogPrintera() { MozeNastaviti = true, LogLevel = LogLevel.Information, Poruka = $"Staus ESIRA {sts.Code} - {sts.Info}" });
                        }
                    }
                    PorukaEvent.Invoke(this, new PorukaFiskalnogPrintera() { IsError = false, MozeNastaviti = true, LogLevel = LogLevel.Information, Poruka = "Fiskalni printer je spreman" });
               
                   return new PorukaFiskalnogPrintera() {  IsError=false, LogLevel= LogLevel.Information, MozeNastaviti = true , Poruka=$"Fiskalni printer je spreman {((esirCurrent?.auditRequired??false)?"potrebna je intervvecija":"")} uid:{esirCurrent?.uid}"};
                }
                else
                {                    
                    var elseGr = new PorukaFiskalnogPrintera() { IsError = true, LogLevel = LogLevel.Error, MozeNastaviti = false, Poruka = $"Nismo dobili ispravan staus ESIR-a sa http kodom {response.StatusCode} odgovor ESIR-a: {res}" };   
                    PorukaEvent.Invoke(this, elseGr);
                    return elseGr;
                }
                
            }
            catch (Exception ex)
            {
                var exMsg = new PorukaFiskalnogPrintera() { IsError = true, LogLevel = LogLevel.Error, MozeNastaviti = false, Poruka = $"Nismo dobili ispravan staus ESIR-a sa greškom {ex.Message}" };

                PorukaEvent.Invoke(this, exMsg);
                return exMsg;
                
            }
        }

        private async Task<PorukaFiskalnogPrintera> provjeriDostupan()
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "/api/attention/");
                var response = await _httpClient.SendAsync(request);
                
                if (response.IsSuccessStatusCode)
                    return new PorukaFiskalnogPrintera() { IsError = false, LogLevel = LogLevel.Information, MozeNastaviti = true, Poruka = "Sistem je aktivan (dostupan)" };
                
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return new PorukaFiskalnogPrintera() { IsError = true, LogLevel = LogLevel.Error, MozeNastaviti = false, Poruka = $"Niste autorizvoani provjerite api kljuć" };
                }
                else
                {
                    var poruka = await response.Content.ReadAsStringAsync();
                    return new PorukaFiskalnogPrintera() { IsError = true, LogLevel = LogLevel.Error, MozeNastaviti = false, Poruka = $"Sistem nije aktivan: {poruka}" };

                }


            }
            catch (HttpRequestException ex)
            {
               return new PorukaFiskalnogPrintera() { Poruka = $"ESIR sistem nije aktivan sa http greškom: {ex.StatusCode} {ex.Message} ", IsError = true, MozeNastaviti = false, LogLevel = LogLevel.Error };
            }
            catch (Exception ex)
            {
                return new PorukaFiskalnogPrintera() { Poruka = $"ESIR sistem nije aktivan sa sistemom greškom:  {ex.Message} ", IsError = true, MozeNastaviti = false, LogLevel = LogLevel.Error };

            }

        }
    

        

      public async Task<InvoiceResponseModel>  OstampajRacun(InvoiceModel invoiceRequestModel)
        {
            try
            {
                if (ImamoLiConfig().IsError)
                {
                    PorukaEvent.Invoke(this,new PorukaFiskalnogPrintera() { IsError=true,MozeNastaviti=false, LogLevel = LogLevel.Error, Poruka=$"Nemožemo štampati faktru kada printer nije knifugrsan"});
                    throw new Exception("Fiskalni printer nije konfigursan");
                }


                var payment = invoiceRequestModel.invoiceRequest.payment.Sum(x=>x.amount);
                var total = invoiceRequestModel.invoiceRequest.items.Sum(x=>x.totalAmount);

                if (total > payment)
                {
                    invoiceRequestModel.invoiceRequest.payment.Add(new PaymentModel() { paymentType= PaymentTypes.Cash, amount= total- payment });
                }

                invoiceRequestModel.invoiceRequest.invoiceType = _esirConfig.OperationMode;

                if (invoiceRequestModel.invoiceRequest.transactionType == TranscationType.Refund) 
                {
                    invoiceRequestModel.invoiceRequest.referentDocumentNumber = $"{GetEsirUid()}-{GetEsirUid()}-{invoiceRequestModel?.invoiceRequest?.referentDocumentNumber}";
                }


                var request = new HttpRequestMessage(HttpMethod.Post, "/api/invoices/");


                
                var json = JsonSerializer.Serialize(invoiceRequestModel, _jsonSerializerOptions);

                var content = new StringContent(json, null, "application/json");
                request.Content = content;

                var response = await _httpClient.SendAsync(request);

                string res = await response.Content.ReadAsStringAsync();


                if (response.StatusCode == System.Net.HttpStatusCode.OK )
                {
                    lastInvoiceResponseModel = JsonSerializer.Deserialize<InvoiceResponseModel>(res,_jsonSerializerOptions);
                    return lastInvoiceResponseModel;
                }
                else
                {
                    EsirErrorResopnse esirErrorResopnse = JsonSerializer.Deserialize<EsirErrorResopnse>(res,_jsonSerializerOptions);

                    

                    throw new Exception(esirErrorResopnse?.message??"Greška u odgovru");
                    
                }
                
                

                

                


            }
            catch (HttpRequestException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;

            }

        }

        




    }
}
