using Html2Sql.Controllers;
using Html2Sql;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Html2Sql.tools;
using Microsoft.AspNetCore.Authorization;
using System.Globalization;
using trvotes.Interfaces;
using NuGet.Packaging;
using trvotes.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace trvotes.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class TrVotesController : ControllerBase
    {
        private ILogger<TrVotesController> _logger;
        private MyDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public TrVotesController(
            ILogger<TrVotesController> logger,
            MyDbContext context,
            IWebHostEnvironment environment
        )
        {
            _logger = logger;
            _context = context;
            _environment = environment;
        }

        [HttpGet]
        [Authorize]
        public async Task<IEnumerable<VotingSession>> GetAllSessions(DateTime? from, DateTime? to)
        {
            var from_ = from ?? new DateTime(0);
            var jFrom= from_.ToString("yyyy/MM/dd",new CultureInfo("fa-IR"));
            var to_ = to ?? DateTime.Now;
            var jTo=to_.ToString("yyyy/MM/dd",new CultureInfo("fa-IR"));
            return await _context.VotingSessions
                .Where(x => string.Compare( x.Jdate,  jFrom)>=0 &&string.Compare( x.Jdate , jTo)<=0)
                .ToListAsync();
        }

        [HttpGet("GetAllMembers")]
        public async Task<IEnumerable<Member>> GetAllMembers()
        {
            return await _context.Members.ToArrayAsync();
        }

        [HttpGet("GetMember")]
        public async Task<ActionResult<Member>> GetMember(int memId)
        {
            var data = await _context.Members
                .Include(x => x.Votes)
                .ThenInclude(x => x.VotingSession)
                .FirstOrDefaultAsync(x => x.MajCode == memId);
            data.Votes
                .Where(x =>
                {
                    x.ActivityName = utils.AttendanceTypeValues[(int)x.Activity];
                    return true;
                })
                .ToList();
            if (data == null)
                return NotFound();
            return Ok(data);
        }

        [HttpGet("GetMemberVotes")]
        public async Task<IEnumerable<Vote>> GetMemberVotes(
            [FromBody] int memId,
            int page,
            int pageSize
        )
        {
            var data = await _context.Votes
                .Include(a => a.VotingSession)
                .Where(x => x.MemberId == memId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToArrayAsync();
            return data;
        }

        [HttpGet("GetSession")]
        public async Task<ActionResult<VotingSession>> GetSession(int sessionId)
        {
            var data = await _context.VotingSessions
                .Include(x => x.Votes)
                .ThenInclude(x => x.Member)
                .FirstOrDefaultAsync(x => x.Id == sessionId);
            if (data == null)
                return NotFound();
            data.Votes
                .Where(x =>
                {
                    x.ActivityName = utils.AttendanceTypeValues[(int)x.Activity];
                    return true;
                })
                .OrderByDescending(x => x.Jdate)
                .ToList();
            return Ok(data);
        }

        [HttpPut("UpdateFirstVoteState")]
        public async Task<ActionResult> UpdateVoteState()
        {
            var okMems = _context.Members
                .ToList()
                .Select(x => x.MajCode)
                .ToDictionary(x => x, y => true);
            var all_Mems = _context.AllMembers.ToList();
            all_Mems.All(x =>
            {
                x.IsClarified = okMems.GetValueOrDefault(x.MajCode, false)?'t':'f';
                return true;
            });
            _context.AllMembers.UpdateRange(all_Mems);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("updateMembersState")]
        public async Task<ActionResult> UpdateAllMembersstate()
        {
            await _context.Database.ExecuteSqlRawAsync(
                "USE [master];DELETE FROM [dbo].[TmpMemberStates];"
            );

            var votes = await _context.Votes.Include(x => x.Member).ToListAsync();
            Dictionary<int, TmpMemberState> dict = new Dictionary<int, TmpMemberState>();
            foreach (var vote in votes)
                dict[vote.Member.MajCode] = new TmpMemberState { MemberId = vote.Member.MajCode };
            foreach (var vote in votes)
            {
                var t = dict[vote.Member.MajCode];
                if (vote.Activity == (int)AttendanceType.against)
                    t.Against++;
                else if (vote.Activity == (int)AttendanceType.favor)
                    t.Favor++;
                else if (vote.Activity == (int)AttendanceType.nonParticipation)
                    t.NonParticipation++;
                else if (vote.Activity == (int)AttendanceType.absence)
                    t.Absence++;
                else if (vote.Activity == (int)AttendanceType.abstaining)
                    t.Abstaining++;
            }
            var data = _context.Members
                .ToList()
                .Select(x =>
                {
                    x.TmpMemberState = dict[x.MajCode];
                    return x;
                });
            _context.Members.UpdateRange(data);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("GetAllMembersWithVotes")]
        public async Task<ActionResult> GetAllMembersWithVotes()
        {
            var data = _context.Members
                .Include(x => x.TmpMemberState)
                .ToList()
                .Select(
                    x =>
                        new IAllMembers
                        {
                            Absence = x.TmpMemberState.Absence,
                            Against = x.TmpMemberState.Against,
                            Favor = x.TmpMemberState.Favor,
                            NonParticipation = x.TmpMemberState.NonParticipation,
                            Abstaining = x.TmpMemberState.Abstaining,
                            Family = x.Family,
                            ImageUrl = x.ImageUrl,
                            IsClarified = 't',
                            MajCode = x.MajCode,
                            Name = x.Name,
                            Region = x.Region,
                            jFirstVote = x.JFirstVote,
                        }
                )
                .ToList();
            var all_Mems = _context.AllMembers.Where(x => x.IsClarified =='f')
                .ToArray()
                .Select(
                    x =>
                        new IAllMembers
                        {
                            MajCode = x.MajCode,
                            Family = x.Family,
                            IsClarified = x.IsClarified,
                            ImageUrl = x.ImageUrl,
                            Name = x.Name,
                            Region = x.Region,
                        }
                )
                .ToArray();
            data.AddRange(all_Mems);
            return Ok(data);
        }

        [HttpGet("GetFirstVotesCount")]
        public async Task<ActionResult> GetFirstVotesCount()
        {
            var data = await _context.Members
                .GroupBy(x => x.JFirstVote)
                .Select(x => new { date = x.Key ?? "", count = x.Count() })
                .OrderBy(x => x.date)
                .ToListAsync();
            if (data == null)
                return NotFound();
            return Ok(data);
        }

        [HttpGet("GetMemberImage/{id}")]
        public async Task<FileResult> GetMemberImage([FromRoute] int id)
        {
            string path = Path.Combine(_environment.WebRootPath, "images", $"{id}.jpg");
            var imageFileStream = await System.IO.File.ReadAllBytesAsync(path);
            return File(imageFileStream, "image/jpg");
        }
    }
}
