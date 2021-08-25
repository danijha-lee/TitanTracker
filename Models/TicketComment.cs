using System;
using System.ComponentModel;

namespace TitanTracker.Models
{
    public class TicketComment
    {
        public int Id { get; set; }

        [DisplayName("Member Comment")]
        public string Comment { get; set; }

        //created
        [DisplayName("Date")]
        public DateTimeOffset Created { get; set; }

        //TicketId (Foreign Key)
        [DisplayName("Ticket")]
        public int TicketId { get; set; }

        //UserId (Foreign Key)
        [DisplayName("Team Member")]
        public string UserId { get; set; }

        //Navigation properties--------->
        //Ticket
        public virtual Ticket Ticket { get; set; }

        //User
        public virtual BTUser User { get; set; }
    }
}