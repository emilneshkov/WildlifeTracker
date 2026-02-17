using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WildlifeTracker.Data;
using WildlifeTracker.Models;
using WildlifeTracker.Models.Identity;
using WildlifeTracker.Services;
using WildlifeTracker.ViewModels.PopulationChanges;

namespace WildlifeTracker.Controllers
{
    [Authorize(Roles = "Volunteer")]
    public class PopulationChangesController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly PopulationService _populationService;

        public PopulationChangesController(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            PopulationService populationService)
        {
            _db = db;
            _userManager = userManager;
            _populationService = populationService;
        }

        // GET: /PopulationChanges/Create
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (user.SettlementId == null)
                return Forbid();

            var settlement = await _db.Settlements
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == user.SettlementId.Value);

            var vm = new PopulationChangeCreateVm
            {
                SettlementName = settlement?.Name,
                Year = DateTime.UtcNow.Year
            };

            await FillDropdownsAsync(vm);

            return View(vm);
        }

        // POST: /PopulationChanges/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PopulationChangeCreateVm vm)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (user.SettlementId == null)
                return Forbid();

            await FillDropdownsAsync(vm);

            if (!ModelState.IsValid)
                return View(vm);

            var settlementId = user.SettlementId.Value;
            var speciesId = vm.SpeciesId!.Value;

            var hasInitial = await _db.InitialPopulations
                .AnyAsync(x => x.SettlementId == settlementId && x.SpeciesId == speciesId);

            if (!hasInitial)
            {
                ModelState.AddModelError("", "Липсват начални данни за избрания вид в това населено място.");
                return View(vm);
            }

            var exists = await _db.PopulationChanges.AnyAsync(x =>
                x.SettlementId == settlementId &&
                x.SpeciesId == speciesId &&
                x.Year == vm.Year);

            if (exists)
            {
                ModelState.AddModelError("", "Вече има въведени данни за тази година и вид.");
                return View(vm);
            }

            var before = await _populationService.GetCountUpToYearAsync(settlementId, speciesId, vm.Year - 1);
            vm.CurrentBefore = before;

            var (ok, error) = await _populationService.CanApplyDeltaAsync(settlementId, speciesId, vm.Year, vm.Delta);
            if (!ok)
            {
                ModelState.AddModelError(nameof(vm.Delta), error!);
                return View(vm);
            }

            var entity = new PopulationChange
            {
                SettlementId = settlementId,
                SpeciesId = speciesId,
                Year = vm.Year,
                Delta = vm.Delta,
                EnteredByUserId = user.Id,
                CreatedAtUtc = DateTime.UtcNow
            };

            _db.PopulationChanges.Add(entity);

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Неуспешно записване. Вероятно вече има запис за тази година и вид.");
                return View(vm);
            }

            TempData["Success"] = "Данните са записани успешно.";
            return RedirectToAction(nameof(MyHistory));
        }

        // GET: /PopulationChanges/MyHistory
        public async Task<IActionResult> MyHistory()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            if (user.SettlementId == null) return Forbid();

            var settlementId = user.SettlementId.Value;

            var items = await _db.PopulationChanges
                .AsNoTracking()
                .Where(x => x.SettlementId == settlementId)
                .OrderByDescending(x => x.Year)
                .ThenBy(x => x.Species.Name)
                .Select(x => new
                {
                    x.Id,
                    Species = x.Species.Name,
                    x.Year,
                    x.Delta,
                    x.CreatedAtUtc
                })
                .ToListAsync();

            ViewBag.SettlementName = await _db.Settlements
                .AsNoTracking()
                .Where(s => s.Id == settlementId)
                .Select(s => s.Name)
                .FirstOrDefaultAsync();

            return View(items);
        }

        private async Task FillDropdownsAsync(PopulationChangeCreateVm vm)
        {
            var species = await _db.Species
                .AsNoTracking()
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name
                })
                .ToListAsync();

            vm.SpeciesOptions = species;

            var currentYear = DateTime.UtcNow.Year;

            vm.YearOptions = new List<SelectListItem>
    {
        new SelectListItem
        {
            Value = currentYear.ToString(),
            Text = currentYear.ToString(),
            Selected = true
        }
    };

            vm.Year = currentYear;
        }

    }
}
