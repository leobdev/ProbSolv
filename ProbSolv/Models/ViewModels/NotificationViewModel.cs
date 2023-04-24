using Microsoft.AspNetCore.Mvc.Rendering;

namespace ProbSolv.Models.ViewModels
{
    public class NotificationViewModel
    {
        public List<PSUser> Users { get; set; }

        public SelectList RecipientLists { get; set; }

        public List<Notification> Notifications { get; set; }

        public SelectList Projects { get; set; }
    }
}
