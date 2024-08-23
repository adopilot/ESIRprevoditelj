using FiskalniPrevoditelj.Servisi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Printing;
using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;
using System.Drawing.Printing;
using Microsoft.FluentUI.AspNetCore.Components;
using System.Drawing;
using System.Drawing.Printing;
using Image = System.Drawing.Image;

namespace FiskalniPrevoditelj.Platforms.Windows
{
    public class WindowsPrinterService : IPrinterService
    {
        private string _filePath;
        private string _printerName;


        public List<string> PaperSizes(string printerName)
        {
            List<string> sizes = new List<string>();
            if (string.IsNullOrEmpty(printerName) ) 
                return sizes;

            PrintDocument printDoc = new PrintDocument();
            printDoc.PrinterSettings.PrinterName = _printerName;
            var printerSettings = printDoc.PrinterSettings;
            foreach (var paperSize in printerSettings.PaperSizes)
            {
                sizes.Add(paperSize.ToString());
            }
            return sizes;

        }
        public List<string> ListPirnters()
        {
            List<string> pritners = new List<string>();
            foreach (string printer in System.Drawing.Printing.PrinterSettings.InstalledPrinters)
            {
                pritners.Add(printer);
            }
            return pritners;
        }
        public void PrintBase64Image(string base64,string printerName,string paperSize)
        {
            float paperWidthInches = 1.4f;//  10 / 25.4f; // 1 inch = 25.4 mm

            byte[] imageBytes = Convert.FromBase64String(base64);

            PrintDocument printDoc = new PrintDocument();
            printDoc.PrinterSettings.PrinterName = printerName;

            using (var ms = new MemoryStream(imageBytes))
            {
                using (Image image = Image.FromStream(ms))
                {
                    printDoc.PrintPage += (sender, e) =>
                    {
                        // Get the DPI (dots per inch) of the printer
                        float dpiX = e.Graphics.DpiX;

                        // Calculate the width in pixels for 80mm
                        int widthInPixels = (int)(paperWidthInches * dpiX);

                        // Calculate the height in pixels to maintain the aspect ratio
                        float aspectRatio = (float)image.Height / image.Width;
                        int heightInPixels = (int)(widthInPixels * aspectRatio);

                        // Create a rectangle with the calculated dimensions
                        var rect = new Rectangle(0, 0, widthInPixels, heightInPixels);

                        // Draw the image at the specified location and size
                        e.Graphics.DrawImage(image, rect);

                        e.HasMorePages = false;
                    };

                    // Trigger the printing process
                    printDoc.Print();
                }
            }

        }
        public Task PrintPdfAsync(string filePath, string _printerName)
        {
            _filePath = filePath;

            // Create a new PrintDocument
            PrintDocument printDoc = new PrintDocument();
            printDoc.PrintPage += OnPrintPage;
            printDoc.DocumentName = Path.GetFileName(filePath);

            var paperSize = new PaperSize("Osadesetka",3,11); // Width and height in hundredths of an inch
            
            printDoc.DefaultPageSettings.PaperSize = paperSize;

            // Set printer settings
            printDoc.DefaultPageSettings.Landscape = false; // Change to true if needed
            printDoc.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0); // Remove margins


            // Show print dialog (optional)
            

            // Print directly without showing the print dialog
            printDoc.Print();

            return Task.CompletedTask;
        }
      
        private void OnPrintPage(object sender, PrintPageEventArgs e)
        {
            // Load and print the PDF page
            using (var stream = new FileStream(_filePath, FileMode.Open, FileAccess.Read))
            {
                var image = System.Drawing.Image.FromStream(stream);
                e.Graphics.DrawImage(image, new System.Drawing.Rectangle(0, 0, e.PageBounds.Width, e.PageBounds.Height));
            }

            // If your PDF has multiple pages, handle them here
            e.HasMorePages = false; // Set to true if there are more pages
        }
    }
}
