using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using TerraVision.Models;

namespace TerraVision
{
    public interface ICountry
    {
        string Code { get; set; }
        string CommonName { get; set; }
        string OfficialName { get; set; }
        string Flag { get; set; }
        string Capital { get; set; }
        int Population { get; set; }
        string Area { get; set; }
        IContinent Continent { get; set; }
        string Subregion { get; set; }
        string[] Languages { get; set; }
        string[] Currencies { get; set; }
        string[] Timezones { get; set; }
        double Lat { get; set; }
        double Lng { get; set; }
        Weather CurrentWeather { get; set; } 
    }

    public interface IContinent
    {
        string Name { get; set; }
        List<ICountry> Countries { get; set; }
    }
    
    public interface ICountryService
    {
        Task<ICountry> GetCountryByName(string countryName, Label loggedInUserLabel, ComboBox searchBox);
    }
    
    public interface ICountryInfo
    {
        void ShowCountryInfo(ICountry country);
    }
}