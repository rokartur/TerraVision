using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace TerraVision.Models
{
    public class CountryService : UtilsForm, ICountryService
    {
        private readonly HttpClient _httpClient;
        
        public async Task<ICountry> GetCountryByName(string countryName, Label _loggedInUserLabel, ComboBox _searchBox)
        {
            var allCountries = await _httpClient.GetStringAsync("https://restcountries.com/v3.1/all");
            var countriesData = JArray.Parse(allCountries);
            var country = countriesData.SelectToken($"$[?(@.name.common == '{countryName}')]");
            
            if (country == null) return null;

            var countryCommonName = country["name"]?["common"]?.ToString();
            var countryOfficialName = country["name"]?["official"]?.ToString();
            var countryCode = country["cca2"]?.ToString();
            var countryCapital = country["capital"]?[0]?.ToString();
            var countryCurrencies = country["currencies"]?.ToString();
            var countryContinents = country["continents"]?[0]?.ToString();
            var countrySubRegion = country["subregion"]?.ToString();
            var countryLanguages = country["languages"]?.ToString();
            var countryPopulation = country["population"]?.ToString();
            var countryTimezones = country["timezones"]?.ToString();
            var countryArea = country["area"]?.ToString();
            var countryFlag = country["flags"]?["png"]?.ToString();
            var countryLat = Convert.ToDouble(country["latlng"][0]);
            var countryLng = Convert.ToDouble(country["latlng"][1]);

            var countryWeatherResponse = await _httpClient.GetStringAsync(
                $"https://api.open-meteo.com/v1/forecast?latitude={countryLat}&longitude={countryLng}&current=temperature_2m,wind_speed_10m");
            var countryWeatherData = JObject.Parse(countryWeatherResponse);
            var temperatureUnit = countryWeatherData["current_units"]["temperature_2m"].ToString();
            var speedUnit = countryWeatherData["current_units"]["wind_speed_10m"].ToString();
            var currentTemperature = Convert.ToDouble(countryWeatherData["current"]["temperature_2m"]);
            var currentWindSpeed = Convert.ToDouble(countryWeatherData["current"]["wind_speed_10m"]);

            var weather = new Weather
            {
                TemperatureUnit = temperatureUnit,
                SpeedUnit = speedUnit,
                CurrentTemperature = currentTemperature,
                CurrentWindSpeed = currentWindSpeed
            };
            
            var newCountry = new Country()
            {
                Code = countryCode,
                CommonName = countryCommonName,
                OfficialName = countryOfficialName,
                Flag = countryFlag,
                Capital = countryCapital,
                Population = int.Parse(countryPopulation),
                Area = countryArea,
                Continent = new Continent { Name = countryContinents },
                Subregion = countrySubRegion,
                Languages = countryLanguages.Split(','),
                Currencies = countryCurrencies.Split(','),
                Timezones = countryTimezones.Split(','),
                Lat = countryLat,
                Lng = countryLng,
                CurrentWeather = weather
            };
            
            var users = LoadUsers();
            var loggedInUser = users.Find(u => u.Username == _loggedInUserLabel.Text.Split(':')[1].Trim());
            
            if (loggedInUser.SearchHistory == null)
            {
                loggedInUser.SearchHistory = new List<string>();
            }
            loggedInUser.SearchHistory.Add(countryName);

            while (loggedInUser.SearchHistory.Count > 10)
            {
                loggedInUser.SearchHistory.RemoveAt(0);
            }

            SaveUsers(users);
            
            var users2 = LoadUsers();
            var loggedInUser2 = users2.Find(u => u.Username == _loggedInUserLabel.Text.Split(':')[1].Trim());
            
            _searchBox.Items.Clear();
                
            if (loggedInUser2.SearchHistory != null)
            {
                loggedInUser2.SearchHistory.Reverse();
                foreach (var searchItem in loggedInUser2.SearchHistory)
                {
                    _searchBox.Items.Add(searchItem);
                }
            }
            
            SaveUsers(users);
            
            return newCountry;
        }
    }
}