using Microsoft.AspNetCore.Mvc.Rendering;

namespace WildlifeTracker.ViewModels.Reports
{
    public class GrowthVm : ReportSelectYearVm
    {
        public int? SettlementId { get; set; }
        public int? SpeciesId { get; set; }

        public List<SelectListItem> SettlementOptions { get; set; } = new();
        public List<SelectListItem> SpeciesOptions { get; set; } = new();

        public string? SettlementName { get; set; }
        public string? SpeciesName { get; set; }

        public int? PreviousCount { get; set; }
        public int? CurrentCount { get; set; }

        public decimal? PercentChange { get; set; }
    }
}
