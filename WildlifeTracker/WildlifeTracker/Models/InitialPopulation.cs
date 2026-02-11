using System.ComponentModel.DataAnnotations;

namespace WildlifeTracker.Models
{
    public class InitialPopulation
    {
        public int Id { get; set; }

        public int SettlementId { get; set; }
        public Settlement Settlement { get; set; } = null!;

        public int SpeciesId { get; set; }
        public Species Species { get; set; } = null!;

        [Range(0, int.MaxValue)]
        public int InitialCount { get; set; }
    }
}
