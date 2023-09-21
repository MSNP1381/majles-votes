using Html2Sql.Controllers;
using Html2Sql;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Html2Sql.tools;
using Microsoft.AspNetCore.Authorization;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace trvotes.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    [Authorize]
    public class TrVotesController : ControllerBase
    {
        private ILogger<Main> _logger;
        private DataContext _context;
        
        public TrVotesController(ILogger<Main> logger, DataContext context)
        {
            _logger = logger;
            _context = context;
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
            var data = await _context.Members.FirstOrDefaultAsync(x => x.MemId == memId);
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
            var data = await _context.Votes
                .Include(x => x.Member)
                .Where(x => x.VotingSessionId == sessionId)
                .ToListAsync();
            if (data == null)
                return NotFound();
            data= data.Where(x =>
            {
                x.ActivityName = utils.AttendanceTypeValues[(int)x.activity];
                return true;
            }).ToList();
            return Ok(data);
        }
    }
}
