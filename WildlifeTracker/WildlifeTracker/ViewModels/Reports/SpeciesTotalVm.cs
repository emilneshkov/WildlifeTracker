using Microsoft.AspNetCore.Mvc.Rendering;

namespace WildlifeTracker.ViewModels.Reports
{
    public class SpeciesTotalVm : ReportSelectYearVm
    {
        public int? SpeciesId { get; set; }
        public List<SelectListItem> SpeciesOptions { get; set; } = new();

        public string? SpeciesName { get; set; }
        public int? Total { get; set; }
    }
}
