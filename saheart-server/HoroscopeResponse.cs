namespace saheart_server
{
    public class HoroscopeResponse
    {
        public HoroscopeResponse()
        {
            Text = string.Empty;
            PathToImage = string.Empty;
        }
        public string Text { get; set; }
        public string PathToImage { get; set; }
    }
}
