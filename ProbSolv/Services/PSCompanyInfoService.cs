using Microsoft.EntityFrameworkCore;
using ProbSolv.Data;
using ProbSolv.Models;
using ProbSolv.Services.Interfaces;
using System.ComponentModel.Design;

namespace ProbSolv.Services
{
    public class PSCompanyInfoService : IPSCompanyInfoService
    {

        private readonly ApplicationDbContext _context;

        public PSCompanyInfoService(ApplicationDbContext context)
        {
            _context = context;
        }


        public async Task<List<PSUser>> GetAllMembersAsync(int companyId)
        {
            List<PSUser> result = new List<PSUser>();

            result = await _context.Users.Where(x => x.CompanyId == companyId).ToListAsync();

            return result;
        }

        public async Task<List<Project>> GetAllProjectsAsync(int companyId)
        {
            List<Project> result = new List<Project>();

            result = await _context.Projects.Where(x => x.CompanyId == companyId)
                                            .Include(x => x.Members)
                                            .Include(x => x.Tickets)
                                                .ThenInclude(t => t.Comments)
                                            .Include(x => x.Tickets)
                                                .ThenInclude(t => t.TicketStatus)
                                            .Include(x => x.Tickets)
                                                .ThenInclude(t => t.TicketPriority)
                                            .Include(x => x.Tickets)
                                                .ThenInclude(t => t.TicketType)
                                            .Include(x => x.Tickets)
                                                .ThenInclude(t => t.Histories)
                                            .Include(x => x.Tickets)
                                                .ThenInclude(t => t.DeveloperUser)
                                            .Include(x => x.Tickets)
                                                .ThenInclude(t => t.OwnerUser)
                                            .Include(x => x.Tickets)
                                                .ThenInclude(t => t.Notifications)
                                            .Include(x => x.ProjectPriority)
                                            .ToListAsync();

            return result;

        }

        public async Task<List<Ticket>> GetAllTicketsAsync(int companyId)
        {
            List<Ticket> result = new List<Ticket>();

            List<Project> projects = new List<Project>();

            projects = await GetAllProjectsAsync(companyId);

            result = projects.SelectMany(x => x.Tickets).ToList();

            return result;
        }

        public async Task<Company> GetCompanyInfoByIdAsync(int? companyId)
        {
            Company result = new();

            if (companyId is not null)
            {
                result = await _context.Companies
                    .Include(x => x.Members)
                    .Include(x => x.Projects)
                .Include(x => x.Invites)
                    .FirstOrDefaultAsync(x => x.Id == companyId);
            }

            return result;
        }
    }
}
