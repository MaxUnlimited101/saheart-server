using System.Reflection.Metadata;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace saheart_server
{
    public class HoroscopeGenerator
    {
        private static readonly string pathToStateFile = "./res/horoscopeStateMap.json";
        private static readonly string pathToHoroscopes = "./res/horoscope_text_full_ENG.txt";
        private string pathToImageForToday;
        private List<string> horoscopes;
        private Dictionary<string, string> horoscopeStateMap;
        private Random random;
        public readonly string[] zodiacSigns;
        public HoroscopeGenerator() 
        {
            zodiacSigns = ["aries", "taurus", "gemini", "cancer", "leo", "virgo", "libra", "scorpio", "sagittarius", "capricorn", "aquarius", "pisces"];
            random = new();
            horoscopes = [];
            horoscopeStateMap = [];
            pathToImageForToday = "";

            List<string> allImages = Directory.EnumerateFiles("wwwroot/images").ToList();
            string rawPath = allImages[random.Next(0, allImages.Count)];
            rawPath = rawPath.Substring(rawPath.IndexOf('/'));
            pathToImageForToday = rawPath.Replace('\\', '/');

            if (!File.Exists(pathToHoroscopes))
            {
                throw new FileNotFoundException("Horoscope file not found!");
            }
            using (StreamReader sr = new(pathToHoroscopes))
            {
                string? line;
                do
                {
                    line = sr.ReadLine();
                    if (line != null)
                    {
                        horoscopes.Add(line);
                    }
                } while (!string.IsNullOrEmpty(line));
            }

            if (!File.Exists(pathToStateFile))
            {
                InitStateDict();
            }
            else
            {
                DateTime creationTime = File.GetCreationTimeUtc(pathToStateFile);
                // reset the horoscopes each new day
                if ((creationTime - DateTime.UtcNow).TotalDays >= 1)
                {
                    InitStateDict();
                    return;
                }
                using (StreamReader sr = new(pathToStateFile))
                {
                    string? line = sr.ReadLine();
                    if (string.IsNullOrEmpty(line))
                    {
                        Console.WriteLine("State file not correct, making new one.");
                        InitStateDict();
                    }
                    else
                    {
                        horoscopeStateMap = JsonSerializer.Deserialize<Dictionary<string, string>>(line);
                        if (horoscopeStateMap == null)
                        {
                            Console.WriteLine("JSON deserialization failed, creating new one.");
                            InitStateDict();
                        }
                    }
                }
            }
        }

        private void InitStateDict()
        {
            horoscopeStateMap = [];
            foreach (string sign in zodiacSigns)
            {
                horoscopeStateMap[sign] = horoscopes[random.Next(0, horoscopes.Count)];
            }
            string json = JsonSerializer.Serialize(horoscopeStateMap);
            using (StreamWriter sw = new(pathToStateFile))
            {
                sw.WriteLine(json);
            }
        }

        public HoroscopeResponse Generate(string zodiacSign)
        {
            HoroscopeResponse response = new();

            response.Text = horoscopeStateMap[zodiacSign];
            response.PathToImage = pathToImageForToday;
            return response;
        }
    }
}
