using System.ComponentModel;

namespace ProbSolv.Models
{
    public class Company
    {
        public int Id { get; set; } 

        [DisplayName("Company Name")]
        public string Name { get; set; }

        [DisplayName("Company Description")]
        public string Description { get; set; }    



        //navigation properties

        public virtual ICollection<Project> Projects { get; set; } = new HashSet<Project>();

        public virtual ICollection<PSUser> Members { get; set; } = new HashSet<PSUser>();

        public virtual ICollection<Invite> Invites { get; set; } = new HashSet<Invite>();
    }
}
