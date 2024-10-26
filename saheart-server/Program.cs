using Microsoft.AspNetCore.Html;
using System.Text.Json;
using System.Text.Json.Serialization;

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
                options.AddDefaultPolicy(builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });

            var app = builder.Build();

            app.UseStaticFiles();

            app.UseCors();

            app.MapGet("/", async (HttpContext context) =>
            {
                context.Response.ContentType = "text/html";
                await context.Response.SendFileAsync("wwwroot/index.html");
            });

            HoroscopeGenerator horoscopeGenerator = new();

            foreach (string lang in HoroscopeGenerator.allLanguages)
            {
                foreach (string sign in HoroscopeGenerator.zodiacSigns)
                {
                    app.MapGet($"/{lang}/{sign}", async (HttpContext context) =>
                    {
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
                        HoroscopeResponse response = horoscopeGenerator.Generate($"{sign}", reqestDate, lang);

                        return Results.Ok(response);
                    });
                }
            }

            app.Run();
        }
    }
}
