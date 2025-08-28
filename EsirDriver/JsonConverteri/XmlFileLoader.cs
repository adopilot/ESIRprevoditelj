using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;


namespace EsirDriver.JsonConverteri
{
    public static class XmlFileLoader
    {
        /// <summary>
        /// Loads an XML file with encoding detected from its declaration.
        /// </summary>
        /// <param name="filePath">Full path to the XML file.</param>
        /// <param name="defaultEncoding">Fallback encoding if none is declared.</param>
        /// <returns>Loaded XmlDocument.</returns>
        public static async Task<XmlDocument> LoadXmlAsync(string filePath, string defaultEncoding = "windows-1250")
        {
            string encodingName = await DetectEncodingAsync(filePath, defaultEncoding);
            Encoding encoding = Encoding.GetEncoding(encodingName);

            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (var reader = new StreamReader(fs, encoding))
            {
                string xmlContent = await reader.ReadToEndAsync();
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xmlContent);
                return doc;
            }
        }

        /// <summary>
        /// Reads the XML declaration to detect encoding.
        /// </summary>
        private static async Task<string> DetectEncodingAsync(string filePath, string fallbackEncoding)
        {
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (var reader = new StreamReader(fs, Encoding.ASCII, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: true))
            {
                char[] buffer = new char[1024];
                int readCount = await reader.ReadAsync(buffer, 0, buffer.Length);
                string headerSample = new string(buffer, 0, readCount);

                var match = Regex.Match(headerSample, @"<\?xml.*encoding\s*=\s*[""'](?<enc>[^""']+)[""'].*\?>", RegexOptions.IgnoreCase);
                return match.Success ? match.Groups["enc"].Value : fallbackEncoding;
            }
        }
    }

}
