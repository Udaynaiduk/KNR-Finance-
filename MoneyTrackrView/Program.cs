using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
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
                    new MySqlServerVersion(new Version(8, 0, 36))
                )
            );

            // Cultures
            var defaultCulture = new CultureInfo("en-GB");
            CultureInfo.DefaultThreadCurrentCulture = defaultCulture;
            CultureInfo.DefaultThreadCurrentUICulture = defaultCulture;

            // Data Protection: persist keys inside content root so Docker can mount them
            var keysPath = Path.Combine(builder.Environment.ContentRootPath, "keys");
            builder.Services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(keysPath))
                .SetApplicationName("MoneyTrackrApp");

            // Antiforgery cookie settings
            builder.Services.AddAntiforgery(options =>
            {
                options.Cookie.Name = "MoneyTrackr.AntiForgery";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.Cookie.SameSite = SameSiteMode.Lax;
            });

            // AuthN/AuthZ
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Home/SignIn";
                    options.AccessDeniedPath = "/Home/AccessDenied";
                    options.ExpireTimeSpan = TimeSpan.FromHours(1);
                });
            builder.Services.AddAuthorization();

            // Services + MVC
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

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
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