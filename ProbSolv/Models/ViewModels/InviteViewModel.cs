using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ProbSolv.Models.ViewModels
{
    public class InviteViewModel
    {
        [DisplayName("First Name")]
        public string FirstName { get; set; }

        [DisplayName("Last Name")]
        public string LastName { get; set; }

        
        public string Email { get; set; }

        public string Message { get; set; }

        public SelectList ProjectList { get; set; }

        [DisplayName("Project")]
        public int ProjectId { get; set; }

    }
}
