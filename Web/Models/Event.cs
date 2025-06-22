using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web.Models
{
    public class Event
    {
        public int EventId { get; set; }

        [Required]
        public string EventName { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime EventDate { get; set; }

        public string Description { get; set; }

        [Required]
        public int VenueId { get; set; }

        [ForeignKey("VenueId")]
        public Venue? Venue { get; set; }

        // 🔽 New field to associate with EventType
        [Required]
        public int EventTypeId { get; set; }

        [ForeignKey("EventTypeId")]
        public EventType? EventType { get; set; }
    }
}
