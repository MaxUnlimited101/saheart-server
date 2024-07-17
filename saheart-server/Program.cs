using Microsoft.AspNetCore.Html;

namespace saheart_server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var app = builder.Build();

            app.MapGet("/", async (HttpContext context) => {
                context.Response.ContentType = "text/html";
                await context.Response.SendFileAsync("wwwroot/index.html");
            });

            string[] zodiacSigns = ["aries", "taurus", "gemini", "cancer", "leo", "virgo", "libra", "scorpio", "sagittarius", "capricorn", "aquarius", "pisces"];

            foreach (string sign in zodiacSigns)
            {
                app.MapGet($"/{sign}", async (HttpContext context) => {
                    HoroscopeGenerator horoscopeGenerator = new HoroscopeGenerator();
                    HoroscopeResponse response = horoscopeGenerator.Generate($"{sign}");
                    return Results.Ok(response);
                });
            }

            app.Run();
        }

        public static HoroscopeResponse GenerateGoroscopeForZodiacSign(HoroscopeRequest request)
        {
            return new HoroscopeResponse();
        }
    }
}
