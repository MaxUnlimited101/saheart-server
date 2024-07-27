using Microsoft.AspNetCore.Html;

namespace saheart_server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add CORS services
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });

            var app = builder.Build();

            app.UseStaticFiles();

            app.UseCors("AllowAll");

            app.MapGet("/", async (HttpContext context) =>
            {
                context.Response.ContentType = "text/html";
                await context.Response.SendFileAsync("wwwroot/index.html");
            });

            //foreach (string img in Directory.EnumerateFiles("wwwroot/img"))
            //{
            //    app.MapGet(img, async (HttpContext context) => {
            //        context.Response.ContentType = "img/jpeg";
            //        await context.Response.SendFileAsync($"{img}");
            //    });
            //}

            HoroscopeGenerator horoscopeGenerator = new();

            foreach (string sign in horoscopeGenerator.zodiacSigns)
            {
                app.MapGet($"/{sign}", (HttpContext context) => {
                    context.Response.ContentType = "application/json";
                    HoroscopeResponse response = horoscopeGenerator.Generate($"{sign}");
                    return Results.Ok(response);
                });
            }

            app.Run();
        }
    }
}
