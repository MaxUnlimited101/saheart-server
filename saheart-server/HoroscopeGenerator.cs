namespace saheart_server
{
    public class HoroscopeGenerator
    {
        public HoroscopeGenerator() {}
        public HoroscopeResponse Generate(string zodiacSign)
        {
            HoroscopeResponse response = new HoroscopeResponse();
            response.Text = $"You, mr. {zodiacSign}, will have a great day!";
            return response;
        }
    }
}
