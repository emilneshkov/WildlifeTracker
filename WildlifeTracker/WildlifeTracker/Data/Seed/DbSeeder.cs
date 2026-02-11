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
            // Municipalities (10)
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

            if (!await context.Municipalities.AnyAsync())
            {
                context.Municipalities.AddRange(
                    municipalityNames.Select(n => new Municipality { Name = n })
                );
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
                ["Горна Оряховица"] = new[] { "Горна Оряховица", "Лясковец (гр.)", "Долна Оряховица", "Първомайци", "Поликраище" },
                ["Свищов"] = new[] { "Свищов", "Овча могила", "Царевец", "Българско Сливово", "Вардим" },
                ["Павликени"] = new[] { "Павликени", "Бяла черква", "Върбовка", "Дъскот", "Михалци" },
                ["Елена"] = new[] { "Елена", "Мийковци", "Константин", "Беброво", "Яковци" },
                ["Полски Тръмбеш"] = new[] { "Полски Тръмбеш", "Климентово", "Обединение", "Петко Каравелово", "Раданово" },
                ["Лясковец"] = new[] { "Лясковец", "Джулюница", "Козаревец", "Добри дял", "Драгижево" },
                ["Стражица"] = new[] { "Стражица", "Камен", "Благоево", "Балканци", "Виноград" },
                ["Сухиндол"] = new[] { "Сухиндол", "Горско Косово", "Коевци", "Въглевци", "Караисен" },
                ["Златарица"] = new[] { "Златарица", "Горско Ново село", "Родина", "Дединци", "Средно село" }
            };

            if (!await context.Settlements.AnyAsync())
            {
                var settlementsToAdd = new List<Settlement>();

                foreach (var kvp in settlementsByMunicipality)
                {
                    var municipalityName = kvp.Key;
                    var settlementNames = kvp.Value;

                    var municipalityId = municipalities[municipalityName];

                    foreach (var sName in settlementNames)
                    {
                        settlementsToAdd.Add(new Settlement
                        {
                            Name = sName,
                            MunicipalityId = municipalityId
                        });
                    }
                }

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

            if (!await context.Species.AnyAsync())
            {
                context.Species.AddRange(speciesNames.Select(n => new Species { Name = n }));
                await context.SaveChangesAsync();
            }

            // ---------------------
            // Initial Populations 
            // ---------------------

            var settlementIds = await context.Settlements.AsNoTracking().Select(s => s.Id).ToListAsync();
            var speciesIds = await context.Species.AsNoTracking().Select(s => s.Id).ToListAsync();

            if (!await context.InitialPopulations.AnyAsync())
            {
                var initialList = new List<InitialPopulation>();

                foreach (var stId in settlementIds)
                {
                    foreach (var spId in speciesIds)
                    {
                        var val = 10 + ((stId * 3 + spId * 7) % 191);

                        initialList.Add(new InitialPopulation
                        {
                            SettlementId = stId,
                            SpeciesId = spId,
                            InitialCount = val
                        });
                    }
                }

                context.InitialPopulations.AddRange(initialList);
                await context.SaveChangesAsync();
            }

            // ---------------------
            // Test users
            // ---------------------
            // Employee
            const string employeeEmail = "employee@test.com";
            var existingEmployee = await userManager.FindByEmailAsync(employeeEmail);
            if (existingEmployee == null)
            {
                var employee = new ApplicationUser
                {
                    UserName = employeeEmail,
                    Email = employeeEmail
                };

                await userManager.CreateAsync(employee, "Employee123!");
                await userManager.AddToRoleAsync(employee, "Employee");
            }

            // Volunteer
            const string volunteerEmail = "volunteer@test.com";
            var existingVolunteer = await userManager.FindByEmailAsync(volunteerEmail);

            var firstSettlementId = await context.Settlements.AsNoTracking()
                .OrderBy(s => s.Id)
                .Select(s => s.Id)
                .FirstAsync();

            var volunteerForSettlementExists = await context.Users
                .AsNoTracking()
                .AnyAsync(u => u.SettlementId == firstSettlementId);

            if (existingVolunteer == null)
            {
                if (volunteerForSettlementExists)
                {

                }
                else
                {
                    var volunteer = new ApplicationUser
                    {
                        UserName = volunteerEmail,
                        Email = volunteerEmail,
                        FirstName = "Иван",
                        LastName = "Иванов",
                        SettlementId = firstSettlementId
                    };

                    await userManager.CreateAsync(volunteer, "Volunteer123!");
                    await userManager.AddToRoleAsync(volunteer, "Volunteer");
                }
            }
            else
            {
                if (existingVolunteer.SettlementId == null)
                    existingVolunteer.SettlementId = firstSettlementId;

                if (!await userManager.IsInRoleAsync(existingVolunteer, "Volunteer"))
                    await userManager.AddToRoleAsync(existingVolunteer, "Volunteer");

                await userManager.UpdateAsync(existingVolunteer);
            }
        }
    }
}
