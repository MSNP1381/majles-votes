using Html2Sql.tools;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.Scaffolding.Shared.CodeModifier.CodeChange;
using Microsoft.EntityFrameworkCore;
using NuGet.DependencyResolver;
using RestSharp;
using System;
using System.Globalization;
using System.Text;
using System.Xml.Linq;
using Yaap;
using Formatting = Newtonsoft.Json.Formatting;
using Method = RestSharp.Method;

namespace Html2Sql.Controllers
{
    [ApiController]
    [Route("[controller]")]

    public class Main : ControllerBase
    {
        private string base_url = "https://trvotes.parliran.ir";
        private readonly ILogger<Main> _logger;
        private readonly DataContext _context;
        private readonly IWebHostEnvironment _environment;
        string desktopPath;

        public Main(ILogger<Main> logger, DataContext context, IWebHostEnvironment? environment)
        {
            _logger = logger;
            _context = context;
            _environment = environment;
            desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            //var l = utils.AttendanceTypeValues
            //    .Select(x => new AttendanceTypeTbl { Id = x.Key, type_value = x.Value })
            //    .ToList();
            //_context.AttendeceTypes.RemoveRange(_context.AttendeceTypes);

            //context.AddRange(l);
            //context.SaveChanges();
        }

        private AttendanceType Stat2enum(string stat)
        {
            stat = stat.Replace(" ", "").Replace("\n", "");
            switch (stat)
            {
                case "----":
                    return AttendanceType.absence;
                case "عدممشارکت":
                    return AttendanceType.nonParticipation;
                case "مخالف":
                    return AttendanceType.against;
                case "موافق":
                    return AttendanceType.favor;
                case "ممتنع":
                    return AttendanceType.abstaining;
            }
            return AttendanceType.absence;
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [NonAction]
        public static void tqdm(int index, int len, DateTime time)
        {
            var tmp_time = TimeSpan
                .FromSeconds(((DateTime.Now - time).TotalSeconds * (len - index)))
                .ToString("h'h 'm'm 's's'");
            Console.WriteLine(
                $"{index}/{len} it/s:{(1 / ((DateTime.Now - time).TotalSeconds + .00001)).ToString("F2")} time remain:{tmp_time}"
            );
        }

        [HttpPut(Name = "")]
        public async Task<ActionResult> PutGropuify()
        {
            var l = await _context.VotingSessions.ToListAsync();
            var index = 0;
            var dis = l.Select(x => x.jdate).Distinct().ToDictionary(x => x, x => index++);
            for (var i = 0; i < l.Count; i++)
            {
                l[i].group_id = dis[l[i].jdate];
            }
            _context.VotingSessions.UpdateRange(l);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("get")]
        public async Task<ActionResult> Get()
        {
            StreamReader r = new StreamReader(
                Path.Combine(_environment.WebRootPath, "Resources", "votes", "parsed.json")
            );
            string json = r.ReadToEnd();
            r.Close();
            List<Item>? items = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Item>>(json);
            var len_item = items.Count();
            var f_it = DateTime.Now;
            foreach (var i in items.Yaap())
            {
                f_it = DateTime.Now;
                //var i = items[index];
                var id = i.url.Split("/").Last();
                var jdate = i.time;
                var hdoc = new HtmlDocument();
                var html = new StreamReader(
                    Path.Combine(
                        _environment.WebRootPath,
                        "Resources",
                        "votes",
                        "pages",
                        "parsed.json"
                    )
                );
                hdoc.Load(html);
                var vote_title = hdoc.QuerySelector(
                    "#page-wrapper > div.row > div.col-lg-12 > div > div.panel-footer"
                )
                    .InnerText.s_();

                var stats = hdoc.QuerySelectorAll(".inner h3").Select(x => x.InnerText).ToArray();
                var favor = int.Parse(stats[0]);
                var against = int.Parse(stats[1]);
                var abstaining = int.Parse(stats[2]);
                var members_count = int.Parse(stats[3]);
                var rows = hdoc.DocumentNode.SelectNodes("//tr").Skip(1).ToArray();
                var all_members = new List<Member>();
                var voting_ses = new VotingSession
                {
                    Abstaining = abstaining,
                    Favor = favor,
                    Against = against,
                    title = vote_title,
                    jdate = jdate,
                    Votes = new()
                };
                int len_row = rows.Length;
                var x_it = DateTime.Now;
                for (var index_r = 0; index_r < len_row; index_r++)
                {
                    //Console.Write('\t');
                    //tqdm(index_r, len_row, x_it);
                    x_it = DateTime.Now;
                    var row = rows[index_r];

                    var tmp = row.QuerySelectorAll("th");
                    if (tmp.Count() == 0)
                        continue;
                    var img_url =
                        this.base_url + tmp[0].QuerySelector("img").GetAttributeValue("src", "");
                    var mem_id = int.Parse(img_url.Split('/').Last().Split('.')[0]);
                    var name = tmp[1].InnerText.s_();
                    var family_city = tmp[2].InnerText.s_().Split('(', ')');
                    var family = family_city[0];
                    var city = family_city[1];
                    var stat = Stat2enum(tmp[4].InnerText.s_());

                    var member = _context.Members.FirstOrDefault(
                        x =>
                            (x.Name == name && x.Family == family && city == x.Region)
                            || x.MemId == mem_id
                    );
                    if (member == null)
                    {
                        //var b64Img = utils.GetImageAsBase64Url(img_url);
                        member = new Member
                        {
                            Name = name,
                            Family = family,
                            MemId = mem_id,
                            //Image = b64Img,
                            ImageUrl = img_url,
                            Region = city,
                        };
                        var res = await _context.Members.AddAsync(member);
                        if (res.State != Microsoft.EntityFrameworkCore.EntityState.Added)
                            return StatusCode(501, member);
                    }

                    var vote = new Vote
                    {
                        activity = stat,
                        jdate = jdate,
                        Member = member
                    };
                    voting_ses.Votes.Add(vote);
                }
                await _context.AddAsync(voting_ses);
                await _context.SaveChangesAsync();
            }
            return Ok();
        }

        [HttpPost("AddFirstVoteDate")]
        public ActionResult Post()
        {
            var members_dct = _context.Members.ToDictionary(x => x.Id, x => x);
            var t = _context.Votes
                .ToList()
                .GroupBy(x => x.MemberId)
                .Select(x => x.MinBy(y => y.Date));
            var mem_date = t.Select(x =>
            {
                var tmp = members_dct[x.MemberId];
                tmp.jFirstVote = x.jdate;
                return tmp;
            });
            _context.UpdateRange(mem_date);
            _context.SaveChangesAsync();
            return Ok();
        }

        class Item
        {
            public string title { get; set; }
            public string time { get; set; }
            public string url { get; set; }
        }

        [HttpGet("GetIndex")]
        public async Task<ActionResult> get_index()
        {

            var options = new RestClientOptions("https://trvotes.parliran.ir")
            {
                MaxTimeout = -1,
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/118.0",
            };
            var client = new RestClient(options);
            var request = new RestRequest("/", Method.Post);
            request.AddHeader("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8");
            request.AddHeader("Accept-Language", "en-US,en;q=0.5");
            request.AddHeader("Accept-Encoding", "gzip, deflate, br");
            request.AddHeader("Referer", "https://trvotes.parliran.ir/");
            request.AddHeader("Origin", "https://trvotes.parliran.ir");
            request.AddHeader("DNT", "1");
            request.AddHeader("Connection", "keep-alive");
            request.AddHeader("Cookie", "__RequestVerificationToken=O0U-oVgKAhXKPUHpWrcuVWzl9V-SrSI_aOTPnhM9bRR4CdLUm87gFdG4syEMQ_vi2Txo_jAsIzr4dcbvMU5ipC_zLx8uzCO-gfylKp4A1aE1; AHAS=dxo3trt0fzfr2342se2rrtuz");
            request.AddHeader("Upgrade-Insecure-Requests", "1");
            request.AddHeader("Sec-Fetch-Dest", "document");
            request.AddHeader("Sec-Fetch-Mode", "navigate");
            request.AddHeader("Sec-Fetch-Site", "same-origin");
            request.AddHeader("Sec-Fetch-User", "?1");
            request.AddHeader("Sec-GPC", "1");
            request.AlwaysMultipartFormData = true;
            request.AddParameter("__RequestVerificationToken", "6vJt-f-r_oVSrM4F36DYj6V43DQvTzljggcYbAcNeBpWUxk0UFGWaiYQpLt9wlE5T15Q0zNjPNx4jHE0sah0rtRCKNIKO1LmvMQw8qBBUG81");
            var today = DateTime.Now.ToString("yyyy/MM/dd", new CultureInfo("fa-IR"));

            request.AddParameter("StartTime", "1401/06/12");
            request.AddParameter("EndTime", today);
            request.AddParameter("myTable_length", "100");

            RestResponse response = await client.ExecuteAsync(request);
            using (
                StreamWriter sw =
                    new(
                       path: Path.Combine(
                            _environment.WebRootPath,
                            "Resources",
                            "votes",
                            "index.html"
                        )
                       ,
                       append: false
                        , encoding: Encoding.UTF8
                    )
            )
            {
                var res = response.Content;


                await sw.WriteAsync(res);
            }

            return Ok();
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("Index2Json")]
        public async Task<ActionResult> index2Json(
            string from = "1401/12/21",
            string to = "1500/01/01"
        )
        {
            var from_ = from.persianDate2utc();
            var to_ = to.persianDate2utc();
            using (
                var html = new StreamReader(
                    Path.Combine(_environment.WebRootPath, "Resources", "votes", "index.html")
                )
            )
            {
                var hdoc = new HtmlDocument();
                hdoc.Load(html);
                var vote_sessions = hdoc.QuerySelectorAll("tr").Skip(1).ToList();
                var data = vote_sessions
                    .Select(x =>
                    {
                        var data = x.QuerySelectorAll("td").ToArray();
                        var title = data[0].InnerText.s_();
                        var time = data[1].InnerText.s_();
                        var url = base_url + data[2].GetAttributeValue("href", "");

                        return new
                        {
                            title,
                            time,
                            url,
                            date_ = time.persianDate2utc()
                        };
                    })
                    .Where(x => x.date_ >= from_ && x.date_ <= to_)
                    .ToArray();

                var json = Newtonsoft.Json.JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented);
                using (
                    var jsonsw = new StreamWriter(
                        Path.Combine(_environment.WebRootPath, "Resources", "votes", "parsed.json")
                    )
                )

                    await jsonsw.WriteAsync(json);
            return Ok(json);
            }
        }
    }
}
