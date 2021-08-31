using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TitanTracker.Models.ViewModels
{
    public class ProjectMembersViewModel
    {
        public Project Project { get; set; }
        public MultiSelectList Users { get; set; }  //popuolates list box
        public List<string> SelectedUsers { get; set; } //recieves list of users ??
    }
}