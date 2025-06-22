using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Web.Models
{
    public class EventType
    {
        public int EventTypeId { get; set; }

        [Required]
        public string Name { get; set; }

        // Navigation property
        public ICollection<Venue> Venues { get; set; }
    }
}
