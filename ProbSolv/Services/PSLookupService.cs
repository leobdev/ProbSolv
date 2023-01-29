using Microsoft.EntityFrameworkCore;
using ProbSolv.Data;
using ProbSolv.Models;
using ProbSolv.Models.Enums;
using ProbSolv.Services.Interfaces;

namespace ProbSolv.Services
{
    public class PSLookupService : IPSLookupService
    {
        private readonly ApplicationDbContext _context;

        public PSLookupService(ApplicationDbContext context)
        {
            _context = context;
        }

        public string GetBadgeByStatusAsync(string status)
        {
            try
            {
                string badge = string.Empty;

                if (status == nameof(PSTicketStatus.New))
                {
                    badge = "bg-success";
                }
                else if (status == nameof(PSTicketStatus.Development))
                {
                    badge = "bg-primary";
                }
                else if (status == nameof(PSTicketStatus.Testing))
                {
                    badge = "bg-info";
                }
                else if (status == nameof(PSTicketStatus.Resolved))
                {
                    badge = "bg-light";
                }

                return badge;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<ProjectPriority>> GetProjectPrioritiesAsync()
        {
            try
            {
                return await _context.ProjectPriorities.ToListAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<TicketPriority>> GetTicketPrioritiesAsync()
        {
            try
            {
                return await _context.TicketPriorities.ToListAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<TicketStatus>> GetTicketStatusesAsync()
        {
            try
            {
                return await _context.TicketStatuses.ToListAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<TicketType>> GetTicketTypesAsync()
        {
            try
            {
                return await _context.TicketTypes.ToListAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
