using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TitanTracker.Data;
using TitanTracker.Extensions;
using TitanTracker.Models;
using TitanTracker.Models.Enums;
using TitanTracker.Models.ViewModels;
using TitanTracker.Services.Interfaces;

namespace TitanTracker.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTCompanyInfoService _companyInfoService;
        private readonly IBTRolesService _rolesService;
        private readonly IBTProjectService _projectService;
        private readonly IBTFileService _fileService;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTNotificationService _notificationService;
        private readonly IBTTicketService _ticketService;

        public ProjectsController(ApplicationDbContext context,
                                  IBTCompanyInfoService companyInfoService,
                                  IBTRolesService rolesService,
                                  IBTProjectService projectService,
                                  IBTFileService fileService,
                                  UserManager<BTUser> userManager,
                                  IBTNotificationService notificationService,
                                  IBTTicketService ticketService)
        {
            _context = context;
            _companyInfoService = companyInfoService;
            _rolesService = rolesService;
            _projectService = projectService;
            _fileService = fileService;
            _userManager = userManager;
            _notificationService = notificationService;
            _ticketService = ticketService;
        }

        // GET: Projects
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            int companyId = User.Identity.GetCompanyId().Value;
            List<Project> projects = await _projectService.GetAllProjectsByCompany(companyId);
            return View(projects);
        }

        [HttpGet]
        public async Task<IActionResult> AddPM(int id)
        {
            AddProjectWithPMViewModel model = new();
            int companyId = User.Identity.GetCompanyId().Value;
            model.Project = await _projectService.GetProjectByIdAsync(id, companyId);
            model.PMList = new SelectList(await _rolesService.GetUsersInRoleAsync(Roles.ProjectManager.ToString(), companyId), "Id", "FullName");

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddPM(AddProjectWithPMViewModel model, int projectId)
        {
            if (ModelState.IsValid)
            {
                if (model.PmId != null)
                {
                    await _projectService.AddProjectManagerAsync(model.PmId, projectId);
                }
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> AssignMembers(int id)
        {
            ProjectMembersViewModel model = new();

            int companyId = User.Identity.GetCompanyId().Value;
            //All Company Projedcts based on "id" parameter
            List<Project> projects = await _projectService.GetAllProjectsByCompany(companyId);
            Project project = projects.FirstOrDefault(p => p.Id == id);

            model.Project = project;

            List<BTUser> developers = await _rolesService.GetUsersInRoleAsync(Roles.Developer.ToString(), companyId);
            List<BTUser> submitters = await _rolesService.GetUsersInRoleAsync(Roles.Submitter.ToString(), companyId);

            List<BTUser> users = developers.Concat(submitters).ToList();

            List<string> members = project.Members.Select(m => m.Id).ToList();
            model.Users = new MultiSelectList(users, "Id", "FullName", members);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignMembers(ProjectMembersViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.SelectedUsers != null)
                {
                    List<string> memberIds = (await _projectService.GetAllProjectMembersExceptPMAsync(model.Project.Id))
                                                                   .Select(m => m.Id).ToList();

                    foreach (string item in memberIds)
                    {
                        await _projectService.RemoveUserFromProjectAsync(item, model.Project.Id);
                    }

                    foreach (string item in model.SelectedUsers)
                    {
                        await _projectService.AddUserToProjectAsync(item, model.Project.Id);
                    }

                    int companyId = User.Identity.GetCompanyId().Value;
                    BTUser btUser = await _userManager.GetUserAsync(User);
                    List<BTUser> developers = await _projectService.GetProjectMembersByRoleAsync(model.Project.Id, Roles.Developer.ToString());

                    List<BTUser> submitters = await _projectService.GetProjectMembersByRoleAsync(model.Project.Id, Roles.Submitter.ToString());
                    BTUser projectManager = await _projectService.GetProjectManagerAsync(model.Project.Id);

                    Notification notification = new()
                    {
                        Title = $"{btUser.FullName} has added a new member to a Project",
                        Message = $" A new member has been added to the Project: {model.Project.Name} by {btUser.FullName}.",
                        Created = DateTimeOffset.Now,
                        ProjectId = model.Project.Id,
                        RecipientId = projectManager?.Id,
                        SenderId = btUser.Id
                    };
                    if (projectManager != null)
                    {
                        await _notificationService.AddNotificationAsync(notification);
                        await _notificationService.SendEmailNotificationAsync(notification, "New member has been added to a project");
                    }
                    else
                    {
                        //notification.RecipientId = admin.Id;
                        await _notificationService.AddAdminNotificationAsync(notification, companyId);
                        await _notificationService.SendEmailNotificationsByRoleAsync(notification, companyId, Roles.Admin.ToString());
                    }

                    foreach (BTUser developer in developers)
                    {
                        Notification developerNotification = new()
                        {
                            Title = $"{btUser.FullName} has Added you to a Project",
                            Message = $"You have been added to the Project: {model.Project.Name}.",
                            Created = DateTimeOffset.Now,
                            ProjectId = model.Project.Id,
                            RecipientId = developer?.Id,
                            SenderId = btUser.Id
                        };

                        await _notificationService.AddNotificationAsync(developerNotification);
                        await _notificationService.SendEmailNotificationAsync(developerNotification, "New Project Assignment");
                    }

                    foreach (BTUser submitter in submitters)
                    {
                        Notification submitterNotification = new()
                        {
                            Title = $"{btUser.FullName} has Added you to a Project",
                            Message = $"You have been added to the Project: {model.Project.Name}.",
                            Created = DateTimeOffset.Now,
                            ProjectId = model.Project.Id,
                            RecipientId = submitter?.Id,
                            SenderId = btUser.Id
                        };

                        await _notificationService.AddNotificationAsync(submitterNotification);
                        await _notificationService.SendEmailNotificationAsync(submitterNotification, "New Project Assignment");
                    }
                }
            }
            //return RedirectToAction("AssignMembers", new { id = model.Project.Id });

            //goto project details
            return RedirectToAction("Details", "Projects", new { id = model.Project.Id });
        }

        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .Include(p => p.Company)
                .Include(p => p.ProjectPriority)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // GET: Projects/Create
        [Authorize(Roles = "Admin,ProjectManager")]
        public async Task<IActionResult> Create()
        {
            int companyId = User.Identity.GetCompanyId().Value;

            //Add ViewModel instance "AddProjectWithPmViewModel"
            AddProjectWithPMViewModel model = new();
            //Load SelectLists with data ie. PmList & PriorityList
            model.PMList = new SelectList(await _rolesService.GetUsersInRoleAsync(Roles.ProjectManager.ToString(), companyId), "Id", "FullName");
            model.PriorityList = new SelectList(_context.ProjectPriorities, "Id", "Name");

            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Id");
            ViewData["ProjectPriorityId"] = new SelectList(_context.ProjectPriorities, "Id", "Name");
            return View(model);
        }

        // POST: Projects/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AddProjectWithPMViewModel model)

        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (model.Project.ImageFormFile != null)
                    {
                        model.Project.FileData = await _fileService.ConvertFileToByteArrayAsync(model.Project.ImageFormFile);
                        model.Project.FileName = model.Project.ImageFormFile.FileName;
                        model.Project.FileContentType = model.Project.ImageFormFile.ContentType;
                    }
                    model.Project.CompanyId = User.Identity.GetCompanyId().Value;

                    await _projectService.AddNewProjectAsync(model.Project);

                    //Add PM
                    if (!string.IsNullOrEmpty(model.PmId))
                    {
                        await _projectService.AddProjectManagerAsync(model.PmId, model.Project.Id);

                        BTUser btUser = await _userManager.GetUserAsync(User);
                        int companyId = User.Identity.GetCompanyId().Value;
                        int projectId = model.Project.Id;
                        BTUser Pm = await _projectService.GetProjectManagerAsync(projectId);

                        //SEND PM NOTIFICATION HERE ----------------->

                        Notification notification = new()
                        {
                            Title = $"{btUser.FullName} has Added you to a Project",
                            Message = $" You have been added to the Project: {model.Project.Name} by {btUser.FullName}.  ",
                            Created = DateTimeOffset.Now,
                            ProjectId = model.Project.Id,
                            RecipientId = Pm?.Id,
                            SenderId = btUser.Id
                        };
                        if (Pm != null)
                        {
                            await _notificationService.AddNotificationAsync(notification);
                            await _notificationService.SendEmailNotificationAsync(notification, "New Project Added");
                        }
                        else
                        {
                            //notification.RecipientId = admin.Id;
                            await _notificationService.AddAdminNotificationAsync(notification, companyId);
                            await _notificationService.SendEmailNotificationsByRoleAsync(notification, companyId, Roles.Admin.ToString());
                        }
                    }
                }
                catch (Exception)
                {
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        //Get: Projects/MyProjects
        public async Task<IActionResult> MyProjects()
        {
            BTUser userId = await _userManager.GetUserAsync(User);
            List<Project> projects = await _projectService.GetUserProjectsAsync(userId.Id);
            return View(projects);
        }

        // GET: Projects/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Id", project.CompanyId);
            ViewData["ProjectPriorityId"] = new SelectList(_context.ProjectPriorities, "Id", "Id", project.ProjectPriorityId);
            return View(project);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CompanyId,Name,Description,StartDate,EndDate,ProjectPriorityId,FileName,FileData,FileContentType,ImageFormFile,")] Project project)
        {
            if (id != project.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (project.ImageFormFile != null)
                    {
                        byte[] newImageData = await _fileService.ConvertFileToByteArrayAsync(project.ImageFormFile);
                        project.FileData = newImageData;
                        project.FileName = project.ImageFormFile.FileName;
                        project.FileContentType = project.ImageFormFile.ContentType;
                    }

                    await _projectService.UpdateProjectAsync(project);

                    //-------START NOTIFICATION SERVICE HERE-------------->

                    int companyId = User.Identity.GetCompanyId().Value;
                    BTUser btUser = await _userManager.GetUserAsync(User);
                    List<BTUser> developers = await _projectService.GetProjectMembersByRoleAsync(project.Id, Roles.Developer.ToString());

                    List<BTUser> submitters = await _projectService.GetProjectMembersByRoleAsync(project.Id, Roles.Submitter.ToString());
                    BTUser projectManager = await _projectService.GetProjectManagerAsync(project.Id);

                    Notification notification = new()
                    {
                        Title = $"{btUser.FullName} has modified a project that you are apart of  ",
                        Message = $" The Project: {project.Name} has been revised by {btUser.FullName}.",
                        Created = DateTimeOffset.Now,
                        ProjectId = project.Id,
                        RecipientId = projectManager?.Id,
                        SenderId = btUser.Id
                    };
                    if (projectManager != null)
                    {
                        await _notificationService.AddNotificationAsync(notification);
                        await _notificationService.SendEmailNotificationAsync(notification, "A Project has been modified");
                    }
                    else
                    {
                        //notification.RecipientId = admin.Id;
                        await _notificationService.AddAdminNotificationAsync(notification, companyId);
                        await _notificationService.SendEmailNotificationsByRoleAsync(notification, companyId, Roles.Admin.ToString());
                    }

                    foreach (BTUser developer in developers)
                    {
                        Notification developerNotification = new()
                        {
                            Title = $"{btUser.FullName} has modified a project that you are apart of  ",
                            Message = $" The Project: {project.Name} has been revised by {btUser.FullName}.",
                            Created = DateTimeOffset.Now,
                            ProjectId = project.Id,
                            RecipientId = developer?.Id,
                            SenderId = btUser.Id
                        };

                        await _notificationService.AddNotificationAsync(developerNotification);
                        await _notificationService.SendEmailNotificationAsync(developerNotification, "A Project has been modified");
                    }

                    foreach (BTUser submitter in submitters)
                    {
                        Notification submitterNotification = new()
                        {
                            Title = $"{btUser.FullName} has modified a project that you are apart of  ",
                            Message = $" The Project: {project.Name} has been revised by {btUser.FullName}.",
                            Created = DateTimeOffset.Now,
                            ProjectId = project.Id,
                            RecipientId = submitter?.Id,
                            SenderId = btUser.Id
                        };

                        await _notificationService.AddNotificationAsync(submitterNotification);
                        await _notificationService.SendEmailNotificationAsync(submitterNotification, "A Project has been modified");
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(project.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index", "Projects");
            }
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Id", project.CompanyId);
            ViewData["ProjectPriorityId"] = new SelectList(_context.ProjectPriorities, "Id", "Id", project.ProjectPriorityId);
            return View(project);
        }

        // GET: Projects/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .Include(p => p.Company)
                .Include(p => p.ProjectPriority)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProjectExists(int id)
        {
            return _context.Projects.Any(e => e.Id == id);
        }
    }
}