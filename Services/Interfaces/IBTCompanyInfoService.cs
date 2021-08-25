using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TitanTracker.Models;

namespace TitanTracker.Services.Interfaces
{
    public interface IBTCompanyInfoService
    {
        public Task<Company> GetCompanyInfoByIdAsync(int? companyId);

        public Task<List<BTUser>> GetAllMembersAsync(int companyId);

        public Task<List<Project>> GetAllProjects(int companyId);

        public Task<List<Ticket>> GetAllTicketsAsync(int companyId);
    }
}