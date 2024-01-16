using Microsoft.AspNetCore.Mvc.Rendering;

namespace ProbSolv.Models.ViewModels
{
    public class UserRolesViewModel
    {
        public PSUser User { get; set; }

        public List<string> Roles { get; set; } = new List<string>();

        public string SelectedRoleName { get; set; } = string.Empty;

        public string SelectedUserId { get; set; } = string.Empty;

       

    }
}
