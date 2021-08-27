using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using TitanTracker.Data;
using TitanTracker.Models;
using TitanTracker.Services.Interfaces;

namespace TitanTracker.Services
{
    public class BTNotificationService : IBTNotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender emailSender;
        private readonly IBTRolesService _rolesService;

        public BTNotificationService(ApplicationDbContext context, IEmailSender emailSender, IBTRolesService rolesService)
        {
            _context = context;
            _emailSender = emailSender;
            _rolesService = rolesService;
        }

        public Task AddNotificationAsync(Notification notification)
        {
            throw new NotImplementedException();
        }

        public async Task<List<Notification>> GetReceivedNotificationsAsync(string userId)
        {
            try
            {
                List<Notification> notifications = await _context.Notifications
                                                 .Include(n => n.Recipient)
                                                 .Include(n => n.Sender)
                                                 .Include(n => n.Ticket)
                                                 .ThenInclude(t => t.Project)
                                                .Where(n => n.RecipientId == userId).ToListAsync();
                return notifications;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Notification>> GetSentNotificationsAsync(string userId)
        {
            try
            {
                List<Notification> notifications = await _context.Notifications
                                                 .Include(n => n.Recipient)
                                                 .Include(n => n.Sender)
                                                 .Include(n => n.Ticket)
                                                 .ThenInclude(t => t.Project)
                                                .Where(n => n.SenderId == userId).ToListAsync();
                return notifications;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Task<bool> SendEmailNotificationAsync(Notification notification, string emailSubject)
        {
            throw new NotImplementedException();
        }

        public Task SendEmailNotificationsByRoleAsync(Notification notification, int companyId, string role)
        {
            throw new NotImplementedException();
        }

        public Task SendMembersEmailNotificationsAsync(Notification notification, List<BTUser> members)
        {
            throw new NotImplementedException();
        }
    }
}