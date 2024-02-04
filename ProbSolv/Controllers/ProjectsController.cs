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


        public ProjectsController( IPSRolesService rolesService, IPSLookupService lookupService, IPSFileService fileService, IPSProjectService projectService, UserManager<PSUser> userManager, IPSCompanyInfoService companyInfoService)
        {
            
            _rolesService = rolesService;
            _lookupService = lookupService;
            _fileService = fileService;
            _projectService = projectService;
            _userManager = userManager;
            _companyInfoService = companyInfoService;
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
        public async Task<IActionResult> AssignPM(int projectId)
        {
            int companyId = User.Identity.GetCompanyId().Value;

            AssignPMViewModel model = new();

            model.Project = await _projectService.GetProjectByIdAsync(projectId, companyId);
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
        public async Task<IActionResult> AssignMembers(int projectId)
        {
            ProjectMembersViewModel model = new();

            int companyId = User.Identity.GetCompanyId().Value;

            model.Project = await _projectService.GetProjectByIdAsync(projectId, companyId);


            IEnumerable<PSUser> projectDevs = await _projectService.GetProjectMembersByRoleAsync(projectId, nameof(Roles.Developer));
            IEnumerable<PSUser> projectSubms = await _projectService.GetProjectMembersByRoleAsync(projectId, nameof(Roles.Submitter));

            IEnumerable<PSUser> developers = (await _rolesService.GetUsersInRoleAsync(nameof(Roles.Developer), companyId)).Except(projectDevs);
            IEnumerable<PSUser> submitters = (await _rolesService.GetUsersInRoleAsync(nameof(Roles.Submitter), companyId)).Except(projectSubms);

            model.Developers = new MultiSelectList(developers, "Id", "FullName", projectDevs);
            model.Submitters = new MultiSelectList(submitters, "Id", "FullName", projectSubms);

            model.SelectedDevelopers = new MultiSelectList(developers, "Id", "FullName", projectDevs);
            model.SelectedSubmitters = new MultiSelectList(submitters, "Id", "FullName", projectSubms);

            return View(model);
        }

        [Authorize(Roles = "Admin, ProjectManager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignMembers(ProjectMembersViewModel model)
        {
            if(model.SelectedDevelopers != null)
            {

                foreach(var member in model.Developers)
                {
                    await _projectService.RemoveUserFromProjectAsync(member.Text, model.Project.Id);
                }

                foreach(var member in model.SelectedDevelopers)
                {
                    await _projectService.AddUserToProjectAsync(member.Text, model.Project.Id);
                }

                return RedirectToAction("Details", "Projects", new { id = model.Project.Id });

            }

            if (model.SelectedSubmitters != null)
            {
                foreach (var member in model.Submitters)
                {
                    await _projectService.RemoveUserFromProjectAsync(member.Text, model.Project.Id);
                }

                foreach (var member in model.SelectedSubmitters)
                {
                    await _projectService.AddUserToProjectAsync(member.Text, model.Project.Id);
                }

                return RedirectToAction("Details", "Projects", new { id = model.Project.Id });

            }

            return RedirectToAction(nameof(AssignMembers), new {projectId = model.Project.Id});

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
