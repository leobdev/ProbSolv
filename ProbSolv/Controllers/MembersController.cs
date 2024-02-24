using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProbSolv.Models;
using ProbSolv.Services.Interfaces;

namespace ProbSolv.Controllers
{
    public class MembersController : Controller
    {
        private readonly ILogger<MembersController> _logger;
        private readonly IMemberService _memberService;
        private readonly IPSCompanyInfoService _companyService;
        private readonly UserManager<PSUser> _userManager;
        private readonly IPSProjectService _projectService;
        private readonly IPSTicketService _ticketService;

        public MembersController(ILogger<MembersController> logger, IMemberService memberService, IPSCompanyInfoService companyService, UserManager<PSUser> userManager, IPSProjectService projectService, IPSTicketService ticketService)
        {
            _logger = logger;
            _memberService = memberService;
            _companyService = companyService;
            _userManager = userManager;
            _projectService = projectService;
            _ticketService = ticketService;
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
