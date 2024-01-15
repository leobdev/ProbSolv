using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<PSUser> _userManager;
        public UserRolesController(IPSRolesService rolesService, IPSCompanyInfoService companyInfoService, UserManager<PSUser> userManager)
        {
            _rolesService = rolesService;
            _companyInfoService = companyInfoService;
            _userManager = userManager;
        }


        [HttpGet]
        public async Task<IActionResult> UserRolesIndex()
        {
            List<UserRolesViewModel> model = new();
            int companyId = User.Identity.GetCompanyId().Value;
            List<PSUser> users = await _companyInfoService.GetAllMembersAsync(companyId);

            foreach(var user in users)
            {
                UserRolesViewModel vm = new();

                IEnumerable<string> roles = await _rolesService.GetUserRolesAsync(user);

                vm.User = user;
                vm.Roles = roles.ToList();

                model.Add(vm);
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ManageUserRoles()
        {

            
            int companyId = User.Identity.GetCompanyId().Value;
            List<ManageUserRolesViewModel> model = new();

            List<PSUser> users = await _companyInfoService.GetAllMembersAsync(companyId);



            //Get CompanyId

            //Get all company users

            //Loop over the users to populate the ViewModel
            // - instantiate ViewModel
            // - use _rolesService
            // - Create multi-selects
            foreach (PSUser user in users)
            {
                ManageUserRolesViewModel viewModel = new();
                viewModel.PSUser = user;
                IEnumerable<string> selected = await _rolesService.GetUserRolesAsync(user);
                viewModel.Roles = new SelectList(await _rolesService.GetRolesAsync(), "Name", "Name", selected);

                model.Add(viewModel);
            }

            //return the model to the view

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageUserRoles(string userId, string selectedRole)
        {
            // Get the companyId
            int companyId = User.Identity.GetCompanyId().Value;

            //Instantiate the PSUser
            PSUser user = (await _companyInfoService.GetAllMembersAsync(companyId)).FirstOrDefault(u => u.Id == userId);

            //Get Roles for the user
            //IEnumerable<string> roles = await _rolesService.GetUserRolesAsync(user);

            

            //Add user to the new role
            await _rolesService.AddUserRoleAsync(user, selectedRole);

            //Navigate back to the view
            return RedirectToAction(nameof(ManageUserRoles));

        }



    }
}
