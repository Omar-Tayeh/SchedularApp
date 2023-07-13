using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchedularCalendar.Models
{
    public class Event
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set;}
        public int Days { get; set; }
        public string CreatorId { get; set; }
        public EventStatus Status { get; set; }

    }

    public enum EventStatus
    {
        Pending,Approved,Rejected
    }
    
}
