using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using _402410745_Staybnb_assignment.Data;
using _402410745_Staybnb_assignment.Models;

var builder = WebApplication.CreateBuilder(args);

// Database (SQL Server through EF Core)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Login system + roles (Guest, Host, Admin, SuperAdmin)
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireDigit = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// Re-read the user's roles on every request, so a role change (Guest -> Host)
// works straight away without the user having to log out and in again.
builder.Services.Configure<SecurityStampValidatorOptions>(options =>
    options.ValidationInterval = TimeSpan.Zero);

builder.Services.AddControllersWithViews();

var app = builder.Build();

// On startup: create/update the database, then seed roles + SuperAdmin.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (db.Database.GetMigrations().Any())
        db.Database.Migrate();        // uses the EF Core migrations
    else
        db.Database.EnsureCreated();  // fallback if no migration has been added yet
    await RoleSeeder.SeedAsync(scope.ServiceProvider, app.Configuration);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
