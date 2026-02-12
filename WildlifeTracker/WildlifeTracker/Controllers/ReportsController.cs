using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WildlifeTracker.Services;

namespace WildlifeTracker.Controllers
{
    [Authorize(Roles = "Employee")]
    public class ReportsController : Controller
    {
        private readonly ReportsService _reports;

        public ReportsController(ReportsService reports) => _reports = reports;

        public IActionResult Index() => View();

        public async Task<IActionResult> SettlementMatrix(int year = 0)
        {
            if (year == 0) year = DateTime.UtcNow.Year;
            var vm = await _reports.BuildSettlementMatrixAsync(year);
            return View(vm);
        }

        public async Task<IActionResult> MunicipalityTotal(int year = 0, int? municipalityId = null, int? speciesId = null)
        {
            if (year == 0) year = DateTime.UtcNow.Year;
            var vm = await _reports.BuildMunicipalityTotalAsync(year, municipalityId, speciesId);
            return View(vm);
        }

        public async Task<IActionResult> SpeciesTotal(int year = 0, int? speciesId = null)
        {
            if (year == 0) year = DateTime.UtcNow.Year;
            var vm = await _reports.BuildSpeciesTotalAsync(year, speciesId);
            return View(vm);
        }

        public async Task<IActionResult> EndangeredSpecies(int year = 0)
        {
            if (year == 0) year = DateTime.UtcNow.Year;
            var vm = await _reports.BuildEndangeredAsync(year);
            return View(vm);
        }

        public async Task<IActionResult> Growth(int year = 0, int? settlementId = null, int? speciesId = null)
        {
            if (year == 0) year = DateTime.UtcNow.Year;
            var vm = await _reports.BuildGrowthAsync(year, settlementId, speciesId);
            return View(vm);
        }
    }
}
