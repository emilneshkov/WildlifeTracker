using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WildlifeTracker.Data;
using WildlifeTracker.ViewModels.Reports;

namespace WildlifeTracker.Services
{
    public class ReportsService
    {
        private readonly ApplicationDbContext _db;
        public ReportsService(ApplicationDbContext db) => _db = db;

        public List<SelectListItem> BuildYearOptions(int? selectedYear = null)
        {
            const int startYear = 2023;
            var currentYear = DateTime.UtcNow.Year;

            var sel = selectedYear ?? currentYear;
            if (sel < startYear) sel = startYear;
            if (sel > currentYear) sel = currentYear;

            var years = Enumerable.Range(startYear, currentYear - startYear + 1)
                .Reverse()
                .Select(y => new SelectListItem
                {
                    Value = y.ToString(),
                    Text = y.ToString(),
                    Selected = y == sel
                })
                .ToList();

            return years;
        }

        public async Task<Dictionary<(int settlementId, int speciesId), int>> GetCountsForYearAsync(int year)
        {
            var initial = await _db.InitialPopulations
                .AsNoTracking()
                .Select(x => new { x.SettlementId, x.SpeciesId, x.InitialCount })
                .ToListAsync();

            var deltas = await _db.PopulationChanges
                .AsNoTracking()
                .Where(x => x.Year <= year)
                .GroupBy(x => new { x.SettlementId, x.SpeciesId })
                .Select(g => new { g.Key.SettlementId, g.Key.SpeciesId, SumDelta = g.Sum(x => x.Delta) })
                .ToListAsync();

            var dict = initial.ToDictionary(
                k => (k.SettlementId, k.SpeciesId),
                v => v.InitialCount
            );

            foreach (var d in deltas)
            {
                var key = (d.SettlementId, d.SpeciesId);
                dict[key] = dict.TryGetValue(key, out var cur) ? cur + d.SumDelta : d.SumDelta;
            }

            return dict;
        }

        public async Task<SettlementMatrixVm> BuildSettlementMatrixAsync(int year)
        {
            var settlements = await _db.Settlements.AsNoTracking()
                .OrderBy(s => s.Name)
                .Select(s => new { s.Id, s.Name })
                .ToListAsync();

            var species = await _db.Species.AsNoTracking()
                .OrderBy(sp => sp.Name)
                .Select(sp => new { sp.Id, sp.Name })
                .ToListAsync();

            var counts = await GetCountsForYearAsync(year);

            var vm = new SettlementMatrixVm
            {
                Year = year,
                YearOptions = BuildYearOptions(year),
                SpeciesHeaders = species.Select(x => x.Name).ToList()
            };

            foreach (var st in settlements)
            {
                var row = new SettlementMatrixRowVm { SettlementName = st.Name };

                foreach (var sp in species)
                {
                    row.Cells.Add(counts.TryGetValue((st.Id, sp.Id), out var c) ? c.ToString() : "-");
                }

                vm.Rows.Add(row);
            }

            return vm;
        }

        public async Task<MunicipalityTotalVm> BuildMunicipalityTotalAsync(int year, int? municipalityId, int? speciesId)
        {
            var municipalitiesRaw = await _db.Municipalities.AsNoTracking()
                .OrderBy(m => m.Name)
                .Select(m => new { m.Id, m.Name })
                .ToListAsync();

            var municipalities = municipalitiesRaw
                .Select(m => new SelectListItem
                {
                    Value = m.Id.ToString(),
                    Text = m.Name,
                    Selected = municipalityId.HasValue && municipalityId.Value == m.Id
                })
                .ToList();

            var speciesRaw = await _db.Species.AsNoTracking()
                .OrderBy(s => s.Name)
                .Select(s => new { s.Id, s.Name })
                .ToListAsync();

            var speciesOptions = speciesRaw
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name,
                    Selected = speciesId.HasValue && speciesId.Value == s.Id
                })
                .ToList();

            var vm = new MunicipalityTotalVm
            {
                Year = year,
                YearOptions = BuildYearOptions(year),
                MunicipalityOptions = municipalities,
                MunicipalityId = municipalityId,

                SpeciesOptions = speciesOptions,
                SpeciesId = speciesId
            };

            if (municipalityId == null || speciesId == null)
                return vm;

            var settlementIds = await _db.Settlements.AsNoTracking()
                .Where(s => s.MunicipalityId == municipalityId.Value)
                .Select(s => s.Id)
                .ToListAsync();

            var counts = await GetCountsForYearAsync(year);

            vm.Total = counts
                .Where(kvp =>
                    settlementIds.Contains(kvp.Key.settlementId) &&
                    kvp.Key.speciesId == speciesId.Value)
                .Sum(kvp => kvp.Value);

            vm.MunicipalityName = municipalitiesRaw
                .FirstOrDefault(x => x.Id == municipalityId.Value)?.Name;

            vm.SpeciesName = speciesRaw
                .FirstOrDefault(x => x.Id == speciesId.Value)?.Name;

            return vm;
        }

        public async Task<SpeciesTotalVm> BuildSpeciesTotalAsync(int year, int? speciesId)
        {
            var speciesRaw = await _db.Species.AsNoTracking()
                .OrderBy(s => s.Name)
                .Select(s => new { s.Id, s.Name })
                .ToListAsync();

            var speciesOptions = speciesRaw
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name,
                    Selected = speciesId.HasValue && speciesId.Value == s.Id
                })
                .ToList();

            var vm = new SpeciesTotalVm
            {
                Year = year,
                YearOptions = BuildYearOptions(year),
                SpeciesOptions = speciesOptions,
                SpeciesId = speciesId
            };

            if (speciesId == null)
                return vm;

            var counts = await GetCountsForYearAsync(year);

            vm.Total = counts
                .Where(kvp => kvp.Key.speciesId == speciesId.Value)
                .Sum(kvp => kvp.Value);

            vm.SpeciesName = speciesRaw.FirstOrDefault(x => x.Id == speciesId.Value)?.Name;
            return vm;
        }

        public async Task<EndangeredSpeciesVm> BuildEndangeredAsync(int year)
        {
            var initialBySpecies = await _db.InitialPopulations.AsNoTracking()
                .GroupBy(x => x.SpeciesId)
                .Select(g => new { SpeciesId = g.Key, InitialSum = g.Sum(x => x.InitialCount) })
                .ToListAsync();

            var counts = await GetCountsForYearAsync(year);

            var currentBySpecies = counts
                .GroupBy(kvp => kvp.Key.speciesId)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Value));

            var speciesNames = await _db.Species.AsNoTracking()
                .ToDictionaryAsync(s => s.Id, s => s.Name);

            var items = new List<EndangeredRowVm>();

            foreach (var init in initialBySpecies)
            {
                currentBySpecies.TryGetValue(init.SpeciesId, out var currentSum);

                if (currentSum < init.InitialSum)
                {
                    items.Add(new EndangeredRowVm
                    {
                        SpeciesName = speciesNames[init.SpeciesId],
                        InitialTotal = init.InitialSum,
                        CurrentTotal = currentSum
                    });
                }
            }

            items = items.OrderBy(x => x.SpeciesName).ToList();

            return new EndangeredSpeciesVm
            {
                Year = year,
                YearOptions = BuildYearOptions(year),
                Items = items
            };
        }

        public async Task<GrowthVm> BuildGrowthAsync(int year, int? settlementId, int? speciesId)
        {
            var settlementsRaw = await _db.Settlements.AsNoTracking()
                .OrderBy(s => s.Name)
                .Select(s => new { s.Id, s.Name })
                .ToListAsync();

            var settlementOptions = settlementsRaw
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name,
                    Selected = settlementId.HasValue && settlementId.Value == s.Id
                })
                .ToList();

            var speciesRaw = await _db.Species.AsNoTracking()
                .OrderBy(s => s.Name)
                .Select(s => new { s.Id, s.Name })
                .ToListAsync();

            var speciesOptions = speciesRaw
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name,
                    Selected = speciesId.HasValue && speciesId.Value == s.Id
                })
                .ToList();

            var vm = new GrowthVm
            {
                Year = year,
                YearOptions = BuildYearOptions(year),

                SettlementOptions = settlementOptions,
                SpeciesOptions = speciesOptions,

                SettlementId = settlementId,
                SpeciesId = speciesId
            };

            if (settlementId == null || speciesId == null)
                return vm;

            var countsY = await GetCountsForYearAsync(year);
            var countsPrev = await GetCountsForYearAsync(year - 1);

            var key = (settlementId.Value, speciesId.Value);
            var current = countsY.TryGetValue(key, out var c) ? c : 0;
            var prev = countsPrev.TryGetValue(key, out var p) ? p : 0;

            vm.CurrentCount = current;
            vm.PreviousCount = prev;

            vm.PercentChange = prev == 0
                ? null
                : ((decimal)(current - prev) / prev) * 100m;

            vm.SettlementName = settlementsRaw.FirstOrDefault(x => x.Id == settlementId.Value)?.Name;
            vm.SpeciesName = speciesRaw.FirstOrDefault(x => x.Id == speciesId.Value)?.Name;

            return vm;
        }
    }
}
