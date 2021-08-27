using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TitanTracker.Data;
using TitanTracker.Models;
using TitanTracker.Models.Enums;
using TitanTracker.Services.Interfaces;

namespace TitanTracker.Services
{
    public class BTTicketService : IBTTicketService
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTRolesService _rolesService;
        private readonly IBTProjectService _projectService;

        public BTTicketService(ApplicationDbContext context, IBTRolesService rolesService, IBTProjectService projectService)
        {
            _context = context;
            _rolesService = rolesService;
            _projectService = projectService;
        }

        public async Task AddNewTicketAsync(Ticket ticket)
        {
            try
            {
                await _context.AddAsync(ticket);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task ArchiveTicketAsync(Ticket ticket)
        {
            try
            {
                ticket.Archived = true;
                _context.Update(ticket);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task AssignTicketAsync(int ticketId, string userId)
        {
            try
            {
                Ticket ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId);
                if (ticket != null)
                {
                    try
                    {
                        ticket.DeveloperUserId = userId;
                        ticket.TicketStatusId = (await LookupTicketStatusIdAsync(BTTicketStatus.Development.ToString()
                        await UpdateTicketAsync(ticket);
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Ticket>> GetAllTicketsByCompanyAsync(int companyId)
        {
            List<Ticket> tickets = new();

            try
            {
                tickets = await _context.Projects.Where(p => p.CompanyId == companyId)
                                                          .SelectMany(p => p.Tickets)
                                                          .Where(t => t.Archived == false)
                                                          .Include(t => t.Attachments)
                                                          .Include(t => t.Comments)
                                                           .Include(t => t.DeveloperUser)
                                                            .Include(t => t.History)
                                                             .Include(t => t.OwnerUser)
                                                              .Include(t => t.TicketPriority)
                                                               .Include(t => t.TicketStatus)
                                                                .Include(t => t.TicketType)
                                                                 .Include(t => t.Project)
                                                           .ToListAsync();

                return tickets;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Ticket>> GetAllTicketsByPriorityAsync(int companyId, string priorityName)
        {
            List<Ticket> tickets = new();
            try
            {
                int priorityId = (await LookupTicketPriorityIdAsync(priorityName)).Value;
                tickets = (await GetAllTicketsByCompanyAsync(companyId)).Where(t => t.TicketPriotiryId == priorityId).ToList();
                return tickets;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Ticket>> GetAllTicketsByStatusAsync(int companyId, string statusName)
        {
            List<Ticket> tickets = new();
            try
            {
                int statusId = (await LookupTicketStatusIdAsync(statusName)).Value;
                tickets = (await GetAllTicketsByCompanyAsync(companyId)).Where(t => t.TicketStatusId == statusId).ToList();
                return tickets;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Ticket>> GetAllTicketsByTypeAsync(int companyId, string typeName)
        {
            List<Ticket> tickets = new();
            try
            {
                int typeId = (await LookupTicketTypeIdAsync(typeName)).Value;
                tickets = (await GetAllTicketsByCompanyAsync(companyId)).Where(t => t.TicketTypeId == typeId).ToList();
                return tickets;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Ticket>> GetArchivedTicketsAsync(int companyId)
        {
            List<Ticket> tickets = new();

            try
            {
                tickets = await _context.Projects.Where(p => p.CompanyId == companyId)
                                                          .SelectMany(p => p.Tickets)
                                                          .Where(t => t.Archived == true)
                                                          .Include(t => t.Attachments)
                                                          .Include(t => t.Comments)
                                                           .Include(t => t.DeveloperUser)
                                                            .Include(t => t.History)
                                                             .Include(t => t.OwnerUser)
                                                              .Include(t => t.TicketPriority)
                                                               .Include(t => t.TicketStatus)
                                                                .Include(t => t.TicketType)
                                                                 .Include(t => t.Project)
                                                           .ToListAsync();

                return tickets;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Ticket>> GetProjectTicketsByPriorityAsync(string priorityName, int companyId, int projectId)
        {
            List<Ticket> tickets = new();
            try
            {
                tickets = (await GetAllTicketsByPriorityAsync(companyId, priorityName)).Where(t => t.ProjectId == projectId).ToList();
                return tickets;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Ticket>> GetProjectTicketsByRoleAsync(string role, string userId, int projectId, int companyId)
        {
            List<Ticket> tickets = new();
            try
            {
                tickets = (await GetTicketsByRoleAsync(role, userId, companyId)).Where(t => t.CompanyId == companyId).ToList();
                return tickets;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Task<List<Ticket>> GetProjectTicketsByStatusAsync(string statusName, int companyId, int projectId)
        {
            throw new NotImplementedException();
        }

        public async Task<List<Ticket>> GetProjectTicketsByTypeAsync(string typeName, int companyId, int projectId)
        {
            List<Ticket> tickets = new();
            try
            {
                tickets = (await GetAllTicketsByTypeAsync(companyId, typeName)).Where(t => t.ProjectId == projectId).ToList();
                return tickets;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Ticket> GetTicketByIdAsync(int ticketId)
        {
            Ticket ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId);
            return ticket;
        }

        public async Task<BTUser> GetTicketDeveloperAsync(int ticketId, int companyId)
        {
            BTUser developer = new();
            try
            {
                Ticket ticket = (await GetAllTicketsByCompanyAsync(companyId)).FirstOrDefault(t => t.Id == ticketId);
                if (ticket.DeveloperUserId != null)
                {
                    developer = ticket.DeveloperUser;
                }
                return ticket.DeveloperUser;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Ticket>> GetTicketsByRoleAsync(string role, string userId, int companyId)
        {
            List<Ticket> tickets = new();
            try
            {
                if (role == Roles.Admin.ToString())
                {
                    tickets = await GetAllTicketsByCompanyAsync(companyId);
                }
                else if (role == Roles.Developer.ToString())
                {
                    tickets = (await GetAllTicketsByCompanyAsync(companyId)).Where(t => t.DeveloperUserId == userId).ToList();
                }
                else if (role == Roles.Submitter.ToString())
                {
                    tickets = (await GetAllTicketsByCompanyAsync(companyId)).Where(t => t.OwnerUserId == userId).ToList();
                }
                else if (role == Roles.ProjectManager.ToString())
                {
                    tickets = await GetTicketsByUserIdAsync(userId, companyId);
                }
                return tickets;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Ticket>> GetTicketsByUserIdAsync(string userId, int companyId)
        {
            List<Ticket> tickets = new();
            try
            {
                BTUser bTUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (await _rolesService.IsUserInRoleAsync(bTUser, Roles.Admin.ToString()))
                {
                    tickets = (await _projectService.GetAllProjectsByCompany(companyId)).SelectMany(p => p.Tickets).ToList();
                }
                else if (await _rolesService.IsUserInRoleAsync(bTUser, Roles.Developer.ToString()))
                {
                    List<Ticket> devTickets = (await _projectService.GetAllProjectsByCompany(companyId))
                                                         .SelectMany(p => p.Tickets).Where(t => t.DeveloperUserId == userId).ToList();
                    List<Ticket> subTickets = (await _projectService.GetAllProjectsByCompany(companyId))
                                                          .SelectMany(p => p.Tickets).Where(t => t.OwnerUserId == userId).ToList();
                    tickets = devTickets.Concat(subTickets).ToList();
                }
                else if (await _rolesService.IsUserInRoleAsync(bTUser, Roles.Submitter.ToString()))
                {
                    tickets = (await _projectService.GetAllProjectsByCompany(companyId))
                                                      .SelectMany(t => t.Tickets).Where(t => t.OwnerUserId == userId).ToList();
                    //TODO:  //Fix concatenation of submitted tickets
                }
                return tickets;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int?> LookupTicketPriorityIdAsync(string priorityName)
        {
            try
            {
                int priorityId = (await _context.TicketPriorities.FirstOrDefaultAsync(p => p.Name == priorityName)).Id;
                return priorityId;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int?> LookupTicketStatusIdAsync(string statusName)
        {
            try
            {
                TicketStatus status = await _context.TicketStatuses.FirstOrDefaultAsync(p => p.Name == statusName);
                return status.Id;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Task<int?> LookupTicketTypeIdAsync(string typeName)
        {
            throw new NotImplementedException();
        }

        public Task UpdateTicketAsync(Ticket ticket)
        {
            throw new NotImplementedException();
        }
    }
}