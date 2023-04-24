using Microsoft.AspNetCore.Mvc.Rendering;

namespace ProbSolv.Models.ViewModels
{
    public class NotificationViewModel
    {
        public PSUser User { get; set; }

        public SelectList RecipientList { get; set; }

        public Notification Notification { get; set; }

        public Ticket Ticket { get; set; }
    }
}
