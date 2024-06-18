using EsirDriver.Modeli.esir;
using FiskalniPrevoditelj.Modeli;
using Microsoft.FluentUI.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiskalniPrevoditelj.Servisi
{
    internal class ConfigSaveServis
    {
        public async Task<EsirDriver.Modeli.EsirConfigModel> GetEsirConfg()
        {
            EsirDriver.Modeli.EsirConfigModel model = new EsirDriver.Modeli.EsirConfigModel();

            model.webserverAddress = await SecureStorage.Default.GetAsync("webserverAddress") ?? "";
            int pinInt = 0;
            int.TryParse((await SecureStorage.Default.GetAsync("pin") ?? "0"), out pinInt);
            model.pin = pinInt;

            model.authorizeLocalClients = bool.Parse(await SecureStorage.Default.GetAsync("authorizeLocalClients") ?? "true");
            model.authorizeRemoteClients = bool.Parse(await SecureStorage.Default.GetAsync("authorizeRemoteClients") ?? "true");

            model.apiKey = await SecureStorage.Default.GetAsync("ApiKey") ?? "23423414312431243";

            var operationMode = await SecureStorage.Default.GetAsync("operationMode") ?? "Normal";

            var operatonModeEnum = InvoiceType.Normal;
            Enum.TryParse(operationMode, out operatonModeEnum);

            model.OperationMode = operatonModeEnum;

            int toInt = 3;
            int.TryParse((await SecureStorage.Default.GetAsync("TimeoutInSec") ?? "3"), out toInt);

            model.TimeoutInSec = toInt;
            return model;
        }
        
        public async Task<AppPostavkeModel> GetAppConfig()
        {
            AppPostavkeModel appPostavkeModel = new AppPostavkeModel();
            bool palimSe = false;
            bool.TryParse(await SecureStorage.Default.GetAsync(nameof(appPostavkeModel.RunAppOnWindowsStartUp) ??"false" ),out palimSe);
            appPostavkeModel.RunAppOnWindowsStartUp = palimSe;
            return appPostavkeModel;
        }
        public async Task<PocoMsg> SetAppConfig(AppPostavkeModel appPostavke)
        {
            try
            {
                await SecureStorage.Default.SetAsync(nameof(appPostavke.RunAppOnWindowsStartUp), (appPostavke?.RunAppOnWindowsStartUp ?? false).ToString());
                return new PocoMsg() { Valid = true, Msg = "Spasili smo postavke aplikcije" };
            }
            catch (Exception ex)
            {
                return new PocoMsg() { Msg = $"Greška kod spašavanja postavki aplikcije ex:{ex.Message}" };
            }
        }

        public async Task<PocoMsg> SetEsirConfg(EsirDriver.Modeli.EsirConfigModel model)
        {

            try
            {
                if (model == null)
                {
                    throw new Exception("Ne možemo spasiti podatke jer je model null");
                }

                await SecureStorage.Default.SetAsync("webserverAddress", model.webserverAddress ?? "http://127.0.0.1:3566");

                await SecureStorage.Default.SetAsync("authorizeLocalClients", model.authorizeLocalClients.ToString());
                await SecureStorage.Default.SetAsync("authorizeRemoteClients", model.authorizeRemoteClients.ToString());
                await SecureStorage.Default.SetAsync("ApiKey", model.apiKey ?? "23423414312431243");
                await SecureStorage.Default.SetAsync("operationMode", model.OperationMode.ToString());
                await SecureStorage.Default.SetAsync("TimeoutInSec", model.TimeoutInSec.ToString());
                await SecureStorage.Default.SetAsync("pin", model.pin.ToString());



                return new PocoMsg() { Msg = "Podatci spašeni", Valid = true };


            }
            catch (Exception ex)
            {
                return new Modeli.PocoMsg() { Msg = $"Greška u spašavnju congiga {ex.Message}" };
            }


        }

        public async Task<EsirDriver.Modeli.PrevoditeljSettingModel> GetPrevoditeljConfg()
        {
            EsirDriver.Modeli.PrevoditeljSettingModel model = new EsirDriver.Modeli.PrevoditeljSettingModel();



            model.KomandePrintera = EsirDriver.Modeli.PrevodimoKomandePrintera.HcpFBiH;

            int toInt = 300;
            int.TryParse((await SecureStorage.Default.GetAsync("ReadFolderEvryMiliSec") ?? "300"), out toInt);

            model.ReadFolderEvryMiliSec = toInt;

            model.AutomaticallyCloseRecept = true;

            model.DefSklSifra = await SecureStorage.Default.GetAsync("DefSklSifra") ?? "000";
            model.PodrazumjevanaPoreskaStopa = await SecureStorage.Default.GetAsync("PodrazumjevanaPoreskaStopa") ?? "F";

            model.Enabled = bool.Parse(await SecureStorage.Default.GetAsync("PrevoditeljEnabled") ?? "false");

            model.EncodingName = await SecureStorage.Default.GetAsync("EncodingName") ?? "windows-1250";

            model.KomandePrintera = EsirDriver.Modeli.PrevodimoKomandePrintera.HcpFBiH;

            model.PathInputFiles = await SecureStorage.Default.GetAsync("PathInputFiles") ?? "c:\\hcp\\to_fp\\";
            model.PathOutputFiles = await SecureStorage.Default.GetAsync("PathOutputFiles") ?? @"c:\hcp\from_fp\";

            return model;

        }

        public async Task<Modeli.PocoMsg> SetPrevoditeljConfg(EsirDriver.Modeli.PrevoditeljSettingModel model)
        {

            try
            {
                if (model == null)
                {
                    throw new Exception("Ne možemo spasiti podatke jer je model null");
                }

                await SecureStorage.SetAsync("ReadFolderEvryMiliSec", model.ReadFolderEvryMiliSec.ToString());

                await SecureStorage.SetAsync("AutomaticallyCloseRecept", model.AutomaticallyCloseRecept.ToString());

                await SecureStorage.Default.SetAsync("DefSklSifra", model.DefSklSifra ?? "000");

                await SecureStorage.Default.SetAsync("PodrazumjevanaPoreskaStopa", model.PodrazumjevanaPoreskaStopa ?? "F");

                await SecureStorage.Default.SetAsync("PrevoditeljEnabled", model.Enabled.ToString() ?? "false");

                await SecureStorage.Default.SetAsync("EncodingName", model.EncodingName ?? "windows-1250");

                await SecureStorage.Default.SetAsync("KomandePrintera", model.KomandePrintera.ToString());

                await SecureStorage.Default.SetAsync("PathInputFiles", model.PathInputFiles ?? @"c:\hcp\to_fp\");

                await SecureStorage.Default.SetAsync("PathOutputFiles", model.PathOutputFiles ?? @"c:\hcp\from_fp\");

                return new Modeli.PocoMsg() { Msg = "Config je spašen", Valid = true };


            }
            catch (Exception ex)
            {
                return new Modeli.PocoMsg() { Msg = $"Greška u spašavnju congiga {ex.Message}" };
            }
        }
    }
}
