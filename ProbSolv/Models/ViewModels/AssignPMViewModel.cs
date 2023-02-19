using Microsoft.AspNetCore.Mvc.Rendering;

namespace ProbSolv.Models.ViewModels
{
    public class AssignPMViewModel
    {
        public Project Project { get; set; }

        public SelectList PMList { get; set; }

        public string PMId { get; set; }

    }
}
