


using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.Authorization;
using WebColegio.Services;

namespace WebColegio
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            //builder.Services.AddRazorPages();
            builder.Services.AddControllers();
            builder.Services.AddScoped<IServicesApi, ServicesApi>();
            //builder.Services.AddControllersWithViews();
            builder.Services.AddControllersWithViews(options =>
            {
                // Proteger todas las acciones por defecto
                options.Filters.Add(new AuthorizeFilter());
            });
            builder.Services.AddHttpContextAccessor();

            builder.Services.AddDistributedMemoryCache(); // Requerido para sesione
            builder.Services.AddSession(
                options =>
                {
                    options.IdleTimeout = TimeSpan.FromMinutes(30);
                    options.Cookie.HttpOnly = true;
                    options.Cookie.IsEssential = true;
                });
            //builder.Configuration["ApiUrl"] = Environment.GetEnvironmentVariable("API_URL");
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Login/Login";  // Ruta completa al método Login
                    options.AccessDeniedPath = "/Login/Login";  // Redirigir a login si no tiene acceso
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                    options.SlidingExpiration = true;
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                });

            // En Program.cs (ASP.NET Core 6+)
          
            var app = builder.Build();


                    
         

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseSession();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Login}/{action=Login}/{id?}");


            app.Run();
        }
    }
}
