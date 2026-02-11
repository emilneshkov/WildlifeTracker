using System.ComponentModel.DataAnnotations;

namespace WildlifeTracker.Models
{
    public class Municipality
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = null!;

        public ICollection<Settlement> Settlements { get; set; } = new List<Settlement>();
    }
}
