using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ProbSolv.Extensions;
using ProbSolv.Models;
using ProbSolv.Models.ViewModels;
using ProbSolv.Services.Interfaces;

namespace ProbSolv.Controllers
{
    public class UserRolesController : Controller
    {
        private readonly IPSRolesService _rolesService;
        private readonly IPSCompanyInfoService _companyInfoService;
        public UserRolesController(IPSRolesService rolesService, IPSCompanyInfoService companyInfoService)
        {
            _rolesService = rolesService;
            _companyInfoService = companyInfoService;
        }

        [HttpGet]
        public async Task<IActionResult> ManageUserRoles()
        {

            //Add an instance of the ViewModel as a List
            List<ManageUserRolesViewModel> model = new();

            //Get CompanyId
            int companyId = User.Identity.GetCompanyId().Value;

            //Get all company users
            List<PSUser> users = await _companyInfoService.GetAllMembersAsync(companyId);

            //Loop over the users to populate the ViewModel
            // - instantiate ViewModel
            // - use _rolesService
            // - Create multi-selects
            foreach (PSUser user in users)
            {
                ManageUserRolesViewModel viewModel = new();
                viewModel.PSUser = user;
                IEnumerable<string> selected = await _rolesService.GetUserRolesAsync(user);
                viewModel.Roles = new MultiSelectList(await _rolesService.GetRolesAsync(), "Name", "Name", selected);

                model.Add(viewModel);
            }

            //return the model to the view

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageUserRoles(ManageUserRolesViewModel member)
        {
            // Get the companyId
            int companyId = User.Identity.GetCompanyId().Value;

            //Instantiate the PSUser
            PSUser psUser = (await _companyInfoService.GetAllMembersAsync(companyId)).FirstOrDefault(u => u.Id == member.PSUser.Id);

            //Get Roles for the user
            IEnumerable<string> roles = await _rolesService.GetUserRolesAsync(psUser);

            //Grab the selected role
            string userRole = member.SelectedRoles.FirstOrDefault();

            if (!string.IsNullOrEmpty(userRole))
            {
                //remove user from his roles
                if (await _rolesService.RemoveUserFromRolesAsync(psUser, roles))
                {
                    //Add user to the new role
                    await _rolesService.AddUserRoleAsync(psUser, userRole);
                }
            }

            //Navigate back to the view
            return RedirectToAction(nameof(ManageUserRoles));

        }



    }
}
