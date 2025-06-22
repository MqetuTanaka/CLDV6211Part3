using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web.Models
{
    public class Venue
    {
        public int VenueId { get; set; }
        public string VenueName { get; set; }
        public string Location { get; set; }
        public int Capacity { get; set; }
        public string ImageUrl { get; set; }

        [NotMapped]
        public IFormFile ImageFile { get; set; }

        public bool IsAvailable { get; set; } // Ensure this matches the IsAvailable column

        [ForeignKey("EventType")]
        public int? EventTypeId { get; set; }

        // Navigation property
        public EventType EventType { get; set; }

        public ICollection<Event> Events { get; set; } = new List<Event>();

    }
}