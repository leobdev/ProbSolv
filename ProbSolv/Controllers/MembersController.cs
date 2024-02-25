using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProbSolv.Extensions;
using ProbSolv.Models;
using ProbSolv.Models.Enums;
using ProbSolv.Models.ViewModels;
using ProbSolv.Services;
using ProbSolv.Services.Interfaces;

namespace ProbSolv.Controllers
{
    public class MembersController : Controller
    {
        private readonly ILogger<MembersController> _logger;
        private readonly IPSMemberService _memberService;
        private readonly IPSCompanyInfoService _companyService;
        private readonly UserManager<PSUser> _userManager;
        private readonly IPSProjectService _projectService;
        private readonly IPSTicketService _ticketService;


        public MembersController(ILogger<MembersController> logger,
                                IPSMemberService memberService,
                                IPSCompanyInfoService companyService,
                                UserManager<PSUser> userManager,
                                IPSProjectService projectService,
                                IPSTicketService ticketService)
        {
            _logger = logger;
            _memberService = memberService;
            _companyService = companyService;
            _userManager = userManager;
            _projectService = projectService;
            _ticketService = ticketService;
        }
        public async Task<IActionResult> Index()
        {
            int companyId = User.Identity.GetCompanyId().Value;

            var members = await _companyService.GetAllMembersAsync(companyId);

            return View(members);
        }


        public async Task<IActionResult> Details(string? id)
        {
            var companyId = User.Identity.GetCompanyId();

            if (companyId is null || id is null) return NotFound();

            PSUser? member = await _memberService.GetMemberByIdAsync(companyId.Value, id);

            if (member is null) return NotFound();

 
            var projects = await _projectService.GetUserProjectsAsync(member.Id);

            var tickets = await _ticketService.GetTicketsByUserIdAsync(member.Id, companyId.Value);

            MemberProfileViewModel memberProfile = new MemberProfileViewModel
            {
                Member = member,
                Projects = projects,
                Tickets = tickets
            };

            return View(memberProfile);
        }



        // GET: Members/Delete/5
        [HttpGet]
        [Authorize(Roles = $"{nameof(Roles.Admin)}")]
        public async Task<IActionResult> Delete(string? id)
        {
            var companyId = User.Identity?.GetCompanyId();

            if (companyId is null || id is null) return NotFound();

            PSUser? member = await _memberService.GetMemberByIdAsync(companyId.Value, id);

            if (member is null) return NotFound();

            return View(member);
        }



        // POST: Members/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{nameof(Roles.Admin)}")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var companyId = User.Identity?.GetCompanyId();

            if (companyId is null || id is null) return NotFound();

            PSUser? member = await _memberService.GetMemberByIdAsync(companyId.Value, id);

            if (member is not null)
            {
                var removed = await _memberService.RemoveMemberAsync(member);

                if (removed)
                {
                    ViewData["Removed"] = $"Member {member.FullName} was successfully removed from the company.";
                }
                else
                {
                    ViewData["Removed"] = $"Member {member.FullName} could nt be reomved at this time.";

                }
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
