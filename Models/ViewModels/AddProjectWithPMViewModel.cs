using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TitanTracker.Models.ViewModels
{
    public class AddProjectWithPMViewModel
    {
        public Project Project { get; set; } = new Project();
        public SelectList PMList { get; set; }
        public string PmId { get; set; }
        public SelectList PriorityList { get; set; }
        public int ProjectPriority { get; set; }

        public List<string> SelectedUsers { get; set; } //recieves list of users ??
    }
}