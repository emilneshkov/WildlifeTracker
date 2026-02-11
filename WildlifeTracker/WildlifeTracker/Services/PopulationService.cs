using Microsoft.EntityFrameworkCore;
using WildlifeTracker.Data;

namespace WildlifeTracker.Services
{
    public class PopulationService
    {
        private readonly ApplicationDbContext _db;

        public PopulationService(ApplicationDbContext db) => _db = db;

 
        public async Task<int> GetCountUpToYearAsync(int settlementId, int speciesId, int year)
        {
            var initial = await _db.InitialPopulations
                .Where(x => x.SettlementId == settlementId && x.SpeciesId == speciesId)
                .Select(x => (int?)x.InitialCount)
                .FirstOrDefaultAsync() ?? 0;

            var sumDelta = await _db.PopulationChanges
                .Where(x => x.SettlementId == settlementId && x.SpeciesId == speciesId && x.Year <= year)
                .SumAsync(x => (int?)x.Delta) ?? 0;

            return initial + sumDelta;
        }


        public async Task<(bool ok, string? error)> CanApplyDeltaAsync(int settlementId, int speciesId, int year, int delta)
        {
            var before = await GetCountUpToYearAsync(settlementId, speciesId, year - 1);
            var after = before + delta;

            if (after < 0)
                return (false, "Въведената промяна води до отрицателен брой животни.");

            return (true, null);
        }
    }
}
