using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using TitanTracker.Data;
using TitanTracker.Models;
using TitanTracker.Services.Interfaces;

namespace TitanTracker.Services
{
    public class BTInviteService : IBTInviteService
    {
        private readonly ApplicationDbContext _context;
        private IEmailSender _emailSender;
        private IBTProjectService _projectService;

        public BTInviteService(ApplicationDbContext context, IEmailSender emailSender, IBTProjectService projectService)
        {
            _context = context;
            _emailSender = emailSender;
            _projectService = projectService;
        }

        public async Task<bool> AcceptInviteAsync(Guid? token, string userId)
        {
            try
            {
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Task<bool> AnyInviteAsync(Guid token, string email)
        {
            throw new NotImplementedException();
        }

        public Task<Invite> GetInviteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<Invite> GetInviteAsync(Guid token, string email)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ValidateInviteCodeAsync(Guid? token)
        {
            throw new NotImplementedException();
        }
    }
}