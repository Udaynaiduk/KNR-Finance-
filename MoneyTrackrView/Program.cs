using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using MoneyTrackr.Borrowers.Repository;
using MoneyTrackr.Borrowers.Services;
using System.Globalization;

namespace MoneyTrackrView
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            // Read connection string from environment variable
            var connectionString = Environment.GetEnvironmentVariable("MoneyTrackrDatabase");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("MoneyTrackrDatabase environment variable is not set.");
            }

            builder.Services.AddDbContext<MoneyTrackrDbContext>(options =>
                options.UseMySql(
                    connectionString,
                    new MySqlServerVersion(new Version(8, 0, 36))  // Adjust version if needed
                )
            );
            var cultureInfo = new CultureInfo("en-IN");
            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;


            var defaultCulture = new CultureInfo("en-GB");
            CultureInfo.DefaultThreadCurrentCulture = defaultCulture;
            CultureInfo.DefaultThreadCurrentUICulture = defaultCulture;
            // Add authentication
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Home/SignIn"; // Redirect here if not logged in
                    options.AccessDeniedPath = "/Home/AccessDenied"; // Redirect here if role is denied
                    options.ExpireTimeSpan = TimeSpan.FromHours(1);
                });

            builder.Services.AddAuthorization();

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            builder.Services.AddScoped<ILoanService, LoanService>();
            var app = builder.Build();
            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("en-GB"),
                SupportedCultures = new List<CultureInfo> { defaultCulture },
                SupportedUICultures = new List<CultureInfo> { defaultCulture }
            });
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
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
