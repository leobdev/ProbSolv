using ProbSolv.Models;

namespace ProbSolv.Services.Interfaces
{
    public interface IPSLookupService
    {
        public Task<List<TicketPriority>> GetTicketPrioritiesAsync();

        public Task<List<TicketStatus>> GetTicketStatusesAsync();

        public Task<List<TicketType>> GetTicketTypesAsync();

        public Task<List<ProjectPriority>> GetProjectPrioritiesAsync();

        public string GetBadgeByStatusAsync(string status);
    }
}
