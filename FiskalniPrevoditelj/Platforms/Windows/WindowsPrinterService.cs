using FiskalniPrevoditelj.Servisi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Printing;
using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;
using System.Drawing.Printing;


namespace FiskalniPrevoditelj.Platforms.Windows
{
    public class WindowsPrinterService : IPrinterService
    {
        private string _filePath;
        private string _printerName;

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
            PrintDialog printDialog = new PrintDialog();
            printDialog.Document = printDoc;
            if (printDialog.ShowDialog() == DialogResult.OK)
            {
                printDoc.Print();
            }

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
