using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TitanTracker.Extensions;
using TitanTracker.Models;
using TitanTracker.Models.Enums;
using TitanTracker.Services.Interfaces;

namespace TitanTracker.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IBTProjectService _projectService;

        public HomeController(ILogger<HomeController> logger, IBTProjectService projectService)
        {
            _logger = logger;
            _projectService = projectService;
        }

        //[Authorize(Roles = "Admin")]
        //[Authorize(Roles = "Developer")]
        //[Authorize(Roles = "Submitter")]
        //[Authorize(Roles = "DemoUser")]
        //[Authorize(Roles = "ProjectManager")]
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Landing()
        {
            return View();
        }

        //[Authorize(Roles = "Admin")]
        //[Authorize(Roles = "Developer")]
        //[Authorize(Roles = "Submitter")]
        //[Authorize(Roles = "DemoUser")]
        //[Authorize(Roles = "ProjectManager")]
        public async Task<IActionResult> DashboardAsync()
        {
            List<Project> model = new();
            int companyId = User.Identity.GetCompanyId().Value;
            model = await _projectService.GetAllProjectsByCompany(companyId);

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