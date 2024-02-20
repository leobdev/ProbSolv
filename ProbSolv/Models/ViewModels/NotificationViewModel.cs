using Microsoft.AspNetCore.Mvc.Rendering;

namespace ProbSolv.Models.ViewModels
{
    public class NotificationViewModel
    {
        public SelectList RecipientLists { get; set; }

        public List<Notification> Notifications { get; set; }

        public SelectList TicketsList { get; set; }

        public string RecipientId { get; set; }

        public string Message { get; set; } 

        public int? TicketId { get; set; }

        public string Subject {get; set;}


    }
}
