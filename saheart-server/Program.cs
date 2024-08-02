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

            HoroscopeGenerator horoscopeGenerator = new();

            foreach (string sign in HoroscopeGenerator.zodiacSigns)
            {
                app.MapGet($"/{sign}", (HttpContext context) => {

                    var date = context.Request.Query["date"];
                    DateTime reqestDate;
                    try
                    {
                        reqestDate = DateTime.Parse(date);
                    }
                    catch (Exception ex)
                    {
                        return Results.BadRequest("Date parameter (in ISO string, like YYYY-MM-DD) is incorrect!");
                    }

                    context.Response.ContentType = "application/json";
                    HoroscopeResponse response = horoscopeGenerator.Generate($"{sign}", reqestDate);
                    return Results.Ok(response);
                });
            }

            app.Run();
        }
    }
}
