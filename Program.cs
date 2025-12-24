using Microsoft.EntityFrameworkCore;
using RSWEBproekt.Data;

namespace RSWEBproekt
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            //ova znaci deka koga ke treba ApplicationDbContext, kreiraj go i daj mu gi ovie podesuvanja
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            //da ima baza 
            options.UseSqlServer(
            //zemeno od appsettings.json
            builder.Configuration.GetConnectionString("DefaultConnection")));

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
//Koga app se startuva:
//1️ ASP.NET Core cita Program.cs
//2️ Go registrira ApplicationDbContext
//3️ Go povrzuva so SQL Server
//4️ Koga nekoj kontoler pobara DbContext → DI go dava