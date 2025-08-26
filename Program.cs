


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
            builder.Services.AddControllersWithViews();
          
                

                var app = builder.Build();


                    
         

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
