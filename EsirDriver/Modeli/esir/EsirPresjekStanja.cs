using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EsirDriver.Modeli.esir
{
    public class EsirPresjekStanja
    {
        public DateTime startOfPeriod { get; set; }
        public decimal total { get; set; }
        public List<TotalByArticle> totalByArticle { get; set; }
        public List<object> totalByArticleAdvance { get; set; }
        public List<TotalByCashier> totalByCashier { get; set; }
        public List<TotalByPaymentType> totalByPaymentType { get; set; }
        public List<TotalByTax> totalByTax { get; set; }
        public decimal totalCash { get; set; }
    }
    public class TotalByArticle
    {
        public decimal amount { get; set; }
        public string articleName { get; set; }
        public object gtin { get; set; }
        public object plu { get; set; }
        public decimal quantity { get; set; }
        public string taxLabel { get; set; }
    }

    public class TotalByCashier
    {
        public decimal amount { get; set; }
        public string name { get; set; }
    }

    public class TotalByPaymentType
    {
        public decimal amount { get; set; }
        public string paymentType { get; set; }
    }

    public class TotalByTax
    {
        public decimal amount { get; set; }
        public string category { get; set; }
        public string label { get; set; }
        public decimal rate { get; set; }
    }

}
