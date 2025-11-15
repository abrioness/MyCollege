


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
            builder.Services.AddScoped<IServicesApi, ServicesApi>();
            builder.Services.AddControllersWithViews();
            builder.Configuration["ApiUrl"] = Environment.GetEnvironmentVariable("API_URL");
            //builder.Services.AddCors(options =>
            //{
            //    options.AddDefaultPolicy(policy =>
            //    {
            //        policy.WithOrigins("http://localhost:3000")
            //              .AllowAnyHeader()
            //              .AllowAnyMethod();
            //    });
            //});


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            //if (!app.Environment.IsDevelopment())
            //{
            //    app.UseExceptionHandler("/Error");
            //    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            //    app.UseHsts();
            //}

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            //app.UseAuthorization();

            //app.MapRazorPages();

            app.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Alumnos}/{action=Create}");


            app.Run();
        }
    }
}
