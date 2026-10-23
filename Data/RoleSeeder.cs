using Microsoft.AspNetCore.Identity;
using _402410745_Staybnb_assignment.Models;

namespace _402410745_Staybnb_assignment.Data;

// Runs once on startup: creates the roles, the SuperAdmin user and a few amenities.
public static class RoleSeeder
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration config)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var db = services.GetRequiredService<ApplicationDbContext>();

        // 1. Roles
        string[] roles = { "SuperAdmin", "Admin", "Host", "Guest" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // 2. SuperAdmin user (details come from appsettings.json under SUperAdmin Group)
        var email = config["SuperAdmin:Email"];
        var superAdmin = await userManager.FindByEmailAsync(email);
        if (superAdmin == null)
        {
            superAdmin = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FirstName = config["SuperAdmin:FirstName"],
                LastName = config["SuperAdmin:LastName"]
            };
            var result = await userManager.CreateAsync(superAdmin, config["SuperAdmin:Password"]);
            if (result.Succeeded)
                await userManager.AddToRoleAsync(superAdmin, "SuperAdmin");
        }

        // 3. Amenities/avaiable features
        if (!db.Amenities.Any())
        {
            string[] names = { "WiFi", "Kitchen", "Washer", "Dryer", "Air Conditioning", "Heating", "Free Parking", "Pool", "Braai", "Balcony" };
            foreach (var name in names)
                db.Amenities.Add(new Amenity { Name = name });
            await db.SaveChangesAsync();
        }
    }
}
