using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
    public class TicketsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTTicketService _ticketService;
        private readonly IBTProjectService _projectService;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTTicketHistoryService _ticketHistoryService;
        private readonly IBTNotificationService _notificationService;
        private readonly IBTCompanyInfoService _companyInfoService;

        public TicketsController(ApplicationDbContext context, IBTTicketService ticketService,
                                                               IBTProjectService projectService,
                                                                UserManager<BTUser> userManager,
                                                                IBTTicketHistoryService ticketHistoryService,
                                                                IBTNotificationService notificationService,
                                                                IBTCompanyInfoService companyInfoService)
        {
            _context = context;
            _ticketService = ticketService;
            _projectService = projectService;
            _userManager = userManager;
            _ticketHistoryService = ticketHistoryService;
            _notificationService = notificationService;
            _companyInfoService = companyInfoService;
        }

        // GET: Tickets
        public async Task<IActionResult> Index()
        {
            int companyId = User.Identity.GetCompanyId().Value;
            List<Ticket> tickets = await _ticketService.GetAllTicketsByCompanyAsync(companyId);
            return View(tickets);
        }

        //ACTION FOR CURRENT USERS TICKETS
        [HttpGet]
        public async Task<IActionResult> AllTickets()
        {
            int companyId = User.Identity.GetCompanyId().Value;
            BTUser user = await _userManager.GetUserAsync(User);
            List<Ticket> tickets = await _ticketService.GetTicketsByUserIdAsync(user.Id, companyId);

            //ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "Id");
            //ViewData["OwnerUserId"] = new SelectList(_context.Users, "Id", "Id");
            if (User.IsInRole(Roles.Admin.ToString()))
            {
                ViewData["ProjectId"] = new SelectList(await _projectService.GetAllProjectsByCompany(companyId), "Id", "Name");
            }
            else
            {
                ViewData["ProjectId"] = new SelectList(await _projectService.GetUserProjectsAsync(user.Id), "Id", "Name");
            }

            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name");
            //ViewData["TicketStatusId"] = new SelectList(_context.TicketStatuses, "Id", "Id");
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name");

            return View(tickets);
        }

        [HttpPost]
        public async Task<IActionResult> AllTickets([Bind("Id,Title,Description,TicketTypeId,ProjectId,TicketPriorityId")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                ticket.TicketStatusId = (await _ticketService.LookupTicketStatusIdAsync(BTTicketStatus.New.ToString())).Value;
                BTUser btUser = await _userManager.GetUserAsync(User);
                ticket.OwnerUserId = btUser.Id;
                ticket.Created = DateTimeOffset.Now;
                await _ticketService.AddNewTicketAsync(ticket);

                Ticket newTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id);

                await _ticketHistoryService.AddHistoryAsync(null, newTicket, btUser.Id);

                BTUser Pm = await _projectService.GetProjectManagerAsync(ticket.ProjectId);
                int companyId = User.Identity.GetCompanyId().Value;

                List<BTUser> developers = await _projectService.GetProjectMembersByRoleAsync(ticket.ProjectId, Roles.Developer.ToString());
                List<BTUser> submitters = await _projectService.GetProjectMembersByRoleAsync(ticket.ProjectId, Roles.Submitter.ToString());

                Notification notification = new()
                {
                    Title = "New Ticket Created",
                    Message = $" A New Ticket: {ticket.Title}, Was Created By {btUser.FullName} for the project: {ticket.Project.Name}.",
                    Created = DateTimeOffset.Now,
                    TicketId = ticket.Id,
                    RecipientId = Pm?.Id,
                    SenderId = btUser.Id
                };

                if (Pm != null)
                {
                    await _notificationService.AddNotificationAsync(notification);
                    await _notificationService.SendEmailNotificationAsync(notification, "New ticket added.");
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
                        Title = "New Ticket Created",
                        Message = $" A New Ticket: {ticket.Title}, Was Created By {btUser.FullName} for the project: {ticket.Project.Name}.",
                        Created = DateTimeOffset.Now,
                        TicketId = ticket.Id,
                        RecipientId = developer?.Id,
                        SenderId = btUser.Id
                    };

                    await _notificationService.AddNotificationAsync(developerNotification);
                    await _notificationService.SendEmailNotificationAsync(developerNotification, "New ticket added.");
                }

                foreach (BTUser submitter in submitters)
                {
                    Notification submitterNotification = new()
                    {
                        Title = "New Ticket Created",
                        Message = $" A New Ticket: {ticket.Title}, Was Created By {btUser.FullName} for the project: {ticket.Project.Name}.",
                        Created = DateTimeOffset.Now,
                        TicketId = ticket.Id,
                        RecipientId = submitter?.Id,
                        SenderId = btUser.Id
                    };

                    await _notificationService.AddNotificationAsync(submitterNotification);
                    await _notificationService.SendEmailNotificationAsync(submitterNotification, "New ticket added.");
                }

                //_context.Add(ticket);
                //await _context.SaveChangesAsync();
                return RedirectToAction("AllTickets");
            }
            //ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "Id", ticket.DeveloperUserId);
            //ViewData["OwnerUserId"] = new SelectList(_context.Users, "Id", "Id", ticket.OwnerUserId);
            //ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name", ticket.ProjectId);
            //ViewData["TicketStatusId"] = new SelectList(_context.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
            //ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            //ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name");

            return RedirectToAction("AllTickets");
        }

        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _ticketService.GetTicketByIdAsync((int)id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        public IActionResult ShowFile(int id)
        {
            TicketAttachment ticketAttachment = _context.TicketAttachments.Find(id);
            string fileName = ticketAttachment.FileName;
            byte[] fileData = ticketAttachment.FileData;
            string ext = Path.GetExtension(fileName).Replace(".", "");

            Response.Headers.Add("Content-Disposition", $"inline; filename={fileName}");
            return File(fileData, $"application/{ext}");
        }

        // GET: Tickets/Create
        public async Task<IActionResult> Create()
        {
            BTUser btUser = await _userManager.GetUserAsync(User);
            int companyId = User.Identity.GetCompanyId().Value;
            //ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "Id");
            //ViewData["OwnerUserId"] = new SelectList(_context.Users, "Id", "Id");
            if (User.IsInRole(Roles.Admin.ToString()))
            {
                ViewData["ProjectId"] = new SelectList(await _projectService.GetAllProjectsByCompany(companyId), "Id", "Name");
            }
            else
            {
                ViewData["ProjectId"] = new SelectList(await _projectService.GetUserProjectsAsync(btUser.Id), "Id", "Name");
            }

            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name");
            //ViewData["TicketStatusId"] = new SelectList(_context.TicketStatuses, "Id", "Id");
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name");
            return View();
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,TicketTypeId,ProjectId,TicketPriorityId")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                ticket.TicketStatusId = (await _ticketService.LookupTicketStatusIdAsync(BTTicketStatus.New.ToString())).Value;
                BTUser btUser = await _userManager.GetUserAsync(User);
                ticket.OwnerUserId = btUser.Id;
                ticket.Created = DateTimeOffset.Now;
                await _ticketService.AddNewTicketAsync(ticket);

                Ticket newTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id);

                await _ticketHistoryService.AddHistoryAsync(null, newTicket, btUser.Id);

                BTUser Pm = await _projectService.GetProjectManagerAsync(ticket.ProjectId);
                int companyId = User.Identity.GetCompanyId().Value;

                List<BTUser> developers = await _projectService.GetProjectMembersByRoleAsync(ticket.ProjectId, Roles.Developer.ToString());
                List<BTUser> submitters = await _projectService.GetProjectMembersByRoleAsync(ticket.ProjectId, Roles.Submitter.ToString());

                Notification notification = new()
                {
                    Title = "New Ticket Created",
                    Message = $" A New Ticket: {ticket.Title}, Was Created By {btUser.FullName} for the project: {ticket.Project.Name}.",
                    Created = DateTimeOffset.Now,
                    TicketId = ticket.Id,
                    RecipientId = Pm?.Id,
                    SenderId = btUser.Id
                };

                if (Pm != null)
                {
                    await _notificationService.AddNotificationAsync(notification);
                    await _notificationService.SendEmailNotificationAsync(notification, "New ticket added.");
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
                        Title = "New Ticket Created",
                        Message = $" A New Ticket: {ticket.Title}, Was Created By {btUser.FullName} for the project: {ticket.Project.Name}.",
                        Created = DateTimeOffset.Now,
                        TicketId = ticket.Id,
                        RecipientId = developer?.Id,
                        SenderId = btUser.Id
                    };

                    await _notificationService.AddNotificationAsync(developerNotification);
                    await _notificationService.SendEmailNotificationAsync(developerNotification, "New ticket added.");
                }

                foreach (BTUser submitter in submitters)
                {
                    Notification submitterNotification = new()
                    {
                        Title = "New Ticket Created",
                        Message = $" A New Ticket: {ticket.Title}, Was Created By {btUser.FullName} for the project: {ticket.Project.Name}.",
                        Created = DateTimeOffset.Now,
                        TicketId = ticket.Id,
                        RecipientId = submitter?.Id,
                        SenderId = btUser.Id
                    };

                    await _notificationService.AddNotificationAsync(submitterNotification);
                    await _notificationService.SendEmailNotificationAsync(submitterNotification, "New ticket added.");
                }

                //_context.Add(ticket);
                //await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Projects", new { id = ticket.ProjectId });
            }
            //ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "Id", ticket.DeveloperUserId);
            //ViewData["OwnerUserId"] = new SelectList(_context.Users, "Id", "Id", ticket.OwnerUserId);
            //ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name", ticket.ProjectId);
            //ViewData["TicketStatusId"] = new SelectList(_context.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
            //ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            //ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name");

            return View(ticket);
        }

        // GET: Tickets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }

            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(_context.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Created,ProjectId,TicketTypeId,TicketPriorityId,TicketStatusId,OwnerUserId,DeveloperUserId")] Ticket ticket)
        {
            if (id != ticket.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    BTUser btUser = await _userManager.GetUserAsync(User);
                    int companyId = User.Identity.GetCompanyId().Value;
                    //int projectId = ticket.Project.Id;

                    Ticket oldTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id);
                    ticket.Updated = DateTimeOffset.Now;
                    await _ticketService.UpdateTicketAsync(ticket);

                    Ticket newTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id);
                    await _ticketHistoryService.AddHistoryAsync(oldTicket, newTicket, btUser.Id);
                    //await _ticketHistoryService.GetProjectTicketsHistoriesAsync(projectId, companyId);

                    BTUser Pm = await _projectService.GetProjectManagerAsync(ticket.ProjectId);
                    //BTUser admin = (await _projectService.GetProjectMembersByRoleAsync(ticket.ProjectId, Roles.Admin.ToString())).FirstOrDefault();
                    List<BTUser> developers = await _projectService.GetProjectMembersByRoleAsync(ticket.ProjectId, Roles.Developer.ToString());
                    List<BTUser> submitters = await _projectService.GetProjectMembersByRoleAsync(ticket.ProjectId, Roles.Submitter.ToString());

                    Notification notification = new()
                    {
                        Title = "Ticket Update",
                        Message = $" The Ticket: {ticket.Title}, Was Updated By {btUser.FullName} for the project: {ticket.Project.Name}.",
                        Created = DateTimeOffset.Now,
                        TicketId = ticket.Id,
                        RecipientId = Pm?.Id,
                        SenderId = btUser.Id
                    };

                    if (Pm != null)
                    {
                        await _notificationService.AddNotificationAsync(notification);
                        await _notificationService.SendEmailNotificationAsync(notification, "A Ticket Was Updated");
                    }
                    else
                    {
                        // notification.RecipientId = admin.Id;
                        await _notificationService.AddAdminNotificationAsync(notification, companyId);
                        await _notificationService.SendEmailNotificationsByRoleAsync(notification, companyId, Roles.Admin.ToString());
                    }
                    foreach (BTUser developer in developers)
                    {
                        Notification developerNotification = new()
                        {
                            Title = "Ticket Update",
                            Message = $" The Ticket: {ticket.Title}, Was Updated By {btUser.FullName} for the project: {ticket.Project.Name}.",
                            Created = DateTimeOffset.Now,
                            TicketId = ticket.Id,
                            RecipientId = developer?.Id,
                            SenderId = btUser.Id
                        };
                        notification.RecipientId = developer.Id;
                        await _notificationService.AddNotificationAsync(notification);
                        await _notificationService.SendEmailNotificationAsync(developerNotification, "New ticket added.");
                    }
                    foreach (BTUser submitter in submitters)
                    {
                        Notification submitterNotification = new()
                        {
                            Title = "Ticket Update",
                            Message = $" The Ticket: {ticket.Title}, Was Updated By {btUser.FullName} for the project: {ticket.Project.Name}.",
                            Created = DateTimeOffset.Now,
                            TicketId = ticket.Id,
                            RecipientId = submitter?.Id,
                            SenderId = btUser.Id
                        };
                        notification.RecipientId = submitter.Id;
                        await _notificationService.AddNotificationAsync(notification);
                        await _notificationService.SendEmailNotificationAsync(notification, "New ticket added.");
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketExists(ticket.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Details", "Tickets", new { id = ticket.Id });
            }
            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(_context.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        // GET: Tickets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets
                .Include(t => t.DeveloperUser)
                .Include(t => t.OwnerUser)
                .Include(t => t.Project)
                .Include(t => t.TicketStatus)
                .Include(t => t.TicketType)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TicketExists(int id)
        {
            return _context.Tickets.Any(e => e.Id == id);
        }

        //public async Task<IActionResult> AssignDeveloper(int id)
        //{
        //    AssignDeveloperViewModel model = new();

        //    model.Ticket = await _ticketService.GetTicketByIdAsync(id);

        //    model.Developers = await _projectService.GetProjectMembersByRoleAsync(model.Ticket.ProjectId, Roles.Developer.ToString()),
        //                         "Id", "FullName");
        //    return View(model);
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult AssignDeveloper(AssignDeveloperViewModel model)
        //{
        //}
    }
}