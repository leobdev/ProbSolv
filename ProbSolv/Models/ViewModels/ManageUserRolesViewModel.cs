using Microsoft.AspNetCore.Mvc.Rendering;

namespace ProbSolv.Models.ViewModels
{
    public class ManageUserRolesViewModel
    {
        public PSUser PSUser { get; set; }

        public SelectList Roles { get; set; }

        public string SelectedRole { get; set; } = string.Empty;

       
    }
}
