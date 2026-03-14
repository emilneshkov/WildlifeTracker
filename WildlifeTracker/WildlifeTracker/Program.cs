using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WildlifeTracker.Data;
using WildlifeTracker.Data.Seed;
using WildlifeTracker.Models.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();


builder.Services.AddControllersWithViews()
    .AddMvcOptions(options =>
    {
        options.ModelBindingMessageProvider.SetValueIsInvalidAccessor(
            value => "Моля въведете валидно число.");

        options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(
            _ => "Полето е задължително.");

        options.ModelBindingMessageProvider.SetValueIsInvalidAccessor(
            value => $"Стойността '{value}' е невалидна.");

        options.ModelBindingMessageProvider.SetMissingBindRequiredValueAccessor(
            name => $"Липсва стойност за '{name}'.");
    });
builder.Services.AddRazorPages();

builder.Services.AddScoped<WildlifeTracker.Services.PopulationService>();
builder.Services.AddScoped<WildlifeTracker.Services.ReportsService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();


using (var scope = app.Services.CreateScope())
{
    await DbSeeder.SeedAsync(scope.ServiceProvider);
}

app.Run();
