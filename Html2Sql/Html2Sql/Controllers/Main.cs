using Html2Sql.tools;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

using Yaap;

namespace Html2Sql.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class Main : ControllerBase
    {
        private string base_url = "https://trvotes.parliran.ir";
        private readonly ILogger<Main> _logger;
        private readonly DataContext _context;

        public Main(ILogger<Main> logger, DataContext context)
        {
            _logger = logger;
            _context = context;

            var l = utils.AttendanceTypeValues
                .Select(x => new AttendanceTypeTbl { Id = x.Key, type_value = x.Value })
                .ToList();
            _context.AttendeceTypes.RemoveRange(_context.AttendeceTypes);

            context.AddRange(l);
            context.SaveChanges();
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

        [HttpGet(Name = "get")]
        public async Task<ActionResult> Get()
        {
            var st = new StreamWriter(@"C:\Users\muhammadS\Desktop\x.txt");
            StreamReader r = new StreamReader(@"C:\Users\muhammadS\Desktop\majles\parsed.json");
            string json = r.ReadToEnd();
            r.Close();
            List<Item>? items = JsonConvert.DeserializeObject<List<Item>>(json);
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
                    @"C:\Users\muhammadS\Desktop\majles\pages\" + id + ".html"
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
                    st.WriteLine($"{tmp[4].InnerText.s_()} => {Stat2enum(tmp[4].InnerText.s_())}");
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

        [HttpPost(Name = "AddFirstVoteDate")]
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
    }
}
