using Microsoft.AspNetCore.Mvc.Rendering;

namespace WildlifeTracker.ViewModels.Reports
{
    public class ReportSelectYearVm
    {
        public int Year { get; set; }
        public List<SelectListItem> YearOptions { get; set; } = new();
    }
}