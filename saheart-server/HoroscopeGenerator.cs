using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace saheart_server
{
    public class HoroscopeGenerator
    {
        private static readonly string volumePath = "/app/data"; // Path where the volume is mounted
        private static readonly string pathToStateFile = "/app/data/horoscopeStateMap.json";
        private static readonly string pathToAllHoroscopesFolder = "./res/";
        private static readonly string pathToAllImages = "wwwroot/images";
        public static readonly string[] zodiacSigns = ["aries", "taurus", "gemini", "cancer", "leo", "virgo", "libra", "scorpio", "sagittarius", "capricorn", "aquarius", "pisces"];
        public static List<string> allLanguages;
        private const int daysTimeout = 1;

        private Dictionary<string, List<string>> allImagePathsMap;
        /// <summary>
        /// key - language code <br/>
        /// 2nd key - horoscope 
        /// </summary>
        private Dictionary<string, Dictionary<string, List<string>>> horoscopeStateMap;
        private DateTime horoscopeCreationDate;

        static HoroscopeGenerator()
        {
            if (!Directory.Exists(pathToAllHoroscopesFolder))
            {
                throw new FileNotFoundException("Horoscope files not found!");
            }

            allLanguages = Directory.EnumerateFiles(pathToAllHoroscopesFolder).Select(s => Path.GetFileName(s)).ToList();
        }

        public HoroscopeGenerator() 
        {
            // Ensure the directory exists
            if (!Directory.Exists(volumePath))
            {
                Directory.CreateDirectory(volumePath);
            }
            horoscopeStateMap = [];
            allImagePathsMap = [];

            foreach (string sign in zodiacSigns)
            {
                List<string> t = Directory.EnumerateFiles(pathToAllImages).ToList();
                t.Remove(Path.Combine(pathToAllImages, "_.txt")); // not an image, but needed for github repository
                allImagePathsMap[sign] = t;
                allImagePathsMap[sign].Shuffle();
            }

            if (!File.Exists(pathToStateFile))
            {
                InitStateDict();
            }
            else
            {
                Console.WriteLine("Reading old State file.");
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
                        horoscopeStateMap = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, List<string>>>>(line);
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

            IEnumerable<string> horoscopeFiles = Directory.EnumerateFiles(pathToAllHoroscopesFolder);

            Dictionary<string, List<string>> allHoroscopesByLanguage = [];

            foreach (string file in horoscopeFiles)
            {
                allHoroscopesByLanguage.Add(Path.GetFileName(file), []);
            }

            foreach (string file in horoscopeFiles)
            {
                using (StreamReader sr = new(file, System.Text.Encoding.UTF8))
                {
                    string? line;
                    do
                    {
                        line = sr.ReadLine();
                        if (line != null)
                        {
                            allHoroscopesByLanguage[Path.GetFileName(file)].Add(line);
                        }
                    } while (!string.IsNullOrEmpty(line));
                }
            }

            foreach (string lang in allLanguages)
            {
                horoscopeStateMap[lang] = [];
            }

            foreach (string sign in zodiacSigns)
            {
                int seed = (int)Math.Abs(DateTime.Now.Ticks % int.MaxValue);
                foreach (string lang in allLanguages)
                {
                    IListExtender.rng = new Random(seed);
                    allHoroscopesByLanguage[lang].Shuffle();
                    horoscopeStateMap[lang][sign] = (List<string>)allHoroscopesByLanguage[lang].Clone();
                }
            }
            FixHoroscopeUniqueness();

            string json = JsonSerializer.Serialize(horoscopeStateMap);
            using (StreamWriter sw = new(pathToStateFile))
            {
                sw.WriteLine(json);
            }
        }

        public HoroscopeResponse Generate(string zodiacSign, DateTime requestDate, string lang)
        {
            HoroscopeResponse response = new();
            response.Text = horoscopeStateMap[lang][zodiacSign][(((int)Math.Abs((requestDate - horoscopeCreationDate).TotalDays)) % horoscopeStateMap[lang][zodiacSign].Count) / daysTimeout];
            
            string rawPath = allImagePathsMap[zodiacSign][(((int)Math.Abs((requestDate - horoscopeCreationDate).TotalDays)) % allImagePathsMap[zodiacSign].Count)];
            rawPath = rawPath.Substring(rawPath.IndexOf('/'));
            response.PathToImage = rawPath.Replace('\\', '/');
            return response;
        }

        /// <summary>
        /// Function to fix if two diferent horoscopes have the same prediction on the same day. 
        /// I know it doesn't completely gurantee no collisions, but good enough
        /// </summary>
        private void FixHoroscopeUniqueness()
        {
            int horoscopeAmountPerLangPerSign = horoscopeStateMap["eng"]["cancer"].Count;
            foreach (string sign in zodiacSigns)
            {
                foreach (string sign2 in zodiacSigns)
                {
                    if (sign != sign2)
                    {
                        for (int i = 0; i < horoscopeAmountPerLangPerSign; i++)
                        {
                            if (horoscopeStateMap["eng"][sign][i] == horoscopeStateMap["eng"][sign2][i])
                            {
                                Console.WriteLine($"{sign},{sign2}, {i}");
                                foreach (string lan in allLanguages)
                                {
                                    (horoscopeStateMap[lan][sign2][i], horoscopeStateMap[lan][sign2][(i + 1) % horoscopeAmountPerLangPerSign]) =
                                        (horoscopeStateMap[lan][sign2][(i + 1) % horoscopeAmountPerLangPerSign], horoscopeStateMap[lan][sign2][i]);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
