using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WildlifeTracker.ViewModels.PopulationChanges
{
    public class PopulationChangeCreateVm
    {
        public string? SettlementName { get; set; }

        [Display(Name = "Вид животно")]
        [Required(ErrorMessage = "Изберете вид животно.")]
        public int? SpeciesId { get; set; }

        [Display(Name = "Година")]
        [Range(1900, 2100, ErrorMessage = "Невалидна година.")]
        public int Year { get; set; }

        [Display(Name = "Промяна (±)")]
        [Required(ErrorMessage = "Въведете промяна.")]
        public int Delta { get; set; }

        public List<SelectListItem> SpeciesOptions { get; set; } = new();
        public List<SelectListItem> YearOptions { get; set; } = new();

        public int? CurrentBefore { get; set; }
    }
}
