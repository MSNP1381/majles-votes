using HtmlAgilityPack;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Html2Sql.tools
{
    public static class MyExtensions
    {
        static List<KeyValuePair<char, char>> arabic2perList = new List<KeyValuePair<char, char>>
        {
            new('ي', 'ی'),
            new('ك', 'ک'),
            new('ة', 'ه')
        };

        public static DateTime persianDate2utc(this string s)
        {
            CultureInfo persianCulture = new CultureInfo("fa-IR");
            DateTime date = DateTime.ParseExact(s, "yyyy/MM/dd", persianCulture).ToUniversalTime();
            return date;
        }

        public static string arabic2per(this string s)
        {
            foreach (var i in arabic2perList)
                s = s.Replace(i.Key, i.Value);
            return s;
        }

        public static string s_(this string s) =>
            Regex
                .Replace(s, @"\s+", " ")
                .Trim()
                .Replace("\n", "")
                .Replace("\r", "")
                .Trim()
                .arabic2per();

        public static int toInt32(this string s) => int.Parse(s);

        public static string css2text(this HtmlDocument hdoc, string css) =>
            hdoc.QuerySelector(css).InnerText.s_();

        public static string css2text(this HtmlNode hnode, string css) =>
            hnode.QuerySelector(css).InnerText.s_();

        public static HtmlNode? findByTextAndCss(this HtmlNode hnode, string css, string text) =>
            hnode.QuerySelectorAll(css).FirstOrDefault(x => x.InnerText.Contains(text));

        public static HtmlNode? findByTextAndCss(
            this HtmlDocument hnode,
            string css,
            string text
        ) => hnode.QuerySelectorAll(css).FirstOrDefault(x => x.InnerText.Contains(text));
    }

    public static class utils
    {
        public static Dictionary<int, string> AttendanceTypeValues = new Dictionary<int, string>()
        {
            { (int)AttendanceType.absence, "غیبت" },
            { (int)AttendanceType.nonParticipation, "عدم مشارکت" },
            { (int)AttendanceType.against, "مخالف" },
            { (int)AttendanceType.favor, "موافق" },
            { (int)AttendanceType.abstaining, "ممتنع" },
        };

        public static int Year2int(string year)
        {
            if (year.Contains("اول"))
                return 1;
            if (year.Contains("دوم"))
                return 2;
            if (year.Contains("سوم"))
                return 3;
            return 4;
        }

        static Dictionary<string, int> farsiNumbers = new Dictionary<string, int>()
        {
            { "اول", 1 },
            { "دوم", 2 },
            { "سوم", 3 },
            { "چهارم", 4 },
            { "پنجم", 5 },
            { "ششم", 6 },
            { "هفتم", 7 },
            { "هشتم", 8 },
            { "نهم", 9 },
            { "دهم", 10 },
            { "یازدهم", 11 },
            { "دوازدهم", 12 },
            { "سیزدهم", 13 },
            { "چهاردهم", 14 },
            { "پانزدهم", 15 },
            { "شانزدهم", 16 },
            { "هفدهم", 17 },
            { "هجدهم", 18 },
            { "نوزدهم", 19 },
            { "بیستم", 20 }
        };

        public static string GetImageAsBase64Url(string url)
        {
            using (var client = new HttpClient())
            {
                var bytes = client.GetByteArrayAsync(url).Result;
                return "image/jpeg;base64," + Convert.ToBase64String(bytes);
            }
        }

        public static int persianNum2int(string s) => farsiNumbers.GetValueOrDefault(s.Trim(), 0);

        public static async Task<string> GetUrlHtml(string url)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage();
            request.RequestUri = new Uri(url);
            request.Method = HttpMethod.Get;

            request.Headers.Add("Accept", "*/*");
            request.Headers.Add("User-Agent", "Thunder Client (https://www.thunderclient.com)");

            var response = await client.SendAsync(request);
            return await response.Content.ReadAsStringAsync();
        }
    }

    public static class UserRoles
    {
        public const string Admin = "Admin";
        public const string User = "User";
    }
}
