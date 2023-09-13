using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using Html2Sql.tools;
using System.Drawing.Drawing2D;
using System.Security.Policy;
using Newtonsoft.Json;
using MongoDB.Driver;
using System.Collections.Generic;
using MongoDB.Bson;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Html2Sql.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MembersDetailsController : ControllerBase
    {

        private string base_url = "https://www.parliran.ir";
        private readonly ILogger<Main> _logger;
        private readonly IMongoCollection<MemeberDetails> _context;
        public MembersDetailsController(ILogger<Main> logger)
        {

            _logger = logger;

            var mongoClient = new MongoClient(
                "mongodb://localhost:27017");

            var mongoDatabase = mongoClient.GetDatabase("majles");

            _context = mongoDatabase.GetCollection<MemeberDetails>("membersDetails");
        }


        public class RawMember { public int index = 0; public string url; public List<BoardType>? BoardType; public List<int>? BoardYear; }

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
            if (s.Contains("ارشد"))
            {
                return new Education(EducationLevel.master, s, is_grad);
            }
            if (s.Contains("دکتر"))
            {
                return new Education(EducationLevel.phd, s, is_grad);
            }
            else
            {
                return new Education(EducationLevel.bachelor, s, is_grad);
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
            //members
            var membersEl = hdoc.QuerySelector(".inner-sec__content.pb-0.pt-0");
            var mem_contents = membersEl.QuerySelectorAll(".members__item");
            foreach (var member in mem_contents)
            {
                var mem_id = member.QuerySelector("img")
                    .GetAttributeValue("src", "")
                    .Split("/").Last().Split(".")[0].toInt32();

                var url = base_url + member.QuerySelector("a").GetAttributeValue("href", "") ?? "";
                var mem = new RawMember { index = mem_id, url = url, BoardType = null, BoardYear = null };
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
                    if (cond.BoardYear == null)
                    {
                        var mem = new RawMember
                        {
                            url = url,
                            BoardType = new List<BoardType> { title2BoardType(title_s) },
                            BoardYear = new List<int> { year.yearNo }
                        };
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

            var dx = DateTime.Now;
            for (var index = 0; index < raw.Count; index++)
            {
                using (var sw2 = new StreamWriter($"C:\\Users\\muhammadS\\Desktop\\majles\\all_members\\pages\\{raw[index].index}.html"))
                {
                    Main.tqdm(index, raw.Count, dx);
                    dx = DateTime.Now;

                    var client = new HttpClient();
                    var request = new HttpRequestMessage();
                    request.RequestUri = new Uri(raw[index].url);
                    request.Method = HttpMethod.Get;

                    request.Headers.Add("Accept", "*/*");
                    request.Headers.Add("User-Agent", "Thunder Client (https://www.thunderclient.com)");

                    var response = await client.SendAsync(request);
                    var result = await response.Content.ReadAsStringAsync();
                    sw2.Write(result);

                }
            }
            return raw;
        }

        [HttpGet("AddAllMembers2Sql")]
        public void Get()
        {
            var keys = new List<List<string>>();
            var json = new StreamReader(@"C:\Users\muhammadS\Desktop\majles\all_members\parsed.json");
            //var keysw = new StreamWriter(@"C:\Users\muhammadS\Desktop\x.json");
            var t = JsonConvert.DeserializeObject<List<RawMember>>(json.ReadToEnd());
            var t_dt = DateTime.Now;
            for (var t_index = 0; t_index < t.Count; t_index++)
            {
                Main.tqdm(t_index + 1, t.Count, t_dt);
                t_dt = DateTime.Now;
                var raw = t[t_index];
                Console.WriteLine($"{ raw.index}");
                var member = new MemeberDetails();
                member.Url = raw.url;
                var hdoc = new HtmlDocument();
                var html = new StreamReader
                    ($"C:\\Users\\muhammadS\\Desktop\\majles\\all_members\\pages\\{raw.index}.html");
                hdoc.Load(html);
                member.BoardType = raw.BoardType ?? new();
                member.BoardYear = raw.BoardYear ?? new();
                member.FullName = hdoc.QuerySelector(".agent-data__title").InnerText.s_();
                member.ChooseRegion = hdoc.QuerySelector(".agent-data__position")
                                  .InnerText.Replace("نماینده", "").s_();
                var dataDict_tmp = hdoc.QuerySelectorAll(".agent-data__list .item")
                               .Select(x => new { k = x.css2text(".label"), v = x.css2text(".data") }).ToList();
                        var dataDict   = dataDict_tmp.ToDictionary(x => x.k, x => x.v);
                keys.Append( dataDict.Keys.ToList());
                var _3=JsonConvert.SerializeObject(keys);
                //keysw.Write(_3);
                member.religious = dataDict.GetValueOrDefault("", "");


                member.jBirth = dataDict.GetValueOrDefault("تاریخ تولد", "");
                member.BirthPlace = dataDict.GetValueOrDefault("محل تولد", "");
                member.Education = dataDict.GetValueOrDefault("تحصیلات دانشگاهی", "").Split('،').Select(x => educationText2obj(x)).ToList();
                member.jcertified = dataDict.GetValueOrDefault("تاریخ انتخاب", "");
                member.ChooseState = dataDict.GetValueOrDefault("مرحله انتخاب", "");
                var vote_details = new int[] { 0, 0 };
                var vote_details_l = dataDict
                    .GetValueOrDefault("آراء ماخوذ", "")
                    .Replace(".", "")
                    .Split("از");
                    if (vote_details_l.Length == 2)
                    vote_details=vote_details_l.Select(x => x.toInt32()).ToArray();
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
                //using (var sw = new StreamWriter(@"C:\Users\muhammadS\Desktop\tmp.txt"))
                //{
                //    var _ = JsonConvert.SerializeObject(member, Formatting.Indented);
                //    sw.Write(_);
                //    //Console.Clear();
                //    //Console.WriteLine(_);
                //}

            }
        }





    }
}
