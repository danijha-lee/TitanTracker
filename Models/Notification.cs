using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace TitanTracker.Models
{
    public class Notification
    {
        //Primary Key
        public int Id { get; set; }

        [Required]
        [DisplayName("Title")]
        public string Title { get; set; }

        [Required]
        [DisplayName("Message")]
        public string Message { get; set; }

        [DataType(DataType.Date)]
        [DisplayName("Date")]
        public DateTimeOffset Created { get; set; }

        public bool Draft { get; set; }

        [DisplayName("Has been viewed")]
        public bool Viewed { get; set; }

        [DisplayName("Has been archived")]
        public bool Archived { get; set; }

        [DisplayName("Important")]
        public bool Important { get; set; }

        [DisplayName("Ticket")]
        public int? TicketId { get; set; }

        [DisplayName("Project")]
        public int? ProjectId { get; set; }

        [Required]
        [DisplayName("Recipient")]
        public string RecipientId { get; set; }

        [Required]
        [DisplayName("Sender")]
        public string SenderId { get; set; }

        //Navigation Properties

        public virtual Ticket Ticket { get; set; }
        public virtual BTUser Recipient { get; set; }
        public virtual BTUser Sender { get; set; }

        public virtual Project Project { get; set; }
    }
}