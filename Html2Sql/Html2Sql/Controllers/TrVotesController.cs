using Html2Sql.Controllers;
using Html2Sql;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Html2Sql.tools;
using Microsoft.AspNetCore.Authorization;
using System.Globalization;
using trvotes.Interfaces;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace trvotes.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class TrVotesController : ControllerBase
    {
        private ILogger<TrVotesController> _logger;
        private DataContext _context;
        private readonly IWebHostEnvironment _environment;

        public TrVotesController(
            ILogger<TrVotesController> logger,
            DataContext context,
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
            from = from ?? new DateTime(0);
            to = to ?? DateTime.Now;
            return await _context.VotingSessions
                .Where(x => x.Date >= from && x.Date <= to)
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
                .FirstOrDefaultAsync(x => x.MemId == memId);
            data.Votes
                .Where(x =>
                {
                    x.ActivityName = utils.AttendanceTypeValues[(int)x.activity];
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
                    x.ActivityName = utils.AttendanceTypeValues[(int)x.activity];
                    return true;
                })
                .ToList();
            return Ok(data);
        }

        [HttpGet("GetFirstVotesCount")]
        public async Task<ActionResult> GetFirstVotesCount()
        {
            var data = await _context.Members
                .GroupBy(x => x.jFirstVote)
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
