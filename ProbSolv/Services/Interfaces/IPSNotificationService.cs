using ProbSolv.Models;

namespace ProbSolv.Services.Interfaces
{
    public interface IPSNotificationService
    {

        public Task AddNotificationAsync(Notification notification);

        public Task<List<Notification>> GetReceivedNotificationsAsync(string userId);

        public Task<List<Notification>> GetSentNotificationsAsync(string userId);

        public Task SendEmailNotificationsByRoleAsync(Notification notification, int companyId, string role);

        public Task SendMembersEmailNotificationsAsync(Notification notification, List<PSUser> members);

        public Task<bool> SendEmailNotificationAsync(Notification notification, string emailSubject);

        public Task<List<Notification>> GetUserNotificationsAsync(string userId);

        public Task MarkAsNewAsync(Notification notification);
        
    }
}
