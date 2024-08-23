using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EsirDriver.JsonConverteri
{
    public class SerbianCyrillicConverter
    {
        private static readonly Dictionary<string, string> LatinToCyrillicMap = new Dictionary<string, string>
    {
        {"A", "А"}, {"B", "Б"}, {"V", "В"}, {"G", "Г"}, {"D", "Д"},
        {"Đ", "Ђ"}, {"E", "Е"}, {"Ž", "Ж"}, {"Z", "З"}, {"I", "И"},
        {"J", "Ј"}, {"K", "К"}, {"L", "Л"}, {"Lj", "Љ"}, {"M", "М"},
        {"N", "Н"}, {"Nj", "Њ"}, {"O", "О"}, {"P", "П"}, {"R", "Р"},
        {"S", "С"}, {"T", "Т"}, {"Ć", "Ћ"}, {"U", "У"}, {"F", "Ф"},
        {"H", "Х"}, {"C", "Ц"}, {"Č", "Ч"}, {"Dž", "Џ"}, {"Š", "Ш"},
        {"a", "а"}, {"b", "б"}, {"v", "в"}, {"g", "г"}, {"d", "д"},
        {"đ", "ђ"}, {"e", "е"}, {"ž", "ж"}, {"z", "з"}, {"i", "и"},
        {"j", "ј"}, {"k", "к"}, {"l", "л"}, {"lj", "љ"}, {"m", "м"},
        {"n", "н"}, {"nj", "њ"}, {"o", "о"}, {"p", "п"}, {"r", "р"},
        {"s", "с"}, {"t", "т"}, {"ć", "ћ"}, {"u", "у"}, {"f", "ф"},
        {"h", "х"}, {"c", "ц"}, {"č", "ч"}, {"dž", "џ"}, {"š", "ш"}
    };

        public static string ConvertToCyrillic(string input)
        {
            foreach (var pair in LatinToCyrillicMap.OrderByDescending(x => x.Key.Length))
            {
                input = input.Replace(pair.Key, pair.Value);
            }
            return input;
        }
    }
}
