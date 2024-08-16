using System.Buffers.Text;

namespace FiskalniPrevoditelj;

public partial class NewPage1 : ContentPage
{
    
    public NewPage1(string base64String)
    {
        
        InitializeComponent();

        
        
        this.PdfWebView.VerticalOptions = LayoutOptions.FillAndExpand;
        this.PdfWebView.HorizontalOptions = LayoutOptions.FillAndExpand;


        LoadPdfInWebView(base64String);
    }
    private void LoadPdfInWebView(string base64String)
    {
        // Replace with your base64 string
        

        // Decode the base64 string to get the PDF content
      //  byte[] pdfBytes = Convert.FromBase64String(base64String);

        // Define the local file path to save the PDF
        string fileName = "sample.pdf";
        string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), fileName);

        // Write the PDF bytes to the local file
       // File.WriteAllBytes(filePath, pdfBytes);

        // Set the local file path as the source for UrlWebViewSource
        var urlWebViewSource = new UrlWebViewSource
        {
            Url = $"file://{filePath}"
        };

        // Assuming you have a WebView named 'webView' in your XAML
        PdfWebView.Source = urlWebViewSource;
    }
}