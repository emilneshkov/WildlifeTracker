using System.ComponentModel.DataAnnotations;

namespace WildlifeTracker.Models
{
    public class Settlement
    {
        public int Id { get; set; }

        [Required, MaxLength(120)]
        public string Name { get; set; } = null!;

        public int MunicipalityId { get; set; }
        public Municipality Municipality { get; set; } = null!;

        public ICollection<InitialPopulation> InitialPopulations { get; set; } = new List<InitialPopulation>();
        public ICollection<PopulationChange> PopulationChanges { get; set; } = new List<PopulationChange>();
    }
}
