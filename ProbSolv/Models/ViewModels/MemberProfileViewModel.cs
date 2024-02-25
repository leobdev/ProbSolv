namespace ProbSolv.Models.ViewModels
{
    public class MemberProfileViewModel
    {
        public PSUser Member { get; set; } = new PSUser();
        public List<Project> Projects { get; set; } = new List<Project>();
        public List<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}
