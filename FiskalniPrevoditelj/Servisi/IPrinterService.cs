using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiskalniPrevoditelj.Servisi
{
    public interface IPrinterService
    {
        Task PrintPdfAsync(string filePath, string _printerName);


        void PrintBase64Image(string base64, string printerName, string paperSize);
        List<string> ListPirnters();

        List<string> PaperSizes(string printerName);


    }
}
