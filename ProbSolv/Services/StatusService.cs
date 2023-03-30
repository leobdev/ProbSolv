using ProbSolv.Models.Enums;
using ProbSolv.Services.Interfaces;

namespace ProbSolv.Services
{
    public class StatusService : IStatusService
    {
        public string GetPriorityBadge(string priorityName)
        {
            //Low
            //Medium
            //High
            //Urgent

            if (priorityName == PSProjectPriority.Low.ToString() || priorityName == PSTicketPriority.Low.ToString())
            {
                return "bg-success";
            }

            if (priorityName == PSProjectPriority.Medium.ToString() || priorityName == PSTicketPriority.Medium.ToString())
            {
                return "bg-secondary";
            }

            if (priorityName == PSProjectPriority.High.ToString() || priorityName == PSTicketPriority.High.ToString())
            {
                return "bg-warning";
            }


            return "bg-danger";


        }





    }
}
