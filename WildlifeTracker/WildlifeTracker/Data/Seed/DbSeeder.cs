using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WildlifeTracker.Models;
using WildlifeTracker.Models.Identity;

namespace WildlifeTracker.Data.Seed
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            await context.Database.MigrateAsync();

            // ---------------------
            // Roles
            // ---------------------
            string[] roles = { "Employee", "Volunteer" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // ---------------------
            // Municipalities
            // ---------------------
            var municipalityNames = new[]
            {
                "Велико Търново",
                "Горна Оряховица",
                "Свищов",
                "Павликени",
                "Елена",
                "Полски Тръмбеш",
                "Лясковец",
                "Стражица",
                "Сухиндол",
                "Златарица"
            };

            var existingMunicipalities = await context.Municipalities
                .AsNoTracking()
                .Select(m => m.Name)
                .ToListAsync();

            var missingMunicipalities = municipalityNames
                .Where(n => !existingMunicipalities.Contains(n))
                .Select(n => new Municipality { Name = n })
                .ToList();

            if (missingMunicipalities.Any())
            {
                context.Municipalities.AddRange(missingMunicipalities);
                await context.SaveChangesAsync();
            }

            var municipalities = await context.Municipalities
                .AsNoTracking()
                .ToDictionaryAsync(m => m.Name, m => m.Id);

            // ---------------------
            // Settlements
            // ---------------------
            var settlementsByMunicipality = new Dictionary<string, string[]>
            {
                ["Велико Търново"] = new[] { "Арбанаси", "Самоводене", "Ресен", "Шемшево", "Килифарево" },
                ["Горна Оряховица"] = new[] { "Горна Оряховица", "Долна Оряховица", "Първомайци", "Поликраище", "Правда" },
                ["Свищов"] = new[] { "Свищов", "Овча могила", "Царевец", "Българско Сливово", "Вардим" },
                ["Павликени"] = new[] { "Павликени", "Бяла черква", "Върбовка", "Дъскот", "Михалци" },
                ["Елена"] = new[] { "Елена", "Мийковци", "Константин", "Беброво", "Яковци" },
                ["Полски Тръмбеш"] = new[] { "Полски Тръмбеш", "Климентово", "Обединение", "Петко Каравелово", "Раданово" },
                ["Лясковец"] = new[] { "Лясковец", "Джулюница", "Козаревец", "Добри дял", "Драгижево" },
                ["Стражица"] = new[] { "Стражица", "Камен", "Благоево", "Балканци", "Виноград" },
                ["Сухиндол"] = new[] { "Сухиндол", "Горско Косово", "Коевци", "Въглевци", "Караисен" },
                ["Златарица"] = new[] { "Златарица", "Горско Ново село", "Родина", "Дединци", "Средно село" }
            };

            var existingSettlements = await context.Settlements
                .AsNoTracking()
                .Select(s => new { s.MunicipalityId, s.Name })
                .ToListAsync();

            var existingSet = new HashSet<(int municipalityId, string name)>(
                existingSettlements.Select(x => (x.MunicipalityId, x.Name))
            );

            var settlementsToAdd = new List<Settlement>();

            foreach (var kvp in settlementsByMunicipality)
            {
                var municipalityName = kvp.Key;
                var municipalityId = municipalities[municipalityName];

                foreach (var sName in kvp.Value)
                {
                    var key = (municipalityId, sName);
                    if (existingSet.Contains(key)) continue;

                    settlementsToAdd.Add(new Settlement
                    {
                        Name = sName,
                        MunicipalityId = municipalityId
                    });

                    existingSet.Add(key);
                }
            }

            if (settlementsToAdd.Any())
            {
                context.Settlements.AddRange(settlementsToAdd);
                await context.SaveChangesAsync();
            }

            // ---------------------
            // Species
            // ---------------------
            var speciesNames = new[]
            {
                "Сърна",
                "Елен",
                "Дива свиня",
                "Лисица",
                "Вълк",
                "Заек",
                "Язовец",
                "Дива котка",
                "Шакал",
                "Мечка"
            };

            var existingSpecies = await context.Species
                .AsNoTracking()
                .Select(s => s.Name)
                .ToListAsync();

            var missingSpecies = speciesNames
                .Where(n => !existingSpecies.Contains(n))
                .Select(n => new Species { Name = n })
                .ToList();

            if (missingSpecies.Any())
            {
                context.Species.AddRange(missingSpecies);
                await context.SaveChangesAsync();
            }

            // ---------------------
            // Initial Populations
            // ---------------------
            var settlementIds = await context.Settlements.AsNoTracking().Select(s => s.Id).ToListAsync();
            var speciesIds = await context.Species.AsNoTracking().Select(s => s.Id).ToListAsync();

            var existingInitialPairs = await context.InitialPopulations
                .AsNoTracking()
                .Select(ip => new { ip.SettlementId, ip.SpeciesId })
                .ToListAsync();

            var initialSet = new HashSet<(int st, int sp)>(existingInitialPairs.Select(x => (x.SettlementId, x.SpeciesId)));

            var initialToAdd = new List<InitialPopulation>();

            foreach (var stId in settlementIds)
            {
                foreach (var spId in speciesIds)
                {
                    if (initialSet.Contains((stId, spId))) continue;

                    var val = 10 + ((stId * 3 + spId * 7) % 191);

                    initialToAdd.Add(new InitialPopulation
                    {
                        SettlementId = stId,
                        SpeciesId = spId,
                        InitialCount = val
                    });

                    initialSet.Add((stId, spId));
                }
            }

            if (initialToAdd.Any())
            {
                context.InitialPopulations.AddRange(initialToAdd);
                await context.SaveChangesAsync();
            }

            // ---------------------
            // Employee user 
            // ---------------------
            const string employeeEmail = "employee@test.com";
            var existingEmployee = await userManager.FindByEmailAsync(employeeEmail);
            if (existingEmployee == null)
            {
                var employee = new ApplicationUser
                {
                    UserName = employeeEmail,
                    Email = employeeEmail,
                    EmailConfirmed = true
                };

                await userManager.CreateAsync(employee, "Employee123!");
                await userManager.AddToRoleAsync(employee, "Employee");
            }
            else
            {
                if (!await userManager.IsInRoleAsync(existingEmployee, "Employee"))
                    await userManager.AddToRoleAsync(existingEmployee, "Employee");
            }

            // ---------------------
            // Volunteers
            // ---------------------
            var firstNames = new[]
            {
                "Иван","Георги","Димитър","Николай","Петър",
                "Александър","Стоян","Тодор","Васил","Христо",
                "Мария","Елена","Десислава","Ивелина","Гергана",
                "Теодора","Надежда","Радостина","Виктория","Силвия"
            };

            var lastNames = new[]
            {
                "Иванов","Петров","Димитров","Николов","Георгиев",
                "Стоянов","Тодоров","Василев","Христов","Александров",
                "Маринова","Николова","Георгиева","Петрова","Иванова",
                "Димитрова","Тодорова","Стоянова","Василева","Христова"
            };

            var rnd = new Random(42);

            var usedSettlementIds = await context.Users
                .Where(u => u.SettlementId != null)
                .Select(u => u.SettlementId!.Value)
                .ToListAsync();

            var usedSet = new HashSet<int>(usedSettlementIds);

            var allSettlements = await context.Settlements
                .OrderBy(s => s.Id)
                .Select(s => new { s.Id, s.Name })
                .ToListAsync();

            int nameIndex = 0;

            foreach (var s in allSettlements)
            {
                if (usedSet.Contains(s.Id))
                    continue;

                var firstName = firstNames[nameIndex % firstNames.Length];
                var lastName = lastNames[nameIndex % lastNames.Length];
                nameIndex++;

                var email = $"vol{s.Id}@test.com";

                var volunteer = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    FirstName = firstName,
                    LastName = lastName,
                    SettlementId = s.Id
                };

                var result = await userManager.CreateAsync(volunteer, "Volunteer123!");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(volunteer, "Volunteer");
                    usedSet.Add(s.Id);
                }
            }

            // ---------------------
            // Seed sample PopulationChanges 
            // ---------------------
            if (!await context.PopulationChanges.AnyAsync())
            {
                var years = new[] { 2023, 2024, 2025 };

                var initialDict = await context.InitialPopulations
                    .AsNoTracking()
                    .ToDictionaryAsync(x => (x.SettlementId, x.SpeciesId), x => x.InitialCount);

                var currentDict = new Dictionary<(int st, int sp), int>(initialDict);

                var changesToAdd = new List<PopulationChange>();

                var volunteerUsers = await context.Users
                    .AsNoTracking()
                    .Where(u => u.SettlementId != null)
                    .Select(u => new { u.Id, SettlementId = u.SettlementId!.Value })
                    .ToListAsync();

                var volunteerBySettlement = volunteerUsers
                    .GroupBy(x => x.SettlementId)
                    .ToDictionary(g => g.Key, g => g.First().Id);


                var endangeredSpeciesNames = new[]
                {
                    "Мечка",
                    "Вълк",
                    "Дива котка"
                };

                var endangeredSpeciesIds = await context.Species
                    .AsNoTracking()
                    .Where(s => endangeredSpeciesNames.Contains(s.Name))
                    .Select(s => s.Id)
                    .ToListAsync();

                foreach (var year in years)
                {
                    foreach (var stId in settlementIds)
                    {
                        foreach (var spId in speciesIds)
                        {
                            int raw;

                            if (endangeredSpeciesIds.Contains(spId))
                            {
                                raw = -5 - ((stId + year) % 8);
                            }
                            else
                            {
                                raw = ((stId * 11 + spId * 17 + year) % 21) - 8;
                            }
                            var key = (stId, spId);

                            var before = currentDict[key];

                            var delta = raw;
                            if (before + delta < 0)
                                delta = -before;

                            if (!volunteerBySettlement.TryGetValue(stId, out var enteredBy))
                                continue;

                            changesToAdd.Add(new PopulationChange
                            {
                                SettlementId = stId,
                                SpeciesId = spId,
                                Year = year,
                                Delta = delta,
                                EnteredByUserId = enteredBy,
                                CreatedAtUtc = DateTime.UtcNow
                            });

                            currentDict[key] = before + delta;
                        }
                    }
                }

                context.PopulationChanges.AddRange(changesToAdd);
                await context.SaveChangesAsync();
            }
           
        }
    }
}
