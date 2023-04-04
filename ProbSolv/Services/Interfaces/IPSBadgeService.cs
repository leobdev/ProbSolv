using Microsoft.AspNetCore.Html;

namespace ProbSolv.Services.Interfaces
{
    public interface IPSBadgeService
    {
        public string GetPriorityBadge(string priorityName);

        public string GetFonticonByHistory(string  historyProperty);

        public string GetFunticonColorByHistory(string historyProperty);

    }
}
