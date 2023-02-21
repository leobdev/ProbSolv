using Microsoft.AspNetCore.Mvc;
using ProbSolv.Extensions;
using ProbSolv.Models;
using ProbSolv.Models.ViewModels;
using ProbSolv.Services.Interfaces;
using System.Diagnostics;

namespace ProbSolv.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IPSCompanyInfoService _companyinfoService;

        public HomeController(ILogger<HomeController> logger, IPSCompanyInfoService companyinfoService)
        {
            _logger = logger;
            _companyinfoService = companyinfoService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Dashboard()
        {
            DashboardViewModel model = new();
            int companyId = User.Identity.GetCompanyId().Value;

            model.Company = await _companyinfoService.GetCompanyInfoByIdAsync(companyId);
            model.Projects = (await _companyinfoService.GetAllProjectsAsync(companyId))
                                                       .Where(p => p.Archived == false)
                                                       .ToList();
            model.Tickets = model.Projects.SelectMany(p => p.Tickets).ToList();
            model.Members = model.Company.Members.ToList();

            return View(model);
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}