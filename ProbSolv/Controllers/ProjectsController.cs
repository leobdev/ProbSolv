using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProbSolv.Data;
using ProbSolv.Extensions;
using ProbSolv.Models;
using ProbSolv.Models.Enums;
using ProbSolv.Models.ViewModels;
using ProbSolv.Services;
using ProbSolv.Services.Interfaces;

namespace ProbSolv.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        
        private readonly IPSRolesService _rolesService;
        private readonly IPSLookupService _lookupService;
        private readonly IPSFileService _fileService;
        private readonly IPSProjectService _projectService;
        private readonly UserManager<PSUser> _userManager;
        private readonly IPSCompanyInfoService _companyInfoService;
        private readonly IPSNotificationService _notificationService;


        public ProjectsController(IPSRolesService rolesService, IPSLookupService lookupService, IPSFileService fileService, IPSProjectService projectService, UserManager<PSUser> userManager, IPSCompanyInfoService companyInfoService, IPSNotificationService notificationService)
        {

            _rolesService = rolesService;
            _lookupService = lookupService;
            _fileService = fileService;
            _projectService = projectService;
            _userManager = userManager;
            _companyInfoService = companyInfoService;
            _notificationService = notificationService;
        }



        public async Task<IActionResult> MyProjects()
        {
            
            string userId = _userManager.GetUserId(User);           

            List<Project> projects = await _projectService.GetUserProjectsAsync(userId);

            return View(projects);
        }


        public async Task<IActionResult> AllProjects()
        {

            int companyId = User.Identity.GetCompanyId().Value;

            string userId = _userManager.GetUserId(User);

            List<Project> projects = new();

            if(User.IsInRole(nameof(Roles.Admin)) || User.IsInRole(nameof(Roles.ProjectManager)))
            {
                
                projects = await _companyInfoService.GetAllProjectsAsync(companyId);
            }
            else
            {
                projects = await _projectService.GetAllProjectsByCompanyAsync(companyId);

            }

            return View(projects);
        }

        public async Task<IActionResult> ProjectsGrid()
        {

            int companyId = User.Identity.GetCompanyId().Value;

            string userId = _userManager.GetUserId(User);

            List<Project> projects = new();

            if (User.IsInRole(nameof(Roles.Admin)) || User.IsInRole(nameof(Roles.ProjectManager)))
            {

                projects = await _companyInfoService.GetAllProjectsAsync(companyId);
            }
            else
            {
                projects = await _projectService.GetAllProjectsByCompanyAsync(companyId);

            }

            return View(projects);
        }


        public async Task<IActionResult> ArchivedProjects()
        {

            int companyId = User.Identity.GetCompanyId().Value;

           List<Project> projects = await _projectService.GetArchivedProjectsByCompanyAsync(companyId);

            return View(projects);
        }

        [Authorize(Roles="Admin")]
        public async Task<IActionResult> UnassignedProjects()
        {
            int companyId = User.Identity.GetCompanyId().Value;

            List<Project> projects = await _projectService.GetUnassignedProjectsAsync(companyId);

            return View(projects);
        }


        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> AssignPM(int id)
        {
            int companyId = User.Identity.GetCompanyId().Value;

            AssignPMViewModel model = new();

            model.Project = await _projectService.GetProjectByIdAsync(id, companyId);
            model.PMList = new SelectList(await _rolesService.GetUsersInRoleAsync(nameof(Roles.ProjectManager), companyId), "Id", "FullName");

            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignPM(AssignPMViewModel model)
        {
            if (!string.IsNullOrEmpty(model.PMId))
            {
                await _projectService.AddProjectManagerAsync(model.PMId, model.Project.Id);

                return RedirectToAction(nameof(Details), new { id = model.Project.Id});
            }

            return RedirectToAction(nameof(AssignPM), new {projectId = model.Project.Id});
        }

        [Authorize(Roles = "Admin, ProjectManager")]
        [HttpGet]
        public async Task<IActionResult> AssignMembers(int id)
        {
            AssignMembersViewModel model = new();

            int companyId = User.Identity.GetCompanyId().Value;

            Project? project = await _projectService.GetProjectByIdAsync(id, companyId);

            IEnumerable<PSUser> selectedDevs = await _projectService.GetProjectMembersByRoleAsync(id, nameof(Roles.Developer));
            IEnumerable<PSUser> selectedSubs = await _projectService.GetProjectMembersByRoleAsync(id, nameof(Roles.Submitter));

            IEnumerable<PSUser> allDevs = (await _rolesService.GetUsersInRoleAsync(nameof(Roles.Developer), companyId));
            IEnumerable<PSUser> allSubs = (await _rolesService.GetUsersInRoleAsync(nameof(Roles.Submitter), companyId));

            IEnumerable<PSUser> availDevs = allDevs.Except(selectedDevs);
            IEnumerable<PSUser> availSubs = allSubs.Except(selectedSubs );

            List<string> projectMemberIds = selectedDevs
                .Concat(selectedSubs)
                .Select(member => member.Id)
                .ToList();

            model.Project = project;

            model.AllDevelopers = new MultiSelectList(allDevs, "Id", "FullName");
            model.AllSubmitters = new MultiSelectList(allSubs, "Id", "FullName");

            model.SelectedDevelopers = new MultiSelectList(selectedDevs, "Id", "FullName");
            model.SelectedSubmitters = new MultiSelectList(selectedSubs, "Id", "FullName");

            model.AvailableDevelopers = new MultiSelectList(availDevs, "Id", "FullName");
            model.AvailableSubmitters  = new MultiSelectList(availSubs, "Id", "FullName");

            return View(model);
        }

        [Authorize(Roles = "Admin, ProjectManager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignMembers(AssignMembersViewModel model)
        {
            if (ModelState.IsValid)
            {
                List<string> memberIds = (await _projectService.GetAllProjectMembersExceptPMAsync(model.Project.Id))
                    .Select(member => member.Id)
                    .ToList();

                // remove previous members from projects
                foreach (string member in memberIds)
                {
                    await _projectService.RemoveUserFromProjectAsync(member, model.Project.Id);

                }

                // add the selected members to the project
                foreach (string memberId in model.SelectedUsers ?? new List<string>())
                {
                    // if memberId is null, skip to the next memberId
                    if (memberId == null)
                        continue;

                    // add user to project
                    await _projectService.AddUserToProjectAsync(memberId, model.Project.Id);

                    // check if the user was already a member
                    /*if (!memberIds.Contains(memberId))
                    {
                        // send notification to new members
                        Notification notification = new()
                        {
                            Title = "New Project Assignment",
                            Message = $"You have been assigned to the project: {model.Project.Name}.",
                            RecipientId = memberId,
                        };

                        await _notificationService.AddNotificationAsync(notification);

                        if (!User.IsInRole(nameof(Roles.DemoUser)))
                        {
                            _ = _notificationService.SendEmailNotificationAsync(notification, notification.Title);

                        }
                    }*/

                }
                return RedirectToAction("Details", new { id = model.Project.Id });
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);

                foreach (var error in errors)
                {
                    ModelState.AddModelError("", error.ErrorMessage);
                }
                
                return View(model); // Return the view with the same model to display the errors
            }

        }
            // GET: Projects/Details/5
            public async Task<IActionResult> Details(int? id)
        {

            if (id == null )
            {
                return NotFound();
            }

            int companyId = User.Identity.GetCompanyId().Value;

            Project project = await _projectService.GetProjectByIdAsync(id.Value, companyId);

                         
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }


        // GET: Projects/Create
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> Create()
        {
            int companyId = User.Identity.GetCompanyId().Value;

            AddProjectWithPMViewModel model = new();

            //Load SelectListr with Data 
            model.PMList = new SelectList(await _rolesService.GetUsersInRoleAsync(Roles.ProjectManager.ToString(), companyId), "Id", "FullName");
            model.PriorityList = new SelectList(await _lookupService.GetProjectPrioritiesAsync(), "Id", "Name");


            return View(model);
        }

        [Authorize(Roles = "Admin, ProjectManager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AddProjectWithPMViewModel model)
        {
            if (model != null)
            {
                int companyId = User.Identity.GetCompanyId().Value;

                try
                {
                    if (model.Project.ImageFormFile != null)
                    {
                        model.Project.ImageFileData = await _fileService.ConvertFileToByteArrayAsync(model.Project.ImageFormFile);
                        model.Project.ImageFileName = model.Project.ImageFormFile.FileName;
                        model.Project.ImageContentType = model.Project.ImageFormFile.ContentType;
                    }

                    model.Project.CompanyId = companyId;

                    await _projectService.AddNewProjectAsync(model.Project);

                    //Add PM if one was chosen
                    if (!string.IsNullOrEmpty(model.PMId))
                    {
                        await _projectService.AddUserToProjectAsync(model.PMId, model.Project.Id);
                    }

                }
                catch (Exception)
                {

                    throw;
                }

                //TODO: Redirect to AllProjects
                return RedirectToAction(nameof(AllProjects));



            }
            return RedirectToAction(nameof(Create));

        }

        // GET: Projects/Edit/5
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> Edit(int? id)
        {
            int companyId = User.Identity.GetCompanyId().Value;

            AddProjectWithPMViewModel model = new();

            model.Project = await _projectService.GetProjectByIdAsync(id.Value, companyId);

            //Load SelectListr with Data 
            model.PMList = new SelectList(await _rolesService.GetUsersInRoleAsync(Roles.ProjectManager.ToString(), companyId), "Id", "FullName");
            model.PriorityList = new SelectList(await _lookupService.GetProjectPrioritiesAsync(), "Id", "Name");


            return View(model);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> Edit(AddProjectWithPMViewModel model)
        {

            if (model != null)
            {

                try
                {
                    if (model.Project.ImageFormFile != null)
                    {
                        model.Project.ImageFileData = await _fileService.ConvertFileToByteArrayAsync(model.Project.ImageFormFile);
                        model.Project.ImageFileName = model.Project.ImageFormFile.FileName;
                        model.Project.ImageContentType = model.Project.ImageFormFile.ContentType;
                    }


                    await _projectService.UpdateProjectAsync(model.Project);

                    //Add PM is one was chosen
                    if (!string.IsNullOrEmpty(model.PMId))
                    {
                        await _projectService.AddUserToProjectAsync(model.PMId, model.Project.Id);
                    }

                    //TODO: Redirect to AllProjects
                    return RedirectToAction(nameof(AllProjects));
                }
                catch (DbUpdateConcurrencyException)
                {

                    if (!await ProjectExists(model.Project.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }


            }
            return RedirectToAction(nameof(Create));
        }

        // GET: Projects/Delete/5
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> Archive(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            int companyId = User.Identity.GetCompanyId().Value;

            var project = await _projectService.GetProjectByIdAsync(id.Value, companyId);

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Delete/5
        [Authorize(Roles = "Admin, ProjectManager")]
        [HttpPost, ActionName("Archive")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveConfirmed(int id)
        {

            int companyId = User.Identity.GetCompanyId().Value;

            var project = await _projectService.GetProjectByIdAsync(id, companyId);


            await _projectService.ArchiveProjectAsync(project);

            return RedirectToAction(nameof(AllProjects));
        }


        // GET: Projects/Delete/5
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> Restore(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            int companyId = User.Identity.GetCompanyId().Value;

            var project = await _projectService.GetProjectByIdAsync(id.Value, companyId);

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Delete/5
        [Authorize(Roles = "Admin, ProjectManager")]
        [HttpPost, ActionName("Restore")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreConfirmed(int id)
        {
            int companyId = User.Identity.GetCompanyId().Value;

            var project = await _projectService.GetProjectByIdAsync(id, companyId);

            await _projectService.RestoreProjectAsync(project);

            return RedirectToAction(nameof(AllProjects));
        }


        private async Task<bool> ProjectExists(int id)
        {
            int companyId = User.Identity.GetCompanyId().Value;

            return (await _projectService.GetAllProjectsByCompanyAsync(companyId)).Any(p => p.Id == id);

        }

              
    }
}
