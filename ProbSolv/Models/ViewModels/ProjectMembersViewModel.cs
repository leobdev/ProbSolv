using Microsoft.AspNetCore.Mvc.Rendering;

namespace ProbSolv.Models.ViewModels
{
    public class ProjectMembersViewModel
    {
        public Project Project { get; set; }

        public MultiSelectList Developers { get; set; }

        public MultiSelectList SelectedDevelopers { get; set; }

        public MultiSelectList Submitters { get; set; }

        public MultiSelectList SelectedSubmitters { get; set; }

    }
}
