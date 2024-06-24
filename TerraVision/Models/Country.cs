namespace TerraVision.Models
{
    public class Country : ICountry
    {
        public string Code { get; set; }
        public string CommonName { get; set; }
        public string OfficialName { get; set; }
        public string Flag { get; set; }
        public string Capital { get; set; }
        public int Population { get; set; }
        public string Area { get; set; }
        public IContinent Continent { get; set; }
        public string Subregion { get; set; }
        public string[] Languages { get; set; }
        public string[] Currencies { get; set; }
        public string[] Timezones { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        public Weather CurrentWeather { get; set; }
    }
}