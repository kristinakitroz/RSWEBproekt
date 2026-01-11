using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RSWEBproekt.Data;
using RSWEBproekt.Models;

namespace RSWEBproekt
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            //  Seed method: Roles + Admin
            static async Task SeedRolesAndUsersAsync(IServiceProvider services)
            {
                // za ulogi (roles)
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

                //  mora da e ApplicationUser 
                var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

                string[] roles = { "Admin", "Professor", "Student" };

                foreach (var role in roles)
                    if (!await roleManager.RoleExistsAsync(role))
                        await roleManager.CreateAsync(new IdentityRole(role));

                
                // Admin user
               
                var adminEmail = "admin@rsweb.com";
                var adminUser = await userManager.FindByEmailAsync(adminEmail);

                if (adminUser == null)
                {
                    adminUser = new ApplicationUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        EmailConfirmed = true
                    };

                    await userManager.CreateAsync(adminUser, "Admin123!");
                }

                if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
                    await userManager.AddToRoleAsync(adminUser, "Admin");
            }

            var builder = WebApplication.CreateBuilder(args);

         
            builder.Services.AddControllersWithViews();

            // ova znaci deka koga ke treba ApplicationDbContext, kreiraj go i daj mu gi ovie podesuvanja
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                // da ima baza
                options.UseSqlServer(
                    // zemeno od appsettings.json
                    builder.Configuration.GetConnectionString("DefaultConnection")));

            // Identity + Roles (cookies auth)
            builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

            builder.Services.AddRazorPages(); //  treba za Identity UI pages

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication(); 

            //  Force change password (ako korisnikot ima MustChangePassword=true)
            app.Use(async (context, next) =>
            {
                if (context.User.Identity?.IsAuthenticated == true)
                {
                    // ne redirektiraj ako vekje e na stranicata za promena, login ili logout
                    var path = context.Request.Path.Value?.ToLower() ?? "";

                    if (!path.StartsWith("/account/forcechangepassword") &&
                        !path.StartsWith("/identity/account/logout") &&
                        !path.StartsWith("/identity/account/login"))
                    {
                        using var scope = context.RequestServices.CreateScope();
                        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                        var user = await userManager.GetUserAsync(context.User);

                        if (user != null && user.MustChangePassword)
                        {
                            context.Response.Redirect("/Account/ForceChangePassword");
                            return;
                        }
                    }
                }

                await next();
            });

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapRazorPages(); //  Identity endpoints (/Identity/Account/Login...)

            //  Seed roles + users on startup
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                await SeedRolesAndUsersAsync(services);
            }

            app.Run();
        }
    }
}

// Koga app se startuva:
// 1️ ASP.NET Core cita Program.cs
// 2️ Go registrira ApplicationDbContext
// 3️ Go povrzuva so SQL Server
// 4️ Koga nekoj kontoler pobara DbContext → DI go dava
