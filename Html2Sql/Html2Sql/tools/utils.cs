using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace Html2Sql.tools
{
    public static class MyExtensions
    {
        public static string s_(this string s) => Regex.Replace(s, @"\s+", " ").Trim().Replace("\n", "").Replace("\r", "");
        public static int toInt32(this string s) => int.Parse(s);
        public static string css2text(this HtmlDocument hdoc, string css) => hdoc.QuerySelector(css).InnerText.s_();
        public static string css2text(this HtmlNode hnode, string css) => hnode.QuerySelector(css).InnerText.s_();
        public static HtmlNode? findByTextAndCss(this HtmlNode hnode, string css, string text)
        =>
            hnode.QuerySelectorAll(css).FirstOrDefault(x => x.InnerText.Contains(text));
        public static HtmlNode? findByTextAndCss(this HtmlDocument hnode, string css, string text)
   =>
       hnode.QuerySelectorAll(css).FirstOrDefault(x => x.InnerText.Contains(text));
    }
    public class utils
    {

        static Dictionary<string, int> farsiNumbers = new Dictionary<string, int>()
{
    {"اول", 1},
    {"دوم", 2},
    {"سوم", 3},
    {"چهارم", 4},
    {"پنجم", 5},
    {"ششم", 6},
    {"هفتم", 7},
    {"هشتم", 8},
    {"نهم", 9},
    {"دهم", 10},
    {"یازدهم", 11},
    {"دوازدهم", 12},
    {"سیزدهم", 13},
    {"چهاردهم", 14},
    {"پانزدهم", 15},
    {"شانزدهم", 16},
    {"هفدهم", 17},
    {"هجدهم", 18},
    {"نوزدهم", 19},
    {"بیستم", 20}
};
        public static string GetImageAsBase64Url(string url)
        {

            using (var client = new HttpClient())
            {
                var bytes = client.GetByteArrayAsync(url).Result;
                return "image/jpeg;base64," + Convert.ToBase64String(bytes);
            }
        }
        public static int persianNum2int(string s)  {s=s.Trim();return farsiNumbers[s];}

        

        }
    }
    



