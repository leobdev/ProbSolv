using Microsoft.AspNetCore.Mvc.Rendering;

namespace ProbSolv.Models.ViewModels
{
    public class ManageUserRolesViewModel
    {
        public PSUser PSUser { get; set; }

        public SelectList Roles { get; set; }

        public List<string> SelectedRole { get; set; } = new List<string>();

        public string SubmitedRole { get; set; }

        

    }
}
