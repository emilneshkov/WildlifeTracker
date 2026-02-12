using Microsoft.AspNetCore.Mvc.Rendering;

namespace WildlifeTracker.ViewModels.Reports
{
    public class MunicipalityTotalVm : ReportSelectYearVm
    {
        public int? MunicipalityId { get; set; }
        public List<SelectListItem> MunicipalityOptions { get; set; } = new();

        public string? MunicipalityName { get; set; }
        public int? Total { get; set; }
        public int? SpeciesId { get; set; }
        public List<SelectListItem> SpeciesOptions { get; set; } = new();
        public string? SpeciesName { get; set; }
    }
}
