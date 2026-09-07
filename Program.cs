

using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.IdentityModel.Tokens;
using WebColegio.Configuration;
using WebColegio.Services;

namespace WebColegio
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.Configure<JwtSettings>(options =>
            {
                builder.Configuration.GetSection(JwtSettings.SectionName).Bind(options);
                var fromEnv = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");
                if (!string.IsNullOrWhiteSpace(fromEnv))
                    options.SecretKey = fromEnv;
            });
            builder.Services.Configure<ApiSettings>(options =>
            {
                builder.Configuration.GetSection(ApiSettings.SectionName).Bind(options);
                var baseUrlEnv = Environment.GetEnvironmentVariable("ApiSettings__BaseUrl");
                if (!string.IsNullOrWhiteSpace(baseUrlEnv))
                    options.BaseUrl = baseUrlEnv;
                var hostEnv = Environment.GetEnvironmentVariable("ApiSettings__Host");
                if (!string.IsNullOrWhiteSpace(hostEnv))
                    options.Host = hostEnv;
            });

            builder.Services.AddControllers();
            builder.Services.AddMemoryCache();
            builder.Services.Configure<WhatsAppSettings>(builder.Configuration.GetSection(WhatsAppSettings.SectionName));
            builder.Services.AddHttpClient("WhatsAppGraph");
            builder.Services.AddSingleton<WhatsAppCampanaStore>();
            builder.Services.AddScoped<IWhatsAppSender, WhatsAppCloudSender>();
            builder.Services.AddScoped<WhatsAppAvisoService>();
            builder.Services.AddHostedService<WhatsAppMensualHostedService>();
            builder.Services.AddScoped<IServicesApi, ServicesApi>();
            builder.Services.AddScoped<IMenuPermisoService, MenuPermisoService>();
            builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
            builder.Services.AddScoped<IApiTokenAccessor, ApiTokenAccessor>();
            builder.Services.AddTransient<ApiHostHeaderHandler>();
            builder.Services.AddTransient<ApiBearerTokenHandler>();

            var allowInvalidApiCertificate = builder.Configuration
                .GetSection(ApiSettings.SectionName)
                .GetValue<bool>(nameof(ApiSettings.AllowInvalidCertificate));

            builder.Services.AddHttpClient("ColegioApi")
                .ConfigureHttpClient((sp, client) =>
                {
                    var configuration = sp.GetRequiredService<IConfiguration>();
                    var baseUrl = configuration["ApiSettings:BaseUrl"]
                        ?? Environment.GetEnvironmentVariable("ApiSettings__BaseUrl");
                    if (!string.IsNullOrWhiteSpace(baseUrl))
                    {
                        client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
                    }

                    client.Timeout = TimeSpan.FromSeconds(60);
                })
                .ConfigurePrimaryHttpMessageHandler(() =>
                {
                    var handler = new HttpClientHandler
                    {
                        UseProxy = false
                    };
                    if (allowInvalidApiCertificate)
                    {
                        handler.ServerCertificateCustomValidationCallback =
                            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                    }
                    return handler;
                })
                .AddHttpMessageHandler<ApiHostHeaderHandler>()
                .AddHttpMessageHandler<ApiBearerTokenHandler>();

            builder.Services.AddControllersWithViews(options =>
            {
                options.Filters.Add(new AuthorizeFilter());
            });
            builder.Services.AddHttpContextAccessor();

            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            var jwtSection = builder.Configuration.GetSection(JwtSettings.SectionName);
            var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
                ?? jwtSection["SecretKey"]
                ?? string.Empty;
            var jwtIssuer = jwtSection["Issuer"] ?? "ColegioWeb";
            var jwtAudience = jwtSection["Audience"] ?? "ColegioApi";

            var authBuilder = builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.LoginPath = "/Login/Login";
                options.AccessDeniedPath = "/Login/Login";
                options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                options.SlidingExpiration = true;
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
                    ? CookieSecurePolicy.SameAsRequest
                    : CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Lax;
            });

            if (!string.IsNullOrWhiteSpace(jwtSecret) && jwtSecret.Length >= 32)
            {
                authBuilder.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtIssuer,
                        ValidAudience = jwtAudience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                        ClockSkew = TimeSpan.FromMinutes(2)
                    };
                });
            }

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

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
