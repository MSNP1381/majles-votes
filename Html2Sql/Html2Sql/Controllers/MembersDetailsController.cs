using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using Html2Sql.tools;
using Newtonsoft.Json;
using MongoDB.Driver;
using Yaap;
using System.Drawing;
using Microsoft.OpenApi.Extensions;
using MongoDB.Bson;
using System.IO;


namespace Html2Sql.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MembersDetailsController : ControllerBase
    {

        private const string base_url = "https://www.parliran.ir";
        private readonly IMongoCollection<MemeberDetails> _context;

        public MembersDetailsController()
        {
            var mongoClient = new MongoClient(
                "mongodb://localhost:27017");

            var mongoDatabase = mongoClient.GetDatabase("majles");




            _context = mongoDatabase.GetCollection<MemeberDetails>("membersDetails");
        }

        public class RawMember { public int index = 0; public string url; public List<BoardType> BoardType = new(); public List<int> BoardYear = new(); }

        private int year2int(string year)
        {
            if (year.Contains("اول")) return 1;
            if (year.Contains("دوم")) return 2;
            if (year.Contains("سوم")) return 3;
            return 4;
        }
        private Education educationText2obj(string s)
        {
            s = s.Trim();
            bool is_grad = true;
            if (s.Contains("دانشجو"))
            {
                is_grad = false;
            }
            if (s.Contains("ارشد") || (s.Contains("فوق") && s.Contains("لیسانس")))
            {
                return new Education { Level = EducationLevel.master, educationName = s, is_graduated = is_grad };
            }
            if (s.Contains("دکتر"))
            {
                return new Education { Level = EducationLevel.phd, educationName = s, is_graduated = is_grad };
            }
            else
            {
                return new Education { Level = EducationLevel.bachelor, educationName = s, is_graduated = is_grad };
            }
        }
        private BoardType title2BoardType(string title)
        {
            if (title.Contains("رئیس")) return BoardType.President;
            if (title.Contains("نائب رئیس اول")) return BoardType.VicePresident;
            if (title.Contains("نائب رئیس دوم")) return BoardType.SecondVicePresident;
            return BoardType.BoardDirector;
        }
        private List<RawMember> GetRawMembers()
        {
            var members = new List<RawMember>();
            var hdoc = new HtmlDocument();
            var html = new StreamReader(@"C:\Users\muhammadS\Desktop\majles\all_members\index.html");
            hdoc.Load(html);
            html.Close();
            //members
            var membersEl = hdoc.QuerySelector(".inner-sec__content.pb-0.pt-0");
            var mem_contents = membersEl.QuerySelectorAll(".members__item");
            foreach (var member in mem_contents)
            {
                var mem_id = member.QuerySelector("img")
                    .GetAttributeValue("src", "")
                    .Split("/").Last().Split(".")[0].toInt32();

                var url = base_url + member.QuerySelector("a").GetAttributeValue("href", "") ?? "";
                var mem = new RawMember { index = mem_id, url = url, BoardType = new(), BoardYear = new() };
                members.Add(mem);
            }
            //boards
            var yearIds = hdoc.GetElementbyId("pills-tab").QuerySelectorAll("a").Select(x =>
            new
            {
                id = x.GetAttributeValue("href", "").Replace("#", ""),
                text = x.InnerText.s_(),
                yearNo = year2int(x.InnerText.s_())
            });
            foreach (var year in yearIds)
            {
                var yearMembers = hdoc.GetElementbyId(year.id);
                var yearMemContents = yearMembers.QuerySelectorAll(".members__item");
                foreach (var member in yearMemContents)
                {
                    var url = base_url + member.QuerySelector("a").GetAttributeValue("href", "") ?? "";
                    var title = member.QuerySelector("div.members__title.text__14.text__medium > span");
                    var title_s = title != null ? title.InnerText.s_() : "";

                    var cond = members.FirstOrDefault(w => w.url == url);
                    if (cond == null)
                    {
                        var mem = new RawMember
                        {
                            url = url,
                            BoardType = new List<BoardType> { title2BoardType(title_s) },
                            BoardYear = new List<int> { year.yearNo }
                        };
                        members.Add(mem);
                    }
                    else
                    {

                        cond.BoardType.Append(title2BoardType(title_s));
                        cond.BoardYear.Append(year.yearNo);
                    }
                }
            }
            return members;
        }


        [HttpGet("AllMembers2Html")]
        public async Task<List<RawMember>> GetAsync()
        {
            var raw = GetRawMembers();
            var parsed = JsonConvert.SerializeObject(raw);
            var sw1 = new StreamWriter(@"C:\Users\muhammadS\Desktop\majles\all_members\parsed.json");
            sw1.Write(parsed);
            sw1.Close();
            foreach (var item in raw.Yaap(total: raw.Count))
            {
                using (var sw2 = new StreamWriter($"C:\\Users\\muhammadS\\Desktop\\majles\\all_members\\pages\\{item.index}.html"))
                {

                    var result = await utils.GetUrlHtml(item.url);
                    result = result.Replace("thirdYear", "سال سوم");
                    sw2.Write(result);

                }
            }

            return raw;
        }

        [HttpGet("AddAllMembers2Mongo")]
        public void Get(bool do_drop = false)
        {
            if (do_drop) _context.Database.DropCollection("membersDetails");
            var keys = new List<string>();
            var json = new StreamReader(@"C:\Users\muhammadS\Desktop\majles\all_members\parsed.json");
            var keysw = new StreamWriter(@"C:\Users\muhammadS\Desktop\x.json");
            var t = JsonConvert.DeserializeObject<List<RawMember>>(json.ReadToEnd());
            json.Close();

            foreach (var raw in t.Yaap(total: t.Count))
            {

                YaapConsole.WriteLine($"{raw.index}");
                var member = new MemeberDetails();
                member.Url = raw.url;
                var hdoc = new HtmlDocument();
                var html = new StreamReader
                    ($"C:\\Users\\muhammadS\\Desktop\\majles\\all_members\\pages\\{raw.index}.html");
                hdoc.Load(html);
                html.Close();
                member.MemId = raw.index;

                var hists=raw.BoardYear.Zip(raw.BoardType).Select(x =>
                new BoardMember
                {
                    BoardYear = x.First,
                    BoardType = x.Second
                });
                member.BoardHist.AddRange(hists);
                member.FullName = hdoc.QuerySelector(".agent-data__title").InnerText.s_();
                member.ChooseRegion = hdoc.QuerySelector(".agent-data__position")
                                  .InnerText.Replace("نماینده", "").s_();
                var dataDict_tmp = hdoc.QuerySelectorAll(".agent-data__list .item")
                               .Select(x => new { k = x.css2text(".label"), v = x.css2text(".data") }).ToList();
                var dataDict = dataDict_tmp.ToDictionary(x => x.k, x => x.v);
                keys.AddRange(dataDict.Keys.ToList());
                var tmp_religion = "";
                if (dataDict.TryGetValue("دین و مذهب", out tmp_religion))
                    member.Religion = tmp_religion;
                member.jBirth = dataDict.GetValueOrDefault("تاریخ تولد", "");
                member.BirthPlace = dataDict.GetValueOrDefault("محل تولد", "");
                member.Educations = dataDict.GetValueOrDefault("تحصیلات دانشگاهی", "").Split('،').Select(x => educationText2obj(x)).ToList();
                member.jcertified = dataDict.GetValueOrDefault("تاریخ انتخاب", "");
                member.ChooseState = dataDict.GetValueOrDefault("مرحله انتخاب", "");
                var hist = dataDict.GetValueOrDefault("سابقه نمایندگی", "");
                if (hist != "")
                {
                    var histsAll = hist.Replace("دوره", "").s_().Split(" ").Select(x =>
                         utils.persianNum2int(x.s_())
                   ).ToList();
                    member.History.AddRange(histsAll);
                }

                var vote_details = new int[] { 0, 0 };
                var vote_details_l = dataDict
                    .GetValueOrDefault("آراء ماخوذ", "")
                    .Replace(".", "")
                    .Split("از");
                if (vote_details_l.Length == 2)
                    vote_details = vote_details_l.Select(x => x.toInt32()).ToArray();
                member.VotesRecived = vote_details[0];
                member.VotesTotal = vote_details[1];
                member.jcertified = dataDict.GetValueOrDefault("تاریخ تصویب اعتبار نامه", "");
                //memberships
                var memberships = hdoc.QuerySelectorAll(".group-sec__content");
                var memberShipList = new List<Membership>();
                for (int i = 0; i < memberships.Count; i++)
                {
                    var membership = memberships[i];
                    var items = membership.QuerySelectorAll(".group-sec__item").Select(x =>
                    new Membership
                    {
                        Title = x.css2text(".group-sec__title"),
                        Year = i == 0 ? utils.persianNum2int(x
                        .css2text(".group-sec__subtitle")
                        .Replace("سال", "")
                        .Replace("دوره", "")) : 0,
                        Type = (MembershipType)i,
                        Url = base_url + x.QuerySelector("a").GetAttributeValue("href", ""),
                    });
                    memberShipList.AddRange(items);
                }
                member.Memberships = memberShipList;
                member.Id = ObjectId.GenerateNewId();
                _context.InsertOne(member);
            }
            keys = keys.Distinct().ToList();
            var _3 = JsonConvert.SerializeObject(keys);
            keysw.Write(_3);
            keysw.Close();
            return;
        }

        [HttpGet("NewsAndSpeeches2Html")]
        public async Task<int> GetNews()
        {
            var json = new StreamReader(@"C:\Users\muhammadS\Desktop\majles\all_members\parsed.json");

            var t = JsonConvert.DeserializeObject<List<RawMember>>(json.ReadToEnd());
            json.Close();

            foreach (var raw in t.Yaap(total: t.Count))
            {

                var hdoc = new HtmlDocument();
                var html = new StreamReader
                    ($"C:\\Users\\muhammadS\\Desktop\\majles\\all_members\\pages\\{raw.index}.html");
                hdoc.Load(html);
                html.Close();

                var news = hdoc.QuerySelectorAll(".news-collection__header");
                if (news != null)
                {
                    var d = news.QuerySelectorAll(".news-collection__header").Select(x =>
                    new
                    {
                        name = x.QuerySelector(".news-collection__main-title").InnerText.s_(),
                        url = base_url + x.QuerySelector("a").GetAttributeValue("href", "")
                    }).ToDictionary(x => x.name, x => x.url);

                    var speeches_url = d.GetValueOrDefault("نطق‌ها و مصاحبه‌ها", "");
                    var news_url = d.GetValueOrDefault("اخبار", "");
                    var s = $"written : {raw.index}";
                    if (speeches_url != "")
                    {
                        using (var sw = new StreamWriter($"C:\\Users\\muhammadS\\Desktop\\majles\\all_members\\speeches\\{raw.index}.html"))
                        {
                            try
                            {
                                var result = await utils.GetUrlHtml(speeches_url);
                                await sw.WriteAsync(result);
                                s = "speeches & " + s;
                            }
                            catch
                            {
                                Console.BackgroundColor = ConsoleColor.Red;
                                Console.WriteLine($"{raw.index}: speeches");
                                Console.BackgroundColor = ConsoleColor.Black;

                            }
                        }
                        if (news_url != "")
                        {
                            using (var sw = new StreamWriter($"C:\\Users\\muhammadS\\Desktop\\majles\\all_members\\news\\{raw.index}.html"))
                            {
                                try
                                {
                                    var result = await utils.GetUrlHtml(news_url);
                                    await sw.WriteAsync(result);
                                    s = "news & " + s;
                                }
                                catch
                                {
                                    Console.BackgroundColor = ConsoleColor.Red;
                                    Console.WriteLine($"{raw.index}: news");
                                    Console.BackgroundColor = ConsoleColor.Black;

                                }
                            }
                        }
                        YaapConsole.WriteLine(s);
                    }
                }


            }
            return 0;
        }
        [HttpPut("AddNewsAndSpeeches2sql")]
        public async Task<int> AddNewsSpeeches()
        {
            Console.Clear();
            //news
            var ls = Directory.GetFiles($"C:\\Users\\muhammadS\\Desktop\\majles\\all_members\\news");
            foreach (var i in ls.Yaap(settings: new YaapSettings
            {
                Description = "news",
                Width = 100,
                ColorScheme = YaapColorScheme.Bright,
                SmoothingFactor = 0.5,
            }))
            {
                var hdoc = new HtmlDocument();
                var html = new StreamReader(i);
                hdoc.Load(html);
                html.Close();
                var mem_id = Path.GetFileName(i).Split(".")[0].toInt32();
                var news = hdoc.QuerySelectorAll(".news-item__content").Select(x => new
                News_Speeches
                {
                    jDate = x.QuerySelector("li.text__12.text__gray").InnerText.s_(),
                    Title = x.QuerySelector("a.news-item__title").InnerText.s_(),
                    Url = base_url + x.QuerySelector("a.news-item__title").GetAttributeValue("href", ""),
                    Desc = x.QuerySelector(".news-item__lead").InnerText.s_(),
                }).ToList();
                YaapConsole.WriteLine($"{mem_id} : {news.Count}");
                var update = Builders<MemeberDetails>
                .Update
                .Set(rec => rec.News, news);
                 _context.FindOneAndUpdate(x => x.MemId == mem_id, update);
            }


            //Speeches  
            ls = Directory.GetFiles($"C:\\Users\\muhammadS\\Desktop\\majles\\all_members\\speeches");
            foreach (var i in ls.Yaap(settings: new YaapSettings
            {
                Description = "speeches",
                Width = 100,
                ColorScheme = YaapColorScheme.Bright,
                SmoothingFactor = 0.5,
            }))
            {
                var hdoc = new HtmlDocument();
                var html = new StreamReader(i);
                hdoc.Load(html);
                html.Close();
                var mem_id = Path.GetFileName(i).Split(".")[0].toInt32();
                var speeches = hdoc.QuerySelectorAll(".news-item__content").Select(x => new
                News_Speeches
                {
                    jDate = x.QuerySelector("li.text__12.text__gray").InnerText.s_(),
                    Title = x.QuerySelector("a.news-item__title").InnerText.s_(),
                    Url = base_url + x.QuerySelector("a.news-item__title").GetAttributeValue("href", ""),
                    Desc = ""
                }).ToList();
                YaapConsole.WriteLine($"{mem_id} : {speeches.Count}");
                var update = Builders<MemeberDetails>
                .Update
                .Set(rec => rec.Speeches, speeches);
                 _context.FindOneAndUpdate(x => x.MemId == mem_id, update);
            }
            return 0;
        }
    }
}
