using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiskalniPrevoditelj.Servisi
{
    public interface IPrinterService
    {
        public void PrintBase64Image(string filePath,string printerName,float scale);
    }
}
