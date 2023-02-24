using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection; //new//
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProbSolv.Data;
using ProbSolv.Extensions;
using ProbSolv.Models;
using ProbSolv.Models.Enums;
using ProbSolv.Models.ViewModels;
using ProbSolv.Services.Interfaces;

namespace ProbSolv.Controllers
{
    public class InvitesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<PSUser> _userManager;
        private readonly IDataProtector _protector;
        private readonly IPSProjectService _projectService;
        private readonly IEmailSender _emailSender;
        private readonly IPSInviteService _inviteService;

        public InvitesController(ApplicationDbContext context,
                                 UserManager<PSUser> userManager,
                                 IDataProtectionProvider protector,
                                 IPSProjectService projectService,
                                 IPSInviteService inviteService,
                                 IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _protector = protector.CreateProtector("LB.ProbSolv.23");
            _projectService = projectService;
            _inviteService = inviteService;
            _emailSender = emailSender;
        }

        // GET: Invites
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Invites.Include(i => i.Company).Include(i => i.Invitee).Include(i => i.Invitor).Include(i => i.Project);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Invites/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Invites == null)
            {
                return NotFound();
            }

            var invite = await _context.Invites
                .Include(i => i.Company)
                .Include(i => i.Invitee)
                .Include(i => i.Invitor)
                .Include(i => i.Project)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (invite == null)
            {
                return NotFound();
            }

            return View(invite);
        }

        // GET: Invites/Create
        public async Task<IActionResult> Create()
        {
            int companyId = User.Identity.GetCompanyId().Value;

            InviteViewModel model = new();

            if (User.IsInRole(nameof(Roles.Admin)))
            {
                model.ProjectList = new SelectList(await _projectService.GetAllProjectsByCompanyAsync(companyId), "Id", "Name");
            }
            else if(User.IsInRole(nameof(Roles.ProjectManager)))
            {
                string userId = _userManager.GetUserId(User);
                List<Project> projects = await _projectService.GetUserProjectsAsync(userId);
                model.ProjectList = new SelectList(projects, "Id", "Name");
            }


            return View(model);
        }

        // POST: Invites/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Email, FirstName, LastName, ProjectId, Message")] InviteViewModel model)
        {
            int companyId = User.Identity.GetCompanyId().Value;
            string userId = _userManager.GetUserId(User);


            if (ModelState.IsValid)
            {
                Guid guid = Guid.NewGuid();

                var token = _protector.Protect(guid.ToString());
                var email = _protector.Protect(model.Email);

                var callbackUrl = Url.Action("ProcessInvite", "Invites", new { token, email }, protocol: Request.Scheme);

                var body = "Please join my company." + Environment.NewLine + "Please click in the following link to join <a href=\"" + callbackUrl + "\">COLLABORATE</a>";
                var destination = model.Email;
                var subject = "Company Invite";

                await _emailSender.SendEmailAsync(destination, subject, body);

                //Create record in Invites table
                Invite invite = new Invite()
                {
                    InviteeEmail = model.Email,
                    InviteeFirstName = model.FirstName,
                    InviteeLastName = model.LastName,
                    CompanyToken = guid,
                    CompanyId = companyId,
                    ProjectId = model.ProjectId,
                    InviteDate = DateTimeOffset.Now,
                    InvitorId = userId,
                    IsValid = true
                };

                await _inviteService.AddNewinviteAsync(invite);
               
                return RedirectToAction(nameof(Index));
            }

            ViewData["ProjectId"] = new SelectList(await _projectService.GetUserProjectsAsync(userId), "Id", "Name", model.ProjectId);

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ProcessInvite(string token, string email)
        {
            if(token == null)
            {
                return NotFound();
            }

            int companyId = User.Identity.GetCompanyId().Value;

            Guid companyToken = Guid.Parse(_protector.Unprotect(token));
            string inviteeEmail = _protector.Unprotect(email);

            

            Invite invite = await _inviteService.GetInviteAsync(companyToken, inviteeEmail, companyId);

            if(invite != null)
            {
                return View(invite);
            }

            return NotFound();

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public  IActionResult ProcessInvite(Invite invite)
        {
            return RedirectToPage("RegisterByInvite", new { invite });
        }


        // GET: Invites/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Invites == null)
            {
                return NotFound();
            }

            var invite = await _context.Invites.FindAsync(id);
            if (invite == null)
            {
                return NotFound();
            }
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Id", invite.CompanyId);
            ViewData["InviteeId"] = new SelectList(_context.Users, "Id", "Id", invite.InviteeId);
            ViewData["InvitorId"] = new SelectList(_context.Users, "Id", "Id", invite.InvitorId);
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name", invite.ProjectId);
            return View(invite);
        }

        // POST: Invites/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,InviteDate,JoinDate,CompanyToken,CompanyId,ProjectId,InvitorId,InviteeId,InviteeEmail,InviteeFirstName,InviteeLastName,IsValid")] Invite invite)
        {
            if (id != invite.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(invite);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InviteExists(invite.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Id", invite.CompanyId);
            ViewData["InviteeId"] = new SelectList(_context.Users, "Id", "Id", invite.InviteeId);
            ViewData["InvitorId"] = new SelectList(_context.Users, "Id", "Id", invite.InvitorId);
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name", invite.ProjectId);
            return View(invite);
        }

        // GET: Invites/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Invites == null)
            {
                return NotFound();
            }

            var invite = await _context.Invites
                .Include(i => i.Company)
                .Include(i => i.Invitee)
                .Include(i => i.Invitor)
                .Include(i => i.Project)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (invite == null)
            {
                return NotFound();
            }

            return View(invite);
        }

        // POST: Invites/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Invites == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Invites'  is null.");
            }
            var invite = await _context.Invites.FindAsync(id);
            if (invite != null)
            {
                _context.Invites.Remove(invite);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool InviteExists(int id)
        {
          return _context.Invites.Any(e => e.Id == id);
        }
    }
}
