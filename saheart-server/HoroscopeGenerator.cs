using System.Reflection.Metadata;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace saheart_server
{
    public class HoroscopeGenerator
    {
        private static readonly string volumePath = "/app/data"; // Path where the volume is mounted
        private static readonly string pathToStateFile = "/app/data/horoscopeStateMap.json";
        private static readonly string pathToHoroscopes = "./res/horoscope_text_full_ENG.txt";
        private static readonly string pathToAllImages = "wwwroot/images";
        public static readonly string[] zodiacSigns = ["aries", "taurus", "gemini", "cancer", "leo", "virgo", "libra", "scorpio", "sagittarius", "capricorn", "aquarius", "pisces"];

        private Dictionary<string, List<string>> allImagePathsMap;
        private List<string> horoscopes;
        private Random random;
        // key -> zodiac sign, List<string> -> list of all horoscopes (shuffled)
        private Dictionary<string, List<string>> horoscopeStateMap;
        private DateTime horoscopeCreationDate;

        public HoroscopeGenerator() 
        {
            // Ensure the directory exists
            if (!Directory.Exists(volumePath))
            {
                Directory.CreateDirectory(volumePath);
            }
            random = new();
            horoscopes = [];
            horoscopeStateMap = [];
            allImagePathsMap = [];

            foreach (string sign in zodiacSigns)
            {
                List<string> t = Directory.EnumerateFiles(pathToAllImages).ToList();
                t.Remove("_.txt"); // not an image, but needed for github repository
                allImagePathsMap[sign] = t;
                allImagePathsMap[sign].Shuffle();
            }

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
                horoscopeCreationDate = File.GetCreationTime(pathToStateFile).Date;
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
                        horoscopeStateMap = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(line);
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
            Console.WriteLine("Initializing new `horoscopeStateMap`");
            horoscopeStateMap = [];
            foreach (string sign in zodiacSigns)
            {
                horoscopes.Shuffle();
                horoscopeStateMap[sign] = (List<string>)horoscopes.Clone();
            }
            string json = JsonSerializer.Serialize(horoscopeStateMap);
            using (StreamWriter sw = new(pathToStateFile))
            {
                sw.WriteLine(json);
            }
        }

        public HoroscopeResponse Generate(string zodiacSign, DateTime requestDate)
        {
            HoroscopeResponse response = new();
            response.Text = horoscopeStateMap[zodiacSign][(((int)Math.Abs((requestDate - horoscopeCreationDate).TotalDays)) % horoscopes.Count) / 3];
            
            string rawPath = allImagePathsMap[zodiacSign][(((int)Math.Abs((requestDate - horoscopeCreationDate).TotalDays)) % horoscopes.Count)];
            rawPath = rawPath.Substring(rawPath.IndexOf('/'));
            response.PathToImage = rawPath.Replace('\\', '/');
            return response;
        }
    }
}
