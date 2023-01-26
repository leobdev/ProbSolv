using System.ComponentModel;

namespace ProbSolv.Models
{
    public class ProjectPriority
    {
        public int Id { get; set; }

        [DisplayName("Project Name")]
        public string Name { get; set; }

    }
}
