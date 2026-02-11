using System.ComponentModel.DataAnnotations;
using WildlifeTracker.Models.Identity;

namespace WildlifeTracker.Models
{
    public class PopulationChange
    {
        public int Id { get; set; }

        public int SettlementId { get; set; }
        public Settlement Settlement { get; set; } = null!;

        public int SpeciesId { get; set; }
        public Species Species { get; set; } = null!;

        [Range(1900, 2100)]
        public int Year { get; set; }

        public int Delta { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        [Required]
        public string EnteredByUserId { get; set; } = null!;
        public ApplicationUser EnteredByUser { get; set; } = null!;
    }
}
