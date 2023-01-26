using System.ComponentModel.DataAnnotations;

namespace ProbSolv.Models.Enums
{
    public enum PSTicketType
    {
        [Display(Name = "New Development")]
        NewDevelopment,
        [Display(Name = ("Work Task"))]
        WorkTask,
        [Display(Name = "Defect")]
        Defect,
        [Display(Name = "Change Request")]
        ChangeRequest,
        [Display(Name = "Enhancement")]
        Enhancement,
        [Display(Name = "General Task")]
        GeneralTask
    }
}
