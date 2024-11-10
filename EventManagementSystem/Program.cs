using EventManagementSystem.Data;
using EventManagementSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Add DbContext with PostgreSQL connection
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("conn")));

// Add ASP.NET Core Identity with custom ApplicationUser
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    
    .AddDefaultTokenProviders();

// Add services for controllers and views
builder.Services.AddControllersWithViews();

// Add authentication and authorization services
builder.Services.AddAuthorization();

// Build the app
var app = builder.Build();

// Seed roles and users (like Admin) if they do not exist
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    await InitializeRoles(services, userManager, roleManager);
    await SeedDatabase(services, userManager);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Enable authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Configure the default route for MVC
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Event}/{action=Index}/{id?}");

app.Run();

// Method to initialize roles (Admin and User) in the database
 static async Task InitializeRoles(IServiceProvider serviceProvider, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
{
    string[] roleNames = { "Admin", "User" };

    foreach (var roleName in roleNames)
    {
        var roleExist = await roleManager.RoleExistsAsync(roleName);
        if (!roleExist)
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    // Create default Admin user if it doesn't exist
    var user = await userManager.FindByEmailAsync("admin@example.com");
    if (user == null)
    {
        user = new ApplicationUser { UserName = "admin@example.com", Email = "admin@example.com" };
        var result = await userManager.CreateAsync(user, "Test@123");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, "Admin");
        }
    }
}

// Optional: Method to seed the database with initial data (like default events)
 static async Task SeedDatabase(IServiceProvider serviceProvider, UserManager<ApplicationUser> userManager)
{
    var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

    // Example: Seed some default events if needed
    if (!context.Events.Any())
    {
        context.Events.AddRange(new List<Event>
    {
        new Event
        {
            Title = "Sample Event 1",
            Date = DateTime.Now.AddDays(5).ToString("yyyy-MM-ddTHH:mm:ss"),
            Location = "Location 1",
            MaxParticipants = 100,
            Description = "This is a description for the sample event."
        },
        new Event
        {
            Title = "Sample Event 2",
            Date = DateTime.Now.AddDays(10).ToString("yyyy-MM-ddTHH:mm:ss"),
            Location = "Location 2",
            MaxParticipants = 150,
            Description = "Description for the second event."
        }
    });
        await context.SaveChangesAsync();
    }

}
